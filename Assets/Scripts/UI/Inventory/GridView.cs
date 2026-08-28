using System.Collections.Generic;
using PS.Game.Inventory;
using SO;
using UnityEngine;

namespace PS.UI
{
    /// <summary>격자 구역. 셀 배열의 인덱스를 좌표로 환산해 그린다.</summary>
    public class GridView : MonoBehaviour
    {
        [Tooltip("씬에 미리 깔아둔 칸들. 격자가 커지면 이걸 복제해 늘린다")]
        [SerializeField] private ItemCell[] m_Cells;

        [Tooltip("칸들의 부모. 비우면 첫 칸의 부모를 쓴다")]
        [SerializeField] private RectTransform m_CellRoot;

        [SerializeField] private Vector2 m_CellSize = new Vector2(78f, 78f);
        [SerializeField] private Vector2 m_Spacing = new Vector2(6f, 6f);

        private readonly List<ItemCell> m_All = new List<ItemCell>();
        private InventoryInput m_Input;
        private int m_Width;

        public int Count => m_All.Count;

        /// <summary>지금 격자를 그리는 데 필요한 크기. 창을 늘릴 때 쓴다.</summary>
        public Vector2 ContentSize { get; private set; }

        /// <summary>격자 크기가 바뀌어 칸을 새로 깔았다.</summary>
        public event System.Action<Vector2> Resized;

        private void Awake() => Collect();

        private void Collect()
        {
            if (m_All.Count > 0) return;

            if (m_Cells != null)
                for (int i = 0; i < m_Cells.Length; i++)
                    if (m_Cells[i] != null) m_All.Add(m_Cells[i]);

            if (m_All.Count == 0) return;

            if (m_CellRoot == null) m_CellRoot = m_All[0].transform.parent as RectTransform;

            var input = m_All[0].GetComponent<InventoryCellInput>();
            if (input != null) m_Input = input.Owner;
        }

        public ItemCell CellAt(Vector2Int at, int gridWidth)
        {
            int index = at.y * gridWidth + at.x;
            return index >= 0 && index < m_All.Count ? m_All[index] : null;
        }

        /// <summary>격자 크기에 맞춰 칸을 만들고 자리를 잡는다. 남는 칸은 끈다.</summary>
        private void Ensure(int width, int height)
        {
            Collect();
            if (m_All.Count == 0 || m_CellRoot == null) return;

            int need = width * height;

            while (m_All.Count < need)
            {
                ItemCell clone = Instantiate(m_All[0], m_CellRoot);
                clone.name = "Cell_clone_" + m_All.Count;
                m_All.Add(clone);
            }

            float stepX = m_CellSize.x + m_Spacing.x;
            float stepY = m_CellSize.y + m_Spacing.y;

            for (int i = 0; i < m_All.Count; i++)
            {
                ItemCell cell = m_All[i];
                if (cell == null) continue;

                bool used = i < need;
                if (cell.gameObject.activeSelf != used) cell.gameObject.SetActive(used);
                if (!used) continue;

                int x = i % width;
                int y = i / width;

                var rt = (RectTransform)cell.transform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.sizeDelta = m_CellSize;
                rt.anchoredPosition = new Vector2(x * stepX, -y * stepY);

                var input = cell.GetComponent<InventoryCellInput>();
                if (input == null) continue;

                input.Coord = new Vector2Int(x, y);
                if (input.Owner == null) input.Owner = m_Input;
            }

            var size = new Vector2(width * m_CellSize.x + (width - 1) * m_Spacing.x,
                                   height * m_CellSize.y + (height - 1) * m_Spacing.y);

            bool changed = m_Width != width || ContentSize != size;
            m_Width = width;
            ContentSize = size;

            if (changed) Resized?.Invoke(size);
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
            Collect();
            if (m_All.Count == 0) return;

            if (grid != null) Ensure(grid.Width, grid.Height);

            for (int i = 0; i < m_All.Count; i++)
            {
                ItemCell cell = m_All[i];
                if (cell == null) continue;

                if (grid == null)
                {
                    cell.Clear();
                    cell.SetDimmed(false);
                    continue;
                }

                if (!cell.gameObject.activeSelf) continue;

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
