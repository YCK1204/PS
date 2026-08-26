namespace PS.Core
{
    /// <summary>풀의 수명 범위.</summary>
    public enum PoolScope
    {
        /// <summary>씬이 바뀌면 풀과 인스턴스가 전부 사라진다. 기본값.</summary>
        Scene,

        /// <summary>씬을 넘어도 유지된다. 여러 씬에서 계속 쓰는 것만.</summary>
        Global,
    }
}
