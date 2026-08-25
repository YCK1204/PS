using System;
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace PS.Game.Inventory
{
    /// <summary>격자 + 포션 + 활성 단어 + 들고 있는 것. MonoBehaviour가 아니라 창을 닫아도 살아있다.</summary>
    public class InventoryState
    {
        private readonly WordScanner m_Scanner;
        private readonly List<WordMatch> m_ScanBuffer = new List<WordMatch>();
        private readonly List<WordMatch> m_Active = new List<WordMatch>();
        private Dictionary<int, WordMatch> m_Previous = new Dictionary<int, WordMatch>();
        private Dictionary<int, WordMatch> m_Next = new Dictionary<int, WordMatch>();
        private readonly Dictionary<int, int> m_Counts = new Dictionary<int, int>();
        private readonly List<WordProgress> m_Progress = new List<WordProgress>();
        private readonly HashSet<string> m_Runs = new HashSet<string>();

        public InventoryGrid Grid { get; }
        public ItemData[] Potions { get; }

        public IReadOnlyList<WordMatch> ActiveWords => m_Active;

        /// <summary>워드 목록에 띄울 줄. 한 글자도 없는 단어는 빠진다.</summary>
        public IReadOnlyList<WordProgress> WordRows => m_Progress;

        /// <summary>사전에 등록된 단어 수. 워드 목록 헤더의 분모.</summary>
        public int KnownWordCount => m_Scanner != null && m_Scanner.Table != null ? m_Scanner.Table.Count : 0;

        /// <summary>Enter로 집어 든 것. 놓기 전까지 격자에 없다.</summary>
        /// <summary>격자 밖(보상 등)에서 들어와 아직 격자에 자리가 없는 것.</summary>
        public static readonly Vector2Int Outside = new Vector2Int(-1, -1);

        public ItemData Held { get; private set; }
        public Vector2Int HeldFrom { get; private set; }
        public int HeldSlot { get; private set; }

        /// <summary>격자·포션·활성 단어 중 뭐라도 바뀌면 발행. UI는 이것만 듣는다.</summary>
        public event Action Changed;

        /// <summary>단어가 켜졌다. 강화도만 바뀐 경우도 Disabled 후 Enabled로 온다.
        /// 모델은 효과가 뭔지 모른다 — 씬 쪽(WordEffectHost)이 구독해서 처리한다.</summary>
        public event Action<WordData, int> WordEnabled;
        public event Action<WordData, int> WordDisabled;

        public InventoryState(InventoryGrid grid, WordScanner scanner, int potionSlots)
        {
            Grid = grid;
            m_Scanner = scanner;
            Potions = new ItemData[Mathf.Max(0, potionSlots)];
        }

        /// <summary>집어 든다. 격자는 그대로 — 놓기 전까지 원래 칸에 남아있고 단어도 살아있다.
        /// 실제로 옮겨질 때 한 번만 효과가 갱신된다.</summary>
        public bool Take(Vector2Int at, int slot = -1)
        {
            if (Held != null || !Grid.InBounds(at)) return false;

            GridCell cell = Grid[at];
            if (cell.Count == 0) return false;

            if (slot < 0) slot = cell.Count - 1;
            if (slot >= cell.Count) return false;

            ItemData item = cell.At(slot);
            if (item == null) return false;
            if (!item.CanUnequip(this, at)) return false;

            Held = item;
            HeldFrom = at;
            HeldSlot = slot;

            Changed?.Invoke();
            return true;
        }

        /// <summary>격자 밖에서 온 것을 집어 든 상태로 만든다. 놓을 때 원래 칸으로 돌릴 곳이 없다.</summary>
        public bool HoldExternal(ItemData item)
        {
            if (Held != null || item == null) return false;

            Held = item;
            HeldFrom = Outside;
            HeldSlot = -1;

            Changed?.Invoke();
            return true;
        }

        public bool HeldIsExternal => Held != null && !Grid.InBounds(HeldFrom);

        /// <summary>놓는다. 원래 칸이면 취소, 소모형이면 칸에 먹이고,
        /// 빈 자리가 있으면 이동, 꽉 찬 칸이면 교환.</summary>
        public bool Place(Vector2Int at)
        {
            if (Held == null) return false;

            if (at == HeldFrom)
            {
                Held = null;
                Changed?.Invoke();
                return true;
            }

            if (!Grid.InBounds(at)) return false;

            if (Held.ConsumedOnPlace) return Consume(at);

            return Grid[at].IsFull ? Swap(at) : Move(at);
        }

        /// <summary>결속형 글리프 — 칸 스펙만 바꾸고 격자에는 남지 않는다.
        /// 달라지는 게 없으면 소모시키지 않는다.</summary>
        private bool Consume(Vector2Int at)
        {
            var glyph = Held as BoundGlyphData;
            if (glyph == null)
            {
                Debug.LogError($"{Held.name}은 ConsumedOnPlace인데 BoundGlyphData가 아니다");
                return false;
            }

            if (!glyph.CanApply(this, at)) return false;

            glyph.Apply(this, at);
            Held = null;

            Rescan();
            return true;
        }

        private bool Move(Vector2Int at)
        {
            ItemData item = Held;

            if (Grid.InBounds(HeldFrom))
            {
                item.OnUnequip(this, HeldFrom);
                Grid.Take(HeldFrom, HeldSlot);
            }

            if (!Grid.Place(item, at)) return false;

            item.OnEquip(this, at);

            Held = null;

            Rescan();
            return true;
        }

        /// <summary>꽉 찬 칸에 놓으면 자리를 맞바꾼다.
        /// 글리프 효과가 섞이지 않게 둘 다 뺀 뒤에 둘 다 넣는다.</summary>
        private bool Swap(Vector2Int at)
        {
            if (HeldIsExternal) return false;

            GridCell target = Grid[at];
            int targetSlot = target.Count - 1;

            ItemData item = Held;
            ItemData other = target.At(targetSlot);
            if (other == null) return false;
            if (!other.CanUnequip(this, at)) return false;

            item.OnUnequip(this, HeldFrom);
            other.OnUnequip(this, at);

            Grid.Take(HeldFrom, HeldSlot);
            Grid.Take(at, targetSlot);

            bool placedItem = Grid.Place(item, at);
            bool placedOther = Grid.Place(other, HeldFrom);

            if (!placedItem || !placedOther)
            {
                Debug.LogError($"교환 실패 — {HeldFrom} ↔ {at}. 용량 규칙이 깨졌다");
                return false;
            }

            item.OnEquip(this, at);
            other.OnEquip(this, HeldFrom);

            Held = null;

            Rescan();
            return true;
        }

        /// <summary>들기 취소. 격자를 안 건드렸으므로 되돌릴 것도 없다.</summary>
        public bool Cancel()
        {
            if (Held == null) return false;

            Held = null;
            Changed?.Invoke();
            return true;
        }

        /// <summary>Del 버리기. 들고 있어도 격자에는 남아있으므로 여기서 실제로 뺀다.</summary>
        public ItemData Drop()
        {
            ItemData item = Held;
            if (item == null) return null;

            if (Grid.InBounds(HeldFrom))
            {
                item.OnUnequip(this, HeldFrom);
                Grid.Take(HeldFrom, HeldSlot);
            }

            Held = null;

            Rescan();
            return item;
        }

        public bool SetPotion(int slot, ItemData item)
        {
            if (slot < 0 || slot >= Potions.Length) return false;

            Potions[slot] = item;
            Changed?.Invoke();
            return true;
        }

        /// <summary>전수 스캔 후 단어 단위로 묶어서 이전 결과와 비교한다.
        /// 같은 단어가 여러 군데 성립하면 효과는 한 번, 대신 강화도에 (성립 수 - 1)을 더한다.
        /// 강화도가 바뀐 단어는 Disable 후 Enable로 재적용된다.</summary>
        public void Rescan()
        {
            m_Scanner.Scan(Grid, m_ScanBuffer, m_Runs);

            ResolveWords();

            foreach (KeyValuePair<int, WordMatch> pair in m_Previous)
            {
                WordMatch next;
                if (m_Next.TryGetValue(pair.Key, out next) && next.Enhancement == pair.Value.Enhancement) continue;
                if (pair.Value.Word != null) WordDisabled?.Invoke(pair.Value.Word, pair.Value.Enhancement);
            }

            foreach (KeyValuePair<int, WordMatch> pair in m_Next)
            {
                WordMatch prev;
                if (m_Previous.TryGetValue(pair.Key, out prev) && prev.Enhancement == pair.Value.Enhancement) continue;
                if (pair.Value.Word != null) WordEnabled?.Invoke(pair.Value.Word, pair.Value.Enhancement);
            }

            Dictionary<int, WordMatch> swap = m_Previous;
            m_Previous = m_Next;
            m_Next = swap;

            m_Active.Clear();
            m_Active.AddRange(m_ScanBuffer);

            BuildProgress();

            Changed?.Invoke();
        }

        /// <summary>매치들을 단어 하나로 접는다. 강화도는 가장 높은 매치 기준 + 중복 보너스,
        /// 합산이 아닌 이유는 같은 단어를 도배해서 강화도를 쌓는 걸 막기 위함.</summary>
        private void ResolveWords()
        {
            m_Next.Clear();
            m_Counts.Clear();

            for (int i = 0; i < m_ScanBuffer.Count; i++)
            {
                WordMatch match = m_ScanBuffer[i];
                if (match.Word == null) continue;

                int id = match.Word.Id;

                int count;
                m_Counts.TryGetValue(id, out count);
                m_Counts[id] = count + 1;

                WordMatch best;
                if (!m_Next.TryGetValue(id, out best) || match.Enhancement > best.Enhancement)
                    m_Next[id] = match;
            }

            var ids = new List<int>(m_Next.Keys);
            for (int i = 0; i < ids.Count; i++)
            {
                WordMatch match = m_Next[ids[i]];
                int bonus = m_Counts[ids[i]] - 1;

                int total = match.Enhancement + bonus;
                if (match.Word.MaxEnhancement > 0) total = Mathf.Min(total, match.Word.MaxEnhancement);

                match.Enhancement = total;
                m_Next[ids[i]] = match;
            }
        }

        /// <summary>단어의 이어진 조각 중 격자에 실제로 놓여있는 가장 긴 것을 진행도로 본다.
        /// WIND라면 WIN·IND·ND처럼 붙어있는 부분만 세고, 흩어진 글자는 안 센다.</summary>
        private void BuildProgress()
        {
            m_Progress.Clear();

            WordTable table = m_Scanner != null ? m_Scanner.Table : null;
            if (table == null) return;

            for (int i = 0; i < table.Words.Count; i++)
            {
                WordData word = table.Words[i];
                string text = word.Word;
                if (string.IsNullOrEmpty(text)) continue;

                WordMatch resolved;
                bool active = m_Previous.TryGetValue(word.Id, out resolved);
                int enhancement = active ? resolved.Enhancement : 0;
                int count = 0;
                if (active) m_Counts.TryGetValue(word.Id, out count);

                int have = active ? text.Length : LongestRun(text);
                if (have <= 0) continue;

                m_Progress.Add(new WordProgress
                {
                    Word = word,
                    Have = have,
                    Total = text.Length,
                    Active = active,
                    Count = count,
                    Enhancement = enhancement,
                });
            }

            m_Progress.Sort(CompareProgress);
        }

        /// <summary>격자에 이어져 있는 이 단어의 조각 중 최장 길이.</summary>
        private int LongestRun(string text)
        {
            for (int len = text.Length; len >= 1; len--)
            {
                for (int start = 0; start + len <= text.Length; start++)
                {
                    if (m_Runs.Contains(text.Substring(start, len))) return len;
                }
            }

            return 0;
        }

        /// <summary>성립한 것 먼저, 그다음 많이 모인 순, 마지막은 가나다순.</summary>
        private static int CompareProgress(WordProgress a, WordProgress b)
        {
            if (a.Active != b.Active) return a.Active ? -1 : 1;

            float ra = a.Total > 0 ? (float)a.Have / a.Total : 0f;
            float rb = b.Total > 0 ? (float)b.Have / b.Total : 0f;
            if (!Mathf.Approximately(ra, rb)) return rb.CompareTo(ra);

            return string.CompareOrdinal(a.Word.Word, b.Word.Word);
        }
    }
}
