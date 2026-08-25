using PS.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PS.Game.Actors
{
    /// <summary>키보드를 캐릭터 동작으로 옮긴다. 키는 GameSettings에서 읽으므로
    /// 설정 화면에서 바꾸면 즉시 반영된다. 조작을 Motor에서 떼어놔야 적이 Motor를 그대로 쓴다.</summary>
    [RequireComponent(typeof(Character))]
    public class PlayerController : MonoBehaviour
    {
        private Character m_Character;

        private Key m_Left;
        private Key m_Right;
        private Key m_Jump;
        private Key m_Dash;
        private Key m_Attack;

        /// <summary>UI가 열려 있는 동안 꺼둔다.</summary>
        public bool Blocked { get; set; }

        private void Awake() => m_Character = GetComponent<Character>();

        private void OnEnable()
        {
            GameSettings.KeysChanged += Reload;
            Reload();
        }

        private void OnDisable() => GameSettings.KeysChanged -= Reload;

        private void Reload()
        {
            m_Left = GameSettings.GetKey(GameAction.MoveLeft);
            m_Right = GameSettings.GetKey(GameAction.MoveRight);
            m_Jump = GameSettings.GetKey(GameAction.Jump);
            m_Dash = GameSettings.GetKey(GameAction.Dash);
            m_Attack = GameSettings.GetKey(GameAction.Attack);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !m_Character.IsAlive) return;

            if (Blocked)
            {
                m_Character.Motor.SetMove(0f);
                return;
            }

            float move = 0f;
            if (keyboard[m_Left].isPressed) move -= 1f;
            if (keyboard[m_Right].isPressed) move += 1f;
            m_Character.Motor.SetMove(move);

            if (keyboard[m_Jump].wasPressedThisFrame) m_Character.Motor.PressJump();

            if (keyboard[m_Dash].wasPressedThisFrame && m_Character.Motor.TryDash())
                m_Character.Combat.CancelAttack();

            if (keyboard[m_Attack].wasPressedThisFrame) m_Character.Combat.TryAttack();
        }
    }
}
