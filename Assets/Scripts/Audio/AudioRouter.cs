using UnityEngine;
using UnityEngine.Audio;
using PS.Core;

namespace PS.Audio
{
    /// <summary>
    /// AudioMixer에 볼륨을 밀어넣는 단일 진입점. 씬을 넘어 살아남는다.
    /// 슬라이더 값(0~1)을 dB로 변환해 노출 파라미터에 쓴다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class AudioRouter : MonoBehaviour
    {
        public const string MasterParam = "MasterVolume";
        public const string BgmParam    = "BGMVolume";
        public const string SfxParam    = "SFXVolume";

        const float k_MinDb = -80f;

        public static AudioRouter Instance { get; private set; }

        public AudioMixer Mixer;
        public AudioSource BgmSource;
        public AudioSource SfxSource;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            GameSettings.AudioChanged += ApplyVolumes;
        }

        // AudioMixer는 Awake 시점에 SetFloat이 먹지 않을 때가 있어 Start에서 한 번 더 민다.
        void Start()
        {
            ApplyVolumes();
        }

        void OnDestroy()
        {
            if (Instance == this) GameSettings.AudioChanged -= ApplyVolumes;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!GameSettings.MuteOnFocusLoss) { AudioListener.pause = false; return; }
            AudioListener.pause = !hasFocus;
        }

        public void ApplyVolumes()
        {
            SetMaster(GameSettings.MasterVolume);
            SetBgm(GameSettings.BgmVolume);
            SetSfx(GameSettings.SfxVolume);
        }

        public void SetMaster(float v01) => Write(MasterParam, v01);
        public void SetBgm(float v01)    => Write(BgmParam, v01);
        public void SetSfx(float v01)    => Write(SfxParam, v01);

        /// <summary>SFX 볼륨 확인용 원샷 재생.</summary>
        public void PlayPreview(AudioClip clip)
        {
            if (clip == null || SfxSource == null) return;
            SfxSource.PlayOneShot(clip);
        }

        void Write(string param, float v01)
        {
            if (Mixer == null) return;
            Mixer.SetFloat(param, LinearToDb(v01));
        }

        public static float LinearToDb(float v01)
        {
            v01 = Mathf.Clamp01(v01);
            return v01 <= 0.0001f ? k_MinDb : Mathf.Log10(v01) * 20f;
        }
    }
}
