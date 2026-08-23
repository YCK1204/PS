using PS.Game.Inventory;
using SO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PS.UI
{
    /// <summary>마우스 드래그로 글자를 옮긴다. 누르면 집고, 뗀 자리에 놓는다.
    /// 격자 밖에서 떼면 취소. 꽉 찬 칸이면 교환.</summary>
    public class InventoryInput : MonoBehaviour
    {
        [SerializeField] private Inventory m_Inventory;
        [SerializeField] private ItemCell m_Ghost;
        [SerializeField] private RectTransform m_GhostRect;
        [SerializeField] private RectTransform m_GhostSpace;
        [SerializeField] private Canvas m_Canvas;

        private InventoryState State => m_Inventory != null ? m_Inventory.State : null;

        private void OnEnable() => ShowGhost(false);

        public void OnCellDown(Vector2Int coord, PointerEventData eventData)
        {
            InventoryState state = State;
            if (state == null || state.Held != null) return;

            if (!state.Take(coord)) return;

            UpdateGhostContent();
            MoveGhost(eventData);
            ShowGhost(true);
        }

        public void OnCellDrag(PointerEventData eventData)
        {
            if (State == null || State.Held == null) return;
            MoveGhost(eventData);
        }

        public void OnCellUp(PointerEventData eventData)
        {
            InventoryState state = State;
            if (state == null || state.Held == null) return;

            InventoryCellInput target = ResolveCell(eventData);

            if (target == null) state.Cancel();
            else if (!state.Place(target.Coord)) state.Cancel();

            ShowGhost(false);
        }

        /// <summary>포인터 아래에 있는 칸. 없으면 null — 격자 밖에서 뗀 것.</summary>
        private static InventoryCellInput ResolveCell(PointerEventData eventData)
        {
            GameObject hit = eventData.pointerCurrentRaycast.gameObject;
            if (hit == null) return null;

            return hit.GetComponentInParent<InventoryCellInput>();
        }

        private void UpdateGhostContent()
        {
            if (m_Ghost == null) return;

            ItemData item = State.Held;
            string label = item is LetterData letter ? letter.Letter.ToString() : null;
            m_Ghost.Bind(item != null ? item.Icon : null, label, 0);
        }

        private void MoveGhost(PointerEventData eventData)
        {
            if (m_GhostRect == null || m_GhostSpace == null) return;

            Camera cam = m_Canvas != null && m_Canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? m_Canvas.worldCamera
                : null;

            Vector2 local;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_GhostSpace, eventData.position, cam, out local))
                m_GhostRect.anchoredPosition = local;
        }

        private void ShowGhost(bool visible)
        {
            if (m_GhostRect != null) m_GhostRect.gameObject.SetActive(visible);
        }
    }
}
