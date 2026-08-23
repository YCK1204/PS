using System;
using SO;
using UnityEngine;

namespace PS.Game.Inventory
{
    /// <summary>격자에서 찾아낸 단어 1건.</summary>
    public struct WordMatch : IEquatable<WordMatch>
    {
        public WordData Word;
        public Vector2Int Origin;
        public Vector2Int Direction;
        public int Length;

        /// <summary>구성 칸 강화도 합. Word.MaxEnhancement가 0보다 크면 거기서 잘린다.</summary>
        public int Enhancement;

        public Vector2Int CellAt(int index) => Origin + Direction * index;

        /// <summary>강화도까지 포함해서 비교한다. 강화도만 바뀌어도 다른 매치로 취급 —
        /// InventoryState의 diff가 Disable 후 Enable을 자동으로 태우게 하기 위함.</summary>
        public bool Equals(WordMatch other)
            => Word == other.Word
               && Origin == other.Origin
               && Direction == other.Direction
               && Length == other.Length
               && Enhancement == other.Enhancement;

        public override bool Equals(object obj) => obj is WordMatch other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Word != null ? Word.GetInstanceID() : 0;
                hash = hash * 397 ^ Origin.GetHashCode();
                hash = hash * 397 ^ Direction.GetHashCode();
                hash = hash * 397 ^ Length;
                hash = hash * 397 ^ Enhancement;
                return hash;
            }
        }

        public override string ToString()
            => Word == null ? "(null)" : $"{Word.Word} {Enhancement}강 @{Origin}{Direction}";
    }
}
