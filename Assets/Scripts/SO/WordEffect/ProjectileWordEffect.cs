using System;
using System.Collections.Generic;
using PS.Game.Actors;
using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>한 타 휘두를 때마다 앞으로 투사체를 뿜는다. 회오리·검기 같은 것.</summary>
    [CreateAssetMenu(menuName = "PS/WordEffect/Projectile", fileName = "Effect_")]
    public class ProjectileWordEffect : WordEffect
    {
        [SerializeField] private Projectile m_Prefab;

        [Tooltip("캐릭터 기준 발사 위치. x는 바라보는 방향으로 뒤집힌다")]
        [SerializeField] private Vector2 m_Offset = new Vector2(0.6f, 0.05f);

        [SerializeField] private float m_Speed = 8f;
        [SerializeField] private float m_SpeedPerLevel = 0.4f;

        [SerializeField] private float m_Damage = 5f;
        [SerializeField] private float m_DamagePerLevel = 2f;

        [Tooltip("최대 타격 횟수. 0 이하면 수명이 다할 때까지")]
        [SerializeField] private int m_MaxHits = 3;
        [SerializeField] private int m_MaxHitsPerLevel;

        [SerializeField] private Element m_Element = Element.Wind;

        /// <summary>캐릭터별·출처별 구독. 에셋에 런타임 상태를 남기지 않게 나눠 담는다.</summary>
        private readonly Dictionary<Combatant, Dictionary<object, Action<int>>> m_Hooks
            = new Dictionary<Combatant, Dictionary<object, Action<int>>>();

        public override void Apply(Combatant target, object source, int enhancement)
        {
            if (target == null || target.Combat == null || m_Prefab == null) return;

            Remove(target, source);

            int level = enhancement;
            Action<int> hook = delegate (int facing) { Fire(target, facing, level); };

            target.Combat.Struck += hook;

            Dictionary<object, Action<int>> bySource;
            if (!m_Hooks.TryGetValue(target, out bySource))
            {
                bySource = new Dictionary<object, Action<int>>();
                m_Hooks[target] = bySource;
            }

            bySource[source] = hook;
        }

        public override void Remove(Combatant target, object source)
        {
            if (target == null) return;

            Dictionary<object, Action<int>> bySource;
            if (!m_Hooks.TryGetValue(target, out bySource)) return;

            Action<int> hook;
            if (!bySource.TryGetValue(source, out hook)) return;

            if (target.Combat != null) target.Combat.Struck -= hook;

            bySource.Remove(source);
            if (bySource.Count == 0) m_Hooks.Remove(target);
        }

        private void Fire(Combatant target, int facing, int enhancement)
        {
            if (target == null || m_Prefab == null) return;

            Vector2 offset = m_Offset;
            offset.x *= facing;

            Vector3 origin = target.transform.position + (Vector3)offset;
            Projectile shot = Instantiate(m_Prefab, origin, Quaternion.identity);

            float speed = m_Speed + m_SpeedPerLevel * enhancement;
            float damage = m_Damage + m_DamagePerLevel * enhancement;

            shot.Launch(target.Combat, new Vector2(facing * speed, 0f), damage, m_Element);
            shot.SetMaxHits(m_MaxHits + m_MaxHitsPerLevel * enhancement);
        }
    }
}
