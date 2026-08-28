using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    /// <summary>보상 풀과 수치. 뭘 얼마나 줄지는 전부 여기서 정한다.</summary>
    [CreateAssetMenu(menuName = "PS/RewardTable", fileName = "RewardTable")]
    public class RewardTable : ScriptableObject
    {
        [Header("알파벳")]
        [Tooltip("낱개로 지정할 수 있는 글자 풀. 비우면 Resources의 전체 글자를 쓴다")]
        [SerializeField] private LetterData[] m_LetterPool;

        [Tooltip("일반 맵 꾸러미 장수 (최소, 최대)")]
        [SerializeField] private Vector2Int m_BundleSize = new Vector2Int(3, 5);

        [Tooltip("보스 꾸러미 장수 (최소, 최대)")]
        [SerializeField] private Vector2Int m_BossBundleSize = new Vector2Int(5, 7);

        [Tooltip("보스에서 낱개 후보를 몇 개 띄우나")]
        [SerializeField] private Vector2Int m_BossLetterChoices = new Vector2Int(2, 3);

        [Header("글리프")]
        [SerializeField] private GlyphData[] m_GlyphPool;

        [Tooltip("한 번에 띄울 글리프 후보 수")]
        [SerializeField] private int m_GlyphChoices = 3;

        [Header("뼈")]
        [Tooltip("보스가 확정으로 주는 뼈 개수")]
        [SerializeField] private int m_BossBones = 1;

        [Tooltip("일반 맵에서 뼈를 골랐을 때 주는 개수")]
        [SerializeField] private int m_MapBones = 1;

        [Header("전직")]
        [Tooltip("1차 전직에 필요한 뼈")]
        [SerializeField] private int m_BonesForTier1 = 2;

        [Tooltip("2차 전직에 필요한 뼈 (1차 이후 추가로)")]
        [SerializeField] private int m_BonesForTier2 = 3;

        public IReadOnlyList<LetterData> LetterPool => m_LetterPool;
        public int GlyphChoices => m_GlyphChoices;
        public int BossBones => m_BossBones;
        public int MapBones => m_MapBones;
        public int BonesForTier1 => m_BonesForTier1;
        public int BonesForTier2 => m_BonesForTier2;

        /// <summary>풀이 비어 있으면 런타임 테이블에서 채운다.</summary>
        public IReadOnlyList<LetterData> ResolveLetters(LetterTable fallback)
            => (m_LetterPool != null && m_LetterPool.Length > 0) ? m_LetterPool : fallback?.Letters;

        public int RollBundleCount(bool boss)
        {
            Vector2Int range = boss ? m_BossBundleSize : m_BundleSize;
            return Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y) + 1);
        }

        public int RollBossLetterChoices()
            => Random.Range(Mathf.Min(m_BossLetterChoices.x, m_BossLetterChoices.y),
                            Mathf.Max(m_BossLetterChoices.x, m_BossLetterChoices.y) + 1);

        /// <summary>꾸러미 내용. 같은 글자가 여러 장 나올 수 있다.</summary>
        public void RollBundle(LetterTable fallback, List<LetterData> results, bool boss)
        {
            results.Clear();

            IReadOnlyList<LetterData> pool = ResolveLetters(fallback);
            if (pool == null || pool.Count == 0) return;

            int count = RollBundleCount(boss);
            for (int i = 0; i < count; i++)
                results.Add(pool[Random.Range(0, pool.Count)]);
        }

        /// <summary>글리프 후보. 중복 없이 뽑는다.</summary>
        public void RollGlyphs(List<GlyphData> results)
        {
            results.Clear();
            if (m_GlyphPool == null || m_GlyphPool.Length == 0) return;

            var bag = new List<GlyphData>(m_GlyphPool);
            int count = Mathf.Min(m_GlyphChoices, bag.Count);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, bag.Count);
                results.Add(bag[index]);
                bag.RemoveAt(index);
            }
        }
    }
}
