using PS.Game.Inventory;
using UnityEngine;

namespace SO
{
    /// <summary>결속형 — 놓은 칸 자체의 용량·강화도를 올린다. 용량 2인 칸은 글자를 둘 담고
    /// 스캔에서 둘 중 하나로 읽힌다(OR).</summary>
    [CreateAssetMenu(menuName = "PS/Glyph/CellUpgrade", fileName = "Glyph_")]
    public class CellUpgradeGlyph : BoundGlyphData
    {
        [Tooltip("칸 용량 증가. 기본 1, 최대 2")]
        [SerializeField] private int m_Capacity;

        [Tooltip("칸 강화도 증가")]
        [SerializeField] private int m_Enhancement;

        public override bool CanApply(InventoryState state, Vector2Int at)
        {
            if (state == null || !state.Grid.InBounds(at)) return false;

            bool capacityHelps = m_Capacity > 0 && state.Grid[at].Capacity < GridCell.MaxCapacity;
            return capacityHelps || m_Enhancement != 0;
        }

        public override void Apply(InventoryState state, Vector2Int at)
        {
            if (m_Capacity != 0) state.Grid.AddCapacity(at, m_Capacity);
            if (m_Enhancement != 0) state.Grid.AddEnhancement(at, m_Enhancement);
        }
    }
}
