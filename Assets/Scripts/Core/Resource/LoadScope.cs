namespace PS.Core
{
    /// <summary>로드한 것의 수명 범위.</summary>
    public enum LoadScope
    {
        /// <summary>씬이 바뀌면 해제된다. 기본값.</summary>
        Scene,

        /// <summary>씬을 넘어도 유지된다. 여러 씬이 공유하는 것만.</summary>
        Global,
    }
}
