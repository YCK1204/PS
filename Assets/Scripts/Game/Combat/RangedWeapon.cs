using PS.Core;
using SO;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>투사체를 쏜다. 발수는 Projectile 스탯을 따라 늘어난다.</summary>
    public class RangedWeapon : Weapon
    {
        private RangedWeaponData m_Data;

        public override void Setup(WeaponData data)
        {
            base.Setup(data);
            m_Data = data as RangedWeaponData;
        }

        public override void Strike(in AttackStep step, int facing)
        {
            if (m_Data == null || Owner == null || m_Data.ProjectilePrefab == null) return;

            int shots = Mathf.Max(1, Mathf.RoundToInt(Owner.Stats.Get(StatType.Projectile)));
            bool critical;
            float damage = Owner.DamageOf(step, out critical);

            Vector2 muzzle = m_Data.MuzzleOffset;
            muzzle.x *= facing;
            Vector3 origin = Owner.transform.position + (Vector3)muzzle;

            float spread = m_Data.SpreadAngle;
            float start = -spread * (shots - 1) * 0.5f;

            for (int i = 0; i < shots; i++)
            {
                float angle = start + spread * i;
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * new Vector3(facing, 0f, 0f);

                Projectile shot = PoolManager.Get(m_Data.ProjectilePrefab, origin, Quaternion.identity);
                shot.Launch(Owner, direction.normalized * m_Data.ProjectileSpeed, damage, Owner.Element, critical);
            }
        }
    }
}
