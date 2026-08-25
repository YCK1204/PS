using PS.Game.Actors;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>대시하는 동안 잔상을 뿌린다. 시각이 아니라 <b>이동한 거리</b>로 위치를 정해서
    /// 프레임이 튀어도 간격이 무너지지 않는다. 대시가 길수록 장수가 늘어난다.</summary>
    [RequireComponent(typeof(CharacterMotor))]
    public class DashTrail : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Source;

        [Tooltip("잔상 색과 진하기. 알파가 투명도다")]
        [SerializeField] private Color m_Tint = new Color(1f, 1f, 1f, 0.45f);

        [SerializeField] private int m_MinCount = 4;
        [SerializeField] private int m_MaxCount = 8;

        [Tooltip("이 거리 이상이면 최대 장수. 그 아래는 최소~최대 사이로 비례")]
        [SerializeField] private float m_MaxCountDistance = 4f;

        [Tooltip("잔상 한 장이 사라지기까지(초)")]
        [SerializeField] private float m_GhostLife = 0.3f;

        [Tooltip("원본보다 뒤에 그린다")]
        [SerializeField] private int m_SortingOffset = -1;

        private CharacterMotor m_Motor;
        private bool m_Emitting;
        private Vector3 m_Start;
        private Vector3 m_Direction;
        private float m_Spacing;
        private int m_Count;
        private int m_Emitted;

        private void Awake()
        {
            m_Motor = GetComponent<CharacterMotor>();
            if (m_Source == null) m_Source = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnEnable() => m_Motor.Dashed += OnDashed;

        private void OnDisable()
        {
            m_Motor.Dashed -= OnDashed;
            m_Emitting = false;
        }

        private void OnDashed(float speed, float time)
        {
            if (m_Source == null) return;

            float distance = Mathf.Abs(speed) * Mathf.Max(0f, time);
            if (distance <= 0.001f) return;

            float ratio = m_MaxCountDistance > 0f ? Mathf.Clamp01(distance / m_MaxCountDistance) : 1f;
            m_Count = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(m_MinCount, m_MaxCount, ratio)),
                Mathf.Min(m_MinCount, m_MaxCount), Mathf.Max(m_MinCount, m_MaxCount));

            m_Spacing = m_Count > 1 ? distance / (m_Count - 1) : distance;
            m_Start = transform.position;
            m_Direction = new Vector3(m_Motor.Facing, 0f, 0f);
            m_Emitted = 0;
            m_Emitting = true;

            EmitNext();
        }

        private void LateUpdate()
        {
            if (!m_Emitting) return;

            Vector3 delta = transform.position - m_Start;
            if (delta.sqrMagnitude > 0.0001f) m_Direction = delta.normalized;

            int should = Mathf.Min(m_Count, Mathf.FloorToInt(delta.magnitude / m_Spacing) + 1);
            while (m_Emitted < should) EmitNext();

            if (m_Motor.IsDashing) return;

            // 대시가 끝났는데 프레임이 튀어 남았으면 나머지를 채워 찍는다.
            while (m_Emitted < m_Count) EmitNext();
            m_Emitting = false;
        }

        private void EmitNext()
        {
            Vector3 at = m_Start + m_Direction * (m_Spacing * m_Emitted);
            SpriteGhost.Spawn(m_Source, at, m_Tint, m_GhostLife, m_SortingOffset);
            m_Emitted++;
        }
    }
}
