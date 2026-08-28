using PS.Game.Inventory;
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
