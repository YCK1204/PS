using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace PS.Core
{
    /// <summary>에셋 로드 창구. Resources와 Addressables를 한 곳에서 다룬다.
    /// Addressables 핸들은 <b>씬 범위</b>가 기본이라 씬이 바뀌면 자동으로 해제된다.
    /// 여러 씬이 공유하는 것만 Global로 올린다.</summary>
    public static class ResourceManager
    {
        private static readonly Dictionary<string, UnityEngine.Object> s_Resources
            = new Dictionary<string, UnityEngine.Object>();

        private static readonly Dictionary<string, AsyncOperationHandle> s_SceneHandles
            = new Dictionary<string, AsyncOperationHandle>();

        private static readonly Dictionary<string, AsyncOperationHandle> s_GlobalHandles
            = new Dictionary<string, AsyncOperationHandle>();

        private static bool s_Hooked;

        public static int ResourceCacheCount => s_Resources.Count;
        public static int SceneHandleCount => s_SceneHandles.Count;
        public static int GlobalHandleCount => s_GlobalHandles.Count;

        // ------------------------------------------------------------ Resources

        /// <summary>Resources 동기 로드. 같은 경로는 캐시에서 준다.</summary>
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path)) return null;

            string key = Key<T>(path);

            UnityEngine.Object cached;
            if (s_Resources.TryGetValue(key, out cached) && cached != null) return (T)cached;

            T loaded = Resources.Load<T>(path);
            if (loaded != null) s_Resources[key] = loaded;

            return loaded;
        }

        /// <summary>폴더 통째로. 캐시하지 않는다 — 보통 시작할 때 한 번만 부른다.</summary>
        public static T[] LoadAll<T>(string path) where T : UnityEngine.Object
            => string.IsNullOrEmpty(path) ? new T[0] : Resources.LoadAll<T>(path);

        /// <summary>Resources 비동기 로드.</summary>
        public static void LoadAsync<T>(string path, Action<T> onDone) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path)) { onDone?.Invoke(null); return; }

            string key = Key<T>(path);

            UnityEngine.Object cached;
            if (s_Resources.TryGetValue(key, out cached) && cached != null) { onDone?.Invoke((T)cached); return; }

            ResourceRequest request = Resources.LoadAsync<T>(path);
            request.completed += delegate
            {
                var loaded = request.asset as T;
                if (loaded != null) s_Resources[key] = loaded;
                onDone?.Invoke(loaded);
            };
        }

        /// <summary>Resources 캐시만 비운다. 참조가 없어진 에셋은 UnloadUnusedAssets가 걷어간다.</summary>
        public static void ClearResourceCache() => s_Resources.Clear();

        // ------------------------------------------------------------ Addressables

        /// <summary>Addressables 동기 로드. 내부적으로 완료를 기다린다 — 로딩 중 프레임이 멈춘다.</summary>
        public static T LoadAsset<T>(string key, LoadScope scope = LoadScope.Scene) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key)) return null;

            AsyncOperationHandle handle = Acquire<T>(key, scope);
            if (!handle.IsValid()) return null;

            if (!handle.IsDone) handle.WaitForCompletion();

            return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result as T : null;
        }

        /// <summary>Addressables 비동기 로드.</summary>
        public static void LoadAssetAsync<T>(string key, Action<T> onDone, LoadScope scope = LoadScope.Scene)
            where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key)) { onDone?.Invoke(null); return; }

            AsyncOperationHandle handle = Acquire<T>(key, scope);
            if (!handle.IsValid()) { onDone?.Invoke(null); return; }

            if (handle.IsDone)
            {
                onDone?.Invoke(handle.Status == AsyncOperationStatus.Succeeded ? handle.Result as T : null);
                return;
            }

            handle.Completed += delegate (AsyncOperationHandle h)
            {
                onDone?.Invoke(h.Status == AsyncOperationStatus.Succeeded ? h.Result as T : null);
            };
        }

        // --- 태그(라벨) 프리로드 ---

        /// <summary>라벨이 붙은 에셋을 전부 미리 로드한다.
        /// 각 에셋은 <b>자기 주소 문자열</b>을 키로 딕셔너리에 들어가고, 이후 Get으로 꺼내 쓴다.
        /// 동기라 로딩이 끝날 때까지 프레임이 멈춘다 — 씬 진입 직전에만 쓴다.</summary>
        public static int Preload(string label, LoadScope scope = LoadScope.Scene)
        {
            if (string.IsNullOrEmpty(label)) return 0;

            EnsureHook();

            AsyncOperationHandle<IList<IResourceLocation>> locations =
                Addressables.LoadResourceLocationsAsync(label);
            locations.WaitForCompletion();

            int count = 0;

            if (locations.Status == AsyncOperationStatus.Succeeded && locations.Result != null)
            {
                foreach (IResourceLocation location in locations.Result)
                {
                    if (Register(location, scope)) count++;
                }
            }

            Addressables.Release(locations);
            return count;
        }

        /// <summary>라벨 프리로드 비동기판. onProgress는 0~1, onDone은 로드한 개수.</summary>
        public static void PreloadAsync(string label, Action<int> onDone = null,
            Action<float> onProgress = null, LoadScope scope = LoadScope.Scene)
        {
            if (string.IsNullOrEmpty(label)) { onDone?.Invoke(0); return; }

            EnsureHook();

            AsyncOperationHandle<IList<IResourceLocation>> locations =
                Addressables.LoadResourceLocationsAsync(label);

            locations.Completed += delegate (AsyncOperationHandle<IList<IResourceLocation>> lh)
            {
                if (lh.Status != AsyncOperationStatus.Succeeded || lh.Result == null || lh.Result.Count == 0)
                {
                    Addressables.Release(lh);
                    onProgress?.Invoke(1f);
                    onDone?.Invoke(0);
                    return;
                }

                int total = lh.Result.Count;
                int finished = 0;
                int loaded = 0;

                foreach (IResourceLocation location in lh.Result)
                {
                    AsyncOperationHandle handle = Acquire(location, scope, out bool fresh);
                    if (fresh) loaded++;

                    if (handle.IsDone)
                    {
                        finished++;
                        onProgress?.Invoke(finished / (float)total);
                        continue;
                    }

                    handle.Completed += delegate
                    {
                        finished++;
                        onProgress?.Invoke(finished / (float)total);
                        if (finished >= total) onDone?.Invoke(loaded);
                    };
                }

                Addressables.Release(lh);

                if (finished >= total) onDone?.Invoke(loaded);
            };
        }

        // --- 키로 꺼내 쓰기 ---

        /// <summary>이미 로드된 것만 준다. 없으면 null — 여기서 로드하지 않는다.</summary>
        public static T Get<T>(string key) where T : UnityEngine.Object
        {
            T asset;
            return TryGet(key, out asset) ? asset : null;
        }

        public static bool TryGet<T>(string key, out T asset) where T : UnityEngine.Object
        {
            asset = null;
            if (string.IsNullOrEmpty(key)) return false;

            AsyncOperationHandle handle;
            if (!s_SceneHandles.TryGetValue(key, out handle) && !s_GlobalHandles.TryGetValue(key, out handle))
                return false;

            if (!handle.IsValid() || !handle.IsDone || handle.Status != AsyncOperationStatus.Succeeded) return false;

            asset = handle.Result as T;
            return asset != null;
        }

        /// <summary>지금 등록돼 있는 키 목록. 디버그·검증용.</summary>
        public static ICollection<string> LoadedKeys(LoadScope scope) => Table(scope).Keys;

        /// <summary>이 키가 이미 등록돼 있는가.</summary>
        public static bool IsRegistered(string key, LoadScope scope = LoadScope.Scene)
            => !string.IsNullOrEmpty(key) && Table(scope).ContainsKey(key);

        /// <summary>키 하나만 해제. 어느 범위에 있든 찾아서 뗀다.</summary>
        public static void Release(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (!ReleaseFrom(s_SceneHandles, key)) ReleaseFrom(s_GlobalHandles, key);
        }

        /// <summary>씬 범위 핸들 전부 해제. 씬 전환 시 자동으로 불린다.</summary>
        public static void ReleaseScene() => ReleaseAll(s_SceneHandles);

        public static void ReleaseGlobal() => ReleaseAll(s_GlobalHandles);

        public static void ReleaseEverything()
        {
            ReleaseScene();
            ReleaseGlobal();
            ClearResourceCache();
        }

        // ------------------------------------------------------------ 내부

        private static Dictionary<string, AsyncOperationHandle> Table(LoadScope scope)
            => scope == LoadScope.Global ? s_GlobalHandles : s_SceneHandles;

        private static AsyncOperationHandle Acquire<T>(string key, LoadScope scope) where T : UnityEngine.Object
        {
            EnsureHook();

            // 이미 다른 범위에 있으면 그걸 쓴다. 같은 에셋을 두 번 잡지 않게.
            AsyncOperationHandle existing;
            if (s_GlobalHandles.TryGetValue(key, out existing) && existing.IsValid()) return existing;
            if (s_SceneHandles.TryGetValue(key, out existing) && existing.IsValid()) return existing;

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            Table(scope)[key] = handle;

            return handle;
        }

        /// <summary>위치 하나를 주소 키로 등록한다. 이미 있으면 건너뛴다.</summary>
        private static bool Register(IResourceLocation location, LoadScope scope)
        {
            bool fresh;
            AsyncOperationHandle handle = Acquire(location, scope, out fresh);

            if (!handle.IsDone) handle.WaitForCompletion();
            return fresh;
        }

        private static AsyncOperationHandle Acquire(IResourceLocation location, LoadScope scope, out bool fresh)
        {
            fresh = false;
            string key = location.PrimaryKey;

            AsyncOperationHandle existing;
            if (s_GlobalHandles.TryGetValue(key, out existing) && existing.IsValid()) return existing;
            if (s_SceneHandles.TryGetValue(key, out existing) && existing.IsValid()) return existing;

            AsyncOperationHandle<UnityEngine.Object> handle =
                Addressables.LoadAssetAsync<UnityEngine.Object>(location);

            Table(scope)[key] = handle;
            fresh = true;

            return handle;
        }

        private static bool ReleaseFrom(Dictionary<string, AsyncOperationHandle> table, string key)
        {
            AsyncOperationHandle handle;
            if (!table.TryGetValue(key, out handle)) return false;

            if (handle.IsValid()) Addressables.Release(handle);
            table.Remove(key);

            return true;
        }

        private static void ReleaseAll(Dictionary<string, AsyncOperationHandle> table)
        {
            foreach (KeyValuePair<string, AsyncOperationHandle> pair in table)
                if (pair.Value.IsValid()) Addressables.Release(pair.Value);

            table.Clear();
        }

        private static string Key<T>(string path) => typeof(T).Name + ":" + path;

        private static void EnsureHook()
        {
            if (s_Hooked) return;

            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
            s_Hooked = true;
        }

        private static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene) => ReleaseScene();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_Resources.Clear();
            s_SceneHandles.Clear();
            s_GlobalHandles.Clear();
            s_Hooked = false;
        }
    }
}
