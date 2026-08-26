using PS.Core;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>얼음 껍질 연출. 자라서 멈췄다가, 깨지기 직전에 부들부들 떨고, 깨진다.</summary>
    [RequireComponent(typeof(Animator))]
    public class FreezeVisual : MonoBehaviour, IPoolable
    {
        [SerializeField] private string m_FormState = "Form";
        [SerializeField] private string m_BreakState = "Break";

        [Tooltip("깨지는 연출 길이(초). 이 시간이 지나면 사라진다")]
        [SerializeField] private float m_BreakLength = 0.32f;

        [Header("깨지기 전 떨림")]
        [Tooltip("깨지기 몇 초 전부터 떨기 시작하나")]
        [SerializeField] private float m_ShakeLead = 0.4f;

        [Tooltip("최대 흔들림 폭(유닛). 끝으로 갈수록 이 값까지 커진다")]
        [SerializeField] private float m_ShakeAmplitude = 0.055f;

        [Tooltip("픽셀 단위로 끊어 흔든다. 스프라이트 PPU를 넣는다. 0이면 매끄럽게")]
        [SerializeField] private float m_PixelsPerUnit = 48f;

        private Animator m_Animator;
        private Vector3 m_Base;
        private bool m_Shaking;
        private float m_ShakeStart;

        public float BreakLength => m_BreakLength;
        public float ShakeLead => m_ShakeLead;

        private void Awake() => m_Animator = GetComponent<Animator>();

        private void OnEnable() => PlayForm();

        /// <summary>기준 위치는 꺼낼 때 잡는다. 풀에서 나오면 부모가 매번 달라진다.</summary>
        public void OnGet()
        {
            m_Base = transform.localPosition;
            m_Shaking = false;
        }

        public void OnRelease()
        {
            m_Shaking = false;
            transform.localPosition = m_Base;
        }

        /// <summary>자라는 연출. 마지막 프레임에서 멈춘 채 유지된다(루프 아님).</summary>
        public void PlayForm()
        {
            if (m_Animator == null) m_Animator = GetComponent<Animator>();

            StopShake();
            m_Animator.Play(m_FormState, 0, 0f);
        }

        public void BeginShake()
        {
            if (m_Shaking) return;

            m_Shaking = true;
            m_ShakeStart = Time.time;
        }

        public void PlayBreak()
        {
            if (m_Animator == null) m_Animator = GetComponent<Animator>();

            StopShake();
            m_Animator.Play(m_BreakState, 0, 0f);
        }

        private void StopShake()
        {
            m_Shaking = false;
            transform.localPosition = m_Base;
        }

        private void Update()
        {
            if (!m_Shaking) return;

            // 끝으로 갈수록 심해진다.
            float t = m_ShakeLead > 0f ? Mathf.Clamp01((Time.time - m_ShakeStart) / m_ShakeLead) : 1f;
            float amplitude = m_ShakeAmplitude * t;

            var offset = new Vector3(Random.Range(-amplitude, amplitude), Random.Range(-amplitude * 0.4f, amplitude * 0.4f), 0f);

            if (m_PixelsPerUnit > 0f)
            {
                offset.x = Mathf.Round(offset.x * m_PixelsPerUnit) / m_PixelsPerUnit;
                offset.y = Mathf.Round(offset.y * m_PixelsPerUnit) / m_PixelsPerUnit;
            }

            transform.localPosition = m_Base + offset;
        }
    }
}
