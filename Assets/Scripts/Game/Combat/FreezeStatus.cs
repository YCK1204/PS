using PS.Core;
using PS.Game.Actors;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>결빙. 대상을 그 자리에 묶고 애니메이션을 정지시킨다.
    /// 이미 얼어 있는 대상은 다시 얼지 않는다 — 겹쳐 걸면 무한 결빙이 되기 때문.</summary>
    public class FreezeStatus : Status
    {
        private Combatant m_Target;
        private Animator m_Animator;
        private CharacterAnimator m_CharAnim;
        private FreezeVisual m_Visual;

        private float m_AnimatorSpeed = 1f;
        private bool m_Breaking;
        private bool m_Shaking;
        private float m_BreakEnd;

        public bool IsBreaking => m_Breaking;

        public static bool IsFrozen(GameObject target)
            => target != null && target.GetComponentInParent<FreezeStatus>() != null;

        public void Bind(FreezeVisual visual) => m_Visual = visual;

        private void Awake()
        {
            m_Target = GetComponentInParent<Combatant>();
            m_Animator = GetComponentInChildren<Animator>();
            m_CharAnim = GetComponentInChildren<CharacterAnimator>();

            if (m_Animator != null)
            {
                m_AnimatorSpeed = m_Animator.speed;
                m_Animator.speed = 0f;
            }

            if (m_CharAnim != null) m_CharAnim.Frozen = true;

            if (m_Target != null)
            {
                m_Target.Combat?.CancelAttack();
                if (m_Target.Motor != null)
                {
                    m_Target.Motor.MoveLocked = true;
                    m_Target.Motor.Stop();
                }
            }
        }

        protected override void Update()
        {
            if (!m_Breaking)
            {
                if (Time.time < m_Expire)
                {
                    // 깨지기 직전 구간에 들어서면 떨기 시작한다.
                    if (m_Shaking || m_Visual == null) return;
                    if (Time.time < m_Expire - m_Visual.ShakeLead) return;

                    m_Shaking = true;
                    m_Visual.BeginShake();
                    return;
                }

                BeginBreak();
                return;
            }

            if (Time.time >= m_BreakEnd) Destroy(this);
        }

        private void BeginBreak()
        {
            m_Breaking = true;
            m_BreakEnd = Time.time + (m_Visual != null ? m_Visual.BreakLength : 0.3f);
            m_Visual?.PlayBreak();
        }

        private void OnDestroy()
        {
            if (m_Animator != null) m_Animator.speed = m_AnimatorSpeed;
            if (m_CharAnim != null) m_CharAnim.Frozen = false;
            if (m_Target != null && m_Target.Motor != null) m_Target.Motor.MoveLocked = false;
            if (m_Visual != null) PoolManager.Release(m_Visual);
        }
    }
}
