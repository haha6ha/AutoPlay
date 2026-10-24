using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

namespace AutoReplay
{
    public static class PoseCalculator
    {
        // 轨道编号(从左到右): 第1轨 = lineIndex 0, 第2轨 = 1, 第3轨 = 2, 第4轨 = 3
        // 规则:
        //   - 第1轨 / 第4轨 的掩体不管 (墙只占外侧轨道时直接忽略, 跨到中间的墙只看它在 2-3 轨的部分)
        //   - 顶部掩体 (天花板墙) 同时盖住第2轨和第3轨 -> 蹲下 (可以是同一面墙, 也可以是两面墙同时存在)
        //   - 其他情况只盖住一条中间轨道 -> 歪头躲, 不蹲
        public static (Vector3 position, Quaternion rotation) CalculateHeadPose(float t, List<ObstacleData> obstacles, bool mirror = false)
        {
            float targetX = 0f;
            float targetY = AutoReplayMath.BaseHeadHeight;
            float targetRoll = 0f;
            float targetPitch = 0f;

            // 顶部掩体对第2轨/第3轨的覆盖情况 (多面墙同时存在时取并集)
            bool ceilingLane2 = false;
            bool ceilingLane3 = false;
            // 顶部掩体只盖住一条中间轨道时的歪头结果, 如果最后判定要蹲下就丢弃
            float ceilingTiltX = 0f;
            float ceilingTiltRoll = 0f;

            foreach (var obs in obstacles)
            {
                if (t < obs.time - 0.20f || t > obs.time + obs.duration + 0.10f) continue;

                // 只保留墙在中间两条轨道(lineIndex 1~2)的部分, 第1轨/第4轨的部分不管
                // 左手模式下游戏会把谱面左右镜像, 墙的轨道也要跟着镜像 (共 4 轨)
                int lineIndex = mirror ? 4 - (obs.lineIndex + obs.width) : obs.lineIndex;
                int c0 = Mathf.Max(lineIndex, 1);
                int c1 = Mathf.Min(lineIndex + obs.width - 1, 2);
                if (c0 > c1) continue; // 墙只在第1轨或第4轨 -> 忽略

                bool coversLane2 = c0 <= 1 && c1 >= 1;
                bool coversLane3 = c0 <= 2 && c1 >= 2;
                bool isCeilingWall = obs.lineLayer != NoteLineLayer.Base || obs.height <= 3;

                // 这面墙单独需要的歪头动作
                float tiltX, tiltRoll;
                if (coversLane2 && coversLane3)
                {
                    // 盖住第2和第3轨 -> 向右靠
                    tiltX = 0.45f; tiltRoll = -40f;
                }
                else if (coversLane2)
                {
                    // 只有第2轨(左中) -> 头向右歪
                    tiltX = 0.42f; tiltRoll = -40f;
                }
                else
                {
                    // 只有第3轨(右中) -> 头向左歪
                    tiltX = -0.42f; tiltRoll = 40f;
                }

                if (isCeilingWall)
                {
                    ceilingLane2 |= coversLane2;
                    ceilingLane3 |= coversLane3;
                    if (tiltX > 0f) { ceilingTiltX = Mathf.Max(ceilingTiltX, tiltX); ceilingTiltRoll = Mathf.Min(ceilingTiltRoll, tiltRoll); }
                    else { ceilingTiltX = Mathf.Min(ceilingTiltX, tiltX); ceilingTiltRoll = Mathf.Max(ceilingTiltRoll, tiltRoll); }
                }
                else
                {
                    // 非顶部的墙: 一律歪头, 不蹲
                    if (tiltX > 0f) { targetX = Mathf.Max(targetX, tiltX); targetRoll = Mathf.Min(targetRoll, tiltRoll); }
                    else { targetX = Mathf.Min(targetX, tiltX); targetRoll = Mathf.Max(targetRoll, tiltRoll); }
                }
            }

            if (ceilingLane2 && ceilingLane3)
            {
                // 第2、3轨同时有顶部掩体 -> 蹲下
                targetY = Mathf.Min(targetY, AutoReplayMath.DuckHeadHeight);
                targetPitch = Mathf.Max(targetPitch, 10f);
            }
            else if (ceilingTiltX > 0f)
            {
                targetX = Mathf.Max(targetX, ceilingTiltX);
                targetRoll = Mathf.Min(targetRoll, ceilingTiltRoll);
            }
            else if (ceilingTiltX < 0f)
            {
                targetX = Mathf.Min(targetX, ceilingTiltX);
                targetRoll = Mathf.Max(targetRoll, ceilingTiltRoll);
            }

            return (new Vector3(targetX, targetY, 0f), Quaternion.Euler(targetPitch, 0f, targetRoll));
        }

        public static (Vector3 position, Quaternion rotation) CalculateHandPose(float t, List<NoteSwingData> swings, List<NoteData> bombs, bool isLeft)
        {
            Vector3 wrist = isLeft ? AutoReplayMath.LeftWristBase : AutoReplayMath.RightWristBase;
            Quaternion rot = NoteSwingPlanner.SampleHandRotation(t, swings, bombs, isLeft);
            return (wrist, rot);
        }
    }
}



