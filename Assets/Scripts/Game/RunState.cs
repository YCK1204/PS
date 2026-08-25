using PS.Game.Inventory;
using SO;
using UnityEngine;

namespace PS.Game
{
    /// <summary>런 1회분의 상태를 만들고 화면에 물린다. 지금은 인벤토리만.</summary>
    public class RunState : MonoBehaviour
    {
        [System.Serializable]
        private struct DebugGlyph
        {
            public GlyphData Glyph;
            public Vector2Int At;
        }

        [SerializeField] private PS.UI.Inventory m_InventoryPanel;

        [Tooltip("격자에서 켜진 단어를 받을 캐릭터. 없으면 효과가 아무 데도 안 걸린다")]
        [SerializeField] private PS.Game.Words.WordEffectHost m_Player;

        [Header("격자")]
        [SerializeField] private int m_GridWidth = 8;
        [SerializeField] private int m_GridHeight = 4;
        [SerializeField] private int m_PotionSlots = 4;

        [Header("디버그")]
        [Tooltip("시작할 때 격자 왼쪽 위부터 채울 글자. 실제 수급 규칙이 정해지면 지운다")]
        [SerializeField] private string m_DebugStartLetters = "FIRE";

        [Tooltip("시작할 때 놓을 글리프와 그 자리. 글자를 채운 뒤에 놓는다")]
        [SerializeField] private DebugGlyph[] m_DebugStartGlyphs;

        public WordTable Words { get; private set; }
        public LetterTable Letters { get; private set; }
        public InventoryState Inventory { get; private set; }

        private void Awake()
        {
            Words = new WordTable();
            Words.Init();

            Letters = new LetterTable();
            Letters.Init();

            var grid = new InventoryGrid(m_GridWidth, m_GridHeight);
            var scanner = new WordScanner(Words, new ScanRules());
            Inventory = new InventoryState(grid, scanner, m_PotionSlots);

            FillDebugLetters(grid);
            PlaceDebugGlyphs();

            Debug.Log($"[RunState] 단어 {Words.Count}개 · 글자 {Letters.Count}개 · 격자 {grid.Width}x{grid.Height}");
        }

        /// <summary>바인딩은 Start에서. Awake에 두면 캐릭터 부품(Combat 등)이 아직 안 붙어 있어
        /// 단어 효과가 조용히 씹힌다.</summary>
        private void Start()
        {
            if (m_InventoryPanel != null) m_InventoryPanel.Bind(Inventory);
            if (m_Player != null) m_Player.Bind(Inventory);

            Inventory.Rescan();
        }

        private void FillDebugLetters(InventoryGrid grid)
        {
            if (string.IsNullOrEmpty(m_DebugStartLetters)) return;

            for (int i = 0; i < m_DebugStartLetters.Length && i < grid.Width * grid.Height; i++)
            {
                LetterData letter = Letters.Get(m_DebugStartLetters[i]);
                if (letter == null)
                {
                    Debug.LogWarning($"[RunState] '{m_DebugStartLetters[i]}' 글자 에셋 없음");
                    continue;
                }

                grid.Place(letter, new Vector2Int(i % grid.Width, i / grid.Width));
            }
        }

        /// <summary>결속형은 놓으면 소모되고, 이동형은 칸에 남는다. 실제 조작과 같은 경로를 탄다.</summary>
        private void PlaceDebugGlyphs()
        {
            if (m_DebugStartGlyphs == null) return;

            for (int i = 0; i < m_DebugStartGlyphs.Length; i++)
            {
                GlyphData glyph = m_DebugStartGlyphs[i].Glyph;
                if (glyph == null) continue;

                Vector2Int at = m_DebugStartGlyphs[i].At;
                if (!Inventory.HoldExternal(glyph)) continue;

                if (!Inventory.Place(at))
                {
                    Debug.LogWarning($"[RunState] 글리프 {glyph.name}를 {at}에 못 놓음");
                    Inventory.Cancel();
                }
            }
        }

    }
}
