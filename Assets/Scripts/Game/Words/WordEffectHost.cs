using PS.Game.Actors;
using PS.Game.Inventory;
using SO;
using UnityEngine;

namespace PS.Game.Words
{
    /// <summary>격자에서 켜진 단어를 캐릭터에 얹는다.
    /// 모델(InventoryState)은 씬을 모르고, 씬이 모델을 구독한다 — 의존은 한 방향.</summary>
    [RequireComponent(typeof(Combatant))]
    public class WordEffectHost : MonoBehaviour
    {
        private Combatant m_Character;
        private InventoryState m_Inventory;

        private void Awake() => m_Character = GetComponent<Combatant>();

        public void Bind(InventoryState inventory)
        {
            Unbind();

            m_Inventory = inventory;
            if (m_Inventory == null) return;

            m_Inventory.WordEnabled += OnWordEnabled;
            m_Inventory.WordDisabled += OnWordDisabled;

            // 붙기 전에 이미 켜져 있던 단어를 따라잡는다.
            for (int i = 0; i < m_Inventory.WordRows.Count; i++)
            {
                WordProgress row = m_Inventory.WordRows[i];
                if (row.Active) OnWordEnabled(row.Word, row.Enhancement);
            }
        }

        public void Unbind()
        {
            if (m_Inventory == null) return;

            m_Inventory.WordEnabled -= OnWordEnabled;
            m_Inventory.WordDisabled -= OnWordDisabled;
            m_Inventory = null;
        }

        private void OnDisable() => Unbind();

        private void OnWordEnabled(WordData word, int enhancement)
        {
            if (word == null) return;

            for (int i = 0; i < word.EffectCount; i++)
                word.EffectAt(i)?.Apply(m_Character, word, enhancement);
        }

        private void OnWordDisabled(WordData word, int enhancement)
        {
            if (word == null) return;

            for (int i = 0; i < word.EffectCount; i++)
                word.EffectAt(i)?.Remove(m_Character, word);
        }
    }
}
