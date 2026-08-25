using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>원거리 — 투사체를 쏜다. 개수는 Projectile 스탯을 따른다.</summary>
    [CreateAssetMenu(menuName = "PS/Weapon/Ranged", fileName = "Weapon_")]
    public class RangedWeaponData : WeaponData
    {
        [SerializeField] private RangedWeapon m_Prefab;
        [SerializeField] private Projectile m_Projectile;

        [SerializeField] private Vector2 m_MuzzleOffset = new Vector2(0.5f, 0.4f);
        [SerializeField] private float m_ProjectileSpeed = 12f;

        [Tooltip("투사체가 여러 발일 때 벌어지는 각도(도)")]
        [SerializeField] private float m_SpreadAngle = 8f;

        public Projectile ProjectilePrefab => m_Projectile;
        public Vector2 MuzzleOffset => m_MuzzleOffset;
        public float ProjectileSpeed => m_ProjectileSpeed;
        public float SpreadAngle => m_SpreadAngle;

        public override Weapon Spawn(Transform mount)
        {
            RangedWeapon weapon = m_Prefab != null
                ? Instantiate(m_Prefab, mount)
                : new GameObject(name).AddComponent<RangedWeapon>();

            weapon.transform.SetParent(mount, false);
            weapon.Setup(this);
            return weapon;
        }
    }
}
