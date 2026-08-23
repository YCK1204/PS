using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    public class LetterTable
    {
        public const string DefaultResourcePath = "Data/Letters";

        private readonly List<LetterData> m_Letters = new List<LetterData>();
        private readonly Dictionary<int, LetterData> m_ById = new Dictionary<int, LetterData>();
        private readonly Dictionary<char, LetterData> m_ByChar = new Dictionary<char, LetterData>();

        public IReadOnlyList<LetterData> Letters => m_Letters;
        public int Count => m_Letters.Count;

        public void Init(string resourcePath = DefaultResourcePath)
        {
            Clear();

            LetterData[] loaded = Resources.LoadAll<LetterData>(resourcePath);
            for (int i = 0; i < loaded.Length; i++)
                Add(loaded[i]);
        }

        public void Init(IEnumerable<LetterData> letters)
        {
            Clear();

            foreach (LetterData letter in letters)
                Add(letter);
        }

        public LetterData Get(int id)
        {
            LetterData letter = null;
            m_ById.TryGetValue(id, out letter);
            return letter;
        }

        public LetterData Get(char letter)
        {
            LetterData result = null;
            m_ByChar.TryGetValue(char.ToUpperInvariant(letter), out result);
            return result;
        }

        private void Clear()
        {
            m_Letters.Clear();
            m_ById.Clear();
            m_ByChar.Clear();
        }

        private void Add(LetterData letter)
        {
            if (letter == null) return;

            if (m_ById.ContainsKey(letter.Id))
            {
                Debug.LogError($"LetterData Id 중복 — {letter.Id} ({letter.Letter}) 무시됨", letter);
                return;
            }

            if (m_ByChar.ContainsKey(letter.Letter))
            {
                Debug.LogError($"LetterData 글자 중복 — {letter.Letter} (Id {letter.Id}) 무시됨", letter);
                return;
            }

            m_Letters.Add(letter);
            m_ById.Add(letter.Id, letter);
            m_ByChar.Add(letter.Letter, letter);
        }
    }
}
