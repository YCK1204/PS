using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>화상. 초당 m_Power만큼 깎는다.</summary>
    public class BurnStatus : Status
    {
        private const float Tick = 0.5f;

        private IDamageable m_Target;
        private float m_NextTick;

        private void Awake() => m_Target = GetComponentInParent<IDamageable>();

        protected override void Update()
        {
            if (m_Target != null && m_Target.IsAlive && Time.time >= m_NextTick)
            {
                m_NextTick = Time.time + Tick;

                m_Target.TakeDamage(new DamageInfo
                {
                    Amount = m_Power * Tick,
                    Element = Element.Fire,
                    Source = gameObject,
                    Point = transform.position,
                });
            }

            base.Update();
        }
    }
}
