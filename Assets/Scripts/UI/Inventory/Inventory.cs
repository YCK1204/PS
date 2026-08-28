using System.Collections.Generic;
using PS.Game.Inventory;
using SO;
using UnityEngine;

namespace PS.UI
{
    /// <summary>인벤토리 창. 구역별 View에 위임만 하고 자기는 상태를 안 갖는다.</summary>
    public class Inventory : UIPanel
    {
        [SerializeField] private GridView m_GridView;
        [SerializeField] private PotionView m_PotionView;
        [SerializeField] private WordListView m_WordListView;

        [Header("격자 확장 대응")]
        [Tooltip("격자를 감싸는 영역. 격자가 커지면 같이 커진다")]
        [SerializeField] private RectTransform m_GridArea;

        [Tooltip("창 자체. 비우면 자기 RectTransform")]
        [SerializeField] private RectTransform m_Window;

        [Tooltip("워드 목록. 창 높이에 맞춰 늘어난다")]
        [SerializeField] private RectTransform m_WordList;

        [Tooltip("GridArea가 격자보다 큰 여백")]
        [SerializeField] private Vector2 m_GridAreaPadding = new Vector2(30f, 30f);

        [Tooltip("창 위쪽 여백 (탭·포션 줄)")]
        [SerializeField] private float m_TopMargin = 90f;

        [Tooltip("창 아래쪽 여백 (안내문)")]
        [SerializeField] private float m_BottomMargin = 78f;

        [Tooltip("워드 목록 위아래 여백 합")]
        [SerializeField] private float m_WordListMargin = 48f;

        [Header("워드 하이라이트")]
        [Tooltip("성립한 단어를 가리켰을 때 그 칸들의 테두리 색")]
        [SerializeField] private Color m_ActiveHighlight = new Color(0.941f, 0.776f, 0.455f, 1f);

        [Tooltip("아직 성립하지 않은 단어를 가리켰을 때 — 재료가 되는 글자들")]
        [SerializeField] private Color m_MaterialHighlight = new Color(0.42f, 0.66f, 0.90f, 1f);

        private readonly List<Vector2Int> m_HighlightBuffer = new List<Vector2Int>();

        private InventoryState m_State;

        public InventoryState State => m_State;

        /// <summary>닫혀 있어도 부를 수 있다. 구독은 열려 있을 때만 건다.</summary>
        public void Bind(InventoryState state)
        {
            Unsubscribe();
            m_State = state;

            if (isActiveAndEnabled) Subscribe();

            Refresh();
        }

        // 닫혀 있는 동안은 구독하지 않는다 — 전투 중 인벤토리 갱신은 낭비.
        // 다시 열릴 때 OnEnable이 한 번에 따라잡는다.
        private void Awake()
        {
            if (m_GridView != null) m_GridView.Resized += ApplyGridSize;
            if (m_WordListView != null) m_WordListView.RowHovered += OnWordHovered;
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (m_GridView != null) m_GridView.Resized -= ApplyGridSize;
            if (m_WordListView != null) m_WordListView.RowHovered -= OnWordHovered;
        }

        protected override void OnClosing()
        {
            m_GridView?.ClearHighlight();
        }

        /// <summary>워드 줄을 가리키면 격자에서 그 단어의 칸을 밝힌다.
        /// 성립한 단어는 실제로 읽히는 칸을, 아직 아닌 단어는 재료가 되는 글자 칸을 보여준다.</summary>
        private void OnWordHovered(WordData word, bool active, bool entered)
        {
            if (m_GridView == null) return;

            if (!entered || word == null || m_State == null)
            {
                m_GridView.ClearHighlight();
                return;
            }

            m_HighlightBuffer.Clear();

            if (active) CollectMatchCells(word);
            else CollectMaterialCells(word);

            m_GridView.SetHighlight(m_HighlightBuffer, m_State.Grid.Width,
                active ? m_ActiveHighlight : m_MaterialHighlight);
        }

        /// <summary>성립한 매치가 실제로 지나간 칸들. 여러 군데 성립하면 전부.</summary>
        private void CollectMatchCells(WordData word)
        {
            IReadOnlyList<WordMatch> matches = m_State.ActiveWords;

            for (int i = 0; i < matches.Count; i++)
            {
                WordMatch match = matches[i];
                if (match.Word != word) continue;

                for (int step = 0; step < match.Length; step++)
                {
                    Vector2Int at = match.CellAt(step);
                    if (!m_HighlightBuffer.Contains(at)) m_HighlightBuffer.Add(at);
                }
            }
        }

        /// <summary>단어를 이루는 글자를 갖고 있는 칸들. 아직 이어지지 않았어도 재료는 보인다.</summary>
        private void CollectMaterialCells(WordData word)
        {
            string text = word.Word;
            if (string.IsNullOrEmpty(text)) return;

            InventoryGrid grid = m_State.Grid;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var at = new Vector2Int(x, y);
                    GridCell cell = grid[at];

                    for (int slot = 0; slot < cell.Count; slot++)
                    {
                        if (!(cell.At(slot) is LetterData letter)) continue;
                        if (text.IndexOf(letter.Letter) < 0) continue;

                        m_HighlightBuffer.Add(at);
                        break;
                    }
                }
            }
        }

        /// <summary>격자가 커지면 창을 아래로 늘린다. 칸을 줄이면 글자가 안 읽힌다.</summary>
        private void ApplyGridSize(Vector2 content)
        {
            if (m_GridArea == null) return;

            Vector2 area = content + m_GridAreaPadding;
            m_GridArea.sizeDelta = area;

            RectTransform window = m_Window != null ? m_Window : transform as RectTransform;
            if (window == null) return;

            float height = m_TopMargin + area.y + m_BottomMargin;
            window.sizeDelta = new Vector2(window.sizeDelta.x, height);

            if (m_WordList != null)
                m_WordList.sizeDelta = new Vector2(m_WordList.sizeDelta.x, Mathf.Max(0f, height - m_WordListMargin));
        }

        private void Subscribe()
        {
            if (m_State == null) return;
            m_State.Changed -= Refresh;
            m_State.Changed += Refresh;
        }

        private void Unsubscribe()
        {
            if (m_State != null) m_State.Changed -= Refresh;
        }

        private void Refresh()
        {
            if (m_GridView != null) m_GridView.Bind(m_State != null ? m_State.Grid : null,
                m_State != null && m_State.Held != null, m_State != null ? m_State.HeldFrom : default);
            if (m_PotionView != null) m_PotionView.Bind(m_State != null ? m_State.Potions : null);
            if (m_WordListView != null) m_WordListView.Bind(m_State != null ? m_State.WordRows : null, m_State != null ? m_State.KnownWordCount : 0);

            if (m_State != null) m_State.Grid.ClearDirty();
        }
    }
}
