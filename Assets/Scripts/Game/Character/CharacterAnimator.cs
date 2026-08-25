using UnityEngine;

namespace PS.Game.Actors
{
    /// <summary>Animator 상태를 이름으로 직접 지정한다.
    /// 트랜지션·파라미터를 쓰지 않으므로 컨트롤러에는 상태만 있으면 된다.</summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour
    {
        public const string Idle = "Idle";
        public const string Run = "Run";
        public const string Jump = "Jump";
        public const string Fall = "Fall";
        public const string Dash = "Dash";
        public const string Hurt = "Hurt";
        public const string Dead = "Dead";

        [SerializeField] private SpriteRenderer m_Renderer;

        private Animator m_Animator;
        private int m_Current;

        /// <summary>이 시간까지는 다른 상태로 못 바꾼다. 공격 모션이 씹히지 않게.</summary>
        private float m_LockUntil;

        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            if (m_Renderer == null) m_Renderer = GetComponentInChildren<SpriteRenderer>();
        }

        public bool IsLocked => Time.time < m_LockUntil;

        /// <summary>결빙처럼 애니메이션을 통째로 얼려야 할 때. 켜면 Play가 전부 무시된다.</summary>
        public bool Frozen { get; set; }

        /// <param name="lockTime">0보다 크면 그동안 다른 상태로 안 바뀐다.</param>
        /// <param name="speed">재생 속도. 공속이 오르면 공격 모션도 빨라진다.</param>
        public void Play(string state, float lockTime = 0f, float speed = 1f)
        {
            if (Frozen) return;
            if (string.IsNullOrEmpty(state) || m_Animator == null) return;
            if (lockTime <= 0f && IsLocked) return;

            int hash = Animator.StringToHash(state);
            m_Animator.speed = Mathf.Max(0.01f, speed);

            if (hash != m_Current || lockTime > 0f)
            {
                m_Animator.Play(hash, 0, 0f);
                m_Current = hash;
            }

            if (lockTime > 0f) m_LockUntil = Time.time + lockTime;
        }

        public void Unlock() => m_LockUntil = 0f;

        public void SetFacing(int facing)
        {
            if (m_Renderer == null || facing == 0) return;
            m_Renderer.flipX = facing < 0;
        }
    }
}
