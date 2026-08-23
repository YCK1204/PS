using System.Collections.Generic;
using System.Text;
using SO;
using UnityEngine;

namespace PS.Game.Inventory
{
    /// <summary>격자를 읽어 단어를 찾는다. 격자를 바꾸지 않는다.
    /// 칸에 글자가 둘이면 "둘 중 하나"로 읽어 갈래를 나눈다 (OR).</summary>
    public class WordScanner
    {
        private readonly WordTable m_Table;
        private readonly ScanRules m_Rules;
        private readonly StringBuilder m_Buffer = new StringBuilder(16);

        public ScanRules Rules => m_Rules;
        public WordTable Table => m_Table;

        public WordScanner(WordTable table, ScanRules rules = null)
        {
            m_Table = table;
            m_Rules = rules ?? new ScanRules();
        }

        public void Scan(InventoryGrid grid, List<WordMatch> results, HashSet<string> runs = null)
        {
            results.Clear();
            if (runs != null) runs.Clear();
            if (grid == null || m_Table == null || m_Table.MaxWordLength <= 0) return;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var origin = new Vector2Int(x, y);
                    if (grid[origin].IsEmpty) continue;

                    for (int d = 0; d < m_Rules.Directions.Length; d++)
                    {
                        m_Buffer.Clear();
                        Step(grid, origin, m_Rules.Directions[d], 0, 0, results, runs);
                    }
                }
            }
        }

        /// <summary>index번째 칸에서 후보 글자마다 갈래를 타고 들어간다.
        /// 접두어가 아니면 즉시 끊어서 대부분의 갈래가 1스텝에 죽는다.</summary>
        private void Step(InventoryGrid grid, Vector2Int origin, Vector2Int direction,
            int index, int enhancement, List<WordMatch> results, HashSet<string> runs)
        {
            if (m_Buffer.Length >= m_Table.MaxWordLength) return;

            Vector2Int at = origin + direction * index;
            if (!grid.InBounds(at)) return;

            GridCell cell = grid[at];
            if (cell.Count == 0) return;

            int total = enhancement + cell.Enhancement;

            for (int slot = 0; slot < cell.Count; slot++)
            {
                if (!(cell.At(slot) is LetterData letter)) continue;

                m_Buffer.Append(letter.Letter);
                string current = m_Buffer.ToString();

                if (m_Table.HasSubstring(current))
                {
                    if (runs != null) runs.Add(current);

                    if (m_Buffer.Length >= m_Rules.MinLength)
                    {
                        WordData word = m_Table.GetWord(current);
                        if (word != null)
                        {
                            var match = new WordMatch
                            {
                                Word = word,
                                Origin = origin,
                                Direction = direction,
                                Length = m_Buffer.Length,
                                Enhancement = word.MaxEnhancement > 0
                                    ? Mathf.Min(total, word.MaxEnhancement)
                                    : total,
                            };

                            if (!IsDuplicate(results, match)) results.Add(match);
                        }
                    }

                    Step(grid, origin, direction, index + 1, total, results, runs);
                }

                m_Buffer.Length--;
            }
        }

        /// <summary>같은 구간을 반대로 읽은 회문(EYE, LEVEL...)과,
        /// 한 칸에 같은 글자가 둘 들어가 생긴 완전 중복을 걸러낸다.</summary>
        private static bool IsDuplicate(List<WordMatch> results, in WordMatch match)
        {
            Vector2Int end = match.CellAt(match.Length - 1);

            for (int i = 0; i < results.Count; i++)
            {
                WordMatch other = results[i];
                if (other.Word != match.Word || other.Length != match.Length) continue;

                if (other.Origin == match.Origin && other.Direction == match.Direction) return true;
                if (other.Origin == end && other.CellAt(other.Length - 1) == match.Origin) return true;
            }

            return false;
        }
    }
}
