using SO;

namespace PS.Game.Inventory
{
    /// <summary>워드 목록 한 줄에 필요한 정보. 성립했으면 Active, 아니면 몇 글자나 모였는지.</summary>
    public struct WordProgress
    {
        public WordData Word;

        /// <summary>격자에 갖고 있는 구성 글자 수.</summary>
        public int Have;

        /// <summary>단어 전체 글자 수.</summary>
        public int Total;

        public bool Active;

        /// <summary>격자에서 몇 군데 성립했나. 1보다 크면 강화도에 (Count-1)이 이미 더해져 있다.</summary>
        public int Count;

        /// <summary>성립했을 때의 강화도. 여러 번 성립하면 가장 높은 것.</summary>
        public int Enhancement;

        public bool IsKnown => Have > 0 || Active;
    }
}
