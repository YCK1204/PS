using DamageNumbersPro;
using PS.Core;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>맞을 때 피해 숫자를 띄운다. 설정에서 끄면 안 뜬다.</summary>
    [RequireComponent(typeof(Health))]
    public class DamagePopup : MonoBehaviour
    {
        [SerializeField] private DamageNumber m_Normal;
        [SerializeField] private DamageNumber m_Critical;

        [Tooltip("숫자가 뜰 위치. 비우면 자기 트랜스폼 + 오프셋")]
        [SerializeField] private Transform m_Anchor;

        [SerializeField] private Vector3 m_Offset = new Vector3(0f, 1.2f, 0f);

        [Tooltip("맞은 지점에 띄울지. 끄면 항상 Anchor 위")]
        [SerializeField] private bool m_UseHitPoint = true;

        private Health m_Health;

        private void Awake() => m_Health = GetComponent<Health>();

        private void OnEnable() => m_Health.Damaged += OnDamaged;
        private void OnDisable() => m_Health.Damaged -= OnDamaged;

        private void OnDamaged(DamageInfo info)
        {
            if (!GameSettings.DamageNumbers) return;

            DamageNumber prefab = info.Critical && m_Critical != null ? m_Critical : m_Normal;
            if (prefab == null) return;

            Vector3 at = m_UseHitPoint && info.Point != Vector2.zero
                ? (Vector3)info.Point
                : (m_Anchor != null ? m_Anchor.position : transform.position + m_Offset);

            prefab.Spawn(at, Mathf.Round(info.Amount));
        }
    }
}
