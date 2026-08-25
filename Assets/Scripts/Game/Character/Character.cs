using PS.Game.Combat;
using SO;
using UnityEngine;

namespace PS.Game.Actors
{
    /// <summary>CharacterData로 세팅되는 전투 액터. 플레이어도 몬스터도 이 클래스를 쓴다 —
    /// 둘의 차이는 붙는 입력원(PlayerInput / AI)뿐이다.</summary>
    [RequireComponent(typeof(CharacterMotor))]
    [RequireComponent(typeof(CharacterCombat))]
    [RequireComponent(typeof(Health))]
    public class Character : Combatant
    {
        [SerializeField] private CharacterData m_Data;

        public CharacterData Data => m_Data;

        protected override void Configure()
        {
            if (m_Data == null) return;

            m_Data.ApplyBaseStats(Stats);
            Motor?.Bind(Stats, m_Data.Motor);
        }

        private void Start()
        {
            if (m_Data != null) Combat?.Equip(m_Data.StartWeapon);
        }

        private void Update()
        {
            if (!IsAlive || Motor == null) return;

            if (Combat != null)
            {
                Combat.Facing = Motor.Facing;
                Motor.MoveLocked = Combat.IsAttacking;
            }
            Anim?.SetFacing(Motor.Facing);

            if (Anim == null || Anim.IsLocked) return;
            Anim.Play(MoveState());
        }

        private string MoveState()
        {
            if (Motor.IsDashing) return CharacterAnimator.Dash;
            if (!Motor.IsGrounded) return Motor.Velocity.y > 0.01f ? CharacterAnimator.Jump : CharacterAnimator.Fall;
            return Mathf.Abs(Motor.Velocity.x) > 0.1f ? CharacterAnimator.Run : CharacterAnimator.Idle;
        }
    }
}
