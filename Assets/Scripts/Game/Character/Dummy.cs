using PS.Game.Combat;
using UnityEngine;

namespace PS.Game.Actors
{
    /// <summary>허수아비. 패턴도 공격도 없고 맞고 죽고 다시 선다.
    /// 액션 없이 정비 루프를 한 바퀴 돌리기 위한 최소 장치.</summary>
    [RequireComponent(typeof(Health))]
    public class Dummy : Combatant
    {
        [SerializeField] private float m_RespawnDelay = 2f;

        [Tooltip("죽었을 때 끌 것. 비우면 자기 렌더러 전부")]
        [SerializeField] private GameObject m_Visual;

        [SerializeField] private Collider2D m_Body;

        private float m_ReviveAt = float.PositiveInfinity;

        protected override void Awake()
        {
            base.Awake();
            if (m_Body == null) m_Body = GetComponent<Collider2D>();
        }

        protected override void OnDied(DamageInfo info)
        {
            base.OnDied(info);

            SetVisible(false);
            m_ReviveAt = m_RespawnDelay > 0f ? Time.time + m_RespawnDelay : float.PositiveInfinity;
        }

        private void Update()
        {
            // 피격 모션이 끝나면 대기로 돌아간다. 안 그러면 마지막 프레임에서 멈춘다.
            if (IsAlive && Anim != null && !Anim.IsLocked) Anim.Play(CharacterAnimator.Idle);

            if (Time.time < m_ReviveAt) return;

            m_ReviveAt = float.PositiveInfinity;
            Respawn(transform.position);
        }

        public override void Respawn(Vector3 position)
        {
            base.Respawn(position);
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            if (m_Visual != null)
            {
                m_Visual.SetActive(visible);
            }
            else
            {
                Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = visible;
            }

            if (m_Body != null) m_Body.enabled = visible;
        }
    }
}
