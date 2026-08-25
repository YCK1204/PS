using UnityEngine;

namespace SO
{
    /// <summary>단어 하나. 효과는 상속이 아니라 조립으로 붙인다 —
    /// 같은 효과 에셋을 여러 단어가 나눠 쓰고, 한 단어에 여러 개를 얹을 수 있다.</summary>
    [CreateAssetMenu(menuName = "PS/Word", fileName = "Word_")]
    public class WordData : ScriptableObject
    {
        [SerializeField] private int m_Id;
        public int Id => m_Id;

        [SerializeField] private Sprite m_Icon;
        public Sprite Icon => m_Icon;

        [SerializeField] private string m_Word;

        /// <summary>대문자로 정규화. 테이블 키이자 스캐너 비교 대상.</summary>
        public string Word => string.IsNullOrEmpty(m_Word) ? string.Empty : m_Word.ToUpperInvariant();

        [Tooltip("0 이하면 상한 없음")]
        [SerializeField] private int m_MaxEnhancement;
        public int MaxEnhancement => m_MaxEnhancement;

        [Tooltip("켜졌을 때 캐릭터에 걸릴 것들")]
        [SerializeField] private WordEffect[] m_Effects;

        public int EffectCount => m_Effects != null ? m_Effects.Length : 0;
        public WordEffect EffectAt(int index) => m_Effects[index];
    }
}
