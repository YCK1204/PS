using System.Collections.Generic;
using PS.Game.Actors;
using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>주변을 도는 위성을 띄운다. 회오리·그림자 구체.</summary>
    [CreateAssetMenu(menuName = "PS/WordEffect/Orbit", fileName = "Effect_")]
    public class OrbitWordEffect : WordEffect
    {
        [SerializeField] private Orbit m_Prefab;
        [SerializeField] private int m_Count = 1;
        [SerializeField] private int m_CountPerLevel;
        [SerializeField] private float m_Radius = 1.2f;
        [SerializeField] private float m_AngularSpeed = 120f;
        [SerializeField] private float m_Damage = 4f;
        [SerializeField] private float m_DamagePerLevel = 2f;
        [SerializeField] private Element m_Element = Element.Wind;

        /// <summary>출처별로 띄운 위성. 에셋에 런타임 상태를 두지 않게 캐릭터별로 나눠 담는다.</summary>
        private readonly Dictionary<Combatant, Dictionary<object, List<Orbit>>> m_Live
            = new Dictionary<Combatant, Dictionary<object, List<Orbit>>>();

        public override void Apply(Combatant target, object source, int enhancement)
        {
            if (target == null || m_Prefab == null) return;

            Remove(target, source);

            int count = Mathf.Max(1, m_Count + m_CountPerLevel * enhancement);
            float damage = m_Damage + m_DamagePerLevel * enhancement;
            var spawned = new List<Orbit>(count);

            for (int i = 0; i < count; i++)
            {
                Orbit orbit = Instantiate(m_Prefab, target.transform.position, Quaternion.identity);
                orbit.Setup(target, 360f / count * i, m_Radius, m_AngularSpeed, damage, m_Element);
                spawned.Add(orbit);
            }

            if (!m_Live.TryGetValue(target, out var bySource))
            {
                bySource = new Dictionary<object, List<Orbit>>();
                m_Live[target] = bySource;
            }

            bySource[source] = spawned;
        }

        public override void Remove(Combatant target, object source)
        {
            if (target == null) return;
            if (!m_Live.TryGetValue(target, out var bySource)) return;
            if (!bySource.TryGetValue(source, out var list)) return;

            for (int i = 0; i < list.Count; i++)
                if (list[i] != null) Destroy(list[i].gameObject);

            bySource.Remove(source);
            if (bySource.Count == 0) m_Live.Remove(target);
        }
    }
}
