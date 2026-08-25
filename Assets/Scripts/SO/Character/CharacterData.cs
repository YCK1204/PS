using System;
using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>캐릭터 1명분. 캐릭터를 늘릴 때 코드가 아니라 이 에셋을 늘린다.</summary>
    [CreateAssetMenu(menuName = "PS/Character", fileName = "Character_")]
    public class CharacterData : ScriptableObject
    {
        /// <summary>이동 감각. 스탯이 아니라 캐릭터 고유값이라 여기 둔다.</summary>
        [Serializable]
        public struct MotorSettings
        {
            public float Gravity;
            public float FallGravity;
            public float DashSpeed;
            public float DashTime;
            public float DashCooldown;
            [Tooltip("공중에서 방향을 얼마나 바꿀 수 있나. 0~1")]
            [Range(0f, 1f)] public float AirControl;
        }

        [SerializeField] private string m_DisplayName;
        [SerializeField] private Sprite m_Portrait;

        [Tooltip("이 캐릭터 전용 시작 장비")]
        [SerializeField] private WeaponData m_StartWeapon;

        [Tooltip("기본 스탯 한 벌")]
        [SerializeField] private StatSheet m_BaseStats;

        [Tooltip("최대 체력. 스탯 시트에는 없는 값이라 여기 둔다")]
        [SerializeField] private float m_MaxHealth = 100f;
        [SerializeField] private MotorSettings m_Motor;

        public string Name => m_DisplayName;
        public Sprite Portrait => m_Portrait;
        public WeaponData StartWeapon => m_StartWeapon;
        public MotorSettings Motor => m_Motor;
        public StatSheet BaseStats => m_BaseStats;
        public float MaxHealth => m_MaxHealth;

        public void ApplyBaseStats(StatBlock stats)
        {
            if (stats == null) return;

            m_BaseStats?.Apply(stats);
            stats.SetBase(StatType.MaxHealth, m_MaxHealth);
        }
    }
}
