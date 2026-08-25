using System;
using PS.Game.Combat;
using SO;
using UnityEngine;

namespace PS.Game.Actors
{
    /// <summary>2D 횡스크롤 이동. 캐릭터가 몇이든 이건 그대로 쓴다.
    /// 감각 수치는 CharacterData가, 이속은 StatBlock이 준다.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor : MonoBehaviour
    {
        [SerializeField] private Transform m_GroundCheck;
        [SerializeField] private float m_GroundRadius = 0.12f;
        [SerializeField] private LayerMask m_GroundMask = ~0;

        [Tooltip("발이 떨어진 뒤에도 점프가 먹히는 시간")]
        [SerializeField] private float m_CoyoteTime = 0.1f;

        [Tooltip("착지 직전에 누른 점프를 기억하는 시간")]
        [SerializeField] private float m_JumpBuffer = 0.1f;

        private Rigidbody2D m_Body;
        private StatBlock m_Stats;
        private CharacterData.MotorSettings m_Settings;

        private float m_Move;
        private float m_LastGrounded = float.NegativeInfinity;
        private float m_JumpPressed = float.NegativeInfinity;
        private float m_LungeUntil;
        private float m_LungeSpeed;
        private int m_LungeDirection;
        private float m_DashUntil;
        private float m_DashReady;
        private int m_DashDirection;

        public bool IsGrounded { get; private set; }

        /// <summary>공격 중처럼 조작을 막아야 할 때. 속도를 0으로 꺾지 않고 입력만 무시한다 —
        /// 공중에서 잠기면 가던 관성은 유지된다.</summary>
        public bool MoveLocked { get; set; }

        public bool IsLunging => Time.time < m_LungeUntil;

        /// <summary>대시가 시작된 순간. 잔상 같은 연출이 구독한다.</summary>
        public event Action<float, float> Dashed;

        public float DashSpeed => m_Settings.DashSpeed;
        public float DashTime => m_Settings.DashTime;
        public bool IsDashing => Time.time < m_DashUntil;
        public bool CanDash => Time.time >= m_DashReady && !IsDashing;
        public int Facing { get; private set; } = 1;
        public Vector2 Velocity => m_Body != null ? m_Body.linearVelocity : Vector2.zero;

        private void Awake() => m_Body = GetComponent<Rigidbody2D>();

        public void Bind(StatBlock stats, in CharacterData.MotorSettings settings)
        {
            m_Stats = stats;
            m_Settings = settings;
        }

        /// <param name="move">-1 ~ 1</param>
        public void SetMove(float move) => m_Move = Mathf.Clamp(move, -1f, 1f);

        public void PressJump() => m_JumpPressed = Time.time;

        /// <summary>공격 한 타마다 앞으로 짧게 밀고 나간다.
        /// 방향키를 누르고 있으면 그쪽으로, 아니면 보던 쪽으로 간다.</summary>
        public void Lunge(float speed, float time)
        {
            if (speed <= 0f || time <= 0f) return;

            m_LungeDirection = Mathf.Abs(m_Move) > 0.01f ? (int)Mathf.Sign(m_Move) : Facing;
            Facing = m_LungeDirection;
            m_LungeSpeed = speed;
            m_LungeUntil = Time.time + time;
        }

        public void CancelLunge() => m_LungeUntil = 0f;

        public bool TryDash()
        {
            if (!CanDash) return false;

            m_DashDirection = Mathf.Abs(m_Move) > 0.01f ? (int)Mathf.Sign(m_Move) : Facing;
            m_DashUntil = Time.time + m_Settings.DashTime;
            m_DashReady = Time.time + m_Settings.DashCooldown;

            Dashed?.Invoke(m_Settings.DashSpeed, m_Settings.DashTime);
            return true;
        }

        public void Stop()
        {
            m_Move = 0f;
            if (m_Body != null) m_Body.linearVelocity = new Vector2(0f, m_Body.linearVelocity.y);
        }

        private void FixedUpdate()
        {
            UpdateGrounded();

            if (IsDashing)
            {
                m_Body.linearVelocity = new Vector2(m_DashDirection * m_Settings.DashSpeed, 0f);
                return;
            }

            if (IsLunging)
            {
                m_Body.linearVelocity = new Vector2(m_LungeDirection * m_LungeSpeed, m_Body.linearVelocity.y);
                ApplyGravity();
                return;
            }

            ApplyHorizontal();
            ApplyJump();
            ApplyGravity();
        }

        private void UpdateGrounded()
        {
            Vector2 point = m_GroundCheck != null ? (Vector2)m_GroundCheck.position : (Vector2)transform.position;
            IsGrounded = Physics2D.OverlapCircle(point, m_GroundRadius, m_GroundMask) != null;

            if (IsGrounded) m_LastGrounded = Time.time;
        }

        private void ApplyHorizontal()
        {
            if (MoveLocked)
            {
                // 땅에서는 미끄러지지 않게 빠르게 멈추고, 공중에서는 관성을 그대로 둔다.
                if (IsGrounded)
                    m_Body.linearVelocity = new Vector2(Mathf.Lerp(m_Body.linearVelocity.x, 0f, 0.4f), m_Body.linearVelocity.y);

                return;
            }

            float speed = m_Stats != null ? m_Stats.Get(StatType.MoveSpeed) : 5f;
            float target = m_Move * speed;

            float control = IsGrounded ? 1f : m_Settings.AirControl;
            float x = Mathf.Lerp(m_Body.linearVelocity.x, target, control);

            m_Body.linearVelocity = new Vector2(x, m_Body.linearVelocity.y);

            if (Mathf.Abs(m_Move) > 0.01f) Facing = (int)Mathf.Sign(m_Move);
        }

        private void ApplyJump()
        {
            bool buffered = Time.time - m_JumpPressed <= m_JumpBuffer;
            bool coyote = Time.time - m_LastGrounded <= m_CoyoteTime;

            if (!buffered || !coyote) return;

            float power = m_Stats != null ? m_Stats.Get(StatType.JumpPower) : 12f;
            m_Body.linearVelocity = new Vector2(m_Body.linearVelocity.x, power);
            m_JumpPressed = float.NegativeInfinity;
            m_LastGrounded = float.NegativeInfinity;
        }

        /// <summary>내려갈 때 더 무겁게. 점프가 붕 뜨지 않게 한다.</summary>
        private void ApplyGravity()
        {
            float scale = m_Body.linearVelocity.y < 0f ? m_Settings.FallGravity : m_Settings.Gravity;
            m_Body.gravityScale = scale > 0f ? scale : 1f;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_GroundCheck == null) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(m_GroundCheck.position, m_GroundRadius);
        }
    }
}
