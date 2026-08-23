using UnityEngine;

namespace PS.Game.Inventory
{
    /// <summary>스캔 규칙. 기획 미확정 항목을 코드가 아니라 여기 값으로 둔다.</summary>
    public class ScanRules
    {
        public static readonly Vector2Int[] Horizontal =
        {
            new Vector2Int(1, 0),
        };

        /// <summary>가로 + 세로.</summary>
        public static readonly Vector2Int[] Cross =
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
        };

        /// <summary>가로 · 세로 · 대각 2 — 역방향 철자는 안 센다.</summary>
        public static readonly Vector2Int[] Forward4 =
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
        };

        /// <summary>가로 · 세로 · 대각 각각 정방향 + 역방향. 기본값.</summary>
        public static readonly Vector2Int[] All8 =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
        };

        public Vector2Int[] Directions = All8;
        public int MinLength = 2;
    }
}
