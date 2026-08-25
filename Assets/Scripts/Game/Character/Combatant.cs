using PS.Game.Combat;
using UnityEngine;

namespace PS.Game.Actors
{
    /// <summary>싸울 수 있는 액터. 부품은 있으면 쓰고 없으면 넘어간다 —
    /// 허수아비는 Motor도 Combat도 없지만 맞고 죽는 건 같다.</summary>
    public abstract class Combatant : Actor
    {
        public StatBlock Stats { get; } = new StatBlock();

        public Health Health { get; private set; }
        public CharacterMotor Motor { get; private set; }
        public CharacterCombat Combat { get; private set; }
        public CharacterAnimator Anim { get; private set; }

        public override bool IsAlive => Health == null || Health.IsAlive;

        protected virtual void Awake()
        {
            Health = GetComponent<Health>();
            Motor = GetComponent<CharacterMotor>();
            Combat = GetComponent<CharacterCombat>();
            Anim = GetComponentInChildren<CharacterAnimator>();

            Configure();

            Combat?.Bind(Stats, Anim, Motor);
            Health?.Bind(Stats);
        }

        /// <summary>스탯 기본값과 장비를 여기서 채운다. Health.Bind 전에 불린다.</summary>
        protected virtual void Configure() { }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Health == null) return;
            Health.Damaged += OnDamaged;
            Health.Died += OnDied;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Health == null) return;
            Health.Damaged -= OnDamaged;
            Health.Died -= OnDied;
        }

        protected virtual void OnDamaged(DamageInfo info)
        {
            if (!IsAlive) return;
            Anim?.Play(CharacterAnimator.Hurt, 0.2f);
        }

        protected virtual void OnDied(DamageInfo info)
        {
            Combat?.CancelAttack();
            Motor?.Stop();
            Anim?.Play(CharacterAnimator.Dead, float.MaxValue);
        }

        public virtual void Respawn(Vector3 position)
        {
            transform.position = position;
            Health?.Revive();
            Anim?.Unlock();
            Anim?.Play(CharacterAnimator.Idle);
        }
    }
}
