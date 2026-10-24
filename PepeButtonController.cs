using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Animations;
using BeatSaberMarkupLanguage.FloatingScreen;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoReplay
{
    /// <summary>
    /// 把 Pepe 按钮放在世界坐标 (2.1, 0.6, 3.2), 也就是歌曲列表和排行榜之间, 并朝向玩家。
    /// 用 BSML 的 FloatingScreen 承载 (自带射线检测), 按钮本体是自己建的 Image + Button,
    /// 使用游戏的 UINoGlow 材质, 所以不发光。
    /// GIF 首次运行时从 GifUrl 下载并缓存到 UserData/AutoReplay/bs-pepe.gif。
    /// </summary>
    public static class PepeButtonController
    {
        private const string ScreenName = "AutoReplayPepeScreen";
        private const string GifUrl = "https://beatleader.com/assets/bs-pepe.gif";

        // ===== 可调参数 =====
        private static readonly Vector3 Position = new Vector3(2.1f, 0.6f, 3.2f);
        private static readonly Vector3 PlayerHead = new Vector3(0f, 1.5f, 0f);
        private const float ButtonSize = 18f;
        private const bool ShowHandle = false;

        private static FloatingScreen _screen;
        private static AnimationControllerData _pepeAnimData;
        private static Sprite _fallbackSprite;
        private static Texture2D _fallbackTexture;
        private static Material _noGlowMat;
        private static bool _loading;   // 防止重复下载/解码

        public static void AttachToLevelFiltering(LevelFilteringNavigationController nav)
        {
            EnsureCreated();
        }

        public static void EnsureCreated()
        {
            try
            {
                if (_screen != null) return;
                Create();
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"[PepeButton] Create failed: {ex}");
            }
        }

        private static void Create()
        {
            float panel = ButtonSize + 2f;
            Quaternion rotation = Quaternion.LookRotation(Position - PlayerHead);

            _screen = FloatingScreen.CreateFloatingScreen(
                new Vector2(panel, panel),
                ShowHandle,
                Position,
                rotation);
            _screen.name = ScreenName;
            if (ShowHandle)
            {
                _screen.ShowHandle = true;
                _screen.HandleSide = FloatingScreen.Side.Bottom;
            }

            var go = new GameObject("PepeButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(_screen.transform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(ButtonSize, ButtonSize);
            rt.localScale = Vector3.one;

            var img = go.GetComponent<Image>();
            img.sprite = GetFallbackSprite();
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.color = Color.white;
            img.raycastTarget = true;
            ApplyNoGlow(img);

            var button = go.GetComponent<Button>();
            button.targetGraphic = img;
            button.interactable = true;
            button.onClick.AddListener(() =>
            {
                Plugin.Log?.Info("[PepeButton] Clicked.");
                try
                {
                    AutoReplayManager.Instance.StartAutoPlay();
                }
                catch (Exception ex)
                {
                    Plugin.Log?.Error($"[PepeButton] StartAutoPlay failed: {ex}");
                }
            });

            // 不要再手动 AddComponent<HoverHint>(): 它需要 Zenject 注入, 否则鼠标一指上去就抛空引用并打断点击。

            // GIF 加载失败/加载中显示的文字
            var labelGo = new GameObject("FallbackLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "PEPE";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 4.0f;
            label.color = Color.black;
            label.raycastTarget = false;
            foreach (var f in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (f != null && f.name == "Teko-Medium SDF") { label.font = f; break; }
            }

            Plugin.Log?.Info($"[PepeButton] FloatingScreen created at {Position}, rotation={rotation.eulerAngles}.");
            LoadPepeGif(img, label);
        }

        private static void ApplyNoGlow(Image img)
        {
            if (_noGlowMat == null)
            {
                foreach (var m in Resources.FindObjectsOfTypeAll<Material>())
                {
                    if (m != null && m.name == "UINoGlow") { _noGlowMat = m; break; }
                }
                if (_noGlowMat == null)
                {
                    foreach (var m in Resources.FindObjectsOfTypeAll<Material>())
                    {
                        if (m != null && m.name.Contains("UINoGlow")) { _noGlowMat = m; break; }
                    }
                }
                if (_noGlowMat == null)
                    Plugin.Log?.Warn("[PepeButton] UINoGlow material not found; button may glow.");
            }
            if (_noGlowMat != null) img.material = _noGlowMat;
        }

        private static Sprite GetFallbackSprite()
        {
            if (_fallbackSprite != null) return _fallbackSprite;

            _fallbackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "PepeButtonFallbackTexture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            _fallbackTexture.SetPixel(0, 0, new Color(0.25f, 0.85f, 0.35f, 1f));
            _fallbackTexture.Apply();
            _fallbackSprite = Sprite.Create(_fallbackTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            _fallbackSprite.name = "PepeButtonFallbackSprite";
            return _fallbackSprite;
        }

        // GIF 文件头: "GIF8"
        private static bool IsGif(byte[] b)
        {
            return b != null && b.Length > 6 && b[0] == 'G' && b[1] == 'I' && b[2] == 'F' && b[3] == '8';
        }

        /// <summary>优先读本地缓存; 没有 (或文件损坏) 就从 GifUrl 下载并写入缓存。</summary>
        private static async Task<byte[]> GetGifBytesAsync(string gifPath)
        {
            if (File.Exists(gifPath))
            {
                byte[] cached = File.ReadAllBytes(gifPath);
                if (IsGif(cached)) return cached;

                Plugin.Log?.Warn("[PepeButton] Cached GIF is invalid, deleting and re-downloading.");
                try { File.Delete(gifPath); } catch { }
            }

            Plugin.Log?.Info($"[PepeButton] Downloading GIF from {GifUrl}");
            try { ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072; } catch { }   // TLS 1.2

            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "AutoReplay/1.0 (BeatSaber mod)";
                byte[] bytes = await wc.DownloadDataTaskAsync(GifUrl);

                if (!IsGif(bytes))
                {
                    Plugin.Log?.Warn($"[PepeButton] Downloaded data is not a GIF ({(bytes == null ? 0 : bytes.Length)} bytes).");
                    return null;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(gifPath));
                File.WriteAllBytes(gifPath, bytes);
                Plugin.Log?.Info($"[PepeButton] GIF downloaded ({bytes.Length} bytes) and cached at {gifPath}");
                return bytes;
            }
        }

        private static async void LoadPepeGif(Image img, TextMeshProUGUI fallbackLabel)
        {
            try
            {
                if (_pepeAnimData == null)
                {
                    if (_loading) return;   // 已有一个加载任务在跑
                    _loading = true;
                    try
                    {
                        string gifPath = Path.Combine(Environment.CurrentDirectory, "UserData", "AutoReplay", "bs-pepe.gif");

                        byte[] bytes = await GetGifBytesAsync(gifPath);
                        if (bytes == null)
                        {
                            Plugin.Log?.Warn("[PepeButton] No GIF data; showing fallback button.");
                            return;
                        }

                        var data = await AnimationLoader.ProcessGifAsync(bytes);
                        if (data == null)
                        {
                            Plugin.Log?.Warn("[PepeButton] GIF decoder returned null; showing fallback button.");
                            return;
                        }

                        // AnimationController 可能还没初始化, 最多等 5 秒
                        for (int i = 0; i < 50 && AnimationController.Instance == null; i++)
                            await Task.Delay(100);

                        if (AnimationController.Instance == null)
                        {
                            Plugin.Log?.Warn("[PepeButton] AnimationController is not ready; showing fallback button.");
                            return;
                        }

                        _pepeAnimData = AnimationController.Instance.Register("AutoPlay_Pepe", data);
                    }
                    finally
                    {
                        _loading = false;
                    }
                }

                // await 期间场景可能已经切换, 按钮被销毁
                if (img == null || img.gameObject == null || _pepeAnimData == null) return;

                var updater = img.gameObject.AddComponent<AnimationStateUpdater>();
                updater.Image = img;
                updater.ControllerData = _pepeAnimData;
                ApplyNoGlow(img);
                if (fallbackLabel != null) fallbackLabel.gameObject.SetActive(false);
                Plugin.Log?.Info("[PepeButton] GIF animation attached successfully.");
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error($"[PepeButton] Failed to load GIF: {ex}");
            }
        }
    }
}
