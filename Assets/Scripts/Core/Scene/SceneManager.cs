using System;
using System.Collections;
using UnityEngine;

namespace PS.Core
{
    /// <summary>씬 전환과 현재 씬 보유. 동기·비동기 둘 다 지원한다.
    /// 비동기는 진행도와 시작·종료 신호만 내보내고, 무엇을 보여줄지는 UI가 정한다.</summary>
    public static class SceneManager
    {
        public const string TitleSceneName = "TitleScene";
        public const string BattleSceneName = "BattleScene";

        private static Runner s_Runner;

        /// <summary>지금 씬의 진입점. BaseScene이 Start에서 스스로 등록한다.</summary>
        public static BaseScene Current { get; private set; }

        public static SceneType CurrentType => Current != null ? Current.Type : SceneType.None;

        public static bool IsLoading { get; private set; }

        /// <summary>0~1. 비동기 전환 중에만 의미가 있다.</summary>
        public static float Progress { get; private set; }

        /// <summary>비동기 전환 시작. 세이브 아이콘 같은 표시를 켤 때 쓴다.</summary>
        public static event Action LoadStarted;

        /// <summary>비동기 전환 끝. 표시를 끌 때.</summary>
        public static event Action LoadFinished;

        /// <summary>새 씬의 BaseScene이 등록을 마친 시점.</summary>
        public static event Action<BaseScene> SceneReady;

        public static string NameOf(SceneType type)
        {
            switch (type)
            {
                case SceneType.Title: return TitleSceneName;
                case SceneType.Battle: return BattleSceneName;
                default: return null;
            }
        }

        internal static void Register(BaseScene scene)
        {
            if (scene == null) return;

            Current = scene;
            SceneReady?.Invoke(scene);
        }

        internal static void Unregister(BaseScene scene)
        {
            if (Current == scene) Current = null;
        }

        // --- 동기 ---

        public static void Load(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || IsLoading) return;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        public static void Load(SceneType type) => Load(NameOf(type));

        // --- 비동기 ---

        public static void LoadAsync(string sceneName, Action onDone = null)
        {
            if (string.IsNullOrEmpty(sceneName) || IsLoading) return;

            EnsureRunner();
            s_Runner.StartCoroutine(LoadRoutine(sceneName, onDone));
        }

        public static void LoadAsync(SceneType type, Action onDone = null) => LoadAsync(NameOf(type), onDone);

        private static IEnumerator LoadRoutine(string sceneName, Action onDone)
        {
            IsLoading = true;
            Progress = 0f;
            LoadStarted?.Invoke();

            AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                IsLoading = false;
                LoadFinished?.Invoke();
                yield break;
            }

            while (!op.isDone)
            {
                // 유니티는 0.9에서 멈춘 뒤 활성화한다. 보이는 값은 0~1로 편다.
                Progress = Mathf.Clamp01(op.progress / 0.9f);
                yield return null;
            }

            Progress = 1f;
            IsLoading = false;
            LoadFinished?.Invoke();
            onDone?.Invoke();
        }

        private static void EnsureRunner()
        {
            if (s_Runner != null) return;

            var go = new GameObject("[SceneManager]");
            go.hideFlags = HideFlags.HideInHierarchy;
            UnityEngine.Object.DontDestroyOnLoad(go);
            s_Runner = go.AddComponent<Runner>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Current = null;
            s_Runner = null;
            IsLoading = false;
            Progress = 0f;
            LoadStarted = null;
            LoadFinished = null;
            SceneReady = null;
        }

        /// <summary>코루틴을 돌릴 몸통. 씬을 넘어가도 살아남는다.</summary>
        private class Runner : MonoBehaviour { }
    }
}
