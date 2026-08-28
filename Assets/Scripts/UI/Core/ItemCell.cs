using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PS.UI
{
    /// <summary>아이콘 + 강화도 테두리를 그리는 칸 1개. 인벤토리·포션·상점 어디서든 쓴다.</summary>
    public class ItemCell : MonoBehaviour
    {
        /// <summary>MinEnhancement 이상이면 이 색. 구간 값은 인스펙터에서 채운다.</summary>
        [System.Serializable]
        public struct EnhancementTier
        {
            public int MinEnhancement;
            public Color Color;
        }

        [SerializeField] private Image m_Icon;

        [Tooltip("스프라이트가 없을 때 대신 보여줄 글자. 프로토타입용")]
        [SerializeField] private TextMeshProUGUI m_Label;

        [SerializeField] private Outline m_Border;
        [SerializeField] private EnhancementTier[] m_BorderTiers;

        private bool m_Highlighted;
        private bool m_SavedBorder;
        private Color m_SavedColor;
        private int m_LastEnhancement;

        /// <summary>워드 목록에서 가리킨 칸. 강화도 테두리를 잠시 덮어쓰고, 끄면 되돌린다.</summary>
        public void SetHighlight(bool on, Color color)
        {
            if (m_Border == null) return;

            if (on)
            {
                if (!m_Highlighted)
                {
                    m_SavedBorder = m_Border.enabled;
                    m_SavedColor = m_Border.effectColor;
                    m_Highlighted = true;
                }

                m_Border.enabled = true;
                m_Border.effectColor = color;
                return;
            }

            if (!m_Highlighted) return;

            m_Highlighted = false;
            m_Border.enabled = m_SavedBorder;
            m_Border.effectColor = m_SavedColor;
        }

        public void Bind(Sprite icon, string label, int enhancement)
        {
            if (m_Icon != null)
            {
                m_Icon.enabled = icon != null;
                m_Icon.sprite = icon;
            }

            if (m_Label != null)
                m_Label.text = string.IsNullOrEmpty(label) ? string.Empty : label;

            ApplyBorder(enhancement);
        }

        public void Bind(Sprite icon, int enhancement) => Bind(icon, null, enhancement);

        public void Clear() => Bind(null, null, 0);

        /// <summary>지금 집어 든 글자가 들어있는 칸. 원래 자리에 남아있다는 걸 흐리게 표시한다.</summary>
        public void SetDimmed(bool dimmed)
        {
            float alpha = dimmed ? 0.35f : 1f;

            if (m_Icon != null)
            {
                Color c = m_Icon.color; c.a = alpha; m_Icon.color = c;
            }

            if (m_Label != null)
            {
                Color c = m_Label.color; c.a = alpha; m_Label.color = c;
            }
        }

        private void ApplyBorder(int enhancement)
        {
            if (m_Border == null) return;

            m_LastEnhancement = enhancement;

            // 하이라이트 중이면 지금 색을 건드리지 않고, 되돌릴 값만 갱신한다.
            if (m_Highlighted)
            {
                ResolveBorder(enhancement, out m_SavedBorder, out m_SavedColor);
                return;
            }

            bool enabled;
            Color color;
            ResolveBorder(enhancement, out enabled, out color);

            m_Border.enabled = enabled;
            if (enabled) m_Border.effectColor = color;
        }

        private void ResolveBorder(int enhancement, out bool enabled, out Color color)
        {
            int best = -1;
            if (m_BorderTiers != null)
            {
                for (int i = 0; i < m_BorderTiers.Length; i++)
                {
                    if (enhancement < m_BorderTiers[i].MinEnhancement) continue;
                    if (best < 0 || m_BorderTiers[i].MinEnhancement > m_BorderTiers[best].MinEnhancement) best = i;
                }
            }

            enabled = best >= 0;
            color = enabled ? m_BorderTiers[best].Color : Color.clear;
        }
    }
}
