using PS.Game.Inventory;
using UnityEngine;

namespace SO
{
    public enum ItemRarity
    {
        Normal,
        Rare,
        Epic,
        Legendary
    }

    public enum ItemType
    {
        None,
        Letter,

        /// <summary>결속형 글리프. 칸에 놓으면 칸 스펙만 바꾸고 자신은 소멸한다.</summary>
        BoundGlyph,

        /// <summary>이동형 글리프. 칸을 차지하고 남아서 주변에 효과를 건다.</summary>
        MobileGlyph,
    }

    public abstract class ItemData : ScriptableObject
    {
        [SerializeField] private int m_Id;
        public int Id => m_Id;
        [SerializeField] private Sprite m_Icon;
        public Sprite Icon => m_Icon;
        [Tooltip("화면에 보이는 이름. UnityEngine.Object.m_Name과 겹치면 안 되므로 m_DisplayName")]
        [SerializeField] private string m_DisplayName;
        public string Name => m_DisplayName;
        [SerializeField] private string m_Description;
        public string Description => m_Description;
        [SerializeField] private ItemRarity m_Rarity;
        public ItemRarity Rarity => m_Rarity;

        public abstract ItemType Type { get; }

        /// <summary>놓는 순간 소모되어 격자에 남지 않는가. 결속형 글리프가 그렇다.</summary>
        public virtual bool ConsumedOnPlace => false;

        /// <summary>스프라이트가 없을 때 칸에 대신 그릴 짧은 표기. 프로토타입용.</summary>
        public virtual string ShortLabel => null;

        /// <summary>격자에 놓인 직후. at은 놓인 칸.</summary>
        public virtual void OnEquip(InventoryState state, Vector2Int at) { }

        /// <summary>격자에서 빠지기 직전. OnEquip에서 한 일을 되돌린다.</summary>
        public virtual void OnUnequip(InventoryState state, Vector2Int at) { }

        /// <summary>지금 이 칸에서 빼도 되는가. 뺐을 때 격자가 깨지는 경우 막는다.
        /// 예 — 용량 글리프가 만든 2글자 칸에 글자가 둘 다 들어있으면 못 뺀다.</summary>
        public virtual bool CanUnequip(InventoryState state, Vector2Int at) => true;
    }
}
