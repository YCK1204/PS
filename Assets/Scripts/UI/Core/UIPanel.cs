using UnityEngine;

namespace PS.UI
{
    /// <summary>
    /// 열고 닫히는 화면의 공통 베이스. 실제 여닫기는 UIStack이 관리한다.
    /// 직접 SetActive 하지 말고 Open()/Close()를 쓴다.
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [Tooltip("ESC로 닫을 수 있는가. 끄면 스택이 무시한다.")]
        public bool CloseOnEscape = true;

        [Tooltip("열릴 때 아래 패널을 가리는가. 끄면 겹쳐 보인다.")]
        public bool BlocksBelow = true;

        public bool IsOpen => gameObject.activeSelf;

        /// <summary>닫힘 알림. 연 쪽이 복구할 때 쓴다.</summary>
        public event System.Action<UIPanel> Closed;

        public void Open()
        {
            if (IsOpen) return;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            UIStack.Push(this);
            OnOpened();
        }

        public void Close()
        {
            if (!IsOpen) return;
            OnClosing();
            UIStack.Remove(this);
            gameObject.SetActive(false);
            Closed?.Invoke(this);
        }

        /// <summary>스택 최상단에서 ESC를 받았을 때. false를 돌려주면 닫히지 않는다.</summary>
        public virtual bool OnEscape() => true;

        protected virtual void OnOpened() { }
        protected virtual void OnClosing() { }

        protected virtual void OnDisable()
        {
            // 외부에서 강제로 꺼도 스택이 어긋나지 않게
            UIStack.Remove(this);
        }
    }
}
