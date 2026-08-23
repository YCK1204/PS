using System.Collections.Generic;
using SO;
using UnityEngine;

namespace PS.UI
{
    /// <summary>포션 구역. 격자 위에 붙는 별도 슬롯 줄.</summary>
    public class PotionView : MonoBehaviour
    {
        [SerializeField] private ItemCell[] m_Slots;

        public int Count => m_Slots != null ? m_Slots.Length : 0;

        public void Bind(IReadOnlyList<ItemData> potions)
        {
            if (m_Slots == null) return;

            for (int i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i] == null) continue;

                ItemData item = potions != null && i < potions.Count ? potions[i] : null;
                m_Slots[i].Bind(item != null ? item.Icon : null, 0);
            }
        }
    }
}
