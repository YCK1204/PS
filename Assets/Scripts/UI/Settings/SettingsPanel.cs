using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PS.Audio;
using PS.Core;

namespace PS.UI
{
    /// <summary>
    /// 설정 화면. 자식 OptionRow들을 Id로 찾아 GameSettings와 연결한다.
    /// 여닫기·ESC는 UIPanel/UIStack이 처리한다.
    /// </summary>
    public class SettingsPanel : UIPanel
    {
        public const string IdMaster = "master";
        public const string IdBgm = "bgm";
        public const string IdSfx = "sfx";
        public const string IdMuteFocus = "muteFocus";
        public const string IdResolution = "resolution";
        public const string IdDisplayMode = "displayMode";
        public const string IdVSync = "vsync";
        public const string IdFrameCap = "frameCap";
        public const string IdLanguage = "language";
        public const string IdScreenShake = "screenShake";
        public const string IdDamageNumbers = "damageNumbers";
        public const string IdShowTimer = "showTimer";

        static readonly string[] s_OnOff = { "꺼짐", "켜짐" };
        static readonly string[] s_DisplayModes = { "전체화면", "테두리 없음", "창모드" };
        static readonly string[] s_FrameCaps = { "무제한", "30", "60", "120", "144", "240" };
        static readonly string[] s_Languages = { "한국어" };

        public TabBar TabBar;
        public Button BackButton;
        public Button ResetButton;

        readonly Dictionary<string, OptionRow> m_Rows = new Dictionary<string, OptionRow>();
        Resolution[] m_Resolutions;
        bool m_Ready;

        void Awake()
        {
            CollectRows();
            CacheResolutions();
            BindRows();

            if (BackButton != null) BackButton.onClick.AddListener(Close);
            if (ResetButton != null) ResetButton.onClick.AddListener(ResetAll);
        }

        protected override void OnOpened()
        {
            if (TabBar != null) TabBar.Select(0);
            PushToUI();
        }

        protected override void OnClosing()
        {
            GameSettings.Save();
        }

        void CollectRows()
        {
            m_Rows.Clear();
            foreach (OptionRow row in GetComponentsInChildren<OptionRow>(true))
            {
                if (string.IsNullOrEmpty(row.Id) || m_Rows.ContainsKey(row.Id)) continue;
                m_Rows.Add(row.Id, row);
            }
        }

        void CacheResolutions()
        {
            Resolution[] all = Screen.resolutions;
            var list = new List<Resolution>();
            var seen = new HashSet<string>();
            for (int i = 0; i < all.Length; i++)
            {
                string key = all[i].width + "x" + all[i].height;
                if (seen.Add(key)) list.Add(all[i]);
            }
            m_Resolutions = list.ToArray();
        }

        string[] ResolutionLabels()
        {
            if (m_Resolutions == null || m_Resolutions.Length == 0) return new[] { "기본값" };
            var labels = new string[m_Resolutions.Length];
            for (int i = 0; i < m_Resolutions.Length; i++)
                labels[i] = m_Resolutions[i].width + " x " + m_Resolutions[i].height;
            return labels;
        }

        void BindRows()
        {
            BindSlider(IdMaster, v => { GameSettings.MasterVolume = v; Router()?.SetMaster(v); });
            BindSlider(IdBgm, v => { GameSettings.BgmVolume = v; Router()?.SetBgm(v); });
            BindSlider(IdSfx, v => { GameSettings.SfxVolume = v; Router()?.SetSfx(v); });
            BindSlider(IdScreenShake, v => GameSettings.ScreenShake = v);

            BindStepper(IdMuteFocus, s_OnOff, i => GameSettings.MuteOnFocusLoss = i == 1);
            BindStepper(IdVSync, s_OnOff, i => { GameSettings.VSync = i == 1; GameSettings.ApplyDisplay(); });
            BindStepper(IdDamageNumbers, s_OnOff, i => GameSettings.DamageNumbers = i == 1);
            BindStepper(IdShowTimer, s_OnOff, i => GameSettings.ShowTimer = i == 1);
            BindStepper(IdDisplayMode, s_DisplayModes, i => { GameSettings.DisplayMode = i; GameSettings.ApplyDisplay(); });
            BindStepper(IdFrameCap, s_FrameCaps, i => { GameSettings.FrameRateCap = i; GameSettings.ApplyDisplay(); });
            BindStepper(IdLanguage, s_Languages, i => GameSettings.Language = i);
            BindStepper(IdResolution, ResolutionLabels(), i => { GameSettings.ResolutionIndex = i; GameSettings.ApplyDisplay(); });

            m_Ready = true;
        }

        void BindSlider(string id, Action<float> onChanged)
        {
            if (m_Rows.TryGetValue(id, out OptionRow row) && row is SliderRow s)
                s.ValueChanged += v => { if (m_Ready) { onChanged(v); GameSettings.Save(); } };
        }

        void BindStepper(string id, string[] options, Action<int> onChanged)
        {
            if (m_Rows.TryGetValue(id, out OptionRow row) && row is StepperRow s)
            {
                s.SetOptions(options, s.Index);
                s.IndexChanged += i => { if (m_Ready) { onChanged(i); GameSettings.Save(); } };
            }
        }

        public void PushToUI()
        {
            SetSlider(IdMaster, GameSettings.MasterVolume);
            SetSlider(IdBgm, GameSettings.BgmVolume);
            SetSlider(IdSfx, GameSettings.SfxVolume);
            SetSlider(IdScreenShake, GameSettings.ScreenShake);

            SetStepper(IdMuteFocus, GameSettings.MuteOnFocusLoss ? 1 : 0);
            SetStepper(IdVSync, GameSettings.VSync ? 1 : 0);
            SetStepper(IdDamageNumbers, GameSettings.DamageNumbers ? 1 : 0);
            SetStepper(IdShowTimer, GameSettings.ShowTimer ? 1 : 0);
            SetStepper(IdDisplayMode, GameSettings.DisplayMode);
            SetStepper(IdFrameCap, GameSettings.FrameRateCap);
            SetStepper(IdLanguage, GameSettings.Language);
            SetStepper(IdResolution, ResolveResolutionIndex());
        }

        int ResolveResolutionIndex()
        {
            if (m_Resolutions == null || m_Resolutions.Length == 0) return 0;
            int saved = GameSettings.ResolutionIndex;
            if (saved >= 0 && saved < m_Resolutions.Length) return saved;
            for (int i = 0; i < m_Resolutions.Length; i++)
                if (m_Resolutions[i].width == Screen.width && m_Resolutions[i].height == Screen.height) return i;
            return m_Resolutions.Length - 1;
        }

        void SetSlider(string id, float v)
        {
            if (m_Rows.TryGetValue(id, out OptionRow row) && row is SliderRow s) s.SetValue(v);
        }

        void SetStepper(string id, int i)
        {
            if (m_Rows.TryGetValue(id, out OptionRow row) && row is StepperRow s) s.SetIndex(i, false);
        }

        public void ResetAll()
        {
            GameSettings.ResetToDefaults();
            GameSettings.Apply();
            PushToUI();
        }

        static AudioRouter Router() => AudioRouter.Instance;
    }
}
