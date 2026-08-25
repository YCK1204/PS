using PS.Game.Actors;
using UnityEngine;

namespace SO
{
    /// <summary>단어가 켜졌을 때 캐릭터에 벌어지는 일 하나.
    /// 단어 하나에 여러 개를 얹을 수 있고, 효과는 단어끼리 재사용된다.</summary>
    public abstract class WordEffect : ScriptableObject
    {
        [Tooltip("툴팁에 보여줄 한 줄. 비우면 안 띄운다")]
        [SerializeField] private string m_Description;

        public string Description => m_Description;

        /// <param name="source">건 주체. 보통 WordData. 이 값으로 통째로 되돌린다</param>
        public abstract void Apply(Combatant target, object source, int enhancement);

        public abstract void Remove(Combatant target, object source);
    }
}
