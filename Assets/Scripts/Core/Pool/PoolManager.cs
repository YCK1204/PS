using System.Collections.Generic;
using UnityEngine;

namespace PS.Core
{
    /// <summary>프리팹별 풀을 모아 둔다. 짧은 수명 · 고빈도 객체는 여기를 거친다.
    /// 기본은 <b>씬 범위</b> — 씬이 바뀌면 풀도 인스턴스도 통째로 사라진다.
    /// 씬을 넘어 살아남아야 하는 것만 Global로 만든다.</summary>
    public static class PoolManager
    {
        private static readonly Dictionary<int, object> s_ScenePools = new Dictionary<int, object>();
        private static readonly Dictionary<int, object> s_GlobalPools = new Dictionary<int, object>();

        private static Transform s_SceneRoot;
        private static Transform s_GlobalRoot;
        private static bool s_Hooked;

        public static int ScenePoolCount => s_ScenePools.Count;
        public static int GlobalPoolCount => s_GlobalPools.Count;
        public static int PoolCount => s_ScenePools.Count + s_GlobalPools.Count;

        // --- 꺼내기 ---

        public static T Get<T>(T prefab, Vector3 position, Quaternion rotation,
            Transform parent = null, PoolScope scope = PoolScope.Scene) where T : Component
        {
            if (prefab == null) return null;
            return PoolOf(prefab, scope).Get(position, rotation, parent);
        }

        public static T Get<T>(T prefab, Transform parent = null, PoolScope scope = PoolScope.Scene)
            where T : Component
            => Get(prefab, Vector3.zero, Quaternion.identity, parent, scope);

        // --- 반납 ---

        /// <summary>풀에서 나온 게 아니면 그냥 파괴한다. 호출부가 출처를 몰라도 되게.</summary>
        public static void Release<T>(T instance) where T : Component
        {
            if (instance == null) return;

            var marker = instance.GetComponent<PooledInstance>();
            if (marker == null)
            {
                Object.Destroy(instance.gameObject);
                return;
            }

            Dictionary<int, object> table = marker.Scope == PoolScope.Global ? s_GlobalPools : s_ScenePools;

            object pool;
            if (!table.TryGetValue(marker.Key, out pool))
            {
                // 씬이 바뀌며 풀이 사라진 뒤 뒤늦게 반납된 경우.
                Object.Destroy(instance.gameObject);
                return;
            }

            ((Pool<T>)pool).Release(instance);
        }

        public static void Prewarm<T>(T prefab, int count, PoolScope scope = PoolScope.Scene) where T : Component
        {
            if (prefab == null || count <= 0) return;
            PoolOf(prefab, scope).Prewarm(count);
        }

        // --- 조회 ---

        public static int IdleCount<T>(T prefab, PoolScope scope = PoolScope.Scene) where T : Component
        {
            if (prefab == null) return 0;

            Dictionary<int, object> table = scope == PoolScope.Global ? s_GlobalPools : s_ScenePools;

            object pool;
            return table.TryGetValue(prefab.GetInstanceID(), out pool) ? ((Pool<T>)pool).IdleCount : 0;
        }

        public static int CreatedCount<T>(T prefab, PoolScope scope = PoolScope.Scene) where T : Component
        {
            if (prefab == null) return 0;

            Dictionary<int, object> table = scope == PoolScope.Global ? s_GlobalPools : s_ScenePools;

            object pool;
            return table.TryGetValue(prefab.GetInstanceID(), out pool) ? ((Pool<T>)pool).Created : 0;
        }

        // --- 비우기 ---

        /// <summary>씬 범위 풀만 비운다. 씬 전환 시 자동으로 불린다.</summary>
        public static void ClearScene()
        {
            Clear(s_ScenePools);

            if (s_SceneRoot != null) Object.Destroy(s_SceneRoot.gameObject);
            s_SceneRoot = null;
        }

        public static void ClearGlobal()
        {
            Clear(s_GlobalPools);

            if (s_GlobalRoot != null) Object.Destroy(s_GlobalRoot.gameObject);
            s_GlobalRoot = null;
        }

        public static void ClearAll()
        {
            ClearScene();
            ClearGlobal();
        }

        private static void Clear(Dictionary<int, object> table)
        {
            foreach (KeyValuePair<int, object> pair in table)
            {
                var clearable = pair.Value as IPoolClear;
                clearable?.Clear();
            }

            table.Clear();
        }

        // --- 내부 ---

        private static Pool<T> PoolOf<T>(T prefab, PoolScope scope) where T : Component
        {
            EnsureHook();

            Dictionary<int, object> table = scope == PoolScope.Global ? s_GlobalPools : s_ScenePools;
            int key = prefab.GetInstanceID();

            object existing;
            if (table.TryGetValue(key, out existing)) return (Pool<T>)existing;

            var pool = new Pool<T>(prefab, RootOf(scope), key, scope);
            table[key] = pool;
            return pool;
        }

        private static Transform RootOf(PoolScope scope)
        {
            if (scope == PoolScope.Global)
            {
                if (s_GlobalRoot == null)
                {
                    var go = new GameObject("[Pools.Global]");
                    Object.DontDestroyOnLoad(go);
                    s_GlobalRoot = go.transform;
                }

                return s_GlobalRoot;
            }

            if (s_SceneRoot == null)
            {
                // 씬에 그대로 둔다 — 씬과 함께 사라지는 게 의도.
                var go = new GameObject("[Pools.Scene]");
                s_SceneRoot = go.transform;
            }

            return s_SceneRoot;
        }

        private static void EnsureHook()
        {
            if (s_Hooked) return;

            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
            s_Hooked = true;
        }

        /// <summary>씬이 내려가면 씬 범위 풀은 통째로 버린다.
        /// 인스턴스는 씬과 함께 이미 파괴되므로 장부만 정리하면 된다.</summary>
        private static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            s_ScenePools.Clear();
            s_SceneRoot = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_ScenePools.Clear();
            s_GlobalPools.Clear();
            s_SceneRoot = null;
            s_GlobalRoot = null;
            s_Hooked = false;
        }
    }

    /// <summary>제네릭 풀을 타입 없이 비우기 위한 통로.</summary>
    internal interface IPoolClear
    {
        void Clear();
    }
}
