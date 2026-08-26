using UnityEngine;
using UnityEngine.UI;
using PS.Core;

namespace PS.UI
{
    /// <summary>타이틀 화면 버튼 배선. 설정 패널 열고 닫기.</summary>
    public class TitleMenu : MonoBehaviour
    {
        public Button StartButton;
        public Button SettingsButton;
        public Button QuitButton;
        public SettingsPanel SettingsPanel;
        public GameObject MenuRoot;

        [Tooltip("게임 시작이 이동할 씬")]
        public SceneType StartScene = SceneType.Battle;

        [Tooltip("비동기로 전환할지")]
        public bool AsyncLoad;

        void Awake()
        {
            if (StartButton != null) StartButton.onClick.AddListener(StartGame);
            if (SettingsButton != null) SettingsButton.onClick.AddListener(OpenSettings);
            if (QuitButton != null) QuitButton.onClick.AddListener(Quit);
            if (SettingsPanel != null)
            {
                SettingsPanel.Closed += OnSettingsClosed;
                SettingsPanel.gameObject.SetActive(false);
            }
            GameSettings.Apply();
        }


        public void StartGame()
        {
            if (AsyncLoad) SceneManager.LoadAsync(StartScene);
            else SceneManager.Load(StartScene);
        }

        public void OpenSettings()
        {
            if (SettingsPanel == null) return;
            SettingsPanel.Open();
            if (MenuRoot != null) MenuRoot.SetActive(false);
        }

        public void CloseSettings()
        {
            if (SettingsPanel != null) SettingsPanel.Close();
            else if (MenuRoot != null) MenuRoot.SetActive(true);
        }

        void OnSettingsClosed(UIPanel panel)
        {
            if (MenuRoot != null) MenuRoot.SetActive(true);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
