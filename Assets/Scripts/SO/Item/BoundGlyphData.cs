using PS.Game.Inventory;
using UnityEngine;

namespace SO
{
    /// <summary>결속형. 칸에 놓으면 그 칸의 스펙(용량·강화도)을 바꾸고 자신은 사라진다.
    /// 격자를 차지하지 않고, 되돌릴 수 없다.</summary>
    public abstract class BoundGlyphData : GlyphData
    {
        public sealed override ItemType Type => ItemType.BoundGlyph;
        public sealed override bool ConsumedOnPlace => true;

        /// <summary>이 칸에 써서 실제로 달라지는 게 있는가. 없으면 소모시키지 않는다.</summary>
        public virtual bool CanApply(InventoryState state, Vector2Int at) => true;

        public abstract void Apply(InventoryState state, Vector2Int at);
    }
}
