using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PS.UI
{
    /// <summary>
    /// 열린 패널을 쌓아두고 ESC를 최상단 하나에만 보낸다.
    /// 씬에 두지 않아도 첫 Push에서 자동 생성된다.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class UIStack : MonoBehaviour
    {
        static readonly List<UIPanel> s_Panels = new List<UIPanel>();
        static UIStack s_Runner;

        public static int Count => s_Panels.Count;
        public static UIPanel Top => s_Panels.Count > 0 ? s_Panels[s_Panels.Count - 1] : null;

        /// <summary>패널이 하나라도 열려 있는가. 게임플레이 입력을 막을 때 쓴다.</summary>
        public static bool AnyOpen => s_Panels.Count > 0;

        /// <summary>키 재설정처럼 ESC를 다른 데서 먹어야 할 때 켠다.</summary>
        public static bool SuppressEscape { get; set; }

        /// <summary>아무것도 안 열린 상태에서 ESC를 눌렀을 때. 게임 중 메뉴 열기에 쓴다.
        /// UIStack이 ESC를 독점하므로 바깥에서 직접 듣지 말고 이걸 구독한다.</summary>
        public static event System.Action EscapeOnEmpty;

        public static void Push(UIPanel panel)
        {
            if (panel == null || s_Panels.Contains(panel)) return;
            s_Panels.Add(panel);
            EnsureRunner();
        }

        public static void Remove(UIPanel panel)
        {
            if (panel == null) return;
            s_Panels.Remove(panel);
        }

        /// <summary>최상단 패널을 닫는다. 닫았으면 true.</summary>
        public static bool CloseTop()
        {
            Prune();
            UIPanel top = Top;
            if (top == null) return false;
            if (!top.CloseOnEscape) return false;
            if (!top.OnEscape()) return false;
            top.Close();
            return true;
        }

        public static void CloseAll()
        {
            for (int i = s_Panels.Count - 1; i >= 0; i--)
                if (s_Panels[i] != null) s_Panels[i].Close();
            s_Panels.Clear();
        }

        static void Prune()
        {
            for (int i = s_Panels.Count - 1; i >= 0; i--)
                if (s_Panels[i] == null || !s_Panels[i].IsOpen) s_Panels.RemoveAt(i);
        }

        /// <summary>ESC를 들으려면 러너가 있어야 한다. 패널을 한 번도 안 연 씬에서 직접 부른다.</summary>
        public static void EnsureRunner()
        {
            if (s_Runner != null) return;
            var go = new GameObject("[UIStack]");
            s_Runner = go.AddComponent<UIStack>();
            DontDestroyOnLoad(go);
        }

        void OnEnable() => UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        void OnDisable() => UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;

        /// <summary>씬이 내려가면 스택을 비운다. 패널은 씬과 함께 사라지므로 참조만 남는다.</summary>
        static void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            s_Panels.Clear();
            SuppressEscape = false;
        }

        void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;
            if (SuppressEscape) return;

            if (!CloseTop()) EscapeOnEmpty?.Invoke();
        }

        void OnDestroy()
        {
            if (s_Runner == this) s_Runner = null;
        }

        /// <summary>플레이 모드 재시작 시 정적 상태 초기화.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_Panels.Clear();
            s_Runner = null;
            SuppressEscape = false;
            EscapeOnEmpty = null;
        }
    }
}
