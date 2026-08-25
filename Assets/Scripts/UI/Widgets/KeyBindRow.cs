using System;
using PS.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace PS.UI
{
    /// <summary>라벨 + 키 버튼. 누르면 다음 키 입력을 받아 바꾼다.</summary>
    public class KeyBindRow : OptionRow
    {
        [SerializeField] private GameAction m_Action;

        public Button BindButton;
        public TMP_Text NameLabel;
        public TMP_Text KeyLabel;

        [SerializeField] private string m_WaitingText = "키를 누르세요";

        public GameAction Action => m_Action;
        public bool IsWaiting { get; private set; }

        public event Action<GameAction, Key> Rebound;

        private void OnEnable()
        {
            if (BindButton != null) BindButton.onClick.AddListener(BeginCapture);
            GameSettings.KeysChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (BindButton != null) BindButton.onClick.RemoveListener(BeginCapture);
            GameSettings.KeysChanged -= Refresh;
            Cancel();
        }

        public void BeginCapture()
        {
            if (IsWaiting) return;

            IsWaiting = true;
            UIStack.SuppressEscape = true;

            if (KeyLabel != null) KeyLabel.text = m_WaitingText;
        }

        public void Cancel()
        {
            if (!IsWaiting) return;

            IsWaiting = false;
            UIStack.SuppressEscape = false;
            Refresh();
        }

        private void Update()
        {
            if (!IsWaiting) return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Cancel();
                return;
            }

            foreach (KeyControl control in keyboard.allKeys)
            {
                if (!control.wasPressedThisFrame) continue;

                Key key = control.keyCode;
                if (key == Key.None) continue;

                IsWaiting = false;
                UIStack.SuppressEscape = false;

                GameSettings.SetKey(m_Action, key);
                GameSettings.Save();

                Refresh();
                Rebound?.Invoke(m_Action, key);
                return;
            }
        }

        public void Refresh()
        {
            if (NameLabel != null) NameLabel.text = GameSettings.LabelOf(m_Action);
            if (KeyLabel != null && !IsWaiting) KeyLabel.text = GameSettings.KeyLabel(GameSettings.GetKey(m_Action));
        }
    }
}
