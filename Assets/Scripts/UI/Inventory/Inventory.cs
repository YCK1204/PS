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

        private void OnDestroy() => Unsubscribe();

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
