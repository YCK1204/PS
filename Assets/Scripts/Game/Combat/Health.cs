using System;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>체력. 최대치는 StatBlock이 있으면 거기서 읽고, 없으면 인스펙터 값을 쓴다.</summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float m_MaxHealth = 100f;

        [Tooltip("맞고 나서 무적인 시간. 0이면 무적 없음")]
        [SerializeField] private float m_InvincibleTime;

        [Tooltip("체력이 안 깎인다. 피격 반응과 데미지 숫자는 그대로 나온다 — 허수아비용")]
        [SerializeField] private bool m_Immortal;

        private StatBlock m_Stats;
        private float m_LastHitTime = float.NegativeInfinity;

        public float Current { get; private set; }
        public float Max => m_Stats != null && m_Stats.GetBase(StatType.MaxHealth) > 0f
            ? m_Stats.Get(StatType.MaxHealth)
            : m_MaxHealth;

        public float Ratio => Max > 0f ? Mathf.Clamp01(Current / Max) : 0f;
        public bool IsAlive => m_Immortal || Current > 0f;

        public bool Immortal
        {
            get { return m_Immortal; }
            set { m_Immortal = value; }
        }

        public event Action<DamageInfo> Damaged;
        public event Action<DamageInfo> Died;
        public event Action Changed;

        private void Awake() => Current = Max;

        /// <summary>StatBlock을 붙이면 최대 체력이 스탯을 따라간다.</summary>
        public void Bind(StatBlock stats)
        {
            m_Stats = stats;
            Current = Max;
            Changed?.Invoke();
        }

        public void TakeDamage(in DamageInfo info)
        {
            if (!IsAlive) return;
            if (Time.time - m_LastHitTime < m_InvincibleTime) return;

            m_LastHitTime = Time.time;

            if (!m_Immortal) Current = Mathf.Max(0f, Current - Mathf.Max(0f, info.Amount));

            Damaged?.Invoke(info);
            Changed?.Invoke();

            if (!IsAlive) Died?.Invoke(info);
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            Current = Mathf.Min(Max, Current + amount);
            Changed?.Invoke();
        }

        public void Revive()
        {
            Current = Max;
            m_LastHitTime = float.NegativeInfinity;
            Changed?.Invoke();
        }
    }
}
