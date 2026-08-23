using System;
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace PS.Game.Inventory
{
    /// <summary>격자 상태. 배치 규칙만 안다 — 단어도 UI도 모른다.</summary>
    public class InventoryGrid
    {
        private static readonly Vector2Int[] k_Neighbors4 =
        {
            new Vector2Int(0, -1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
        };

        private GridCell[,] m_Cells;
        private readonly HashSet<Vector2Int> m_Dirty = new HashSet<Vector2Int>();

        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>마지막 ClearDirty 이후 값이 바뀐 칸. UI 부분 갱신용.</summary>
        public IReadOnlyCollection<Vector2Int> Dirty => m_Dirty;

        public event Action<Vector2Int> CellChanged;
        public event Action Resized;

        public InventoryGrid(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            m_Cells = new GridCell[Width, Height];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    m_Cells[x, y].Capacity = 1;
        }

        public GridCell this[Vector2Int at] => m_Cells[at.x, at.y];
        public GridCell this[int x, int y] => m_Cells[x, y];

        public bool InBounds(Vector2Int at)
            => at.x >= 0 && at.x < Width && at.y >= 0 && at.y < Height;

        public bool CanPlace(ItemData item, Vector2Int at)
            => item != null && InBounds(at) && !m_Cells[at.x, at.y].IsFull;

        public bool Place(ItemData item, Vector2Int at)
        {
            if (!CanPlace(item, at)) return false;

            if (m_Cells[at.x, at.y].Item == null) m_Cells[at.x, at.y].Item = item;
            else m_Cells[at.x, at.y].Item2 = item;

            MarkDirty(at);
            return true;
        }

        /// <summary>slot이 음수면 마지막에 넣은 것부터 뺀다. 뺀 뒤 앞으로 당겨 빈틈을 없앤다.</summary>
        public ItemData Take(Vector2Int at, int slot = -1)
        {
            if (!InBounds(at)) return null;

            GridCell cell = m_Cells[at.x, at.y];
            if (cell.Count == 0) return null;

            if (slot < 0) slot = cell.Count - 1;
            if (slot >= cell.Count) return null;

            ItemData item = cell.At(slot);
            if (item == null) return null;

            if (slot == 0) m_Cells[at.x, at.y].Item = cell.Item2;
            m_Cells[at.x, at.y].Item2 = null;

            MarkDirty(at);
            return item;
        }

        /// <summary>글리프가 부르는 곳. 범위 밖이면 조용히 무시한다 — 글리프가 가장자리에 놓일 수 있어서.</summary>
        public void AddEnhancement(Vector2Int at, int amount)
        {
            if (amount == 0 || !InBounds(at)) return;

            m_Cells[at.x, at.y].Enhancement += amount;
            MarkDirty(at);
        }

        /// <summary>칸 용량 증감. 1~MaxCapacity로 잘린다. 실제로 적용된 변화량을 돌려준다.</summary>
        public int AddCapacity(Vector2Int at, int amount)
        {
            if (amount == 0 || !InBounds(at)) return 0;

            int before = m_Cells[at.x, at.y].Capacity;
            int after = Mathf.Clamp(before + amount, 1, GridCell.MaxCapacity);
            if (after == before) return 0;

            m_Cells[at.x, at.y].Capacity = after;
            MarkDirty(at);
            return after - before;
        }

        /// <summary>용량을 amount만큼 줄여도 담긴 글자가 넘치지 않는가.</summary>
        public bool CanShrinkCapacity(Vector2Int at, int amount)
        {
            if (amount <= 0 || !InBounds(at)) return true;

            GridCell cell = m_Cells[at.x, at.y];
            return Mathf.Max(1, cell.Capacity - amount) >= cell.Count;
        }

        /// <summary>세로로만 늘어난다. 기존 칸은 그대로.</summary>
        public void Expand(int rows)
        {
            if (rows <= 0) return;

            int newHeight = Height + rows;
            var next = new GridCell[Width, newHeight];

            for (int y = 0; y < newHeight; y++)
                for (int x = 0; x < Width; x++)
                    next[x, y] = y < Height ? m_Cells[x, y] : new GridCell { Capacity = 1 };

            m_Cells = next;
            Height = newHeight;
            Resized?.Invoke();
        }

        public IEnumerable<Vector2Int> Neighbors4(Vector2Int at)
        {
            for (int i = 0; i < k_Neighbors4.Length; i++)
            {
                Vector2Int n = at + k_Neighbors4[i];
                if (InBounds(n)) yield return n;
            }
        }

        public void ClearDirty() => m_Dirty.Clear();

        private void MarkDirty(Vector2Int at)
        {
            m_Dirty.Add(at);
            CellChanged?.Invoke(at);
        }
    }
}
