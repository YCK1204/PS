using System;
using PS.Game.Actors;
using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>스탯을 올린다. 강화도 1당 얼마씩 더 오른다.</summary>
    [CreateAssetMenu(menuName = "PS/WordEffect/Stat", fileName = "Effect_")]
    public class StatWordEffect : WordEffect
    {
        [Serializable]
        public struct Entry
        {
            public StatType Stat;
            public float Flat;
            public float FlatPerLevel;
            public float Percent;
            public float PercentPerLevel;
        }

        [SerializeField] private Entry[] m_Entries;

        public override void Apply(Combatant target, object source, int enhancement)
        {
            if (target == null || m_Entries == null) return;

            for (int i = 0; i < m_Entries.Length; i++)
            {
                Entry e = m_Entries[i];
                target.Stats.Add(source, e.Stat,
                    e.Flat + e.FlatPerLevel * enhancement,
                    e.Percent + e.PercentPerLevel * enhancement);
            }
        }

        public override void Remove(Combatant target, object source)
        {
            if (target == null) return;
            target.Stats.RemoveAll(source);
        }
    }
}
