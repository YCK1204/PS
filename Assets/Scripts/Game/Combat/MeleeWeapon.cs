using SO;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>앞쪽 상자 하나를 켜서 겹친 걸 전부 때린다.</summary>
    public class MeleeWeapon : Weapon
    {
        private const int MaxTargets = 16;

        private readonly Collider2D[] m_Buffer = new Collider2D[MaxTargets];
        private MeleeWeaponData m_Data;

        public override void Setup(WeaponData data)
        {
            base.Setup(data);
            m_Data = data as MeleeWeaponData;
        }

        public override void Strike(in AttackStep step, int facing)
        {
            if (m_Data == null || Owner == null) return;

            Vector2 offset = m_Data.HitboxOffset;
            offset.x *= facing;

            Vector2 center = (Vector2)Owner.transform.position + offset;
            int count = Physics2D.OverlapBoxNonAlloc(center, m_Data.HitboxSize, 0f, m_Buffer, Owner.TargetMask);

            bool critical;
            float damage = Owner.DamageOf(step, out critical);

            for (int i = 0; i < count; i++)
            {
                Collider2D target = m_Buffer[i];
                if (target == null || target.transform.IsChildOf(Owner.transform)) continue;

                var info = new DamageInfo
                {
                    Amount = damage,
                    Element = Owner.Element,
                    Source = Owner.gameObject,
                    Point = target.ClosestPoint(center),
                    Knockback = new Vector2(facing * m_Data.Knockback, 0f),
                    Critical = critical,
                };

                Hit(target, info);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (m_Data == null || Owner == null) return;

            Vector2 offset = m_Data.HitboxOffset;
            offset.x *= Owner.Facing;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube((Vector2)Owner.transform.position + offset, m_Data.HitboxSize);
        }
    }
}
