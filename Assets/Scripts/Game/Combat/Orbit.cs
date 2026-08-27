using PS.Core;
using PS.Game.Actors;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>주인 주위를 도는 위성. 닿으면 피해를 주고 잠깐 쿨다운을 둔다.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Orbit : MonoBehaviour, IPoolable
    {
        [SerializeField] private float m_HitInterval = 0.4f;

        private Combatant m_Owner;
        private CharacterCombat m_Combat;
        private float m_Angle;
        private float m_Radius;
        private float m_Speed;
        private float m_Damage;
        private Element m_Element;
        private float m_NextHit;

        public void OnGet() { }

        /// <summary>반납 전 상태 청소. m_NextHit이 남으면 다음에 꺼낸 위성이 그 시각까지 못 때린다.</summary>
        public void OnRelease()
        {
            m_Owner = null;
            m_Combat = null;
            m_NextHit = 0f;
        }

        public void Setup(Combatant owner, float startAngle, float radius, float speed, float damage, Element element)
        {
            m_Owner = owner;
            m_Combat = owner != null ? owner.Combat : null;
            m_Angle = startAngle;
            m_Radius = radius;
            m_Speed = speed;
            m_Damage = damage;
            m_Element = element;
        }

        private void Update()
        {
            if (m_Owner == null)
            {
                PoolManager.Release(this);
                return;
            }

            m_Angle += m_Speed * Time.deltaTime;
            float rad = m_Angle * Mathf.Deg2Rad;

            transform.position = m_Owner.transform.position
                + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * m_Radius;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (m_Combat == null || Time.time < m_NextHit) return;
            if (other.transform.IsChildOf(m_Owner.transform)) return;
            if ((m_Combat.TargetMask.value & (1 << other.gameObject.layer)) == 0) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;

            m_NextHit = Time.time + m_HitInterval;

            var info = new DamageInfo
            {
                Amount = m_Damage,
                Element = m_Element,
                Source = m_Owner.gameObject,
                Point = other.ClosestPoint(transform.position),
                Knockback = (other.transform.position - m_Owner.transform.position).normalized,
            };

            damageable.TakeDamage(info);
        }
    }
}
