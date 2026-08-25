using SO;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>장비의 런타임 몸통. 언제 때릴지는 CharacterCombat이 정하고,
    /// 어떻게 때릴지는 이 클래스가 정한다.</summary>
    public abstract class Weapon : MonoBehaviour
    {
        public WeaponData Data { get; private set; }
        protected CharacterCombat Owner { get; private set; }

        public virtual void Setup(WeaponData data) => Data = data;

        public void Bind(CharacterCombat owner) => Owner = owner;

        /// <summary>한 단계의 피해가 실제로 발생하는 순간.</summary>
        /// <param name="facing">바라보는 방향. +1 오른쪽, -1 왼쪽</param>
        public abstract void Strike(in AttackStep step, int facing);

        /// <summary>맞은 대상에게 피해와 부착 효과를 전달한다.</summary>
        protected void Hit(Component target, in DamageInfo info)
        {
            if (target == null) return;

            var damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null || !damageable.IsAlive) return;

            damageable.TakeDamage(info);
            Owner?.ApplyHitEffects(info, target.gameObject);
        }
    }
}
