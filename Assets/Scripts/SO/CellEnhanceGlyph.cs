using System;
using System.Collections.Generic;
using PS.Game.Inventory;
using UnityEngine;

namespace SO
{
    [Serializable]
    public struct CellEnhanceData
    {
        public Vector2Int Offset;
        public int Amount;
    }
    
    /// <summary>이동형 — 자기 칸 기준 상대 좌표들의 강화도를 올린다. 칸을 차지하는 대신 옮길 수 있다.</summary>
    [CreateAssetMenu(menuName = "PS/Glyph/CellEnhance", fileName = "Glyph_")]
    public class CellEnhanceGlyph : MobileGlyphData
    {
        [SerializeField] List<CellEnhanceData> m_Enhances;

        public override void OnEquip(InventoryState state, Vector2Int at)
            => Apply(state, at);

        public override void OnUnequip(InventoryState state, Vector2Int at)
            => Apply(state, at, false);

        private void Apply(InventoryState state, Vector2Int at, bool equip = true)
        {
            if (m_Enhances == null || m_Enhances.Count == 0) return;

            for (int i = 0; i < m_Enhances.Count; i++)
                state.Grid.AddEnhancement(at + m_Enhances[i].Offset, m_Enhances[i].Amount * (equip ? 1 : -1));
        }
    }
}
