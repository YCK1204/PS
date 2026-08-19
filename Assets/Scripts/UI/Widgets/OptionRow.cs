using UnityEngine;

namespace PS.UI
{
    /// <summary>
    /// 목록 한 줄의 공통 베이스. Id로 바깥에서 찾아 쓴다.
    /// 설정 전용이 아니라 상점·도감 등 어떤 목록에도 쓸 수 있다.
    /// </summary>
    public abstract class OptionRow : MonoBehaviour
    {
        [Tooltip("이 행을 식별하는 키. 화면 쪽에서 이 값으로 찾아 바인딩한다.")]
        public string Id;
    }
}
