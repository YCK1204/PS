using PS.Game.Inventory;
using UnityEngine;

namespace SO
{
    public abstract class WordData : ScriptableObject
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

        /// <summary>단어가 성립한 순간. match로 강화도·위치를 알 수 있다.</summary>
        public abstract void OnEnableWordEffect(in WordMatch match);

        /// <summary>단어가 깨진 순간. 강화도만 바뀐 경우도 Disable 후 Enable로 온다.</summary>
        public abstract void OnDisableWordEffect(in WordMatch match);
    }
}
