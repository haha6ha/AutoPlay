using UnityEngine;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using Vector3 = UnityEngine.Vector3;
using BLNoteCutInfo = BeatLeader.Models.Replay.NoteCutInfo;

namespace AutoReplay
{
    public static class NoteEventBuilder
    {
        public static NoteEvent Create115NoteEvent(NoteData note, bool noArrows = false)
        {
            Vector3 notePos = AutoReplayMath.GetNotePos(note);

            Vector3 cutDir = Vector3.down;
            Vector3 cutNormal = Vector3.right;

            switch (note.cutDirection)
            {
                case NoteCutDirection.Down: cutDir = Vector3.down; cutNormal = Vector3.right; break;
                case NoteCutDirection.Up: cutDir = Vector3.up; cutNormal = Vector3.left; break;
                case NoteCutDirection.Left: cutDir = Vector3.left; cutNormal = Vector3.up; break;
                case NoteCutDirection.Right: cutDir = Vector3.right; cutNormal = Vector3.down; break;
                case NoteCutDirection.UpLeft: cutDir = new Vector3(-0.7071f, 0.7071f, 0f); cutNormal = new Vector3(0.7071f, 0.7071f, 0f); break;
                case NoteCutDirection.UpRight: cutDir = new Vector3(0.7071f, 0.7071f, 0f); cutNormal = new Vector3(0.7071f, -0.7071f, 0f); break;
                case NoteCutDirection.DownLeft: cutDir = new Vector3(-0.7071f, -0.7071f, 0f); cutNormal = new Vector3(-0.7071f, 0.7071f, 0f); break;
                case NoteCutDirection.DownRight: cutDir = new Vector3(0.7071f, -0.7071f, 0f); cutNormal = new Vector3(-0.7071f, -0.7071f, 0f); break;
                default: cutDir = Vector3.down; cutNormal = Vector3.right; break;
            }

            var cutInfo = new BLNoteCutInfo
            {
                speedOK = true,
                directionOK = true,
                saberTypeOK = true,
                wasCutTooSoon = false,
                saberSpeed = 42.0f,
                cutDistanceToCenterPositive = true,
                cutDistanceToCenter = 0.0f,
                cutDirDeviation = 0.0f,
                beforeCutRating = 1.0f,
                afterCutRating = 1.0f,
                saberType = (int)note.colorType,
                timeDeviation = 0.0f,
                cutAngle = 0.0f,
                cutPoint = AutoReplayMath.ToBLVector(notePos),
                cutNormal = AutoReplayMath.ToBLVector(cutNormal),
                saberDir = AutoReplayMath.ToBLVector(cutDir)
            };

            int noteId = ReplayDataUtils.ComputeNoteId(note, false, false, false);
            // NA(无箭头)下, 回放器里的 NoteData 切割方向已经被游戏改成 Any(8),
            // 而 BeatLeader 是靠"时间 + 音符 ID"去匹配事件的, ID 末位就是切割方向.
            // 这里的 ID 如果还是原谱的方向, 就会匹配不上 -> 显示成 miss. 所以 NA 时把方向替换成 Any.
            if (noArrows)
                noteId = noteId - (int)note.cutDirection + (int)NoteCutDirection.Any;
            return new NoteEvent
            {
                noteID = noteId,
                spawnTime = note.time,
                eventTime = note.time,
                eventType = NoteEventType.good,
                noteCutInfo = cutInfo
            };
        }
    }
}


