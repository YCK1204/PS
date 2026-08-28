using System;
using System.Collections.Generic;
using PS.Game.Inventory;
using SO;
using TMPro;
using UnityEngine;

namespace PS.UI
{
    /// <summary>워드 목록 구역. 한 글자라도 갖고 있는 단어만 띄운다.</summary>
    public class WordListView : MonoBehaviour
    {
        [SerializeField] private WordRow[] m_Rows;
        [SerializeField] private TextMeshProUGUI m_Count;

        public int Count => m_Rows != null ? m_Rows.Length : 0;

        /// <summary>줄에 마우스가 올라가거나 벗어났다. (단어, 성립여부, 들어옴)</summary>
        public event Action<WordData, bool, bool> RowHovered;

        private void Awake()
        {
            if (m_Rows == null) return;

            for (int i = 0; i < m_Rows.Length; i++)
            {
                if (m_Rows[i] == null) continue;
                m_Rows[i].Hovered += OnRowHovered;
            }
        }

        private void OnDestroy()
        {
            if (m_Rows == null) return;

            for (int i = 0; i < m_Rows.Length; i++)
            {
                if (m_Rows[i] == null) continue;
                m_Rows[i].Hovered -= OnRowHovered;
            }
        }

        private void OnRowHovered(WordRow row, bool entered)
            => RowHovered?.Invoke(row.Word, row.Active, entered);

        public void Bind(IReadOnlyList<WordProgress> rows, int knownTotal)
        {
            if (m_Rows == null) return;

            int shown = rows != null ? rows.Count : 0;
            int active = 0;

            for (int i = 0; i < m_Rows.Length; i++)
            {
                WordRow row = m_Rows[i];
                if (row == null) continue;

                if (i < shown)
                {
                    row.Bind(rows[i]);
                    if (rows[i].Active) active++;
                }
                else
                {
                    row.Hide();
                }
            }

            if (shown > m_Rows.Length)
                Debug.LogWarning($"워드 줄 부족 — {shown}개 중 {m_Rows.Length}개만 표시됨", this);

            if (m_Count != null)
                m_Count.text = $"{active} / {knownTotal}";
        }
    }
}
