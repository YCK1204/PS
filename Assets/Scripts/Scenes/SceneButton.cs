using PS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PS.UI
{
    /// <summary>누르면 정해진 씬으로 간다.</summary>
    [RequireComponent(typeof(Button))]
    public class SceneButton : MonoBehaviour
    {
        [SerializeField] private string m_Scene = SceneRouter.Title;

        private void Awake() => GetComponent<Button>().onClick.AddListener(Go);

        public void Go() => SceneRouter.Load(m_Scene);
    }
}
