using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>근접 — 앞쪽에 히트박스를 한 번 켠다.</summary>
    [CreateAssetMenu(menuName = "PS/Weapon/Melee", fileName = "Weapon_")]
    public class MeleeWeaponData : WeaponData
    {
        [SerializeField] private MeleeWeapon m_Prefab;

        [Tooltip("캐릭터 기준 히트박스 중심. x는 바라보는 방향으로 뒤집힌다")]
        [SerializeField] private Vector2 m_HitboxOffset = new Vector2(0.6f, 0.4f);

        [SerializeField] private Vector2 m_HitboxSize = new Vector2(1.2f, 0.8f);

        [Tooltip("맞은 대상을 밀어내는 힘")]
        [SerializeField] private float m_Knockback = 4f;

        public Vector2 HitboxOffset => m_HitboxOffset;
        public Vector2 HitboxSize => m_HitboxSize;
        public float Knockback => m_Knockback;

        public override Weapon Spawn(Transform mount)
        {
            MeleeWeapon weapon = m_Prefab != null
                ? Instantiate(m_Prefab, mount)
                : new GameObject(name).AddComponent<MeleeWeapon>();

            weapon.transform.SetParent(mount, false);
            weapon.Setup(this);
            return weapon;
        }
    }
}
