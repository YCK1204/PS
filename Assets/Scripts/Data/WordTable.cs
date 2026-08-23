using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    public class WordTable
    {
        public const string DefaultResourcePath = "Data/Words";

        private readonly List<WordData> m_Words = new List<WordData>();
        private readonly Dictionary<int, WordData> m_WordDict = new Dictionary<int, WordData>();
        private readonly Dictionary<string, WordData> m_ByWord = new Dictionary<string, WordData>();
        private readonly HashSet<string> m_Prefixes = new HashSet<string>();
        private readonly HashSet<string> m_Substrings = new HashSet<string>();

        public IReadOnlyList<WordData> Words => m_Words;
        public int Count => m_Words.Count;

        /// <summary>스캐너가 한 방향으로 걸어갈 최대 칸 수.</summary>
        public int MaxWordLength { get; private set; }

        public void Init(string resourcePath = DefaultResourcePath)
        {
            Clear();

            WordData[] loaded = Resources.LoadAll<WordData>(resourcePath);
            for (int i = 0; i < loaded.Length; i++)
                Add(loaded[i]);

            WarnContained();
        }

        /// <summary>SO 에셋 없이 채울 때 — 테스트·런타임 생성용.</summary>
        public void Init(IEnumerable<WordData> words)
        {
            Clear();

            foreach (WordData word in words)
                Add(word);

            WarnContained();
        }

        public WordData GetWord(int id)
        {
            WordData word = null;
            m_WordDict.TryGetValue(id, out word);
            return word;
        }

        public WordData GetWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return null;

            WordData result = null;
            m_ByWord.TryGetValue(word, out result);
            return result;
        }

        /// <summary>이 글자열로 시작하는 단어가 하나라도 있나. 스캐너가 조기 탈출에 쓴다.</summary>
        public bool HasPrefix(string prefix)
            => !string.IsNullOrEmpty(prefix) && m_Prefixes.Contains(prefix);

        /// <summary>어떤 단어의 이어진 일부인가. 진행도 계산과 스캔 가지치기에 쓴다.</summary>
        public bool HasSubstring(string value)
            => !string.IsNullOrEmpty(value) && m_Substrings.Contains(value);

        /// <summary>한 단어가 다른 단어 안에 통째로 들어있으면 긴 쪽을 놓는 것만으로 짧은 쪽까지 성립한다.
        /// 8방향이라 역철자로 들어있는 경우도 같다. 사전 설계상 안 만들기로 한 형태라 경고만 띄운다.</summary>
        private void WarnContained()
        {
            for (int i = 0; i < m_Words.Count; i++)
            {
                string outer = m_Words[i].Word;

                for (int j = 0; j < m_Words.Count; j++)
                {
                    if (i == j) continue;

                    string inner = m_Words[j].Word;
                    if (inner.Length >= outer.Length) continue;

                    if (outer.Contains(inner))
                        Debug.LogWarning($"단어 포함 — {outer} 안에 {inner}. {outer}를 놓으면 {inner}도 같이 성립함", m_Words[i]);
                    else if (outer.Contains(Reverse(inner)))
                        Debug.LogWarning($"단어 포함(역철자) — {outer} 안에 {inner}의 역철자. {outer}를 놓으면 {inner}도 같이 성립함", m_Words[i]);
                }
            }
        }

        private static string Reverse(string value)
        {
            char[] chars = value.ToCharArray();
            System.Array.Reverse(chars);
            return new string(chars);
        }

        private void Clear()
        {
            m_Words.Clear();
            m_WordDict.Clear();
            m_ByWord.Clear();
            m_Prefixes.Clear();
            m_Substrings.Clear();
            MaxWordLength = 0;
        }

        private void Add(WordData word)
        {
            if (word == null) return;

            string text = word.Word;
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogError($"WordData의 Word가 비어있음 — Id {word.Id}", word);
                return;
            }

            if (m_WordDict.ContainsKey(word.Id))
            {
                Debug.LogError($"WordData Id 중복 — {word.Id} ({text}) 무시됨", word);
                return;
            }

            if (m_ByWord.ContainsKey(text))
            {
                Debug.LogError($"WordData 철자 중복 — {text} (Id {word.Id}) 무시됨", word);
                return;
            }

            m_Words.Add(word);
            m_WordDict.Add(word.Id, word);
            m_ByWord.Add(text, word);

            for (int i = 1; i <= text.Length; i++)
                m_Prefixes.Add(text.Substring(0, i));

            for (int start = 0; start < text.Length; start++)
                for (int len = 1; start + len <= text.Length; len++)
                    m_Substrings.Add(text.Substring(start, len));

            if (text.Length > MaxWordLength) MaxWordLength = text.Length;
        }
    }
}
