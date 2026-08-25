using System.Collections.Generic;
using PS.Game.Actors;
using SO;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>언제 때릴지를 정한다. 어떻게 때리는지는 Weapon이 안다.
    /// 단어가 붙인 속성·명중 효과도 여기 쌓인다.</summary>
    public class CharacterCombat : MonoBehaviour
    {
        /// <summary>단어가 공격에 얹은 것 하나. 출처별로 통째로 뗀다.</summary>
        private struct Brand
        {
            public object Source;
            public Element Element;
            public HitEffect Effect;
            public float Power;
        }

        [SerializeField] private LayerMask m_TargetMask;

        [Tooltip("무기가 붙을 자리. 비우면 자기 자신")]
        [SerializeField] private Transform m_WeaponMount;

        private readonly List<Brand> m_Brands = new List<Brand>();

        private CharacterAnimator m_Anim;
        private CharacterMotor m_Motor;
        private Weapon m_Weapon;

        private int m_Step;
        private float m_StepEnd;
        private float m_HitAt;
        private float m_ComboOpenAt;
        private float m_ComboExpire;
        private bool m_Struck;

        /// <summary>한 타를 실제로 휘두른 순간. 인자는 바라보는 방향.
        /// 단어 효과가 여기 붙어서 타격마다 뭔가를 뿜는다.</summary>
        public event System.Action<int> Struck;

        public StatBlock Stats { get; private set; }
        public LayerMask TargetMask => m_TargetMask;
        public Weapon Weapon => m_Weapon;
        public int Facing { get; set; } = 1;
        public bool IsAttacking => Time.time < m_StepEnd;

        /// <summary>지금 공격에 실릴 속성. 가장 마지막에 붙은 게 이긴다.</summary>
        public Element Element => m_Brands.Count > 0 ? m_Brands[m_Brands.Count - 1].Element : Element.None;

        public void Bind(StatBlock stats, CharacterAnimator anim, CharacterMotor motor = null)
        {
            Stats = stats;
            m_Anim = anim;
            m_Motor = motor;
        }

        public void Equip(WeaponData data)
        {
            if (m_Weapon != null) Destroy(m_Weapon.gameObject);
            m_Weapon = null;

            if (data == null) return;

            m_Weapon = data.Spawn(m_WeaponMount != null ? m_WeaponMount : transform);
            m_Weapon.Bind(this);

            m_Step = 0;
        }

        public bool TryAttack()
        {
            if (m_Weapon == null || m_Weapon.Data == null) return false;
            if (m_Weapon.Data.StepCount == 0) return false;

            if (IsAttacking)
            {
                // 아직 휘두르는 중 — 콤보 창이 열렸으면 다음 단계로 끊고 들어간다.
                if (Time.time < m_ComboOpenAt) return false;
                Advance();
            }
            else if (Time.time <= m_ComboExpire)
            {
                // 이전 타가 끝났지만 아직 이어갈 수 있는 시간.
                Advance();
            }
            else
            {
                m_Step = 0;
            }

            Begin(m_Weapon.Data.StepAt(m_Step));
            return true;
        }

        public void CancelAttack()
        {
            m_Motor?.CancelLunge();
            m_StepEnd = 0f;
            m_Struck = true;
            m_Step = 0;
            m_Anim?.Unlock();
        }

        private void Advance()
        {
            m_Step++;
            if (m_Step >= m_Weapon.Data.StepCount) m_Step = 0;
        }

        private void Begin(in AttackStep step)
        {
            float speed = Mathf.Max(0.1f, Stats != null ? Stats.Get(StatType.AttackSpeed) : 1f);
            float duration = Mathf.Max(0.01f, step.Duration) / speed;
            float hit = Mathf.Clamp(step.HitTime, 0f, step.Duration) / speed;
            float open = (step.ComboOpenTime > 0f ? step.ComboOpenTime : step.HitTime) / speed;

            m_StepEnd = Time.time + duration;
            m_HitAt = Time.time + hit;
            m_ComboOpenAt = Time.time + open;
            m_ComboExpire = m_StepEnd + m_Weapon.Data.ComboResetTime;
            m_Struck = false;

            m_Anim?.Play(step.AnimationState, duration, speed);
            m_Motor?.Lunge(step.LungeSpeed, step.LungeTime / speed);
        }

        /// <summary>타격 백업. 애니메이션 이벤트가 먼저 오면 m_Struck이 서서 여기는 건너뛴다.
        /// 프레임이 크게 튀면 이벤트가 통째로 스킵될 수 있어 안전망이 필요하다.</summary>
        private void Update()
        {
            if (m_Struck || m_Weapon == null || m_Weapon.Data == null) return;
            if (Time.time < m_HitAt) return;

            Strike();
        }

        /// <summary>애니메이션 이벤트가 부르는 지점. 클립의 검격 프레임에 걸어둔다.
        /// Animator와 같은 오브젝트에 있어야 호출된다.</summary>
        public void AnimationHit()
        {
            if (!IsAttacking || m_Struck) return;
            Strike();
        }

        private void Strike()
        {
            if (m_Weapon == null || m_Weapon.Data == null) return;

            m_Struck = true;
            m_Weapon.Strike(m_Weapon.Data.StepAt(m_Step), Facing);
            Struck?.Invoke(Facing);
        }

        public float DamageOf(in AttackStep step)
        {
            bool ignored;
            return DamageOf(step, out ignored);
        }

        /// <summary>한 타의 피해. 치명타는 여기서 한 번만 굴린다 —
        /// 같은 스윙에 여럿이 맞아도 판정은 하나여야 한다.</summary>
        public float DamageOf(in AttackStep step, out bool critical)
        {
            float weapon = m_Weapon != null && m_Weapon.Data != null ? m_Weapon.Data.BaseDamage : 0f;
            float bonus = Stats != null ? Stats.Get(StatType.Attack) : 0f;
            float scale = step.DamageScale > 0f ? step.DamageScale : 1f;
            float damage = (weapon + bonus) * scale;

            float chance = Stats != null ? Mathf.Clamp01(Stats.Get(StatType.CritChance)) : 0f;
            critical = chance > 0f && Random.value < chance;

            if (critical)
            {
                float multiplier = Stats != null ? Stats.Get(StatType.CritMultiplier) : 1f;
                damage *= Mathf.Max(1f, multiplier);
            }

            return damage;
        }

        public void ApplyHitEffects(in DamageInfo info, GameObject target)
        {
            if (target == null) return;

            for (int i = 0; i < m_Brands.Count; i++)
                m_Brands[i].Effect?.OnHit(info, target, m_Brands[i].Power);
        }

        /// <summary>단어가 공격에 속성·효과를 붙인다.</summary>
        public void AddBrand(object source, Element element, HitEffect effect, float power)
        {
            if (source == null) return;

            m_Brands.Add(new Brand { Source = source, Element = element, Effect = effect, Power = power });
        }

        public bool RemoveBrands(object source)
        {
            if (source == null) return false;
            return m_Brands.RemoveAll(b => ReferenceEquals(b.Source, source)) > 0;
        }
    }
}
