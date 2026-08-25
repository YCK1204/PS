using System;
using System.Collections.Generic;

namespace PS.Game.Combat
{
    /// <summary>스탯 합산기. 순수 C# — 씬을 모른다.
    /// 보정치는 출처(source)별로 묶여 있어서 단어가 꺼질 때 그 출처만 통째로 뺀다.</summary>
    public class StatBlock
    {
        public struct Modifier
        {
            public object Source;
            public StatType Stat;
            public float Flat;
            public float Percent;
        }

        private static readonly int s_Count = Enum.GetValues(typeof(StatType)).Length;

        private readonly float[] m_Base = new float[s_Count];
        private readonly List<Modifier> m_Modifiers = new List<Modifier>();

        public event Action Changed;

        public void SetBase(StatType stat, float value)
        {
            m_Base[(int)stat] = value;
            Changed?.Invoke();
        }

        public float GetBase(StatType stat) => m_Base[(int)stat];

        /// <summary>합연산을 먼저 하고 곱연산을 나중에 한다. (기본 + 합) × (1 + 곱)</summary>
        public float Get(StatType stat)
        {
            float flat = 0f;
            float percent = 0f;

            for (int i = 0; i < m_Modifiers.Count; i++)
            {
                if (m_Modifiers[i].Stat != stat) continue;
                flat += m_Modifiers[i].Flat;
                percent += m_Modifiers[i].Percent;
            }

            return (m_Base[(int)stat] + flat) * (1f + percent);
        }

        public void Add(object source, StatType stat, float flat, float percent = 0f)
        {
            if (source == null) return;

            m_Modifiers.Add(new Modifier { Source = source, Stat = stat, Flat = flat, Percent = percent });
            Changed?.Invoke();
        }

        /// <summary>그 출처가 건 보정치를 전부 뺀다. 단어가 꺼질 때 부른다.</summary>
        public bool RemoveAll(object source)
        {
            if (source == null) return false;

            int removed = m_Modifiers.RemoveAll(m => ReferenceEquals(m.Source, source));
            if (removed > 0) Changed?.Invoke();

            return removed > 0;
        }

        public void Clear()
        {
            if (m_Modifiers.Count == 0) return;

            m_Modifiers.Clear();
            Changed?.Invoke();
        }

        public IReadOnlyList<Modifier> Modifiers => m_Modifiers;
    }
}
