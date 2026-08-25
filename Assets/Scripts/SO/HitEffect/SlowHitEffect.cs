using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(menuName = "PS/HitEffect/Slow", fileName = "Hit_")]
    public class SlowHitEffect : HitEffect
    {
        [SerializeField] private float m_Duration = 2f;

        [Tooltip("power 1당 이속 감소 비율. 0.15면 15%")]
        [Range(0f, 1f)]
        [SerializeField] private float m_SlowPerPower = 0.15f;

        public override void OnHit(in DamageInfo info, GameObject target, float power)
            => Status.Attach<SlowStatus>(target, m_Duration, Mathf.Clamp01(power * m_SlowPerPower));
    }
}
