using UnityEngine;

namespace SO
{
    /// <summary>격자를 개조하는 물건. 결속형(BoundGlyphData)과 이동형(MobileGlyphData)으로 갈린다.</summary>
    public abstract class GlyphData : ItemData
    {
        [Tooltip("스프라이트가 없을 때 칸에 그릴 표기. 비우면 '+'")]
        [SerializeField] private string m_Mark;

        public sealed override string ShortLabel => string.IsNullOrEmpty(m_Mark) ? "+" : m_Mark;
    }
}
