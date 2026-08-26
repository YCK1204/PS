using UnityEngine;

namespace PS.Game
{
    /// <summary>대상을 부드럽게 따라간다. 세로는 고정 높이를 유지해서 점프에 화면이 안 흔들린다.</summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform m_Target;
        [SerializeField] private Vector2 m_Offset = new Vector2(0f, 1.2f);
        [SerializeField] private float m_Smooth = 8f;

        [Tooltip("가로 이동 한계. x가 y보다 크면 제한 없음")]
        [SerializeField] private Vector2 m_LimitX = new Vector2(-10f, 10f);

        [Tooltip("세로를 대상에 맞출지. 끄면 Offset.y 높이에 고정")]
        [SerializeField] private bool m_FollowY;

        public void SetTarget(Transform target) => m_Target = target;

        private void LateUpdate()
        {
            if (m_Target == null) return;

            float x = m_Target.position.x + m_Offset.x;
            if (m_LimitX.x < m_LimitX.y) x = Mathf.Clamp(x, m_LimitX.x, m_LimitX.y);

            float y = m_FollowY ? m_Target.position.y + m_Offset.y : m_Offset.y;

            Vector3 target = new Vector3(x, y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-m_Smooth * Time.deltaTime));
        }
    }
}
