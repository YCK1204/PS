using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>캐릭터 기본 스탯 한 벌. 캐릭터마다 이 에셋을 하나씩 만든다.</summary>
    [CreateAssetMenu(menuName = "PS/StatSheet", fileName = "Stats_")]
    public class StatSheet : ScriptableObject
    {
        [Tooltip("초당 이동 거리(유닛)")]
        [SerializeField] private float m_MoveSpeed = 6f;

        [Tooltip("점프 초기 속도. 높을수록 높이 뛴다")]
        [SerializeField] private float m_JumpPower = 12f;

        [Tooltip("공격 모션 배속. 2면 두 배 빠르게 친다")]
        [SerializeField] private float m_AttackSpeed = 1f;

        [Tooltip("무기 기본 피해에 더해지는 값")]
        [SerializeField] private float m_Attack;

        [Tooltip("치명타 확률 0~1")]
        [Range(0f, 1f)]
        [SerializeField] private float m_CritChance = 0.1f;

        [Tooltip("치명타 피해 배수")]
        [SerializeField] private float m_CritMultiplier = 1.8f;

        public float MoveSpeed => m_MoveSpeed;
        public float JumpPower => m_JumpPower;
        public float AttackSpeed => m_AttackSpeed;
        public float Attack => m_Attack;
        public float CritChance => m_CritChance;
        public float CritMultiplier => m_CritMultiplier;

        public void Apply(StatBlock stats)
        {
            if (stats == null) return;

            stats.SetBase(StatType.MoveSpeed, m_MoveSpeed);
            stats.SetBase(StatType.JumpPower, m_JumpPower);
            stats.SetBase(StatType.AttackSpeed, m_AttackSpeed);
            stats.SetBase(StatType.Attack, m_Attack);
            stats.SetBase(StatType.CritChance, m_CritChance);
            stats.SetBase(StatType.CritMultiplier, m_CritMultiplier);
        }
    }
}
