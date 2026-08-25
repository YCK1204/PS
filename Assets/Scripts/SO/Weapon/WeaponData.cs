using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>캐릭터 전용 시작 장비. 격자 칸을 먹지 않는다.
    /// 공격 방식이 무기마다 다르므로 실제 동작은 하위 클래스가 만든 Weapon이 한다.</summary>
    public abstract class WeaponData : ScriptableObject
    {
        [SerializeField] private string m_DisplayName;
        [SerializeField] private Sprite m_Icon;

        [Tooltip("기본 공격력. 캐릭터 스탯의 Attack에 더해진다")]
        [SerializeField] private float m_BaseDamage = 10f;

        [Tooltip("연타 단계. 비우면 공격이 없다")]
        [SerializeField] private AttackStep[] m_Steps;

        [Tooltip("연타가 끊긴 뒤 처음으로 돌아가기까지(초)")]
        [SerializeField] private float m_ComboResetTime = 0.6f;

        public string Name => m_DisplayName;
        public Sprite Icon => m_Icon;
        public float BaseDamage => m_BaseDamage;
        public float ComboResetTime => m_ComboResetTime;

        public int StepCount => m_Steps != null ? m_Steps.Length : 0;
        public AttackStep StepAt(int index) => m_Steps[Mathf.Clamp(index, 0, m_Steps.Length - 1)];

        /// <summary>런타임 무기를 만들어 붙인다. 무기 종류마다 다른 프리팹을 쓴다.</summary>
        public abstract Weapon Spawn(Transform mount);
    }
}
