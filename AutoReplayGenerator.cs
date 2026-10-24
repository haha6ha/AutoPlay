using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models.Replay;

namespace AutoReplay
{
    public static class AutoReplayGenerator
    {
        private static NoteData MirrorNote(NoteData note)
        {
            var copy = note.CopyWith();
            copy.Mirror(4);
            return copy;
        }

        public static Replay GenerateReplay(
            IReadonlyBeatmapData beatmapData,
            BeatmapLevel level,
            BeatmapKey key,
            string hash,
            GameplayModifiers modifiers = null,
            bool leftHanded = false)
        {
            var replay = new Replay();

            IEnumerable<NoteData> sortedNotes = null;
            try
            {
                var method = HarmonyLib.AccessTools.Method("BeatLeader.Replayer.Emulation.ReplayerScoreProcessor:CreateSortedNoteDataList");
                sortedNotes = method?.Invoke(null, new object[] { beatmapData.allBeatmapDataItems }) as IEnumerable<NoteData>;
            }
            catch { }

            if (sortedNotes == null)
            {
                sortedNotes = beatmapData.allBeatmapDataItems.OfType<NoteData>().OrderBy(n => n.time);
            }

            // 左手模式: 游戏会把谱面左右镜像 (轨道翻转、红蓝互换、切割方向镜像).
            // BeatLeader 用"时间 + 音符 ID"匹配事件, ID 来自镜像后的谱面, 所以这里也必须用镜像后的音符.
            // 做法和 BeatLeader 自己镜像音符的方式一致: CopyWith() 复制一份再 Mirror(4), 不改动原数据.
            if (leftHanded)
            {
                sortedNotes = sortedNotes.Select(MirrorNote).ToList();
            }

            var allNotes = sortedNotes
                .Where(n => n.scoringType != NoteData.ScoringType.Ignore &&
                            n.gameplayType != NoteData.GameplayType.Bomb &&
                            (n.colorType == ColorType.ColorA || n.colorType == ColorType.ColorB))
                .ToList();

            var obstacles = beatmapData.allBeatmapDataItems
                .OfType<ObstacleData>()
                .OrderBy(o => o.time)
                .ToList();

            var bombs = beatmapData.allBeatmapDataItems
                .OfType<NoteData>()
                .Where(n => n.gameplayType == NoteData.GameplayType.Bomb)
                .Select(b => leftHanded ? MirrorNote(b) : b)
                .OrderBy(b => b.time)
                .ToList();

            float startTime = 0f;
            float endTime = allNotes.Count > 0 ? allNotes.Max(n => n.time) + 2.5f : 60f;
            if (obstacles.Count > 0)
            {
                endTime = Math.Max(endTime, obstacles.Max(o => o.time + o.duration) + 2.0f);
            }

            var leftNotes = allNotes.Where(n => n.colorType == ColorType.ColorA).ToList();
            var rightNotes = allNotes.Where(n => n.colorType == ColorType.ColorB).ToList();

            // NA(无箭头)模式: 所有方块当点块处理, 上下交替挥, 不看谱面里的切割方向
            bool noArrows = modifiers != null && modifiers.noArrows;

            var leftSwings = NoteSwingPlanner.PlanSwings(leftNotes, bombs, AutoReplayMath.LeftWristBase, true, noArrows);
            var rightSwings = NoteSwingPlanner.PlanSwings(rightNotes, bombs, AutoReplayMath.RightWristBase, false, noArrows);

            var noteEvents = new List<NoteEvent>();
            int totalScore = 0;
            int combo = 0;

            foreach (var note in allNotes)
            {
                noteEvents.Add(NoteEventBuilder.Create115NoteEvent(note, noArrows));
                combo++;
                int mult = combo >= 14 ? 8 : (combo >= 6 ? 4 : (combo >= 2 ? 2 : 1));
                totalScore += 115 * mult;
            }

            const float fps = 90f;
            float dt = 1f / fps;
            int totalFrames = (int)Math.Ceiling((endTime - startTime) * fps);
            var frames = new List<Frame>(totalFrames);

            Vector3 pommelOffset = new Vector3(0f, 0f, 0.20f);

            for (int i = 0; i < totalFrames; i++)
            {
                float t = startTime + i * dt;
                var head = PoseCalculator.CalculateHeadPose(t, obstacles, leftHanded);
                var left = PoseCalculator.CalculateHandPose(t, leftSwings, bombs, true);
                var right = PoseCalculator.CalculateHandPose(t, rightSwings, bombs, false);

                Vector3 leftPos = left.position + (left.rotation * pommelOffset);
                Vector3 rightPos = right.position + (right.rotation * pommelOffset);

                frames.Add(new Frame
                {
                    time = t,
                    fps = 90,
                    head = new BeatLeader.Models.Replay.Transform
                    {
                        position = AutoReplayMath.ToBLVector(head.position),
                        rotation = AutoReplayMath.ToBLQuaternion(head.rotation)
                    },
                    leftHand = new BeatLeader.Models.Replay.Transform
                    {
                        position = AutoReplayMath.ToBLVector(leftPos),
                        rotation = AutoReplayMath.ToBLQuaternion(left.rotation)
                    },
                    rightHand = new BeatLeader.Models.Replay.Transform
                    {
                        position = AutoReplayMath.ToBLVector(rightPos),
                        rotation = AutoReplayMath.ToBLQuaternion(right.rotation)
                    }
                });
            }

            replay.info = new ReplayInfo
            {
                version = "2.0.0",
                gameVersion = "1.40.8",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                playerID = "Auto Play",
                playerName = "Auto Play",
                platform = "steam",
                trackingSytem = "OpenVR",
                hmd = "Oculus",
                controller = "Touch",
                hash = hash,
                songName = level.songName ?? "Unknown",
                mapper = level.allMappers != null && level.allMappers.Length > 0 ? level.allMappers[0] : "Unknown",
                difficulty = key.difficulty.ToString(),
                score = totalScore,
                mode = key.beatmapCharacteristic.serializedName,
                environment = level.GetEnvironmentName(key.beatmapCharacteristic, key.difficulty).ToString(),
                modifiers = AutoReplayMath.EncodeModifiers(modifiers),
                jumpDistance = 0f,
                leftHanded = leftHanded,
                height = AutoReplayMath.BaseHeadHeight,
                startTime = 0f,
                failTime = 0f,
                // BeatLeader 里 speed != 0 会被当成"练习模式"设置(PracticeSettings), 回放就会以练习模式启动.
                // 变速由 modifiers 里的 FS/SF/SS 负责, 所以这里保持 0, 和正常打出来的回放一致.
                speed = 0f
            };

            replay.frames = frames;
            replay.notes = noteEvents;
            replay.walls = new List<WallEvent>();
            replay.heights = new List<AutomaticHeight> { new AutomaticHeight { height = AutoReplayMath.BaseHeadHeight, time = 0f } };
            replay.pauses = new List<Pause>();
            replay.saberOffsets = new SaberOffsets();
            replay.customData = new Dictionary<string, byte[]>();

            return replay;
        }
    }
}
