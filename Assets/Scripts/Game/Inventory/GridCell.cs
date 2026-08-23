using SO;

namespace PS.Game.Inventory
{
    public struct GridCell
    {
        public const int MaxCapacity = 2;

        public ItemData Item;
        public ItemData Item2;

        public int Enhancement;

        /// <summary>이 칸에 담을 수 있는 개수. 기본 1, 글리프가 올린다.</summary>
        public int Capacity;

        public bool IsEmpty => Item == null;
        public int Count => Item == null ? 0 : (Item2 == null ? 1 : 2);
        public bool IsFull => Count >= Capacity;

        public ItemData At(int slot) => slot == 0 ? Item : Item2;
    }
}
