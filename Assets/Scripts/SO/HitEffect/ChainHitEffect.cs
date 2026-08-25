using PS.Game.Combat;
using UnityEngine;

namespace SO
{
    /// <summary>맞은 지점 주변으로 한 번 더 튄다. 원래 맞은 대상은 제외한다.</summary>
    [CreateAssetMenu(menuName = "PS/HitEffect/Chain", fileName = "Hit_")]
    public class ChainHitEffect : HitEffect
    {
        private const int MaxTargets = 8;

        [SerializeField] private float m_Radius = 2.5f;

        [Tooltip("연쇄 대상 수. power에 비례해 늘어난다")]
        [SerializeField] private int m_TargetsPerPower = 1;

        [Range(0f, 1f)]
        [SerializeField] private float m_DamageRatio = 0.5f;

        private readonly Collider2D[] m_Buffer = new Collider2D[MaxTargets];

        public override void OnHit(in DamageInfo info, GameObject target, float power)
        {
            int remaining = Mathf.Max(1, Mathf.RoundToInt(m_TargetsPerPower * power));
            int count = Physics2D.OverlapCircleNonAlloc(info.Point, m_Radius, m_Buffer);

            for (int i = 0; i < count && remaining > 0; i++)
            {
                Collider2D other = m_Buffer[i];
                if (other == null || other.gameObject == target) continue;
                if (info.Source != null && other.transform.IsChildOf(info.Source.transform)) continue;

                var damageable = other.GetComponentInParent<IDamageable>();
                if (damageable == null || !damageable.IsAlive) continue;

                remaining--;

                damageable.TakeDamage(new DamageInfo
                {
                    Amount = info.Amount * m_DamageRatio,
                    Element = Element.Lightning,
                    Source = info.Source,
                    Point = other.transform.position,
                });
            }
        }
    }
}
