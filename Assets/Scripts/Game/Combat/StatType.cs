namespace PS.Game.Combat
{
    /// <summary>단어·장비가 건드리는 수치. 기획 0-6의 5종.</summary>
    public enum StatType
    {
        Attack,
        AttackSpeed,
        MoveSpeed,
        Projectile,
        Range,
        MaxHealth,
        JumpPower,

        /// <summary>0~1. 1이면 항상 치명타.</summary>
        CritChance,

        /// <summary>치명타일 때 피해 배수.</summary>
        CritMultiplier,
    }
}
