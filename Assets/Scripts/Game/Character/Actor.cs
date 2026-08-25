using System.Collections.Generic;
using UnityEngine;

namespace PS.Game.Actors
{
    /// <summary>필드에 올라가는 것의 밑단. 캐릭터·몬스터·NPC가 공유하는 건 여기까지만 둔다.
    /// 체력·전투는 Combatant로 내려간다 — NPC가 안 쓰는 걸 들고 있으면 안 되니까.</summary>
    public abstract class Actor : MonoBehaviour
    {
        private static readonly List<Actor> s_All = new List<Actor>();

        [SerializeField] private Faction m_Faction = Faction.Neutral;

        [Tooltip("조준·이펙트가 붙을 지점. 비우면 자기 트랜스폼")]
        [SerializeField] private Transform m_Center;

        public Faction Faction => m_Faction;
        public Transform Center => m_Center != null ? m_Center : transform;
        public Vector2 Position => Center.position;

        public virtual bool IsAlive => true;

        public static IReadOnlyList<Actor> All => s_All;

        protected virtual void OnEnable() => s_All.Add(this);
        protected virtual void OnDisable() => s_All.Remove(this);

        public bool IsHostileTo(Actor other)
        {
            if (other == null) return false;
            if (m_Faction == Faction.Neutral || other.m_Faction == Faction.Neutral) return false;
            return m_Faction != other.m_Faction;
        }

        /// <summary>반경 안의 액터를 모은다. 물리 질의 없이 목록만 훑는다.</summary>
        public static void Query(Vector2 center, float radius, List<Actor> results, Actor hostileTo = null)
        {
            if (results == null) return;
            results.Clear();

            float sqr = radius * radius;

            for (int i = 0; i < s_All.Count; i++)
            {
                Actor actor = s_All[i];
                if (actor == null || !actor.IsAlive) continue;
                if (hostileTo != null && !hostileTo.IsHostileTo(actor)) continue;
                if (((Vector2)actor.Center.position - center).sqrMagnitude > sqr) continue;

                results.Add(actor);
            }
        }
    }
}
