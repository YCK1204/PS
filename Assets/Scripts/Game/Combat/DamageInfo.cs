using UnityEngine;

namespace PS.Game.Combat
{
    public struct DamageInfo
    {
        public float Amount;
        public Element Element;

        /// <summary>때린 쪽. 자해·아군 판정에 쓴다.</summary>
        public GameObject Source;

        /// <summary>맞은 지점. 데미지 숫자·이펙트를 여기 띄운다.</summary>
        public Vector2 Point;

        /// <summary>맞은 쪽이 밀려날 방향. 크기가 0이면 넉백 없음.</summary>
        public Vector2 Knockback;

        public bool Critical;
    }
}
