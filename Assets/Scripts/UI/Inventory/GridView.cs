using PS.Game.Inventory;
using SO;
using UnityEngine;

namespace PS.UI
{
    /// <summary>격자 구역. 셀 배열의 인덱스를 좌표로 환산해 그린다.</summary>
    public class GridView : MonoBehaviour
    {
        [SerializeField] private ItemCell[] m_Cells;

        public int Count => m_Cells != null ? m_Cells.Length : 0;

        public ItemCell CellAt(Vector2Int at, int gridWidth)
        {
            int index = at.y * gridWidth + at.x;
            return index >= 0 && index < Count ? m_Cells[index] : null;
        }

        private static readonly string s_GlyphHex = ColorUtility.ToHtmlStringRGB(Palette.Glyph);

        /// <summary>빈 슬롯 자리표시. 용량 2인 칸을 눈으로 찾을 수 있게.</summary>
        private const string EmptySlot = "<alpha=#40>_<alpha=#FF>";

        /// <summary>용량 2인 칸은 "T|H"처럼 두 자리를 다 보여준다. 스프라이트가 붙기 전 임시 표기.</summary>
        private static string LabelOf(in GridCell cell)
        {
            string first = MarkOf(cell.Item);
            string second = MarkOf(cell.Item2);

            if (cell.Capacity < 2) return first ?? second;

            return (first ?? EmptySlot) + "<alpha=#40>|<alpha=#FF>" + (second ?? EmptySlot);
        }

        /// <summary>글리프는 글자와 구분되게 색을 입힌다.</summary>
        private static string MarkOf(ItemData item)
        {
            if (item == null) return null;

            string label = item.ShortLabel;
            if (string.IsNullOrEmpty(label)) return null;

            return item is GlyphData ? "<color=#" + s_GlyphHex + ">" + label + "</color>" : label;
        }

        public void Bind(InventoryGrid grid, bool hasHeld = false, Vector2Int heldAt = default)
        {
            if (m_Cells == null) return;

            for (int i = 0; i < m_Cells.Length; i++)
            {
                ItemCell cell = m_Cells[i];
                if (cell == null) continue;

                if (grid == null)
                {
                    cell.Clear();
                    cell.SetDimmed(false);
                    continue;
                }

                var at = new Vector2Int(i % grid.Width, i / grid.Width);
                if (!grid.InBounds(at))
                {
                    cell.Clear();
                    cell.SetDimmed(false);
                    continue;
                }

                GridCell data = grid[at];
                cell.Bind(data.IsEmpty ? null : data.Item.Icon, LabelOf(data), data.Enhancement);
                cell.SetDimmed(hasHeld && at == heldAt);
            }
        }
    }
}
