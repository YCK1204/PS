using System;
using PS.Game.Inventory;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PS.UI
{
    /// <summary>워드 목록 한 줄. 성립하면 강화도, 아니면 몇 글자 모였는지 보여준다.</summary>
    public class WordRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image m_Background;
        [SerializeField] private Image m_Icon;
        [SerializeField] private TextMeshProUGUI m_Word;
        [SerializeField] private TextMeshProUGUI m_Enhancement;

        [Tooltip("아이콘 스프라이트가 없을 때 대신 보여줄 표식")]
        [SerializeField] private TextMeshProUGUI m_IconLabel;

        [SerializeField] private Color m_OnBackground = Color.white;
        [SerializeField] private Color m_OffBackground = Color.gray;
        [SerializeField] private Color m_OnText = Color.white;
        [SerializeField] private Color m_OffText = Color.gray;
        [SerializeField] private Color m_OnIcon = Color.white;
        [SerializeField] private Color m_OffIcon = Color.gray;

        /// <summary>이 줄이 가리키는 단어. 격자 하이라이트가 이걸 본다.</summary>
        public WordData Word { get; private set; }

        public bool Active { get; private set; }

        /// <summary>줄에 마우스가 들어오고 나갈 때. (줄, 들어옴)</summary>
        public event Action<WordRow, bool> Hovered;

        public void OnPointerEnter(PointerEventData eventData) => Hovered?.Invoke(this, true);

        public void OnPointerExit(PointerEventData eventData) => Hovered?.Invoke(this, false);

        public void Bind(in WordProgress progress)
        {
            bool active = progress.Active;
            Word = progress.Word;
            Active = active;
            string right = active ? $"{progress.Enhancement}강" : $"{progress.Have} / {progress.Total}";

            string name = progress.Word != null ? progress.Word.Word : string.Empty;
            if (active && progress.Count > 1)
                name += $" <size=70%><alpha=#99>×{progress.Count}</alpha></size>";

            Apply(name, right, active, progress.Word != null ? progress.Word.Icon : null);

            gameObject.SetActive(true);
        }

        /// <summary>쓰지 않는 줄. 목록에서 사라진다.</summary>
        public void Hide()
        {
            if (gameObject.activeSelf) Hovered?.Invoke(this, false);

            Word = null;
            Active = false;
            gameObject.SetActive(false);
        }

        private void Apply(string word, string right, bool active, Sprite icon)
        {
            if (m_Word != null)
            {
                m_Word.text = word;
                m_Word.color = active ? m_OnText : m_OffText;
            }

            if (m_Enhancement != null)
            {
                m_Enhancement.text = right;
                m_Enhancement.color = active ? m_OnText : m_OffText;
            }

            if (m_Background != null)
                m_Background.color = active ? m_OnBackground : m_OffBackground;

            if (m_IconLabel != null)
                m_IconLabel.color = active ? m_OnIcon : m_OffIcon;

            if (m_Icon != null)
            {
                m_Icon.enabled = icon != null;
                m_Icon.sprite = icon;
            }
        }
    }
}
