using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>맞으면 밀려난다.
    /// Rigidbody2D가 있으면 속도로 밀고, 없으면 위치를 밀었다가 제자리로 돌아온다 — 허수아비처럼.</summary>
    [RequireComponent(typeof(Health))]
    public class KnockbackReceiver : MonoBehaviour
    {
        [Tooltip("받는 힘의 배수. 0이면 안 밀린다")]
        [SerializeField] private float m_Scale = 1f;

        [Tooltip("위로 살짝 띄우는 정도. 리지드바디가 있을 때만")]
        [SerializeField] private float m_Lift;

        [Tooltip("리지드바디가 없을 때 밀리는 거리(유닛). 힘 1당")]
        [SerializeField] private float m_OffsetPerForce = 0.05f;

        [Tooltip("리지드바디가 없을 때 최대로 밀리는 거리")]
        [SerializeField] private float m_MaxOffset = 0.5f;

        [Tooltip("제자리로 돌아오는 속도")]
        [SerializeField] private float m_Recover = 9f;

        private Health m_Health;
        private Rigidbody2D m_Body;
        private Vector3 m_Offset;

        private void Awake()
        {
            m_Health = GetComponent<Health>();
            m_Body = GetComponent<Rigidbody2D>();
        }

        private void OnEnable() => m_Health.Damaged += OnDamaged;
        private void OnDisable() => m_Health.Damaged -= OnDamaged;

        private void OnDamaged(DamageInfo info)
        {
            if (m_Scale <= 0f || info.Knockback.sqrMagnitude <= 0.0001f) return;

            Vector2 push = info.Knockback * m_Scale;

            if (m_Body != null)
            {
                push.y += m_Lift;
                m_Body.linearVelocity = new Vector2(push.x, m_Body.linearVelocity.y + push.y);
                return;
            }

            Vector3 shift = (Vector3)(push * m_OffsetPerForce);
            Vector3 next = Vector3.ClampMagnitude(m_Offset + shift, m_MaxOffset);

            transform.position += next - m_Offset;
            m_Offset = next;
        }

        private void LateUpdate()
        {
            if (m_Body != null || m_Offset.sqrMagnitude < 0.000001f) return;

            Vector3 next = Vector3.Lerp(m_Offset, Vector3.zero, 1f - Mathf.Exp(-m_Recover * Time.deltaTime));
            transform.position += next - m_Offset;
            m_Offset = next;
        }
    }
}
