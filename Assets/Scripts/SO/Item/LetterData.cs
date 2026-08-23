using UnityEngine;

namespace SO
{
    [CreateAssetMenu(menuName = "PS/Letter", fileName = "Letter_")]
    public class LetterData : ItemData
    {
        [SerializeField] private char m_Letter;

        /// <summary>대문자로 정규화. 스캐너는 이 값만 본다.</summary>
        public char Letter => char.ToUpperInvariant(m_Letter);

        public override ItemType Type => ItemType.Letter;

        public override string ShortLabel => Letter.ToString();
    }
}
