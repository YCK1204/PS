using PS.Game.Actors;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>둔화. 이속을 깎았다가 끝나면 되돌린다.</summary>
    public class SlowStatus : Status
    {
        private StatBlock m_Stats;
        private bool m_Applied;

        private void Awake()
        {
            var combatant = GetComponentInParent<Combatant>();
            m_Stats = combatant != null ? combatant.Stats : null;
        }

        public override void Refresh(float duration, float power)
        {
            base.Refresh(duration, power);

            if (m_Stats == null) return;

            m_Stats.RemoveAll(this);
            m_Stats.Add(this, StatType.MoveSpeed, 0f, -Mathf.Clamp01(m_Power));
            m_Applied = true;
        }

        private void OnDestroy()
        {
            if (m_Applied) m_Stats?.RemoveAll(this);
        }
    }
}
