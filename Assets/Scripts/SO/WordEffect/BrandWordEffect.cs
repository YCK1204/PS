using PS.Game.Actors;
using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>공격에 속성을 붙인다. 검에 불이 붙고, 벤 대상에 명중 효과가 걸린다.</summary>
    [CreateAssetMenu(menuName = "PS/WordEffect/Brand", fileName = "Effect_")]
    public class BrandWordEffect : WordEffect
    {
        [SerializeField] private Element m_Element = Element.Fire;

        [Tooltip("명중 순간에 걸 것. 비우면 속성만 붙는다")]
        [SerializeField] private HitEffect m_OnHit;

        [SerializeField] private float m_Power = 1f;
        [SerializeField] private float m_PowerPerLevel = 0.5f;

        public Element Element => m_Element;

        public override void Apply(Combatant target, object source, int enhancement)
        {
            if (target == null) return;
            target.Combat?.AddBrand(source, m_Element, m_OnHit, m_Power + m_PowerPerLevel * enhancement);
        }

        public override void Remove(Combatant target, object source)
        {
            if (target == null) return;
            target.Combat?.RemoveBrands(source);
        }
    }
}
