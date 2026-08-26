using System.Collections.Generic;
using UnityEngine;

namespace PS.Core
{
    /// <summary>프리팹 하나짜리 풀. 꺼낼 게 없으면 새로 만들고, 반납하면 꺼둔 채 보관한다.</summary>
    public class Pool<T> : IPoolClear where T : Component
    {
        private readonly T m_Prefab;
        private readonly Transform m_Root;
        private readonly int m_Key;
        private readonly PoolScope m_Scope;
        private readonly Stack<T> m_Idle = new Stack<T>();

        private int m_Created;

        public int IdleCount => m_Idle.Count;
        public int Created => m_Created;
        public int LiveCount => m_Created - m_Idle.Count;

        public PoolScope Scope => m_Scope;

        public Pool(T prefab, Transform root, int key, PoolScope scope)
        {
            m_Prefab = prefab;
            m_Root = root;
            m_Key = key;
            m_Scope = scope;
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = Create();
                Park(instance);
                m_Idle.Push(instance);
            }
        }

        public T Get(Vector3 position, Quaternion rotation, Transform parent)
        {
            T instance = null;

            // 씬이 내려가며 죽은 것이 섞여 있을 수 있다.
            while (m_Idle.Count > 0 && instance == null) instance = m_Idle.Pop();
            if (instance == null) instance = Create();

            Transform tr = instance.transform;
            tr.SetParent(parent, false);
            tr.SetPositionAndRotation(position, rotation);

            var marker = instance.GetComponent<PooledInstance>();
            if (marker != null) marker.Released = false;

            instance.gameObject.SetActive(true);

            var poolable = instance as IPoolable;
            if (poolable == null) poolable = instance.GetComponent<IPoolable>();
            poolable?.OnGet();

            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null) return;

            var marker = instance.GetComponent<PooledInstance>();
            if (marker != null)
            {
                if (marker.Released) return;
                marker.Released = true;
            }

            var poolable = instance as IPoolable;
            if (poolable == null) poolable = instance.GetComponent<IPoolable>();
            poolable?.OnRelease();

            Park(instance);
            m_Idle.Push(instance);
        }

        public void Clear()
        {
            while (m_Idle.Count > 0)
            {
                T instance = m_Idle.Pop();
                if (instance != null) Object.Destroy(instance.gameObject);
            }

            m_Created = 0;
        }

        private T Create()
        {
            T instance = Object.Instantiate(m_Prefab);
            instance.name = m_Prefab.name;

            var marker = instance.GetComponent<PooledInstance>();
            if (marker == null) marker = instance.gameObject.AddComponent<PooledInstance>();
            marker.Key = m_Key;
            marker.Scope = m_Scope;

            m_Created++;
            return instance;
        }

        private void Park(T instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(m_Root, false);
        }
    }
}
