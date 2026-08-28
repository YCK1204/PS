using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PS.UI
{
    /// <summary>보상 선택 화면. 무엇을 보여줄지는 바깥이 정하고, 여기는 목록을 그리고 고른 걸 알려준다.</summary>
    public class RewardPanel : UIPanel
    {
        [SerializeField] private TMP_Text m_Title;
        [SerializeField] private TMP_Text m_Hint;
        [SerializeField] private Transform m_ChoiceRoot;

        [Tooltip("복제해서 쓸 선택지 버튼. 꺼둔 채로 둔다")]
        [SerializeField] private Button m_ChoiceTemplate;

        private readonly List<Button> m_Spawned = new List<Button>();
        private Action<int> m_OnPick;

        /// <summary>선택 목록을 띄운다. onPick은 고른 인덱스를 받는다.</summary>
        public void Show(string title, string hint, IList<string> options, Action<int> onPick)
        {
            m_OnPick = onPick;

            if (m_Title != null) m_Title.text = title;
            if (m_Hint != null) m_Hint.text = hint;

            Build(options);

            if (!IsOpen) Open();
        }

        protected override void OnClosing()
        {
            Clear();
            m_OnPick = null;
        }

        private void Build(IList<string> options)
        {
            Clear();

            if (m_ChoiceTemplate == null || m_ChoiceRoot == null || options == null) return;

            for (int i = 0; i < options.Count; i++)
            {
                Button button = Instantiate(m_ChoiceTemplate, m_ChoiceRoot);
                button.gameObject.SetActive(true);
                button.name = "Choice_" + i;

                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = options[i];

                int index = i;
                button.onClick.AddListener(delegate { Pick(index); });

                m_Spawned.Add(button);
            }
        }

        private void Pick(int index)
        {
            Action<int> callback = m_OnPick;
            m_OnPick = null;

            callback?.Invoke(index);
        }

        private void Clear()
        {
            for (int i = 0; i < m_Spawned.Count; i++)
                if (m_Spawned[i] != null) Destroy(m_Spawned[i].gameObject);

            m_Spawned.Clear();
        }
    }
}
