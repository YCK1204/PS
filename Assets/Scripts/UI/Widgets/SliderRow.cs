using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PS.UI
{
    /// <summary>라벨 + 슬라이더 + 값 표시. 표시 형식은 인스펙터에서 바꾼다.</summary>
    public class SliderRow : OptionRow
    {
        public Slider Slider;
        public TMP_Text ValueLabel;

        [Tooltip("값 표시 형식. {0}에 DisplayScale을 곱한 값이 들어간다.")]
        public string Format = "{0:0}%";

        [Tooltip("표시용 배율. 0~1 슬라이더를 퍼센트로 보여주려면 100.")]
        public float DisplayScale = 100f;

        public event Action<float> ValueChanged;

        bool m_Suppress;

        void OnEnable()
        {
            if (Slider != null) Slider.onValueChanged.AddListener(OnSliderChanged);
            Refresh(GetValue());
        }

        void OnDisable()
        {
            if (Slider != null) Slider.onValueChanged.RemoveListener(OnSliderChanged);
        }

        public void SetValue(float value)
        {
            m_Suppress = true;
            if (Slider != null) Slider.SetValueWithoutNotify(value);
            m_Suppress = false;
            Refresh(value);
        }

        public float GetValue() => Slider != null ? Slider.value : 0f;

        void OnSliderChanged(float v)
        {
            Refresh(v);
            if (!m_Suppress) ValueChanged?.Invoke(v);
        }

        void Refresh(float v)
        {
            if (ValueLabel == null) return;
            ValueLabel.text = string.Format(Format, v * DisplayScale);
        }
    }
}
