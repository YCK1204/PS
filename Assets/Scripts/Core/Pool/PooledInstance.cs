using UnityEngine;

namespace PS.Core
{
    /// <summary>풀에서 나온 인스턴스라는 표식. 어느 풀 소속인지와 반납 여부를 들고 있다.
    /// 이게 없으면 Release가 그냥 Destroy로 떨어진다.</summary>
    [DisallowMultipleComponent]
    public class PooledInstance : MonoBehaviour
    {
        public int Key { get; internal set; }

        /// <summary>씬을 넘어도 살아남는 풀 소속인가.</summary>
        public PoolScope Scope { get; internal set; }

        /// <summary>이미 반납됐는가. 두 번 반납하면 풀에 같은 객체가 두 개 들어간다.</summary>
        public bool Released { get; internal set; }
    }
}
