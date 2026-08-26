using PS.Core;
using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>맞은 대상을 얼린다. 이미 얼어 있으면 아무 일도 없다 — 중복 결빙 금지.</summary>
    [CreateAssetMenu(menuName = "PS/HitEffect/Freeze", fileName = "Hit_")]
    public class FreezeHitEffect : HitEffect
    {
        [SerializeField] private float m_Duration = 1.4f;

        [Tooltip("세기 1당 늘어나는 시간(초)")]
        [SerializeField] private float m_DurationPerPower = 0.25f;

        [SerializeField] private FreezeVisual m_Visual;

        [Tooltip("얼음이 생길 위치. 대상 발밑 기준")]
        [SerializeField] private Vector3 m_Offset;

        public override void OnHit(in DamageInfo info, GameObject target, float power)
        {
            if (target == null) return;

            var health = target.GetComponentInParent<Health>();
            if (health == null || !health.IsAlive) return;

            GameObject root = health.gameObject;
            if (root.GetComponent<FreezeStatus>() != null) return;

            var status = root.AddComponent<FreezeStatus>();
            status.Refresh(m_Duration + m_DurationPerPower * power, power);

            if (m_Visual != null)
            {
                FreezeVisual visual = PoolManager.Get(m_Visual, root.transform.position + m_Offset, Quaternion.identity, root.transform);
                status.Bind(visual);
            }
        }
    }
}
