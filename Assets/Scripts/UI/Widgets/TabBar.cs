using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PS.UI
{
    /// <summary>
    /// 한 번에 하나만 켜지는 탭 묶음. 설정·인벤토리·도감 어디서든 쓴다.
    /// 색은 인스펙터에서 바꾼다.
    /// </summary>
    public class TabBar : MonoBehaviour
    {
        [Serializable]
        public class Tab
        {
            public Button Button;
            public TMP_Text Label;
            [Tooltip("이 탭이 켜질 때 활성화할 오브젝트. 비워두면 색만 바뀐다.")]
            public GameObject Page;
        }

        public List<Tab> Tabs = new List<Tab>();

        [Header("색")]
        public Color SelectedColor = new Color(0.941f, 0.776f, 0.455f);
        public Color NormalColor = new Color(0.137f, 0.157f, 0.220f);
        public Color SelectedLabelColor = new Color(0.071f, 0.078f, 0.110f);
        public Color NormalLabelColor = new Color(0.545f, 0.576f, 0.655f);

        [Tooltip("활성화될 때 자동으로 첫 탭을 선택할지")]
        public bool SelectFirstOnEnable = true;

        public int SelectedIndex { get; private set; } = -1;

        public event Action<int> TabSelected;

        bool m_Hooked;

        void OnEnable()
        {
            Hook();
            if (SelectFirstOnEnable || SelectedIndex < 0) Select(SelectedIndex < 0 ? 0 : SelectedIndex);
            else Refresh();
        }

        void Hook()
        {
            if (m_Hooked) return;
            for (int i = 0; i < Tabs.Count; i++)
            {
                int captured = i;
                if (Tabs[i].Button != null) Tabs[i].Button.onClick.AddListener(() => Select(captured));
            }
            m_Hooked = true;
        }

        public void Select(int index)
        {
            if (Tabs.Count == 0) return;
            SelectedIndex = Mathf.Clamp(index, 0, Tabs.Count - 1);
            Refresh();
            TabSelected?.Invoke(SelectedIndex);
        }

        void Refresh()
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                bool on = i == SelectedIndex;
                Tab t = Tabs[i];
                if (t.Page != null) t.Page.SetActive(on);
                if (t.Button != null && t.Button.targetGraphic is Image img) img.color = on ? SelectedColor : NormalColor;
                if (t.Label != null) t.Label.color = on ? SelectedLabelColor : NormalLabelColor;
            }
        }
    }
}
