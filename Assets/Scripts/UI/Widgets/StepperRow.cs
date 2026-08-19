using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PS.UI
{
    /// <summary>라벨 + ◀ 값 ▶. 문자열 배열을 순환한다.</summary>
    public class StepperRow : OptionRow
    {
        public Button PrevButton;
        public Button NextButton;
        public TMP_Text ValueLabel;

        [Tooltip("표시할 선택지. 코드에서 SetOptions로 덮어써도 된다.")]
        public string[] Options = new string[0];

        [Tooltip("끝에서 반대편으로 넘어갈지")]
        public bool Wrap = true;

        [Tooltip("선택지가 없을 때 보여줄 문자열")]
        public string EmptyText = "-";

        public event Action<int> IndexChanged;

        [SerializeField] int m_Index;

        public int Index => m_Index;
        public string Current => (Options != null && m_Index >= 0 && m_Index < Options.Length) ? Options[m_Index] : EmptyText;

        void OnEnable()
        {
            if (PrevButton != null) PrevButton.onClick.AddListener(Prev);
            if (NextButton != null) NextButton.onClick.AddListener(Next);
            Refresh();
        }

        void OnDisable()
        {
            if (PrevButton != null) PrevButton.onClick.RemoveListener(Prev);
            if (NextButton != null) NextButton.onClick.RemoveListener(Next);
        }

        public void SetOptions(string[] options, int index)
        {
            Options = options ?? new string[0];
            SetIndex(index, false);
        }

        public void SetIndex(int index, bool notify)
        {
            if (Options == null || Options.Length == 0) { m_Index = 0; Refresh(); return; }
            m_Index = Mathf.Clamp(index, 0, Options.Length - 1);
            Refresh();
            if (notify) IndexChanged?.Invoke(m_Index);
        }

        void Prev() => Step(-1);
        void Next() => Step(1);

        void Step(int dir)
        {
            if (Options == null || Options.Length == 0) return;
            int next = m_Index + dir;
            if (Wrap) next = (next + Options.Length) % Options.Length;
            else next = Mathf.Clamp(next, 0, Options.Length - 1);
            if (next == m_Index) return;
            SetIndex(next, true);
        }

        void Refresh()
        {
            if (ValueLabel != null) ValueLabel.text = Current;
        }
    }
}
