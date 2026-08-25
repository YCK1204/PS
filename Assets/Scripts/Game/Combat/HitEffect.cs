using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>명중 순간에 벌어지는 일. 단어가 캐릭터의 공격에 얹는다.
    /// 화상·둔화·연쇄처럼 "무엇을 하는가"만 알고 누가 붙였는지는 모른다.</summary>
    public abstract class HitEffect : ScriptableObject
    {
        /// <param name="power">강화도에서 계산된 세기.</param>
        public abstract void OnHit(in DamageInfo info, GameObject target, float power);
    }
}
