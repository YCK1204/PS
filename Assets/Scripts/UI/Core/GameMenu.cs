using PS.Core;
using PS.Game.Actors;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PS.UI
{
    /// <summary>게임 중 화면 여닫기를 한 곳에서 본다.
    /// ESC — 열린 게 있으면 UIStack이 닫고, 없으면 설정을 연다.
    /// 인벤토리 키 — 설정에서 바꾼 키를 그대로 쓴다.</summary>
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private SettingsPanel m_Settings;
        [SerializeField] private UIPanel m_Inventory;

        [Tooltip("UI가 열려 있는 동안 조작을 막을 대상")]
        [SerializeField] private PlayerController m_Player;

        private Key m_InventoryKey;

        private void OnEnable()
        {
            UIStack.EnsureRunner();
            UIStack.EscapeOnEmpty += OpenSettings;
            GameSettings.KeysChanged += Reload;
            Reload();
        }

        private void OnDisable()
        {
            UIStack.EscapeOnEmpty -= OpenSettings;
            GameSettings.KeysChanged -= Reload;
        }

        private void Reload() => m_InventoryKey = GameSettings.GetKey(GameAction.Inventory);

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null && m_Inventory != null && keyboard[m_InventoryKey].wasPressedThisFrame)
            {
                if (m_Inventory.IsOpen) m_Inventory.Close();
                else m_Inventory.Open();
            }

            if (m_Player != null) m_Player.Blocked = UIStack.AnyOpen;
        }

        public void OpenSettings()
        {
            if (m_Settings == null || m_Settings.IsOpen) return;
            m_Settings.Open();
        }
    }
}
