using UnityEngine;

namespace PS.Core
{
    /// <summary>
    /// 게임 설정값. PlayerPrefs에 저장되고 Apply()로 실제 반영된다.
    /// </summary>
    public static class GameSettings
    {
        const string k_Prefix = "ps.settings.";

        // --- Sound (0~1) ---
        public static float MasterVolume { get => GetF("masterVolume", 0.8f); set => SetF("masterVolume", value); }
        public static float BgmVolume    { get => GetF("bgmVolume", 0.7f);    set => SetF("bgmVolume", value); }
        public static float SfxVolume    { get => GetF("sfxVolume", 0.9f);    set => SetF("sfxVolume", value); }
        public static bool  MuteOnFocusLoss { get => GetB("muteOnFocusLoss", true); set => SetB("muteOnFocusLoss", value); }

        // --- Display ---
        public static int  ResolutionIndex { get => GetI("resolutionIndex", -1); set => SetI("resolutionIndex", value); }
        public static int  DisplayMode     { get => GetI("displayMode", 1);      set => SetI("displayMode", value); }
        public static bool VSync           { get => GetB("vsync", true);         set => SetB("vsync", value); }
        public static int  FrameRateCap    { get => GetI("frameRateCap", 0);     set => SetI("frameRateCap", value); }

        // --- Gameplay ---
        public static int   Language      { get => GetI("language", 0);        set => SetI("language", value); }
        public static float ScreenShake   { get => GetF("screenShake", 1f);    set => SetF("screenShake", value); }
        public static bool  DamageNumbers { get => GetB("damageNumbers", true); set => SetB("damageNumbers", value); }
        public static bool  ShowTimer     { get => GetB("showTimer", false);   set => SetB("showTimer", value); }

        public static readonly int[] FrameRateOptions = { 0, 30, 60, 120, 144, 240 };

        /// <summary>볼륨이 바뀌었을 때 AudioRouter가 받아가는 신호.</summary>
        public static event System.Action AudioChanged;

        /// <summary>저장된 값을 실제 엔진 설정에 반영한다.</summary>
        public static void Apply()
        {
            ApplyDisplay();
            AudioChanged?.Invoke();
        }

        public static void ApplyDisplay()
        {
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = VSync ? -1 : ResolveFrameRateCap();

            Resolution[] all = Screen.resolutions;
            int idx = ResolutionIndex;
            if (all.Length > 0 && idx >= 0 && idx < all.Length)
            {
                Resolution r = all[idx];
                Screen.SetResolution(r.width, r.height, ToFullScreenMode(DisplayMode), r.refreshRateRatio);
            }
            else
            {
                Screen.fullScreenMode = ToFullScreenMode(DisplayMode);
            }
        }

        static int ResolveFrameRateCap()
        {
            int i = Mathf.Clamp(FrameRateCap, 0, FrameRateOptions.Length - 1);
            int v = FrameRateOptions[i];
            return v == 0 ? -1 : v;
        }

        public static FullScreenMode ToFullScreenMode(int mode)
        {
            switch (mode)
            {
                case 1:  return FullScreenMode.FullScreenWindow;
                case 2:  return FullScreenMode.Windowed;
                default: return FullScreenMode.ExclusiveFullScreen;
            }
        }

        public static void ResetToDefaults()
        {
            MasterVolume = 0.8f; BgmVolume = 0.7f; SfxVolume = 0.9f; MuteOnFocusLoss = true;
            ResolutionIndex = -1; DisplayMode = 1; VSync = true; FrameRateCap = 0;
            Language = 0; ScreenShake = 1f; DamageNumbers = true; ShowTimer = false;
            Save();
        }

        public static void Save() => PlayerPrefs.Save();

        static float GetF(string k, float d) => PlayerPrefs.GetFloat(k_Prefix + k, d);
        static void  SetF(string k, float v) => PlayerPrefs.SetFloat(k_Prefix + k, v);
        static int   GetI(string k, int d)   => PlayerPrefs.GetInt(k_Prefix + k, d);
        static void  SetI(string k, int v)   => PlayerPrefs.SetInt(k_Prefix + k, v);
        static bool  GetB(string k, bool d)  => PlayerPrefs.GetInt(k_Prefix + k, d ? 1 : 0) != 0;
        static void  SetB(string k, bool v)  => PlayerPrefs.SetInt(k_Prefix + k, v ? 1 : 0);
    }
}
