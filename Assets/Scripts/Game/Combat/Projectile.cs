using System.Collections.Generic;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>앞으로 날아가며 닿는 것을 때린다.
    /// 관통 횟수·넉백·벽에 막히면 제자리 정지까지 여기서 다룬다.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float m_Lifetime = 3f;

        [Tooltip("몇 번까지 때리고 사라지나. 0 이하면 수명이 다할 때까지 계속")]
        [SerializeField] private int m_MaxHits = 1;

        [Tooltip("맞은 쪽을 밀어내는 힘")]
        [SerializeField] private float m_Knockback;

        [Tooltip("최대 타격 수를 채우면 사라질지. 끄면 남아서 계속 돈다")]
        [SerializeField] private bool m_DestroyWhenSpent = true;

        [Tooltip("벽에 닿으면 멈춘다. 사라지지 않고 그 자리에서 수명을 채운다")]
        [SerializeField] private bool m_StopOnWall = true;

        [SerializeField] private LayerMask m_WallMask = 1 << 6;

        private readonly HashSet<int> m_Hit = new HashSet<int>();

        private CharacterCombat m_Owner;
        private Vector2 m_Velocity;
        private float m_Damage;
        private Element m_Element;
        private bool m_Critical;
        private float m_Expire;
        private int m_Left;

        public bool Stopped { get; private set; }

        public void Launch(CharacterCombat owner, Vector2 velocity, float damage, Element element, bool critical = false)
        {
            m_Owner = owner;
            m_Velocity = velocity;
            m_Damage = damage;
            m_Element = element;
            m_Critical = critical;
            m_Expire = Time.time + m_Lifetime;
            m_Left = m_MaxHits > 0 ? m_MaxHits : int.MaxValue;
            m_Hit.Clear();
            Stopped = false;

            if (velocity.sqrMagnitude > 0.0001f)
            {
                var scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (velocity.x < 0f ? -1f : 1f);
                transform.localScale = scale;
            }
        }

        /// <summary>관통 한도를 바깥에서 덮어쓴다. 강화도로 늘릴 때 쓴다.</summary>
        public void SetMaxHits(int hits) => m_Left = hits > 0 ? hits : int.MaxValue;

        private void Update()
        {
            if (!Stopped) transform.position += (Vector3)(m_Velocity * Time.deltaTime);
            if (Time.time >= m_Expire) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other) => Touch(other);
        private void OnTriggerStay2D(Collider2D other) => Touch(other);

        private void Touch(Collider2D other)
        {
            if (m_Owner == null || other == null) return;

            if (!Stopped && m_StopOnWall && (m_WallMask.value & (1 << other.gameObject.layer)) != 0)
            {
                Stopped = true;
                m_Velocity = Vector2.zero;
                return;
            }

            if (other.transform.IsChildOf(m_Owner.transform)) return;
            if ((m_Owner.TargetMask.value & (1 << other.gameObject.layer)) == 0) return;

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;

            var root = damageable as Component;
            int id = root != null ? root.gameObject.GetInstanceID() : other.gameObject.GetInstanceID();
            if (!m_Hit.Add(id)) return;

            Vector2 push = m_Velocity.sqrMagnitude > 0.0001f ? m_Velocity.normalized : Vector2.right;

            damageable.TakeDamage(new DamageInfo
            {
                Amount = m_Damage,
                Element = m_Element,
                Source = m_Owner.gameObject,
                Point = other.ClosestPoint(transform.position),
                Knockback = push * m_Knockback,
                Critical = m_Critical,
            });

            m_Owner.ApplyHitEffects(new DamageInfo { Amount = m_Damage, Element = m_Element, Source = m_Owner.gameObject, Point = other.ClosestPoint(transform.position) }, other.gameObject);

            m_Left--;
            if (m_Left <= 0 && m_DestroyWhenSpent) Destroy(gameObject);
        }
    }
}
