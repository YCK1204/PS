using PS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PS.UI
{
    /// <summary>누르면 정해진 씬으로 간다.</summary>
    [RequireComponent(typeof(Button))]
    public class SceneButton : MonoBehaviour
    {
        [Tooltip("이동할 씬")]
        [SerializeField] private SceneType m_Scene = SceneType.Title;

        [Tooltip("비동기로 전환할지. 끄면 즉시 전환")]
        [SerializeField] private bool m_Async;

        private void Awake() => GetComponent<Button>().onClick.AddListener(Go);

        public void Go()
        {
            if (m_Async) SceneManager.LoadAsync(m_Scene);
            else SceneManager.Load(m_Scene);
        }
    }
}
