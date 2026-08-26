using UnityEngine;

namespace PS.Core
{
    /// <summary>씬마다 하나씩 놓는 진입점. 그 씬의 초기화를 책임진다.
    /// 씬에 들어올 때마다 Start에서 자기를 SceneManager에 등록하고 Init을 부른다.</summary>
    public abstract class BaseScene : MonoBehaviour
    {
        [Tooltip("이 씬이 무엇인가. 하위 클래스가 기본값을 정한다")]
        [SerializeField] protected SceneType m_Type = SceneType.None;

        /// <summary>바깥에서는 읽기만 된다.</summary>
        public SceneType Type => m_Type;

        protected virtual void Start()
        {
            SceneManager.Register(this);
            Init();
        }

        /// <summary>씬 초기화. 하위 클래스가 채운다.</summary>
        protected virtual void Init() { }

        protected virtual void OnDestroy() => SceneManager.Unregister(this);
    }
}
