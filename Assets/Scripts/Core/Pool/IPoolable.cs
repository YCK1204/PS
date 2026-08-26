namespace PS.Core
{
    /// <summary>풀에서 꺼내지고 반납될 때 상태를 스스로 정리하는 것.
    /// 구독 해제를 여기서 안 하면 반납된 객체가 이벤트를 계속 받는다.</summary>
    public interface IPoolable
    {
        /// <summary>풀에서 꺼내진 직후. 활성화된 뒤에 불린다.</summary>
        void OnGet();

        /// <summary>풀로 돌아가기 직전. 비활성화되기 전에 불린다.</summary>
        void OnRelease();
    }
}
