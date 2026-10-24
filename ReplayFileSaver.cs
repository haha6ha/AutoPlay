using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using BeatLeader.Models.Replay;

namespace AutoReplay
{
    public static class ReplayFileSaver
    {
        public static void SaveReplay(Replay replay, string hash, string diff)
        {
            try
            {
                string folder = Path.Combine(Environment.CurrentDirectory, "UserData", "BeatLeader", "ReplayerCache");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string safeHash = hash.Replace("/", "_").Replace("\\", "_");
                string file = Path.Combine(folder, $"AutoPlay_{safeHash}_{diff}.bsor");

                var encoderType = AccessTools.TypeByName("BeatLeader.Models.Replay.ReplayEncoder");
                var encodeMethod = encoderType?.GetMethod("Encode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                if (encodeMethod != null)
                {
                    using (var fs = File.Open(file, FileMode.Create, FileAccess.Write))
                    using (var bw = new BinaryWriter(fs))
                    {
                        encodeMethod.Invoke(null, new object[] { replay, bw });
                    }
                    Plugin.Log?.Info($"Saved AutoPlay replay to: {file}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.Warn($"Failed to save replay to disk: {ex.Message}");
            }
        }
    }
}
