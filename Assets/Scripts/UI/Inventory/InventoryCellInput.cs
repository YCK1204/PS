using UnityEngine;
using UnityEngine.EventSystems;

namespace PS.UI
{
    /// <summary>칸 하나의 포인터 입력을 InventoryInput으로 넘긴다. 자기는 아무것도 판단하지 않는다.</summary>
    public class InventoryCellInput : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public Vector2Int Coord;
        public InventoryInput Owner;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Owner != null) Owner.OnCellDown(Coord, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Owner != null) Owner.OnCellDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Owner != null) Owner.OnCellUp(eventData);
        }
    }
}
