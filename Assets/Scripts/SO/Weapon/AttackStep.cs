using System;
using UnityEngine;

namespace SO
{
    /// <summary>연타 1단계. 애니메이션 상태 이름과 타이밍만 갖는다.
    /// 클립 길이에 의존하지 않게 시간을 데이터로 박는다.</summary>
    [Serializable]
    public struct AttackStep
    {
        [Tooltip("Animator의 상태 이름. Attack1 / Attack2 / Attack3")]
        public string AnimationState;

        [Tooltip("이 단계 전체 길이(초). 공속으로 나눠진다")]
        public float Duration;

        [Tooltip("타격 백업 시한(초). 애니메이션 이벤트가 먼저 오면 그쪽이 이기고, 안 오면 이 시각에 친다.\n이벤트 시점보다 살짝 뒤로 잡는다")]
        public float HitTime;

        [Tooltip("공격력 배수. 마무리 타를 세게 하려면 여기서 올린다")]
        public float DamageScale;

        [Tooltip("다음 단계로 이어갈 수 있는 시점(초). 0이면 HitTime을 쓴다")]
        public float ComboOpenTime;

        [Tooltip("이 단계에서 앞으로 밀고 나가는 속도. 0이면 제자리")]
        public float LungeSpeed;

        [Tooltip("밀고 나가는 시간(초)")]
        public float LungeTime;
    }
}
