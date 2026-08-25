using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(menuName = "PS/HitEffect/Burn", fileName = "Hit_")]
    public class BurnHitEffect : HitEffect
    {
        [SerializeField] private float m_Duration = 3f;

        [Tooltip("power 1당 초당 피해")]
        [SerializeField] private float m_DamagePerPower = 2f;

        public override void OnHit(in DamageInfo info, GameObject target, float power)
            => Status.Attach<BurnStatus>(target, m_Duration, power * m_DamagePerPower);
    }
}
