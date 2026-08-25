using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>대상에 붙는 지속 상태. 같은 종류가 다시 걸리면 갱신만 한다.</summary>
    public abstract class Status : MonoBehaviour
    {
        protected float m_Expire;
        protected float m_Power;

        public virtual void Refresh(float duration, float power)
        {
            m_Expire = Mathf.Max(m_Expire, Time.time + duration);
            m_Power = Mathf.Max(m_Power, power);
        }

        protected virtual void Update()
        {
            if (Time.time >= m_Expire) Destroy(this);
        }

        /// <summary>이미 붙어 있으면 갱신, 없으면 새로 붙인다.</summary>
        public static T Attach<T>(GameObject target, float duration, float power) where T : Status
        {
            if (target == null) return null;

            T status = target.GetComponent<T>();
            if (status == null) status = target.AddComponent<T>();

            status.Refresh(duration, power);
            return status;
        }
    }
}
