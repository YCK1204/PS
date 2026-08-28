using System.Collections.Generic;
using PS.Core;
using PS.Game;
using PS.Game.Actors;
using PS.Game.Inventory;
using PS.Game.Combat;
using PS.UI;
using SO;
using TMPro;
using UnityEngine;

namespace PS.Boot
{
    /// <summary>전투 씬의 한 바퀴 — 적을 다 잡으면 보상, 고르면 다음 맵.
    /// 모델(RunState)과 화면(RewardPanel)을 잇는 자리라 PS.Boot에 둔다.</summary>
    public class BattleFlow : MonoBehaviour
    {
        [SerializeField] private RunState m_Run;
        [SerializeField] private RewardPanel m_Reward;
        [SerializeField] private RewardTable m_Table;
        [SerializeField] private PlayerController m_Player;
        [Tooltip("결속형 글리프처럼 직접 놓아야 하는 보상을 받으면 이걸 연다")]
        [SerializeField] private UIPanel m_Inventory;

        [SerializeField] private TMP_Text m_Hud;

        [Tooltip("적이 전멸한 뒤 보상 화면이 뜨기까지")]
        [SerializeField] private float m_ClearDelay = 0.7f;

        [Header("보스 확정 보상")]
        [Tooltip("보스마다 늘려줄 격자 줄 수")]
        [SerializeField] private int m_BossExpandRows = 1;

        [Tooltip("격자 세로 최대. 기획상 8x8")]
        [SerializeField] private int m_MaxGridHeight = 8;

        private readonly List<Dummy> m_Enemies = new List<Dummy>();
        private readonly List<Vector3> m_Spawns = new List<Vector3>();
        private readonly List<LetterData> m_Bundle = new List<LetterData>();
        private readonly List<GlyphData> m_Glyphs = new List<GlyphData>();
        private readonly List<string> m_Options = new List<string>();

        private RunProgress m_Progress;
        private float m_ClearAt = float.PositiveInfinity;
        private bool m_Rewarding;

        private void Start()
        {
            m_Progress = GameManager.EnsureRun();

            foreach (Dummy dummy in Object.FindObjectsByType<Dummy>(FindObjectsSortMode.None))
            {
                m_Enemies.Add(dummy);
                m_Spawns.Add(dummy.transform.position);
            }

            ReviveEnemies();
            RefreshHud();
        }

        private void Update()
        {
            if (m_Rewarding || m_Progress == null || m_Progress.IsOver) return;

            if (AliveEnemies() > 0)
            {
                m_ClearAt = float.PositiveInfinity;
                return;
            }

            if (float.IsPositiveInfinity(m_ClearAt))
            {
                m_ClearAt = Time.time + m_ClearDelay;
                return;
            }

            if (Time.time < m_ClearAt) return;

            m_ClearAt = float.PositiveInfinity;
            OpenReward();
        }

        private int AliveEnemies()
        {
            int alive = 0;
            for (int i = 0; i < m_Enemies.Count; i++)
                if (m_Enemies[i] != null && m_Enemies[i].IsAlive) alive++;

            return alive;
        }

        private void ReviveEnemies()
        {
            for (int i = 0; i < m_Enemies.Count; i++)
                if (m_Enemies[i] != null) m_Enemies[i].Respawn(m_Spawns[i]);
        }

        private void RefreshHud()
        {
            if (m_Hud == null || m_Progress == null) return;
            m_Hud.text = m_Progress.Label + "   뼈 " + m_Progress.Bones;
        }

        // ------------------------------------------------------------ 보상

        private void OpenReward()
        {
            m_Rewarding = true;
            Block(true);

            if (!m_Progress.GivesReward)
            {
                Finish();
                return;
            }

            bool boss = m_Progress.IsBossMap;
            int expanded = 0;

            if (boss)
            {
                m_Progress.AddBones(m_Table != null ? m_Table.BossBones : 1);
                expanded = ExpandGrid();
            }

            m_Options.Clear();
            m_Options.Add("알파벳");
            m_Options.Add("글리프");
            m_Options.Add("뼈");

            m_Reward.Show(
                m_Progress.Label + (boss ? " · 보스 처치" : " · 클리어"),
                boss ? "보스 확정 보상: 뼈 +" + (m_Table != null ? m_Table.BossBones : 1)
                        + (expanded > 0 ? " · 격자 " + expanded + "줄 확장" : " · 격자는 최대치")
                        + " — 하나 더 고르세요"
                     : "무엇을 받을지 고르세요",
                m_Options, OnKindPicked);
        }

        private void OnKindPicked(int index)
        {
            switch (index)
            {
                case 0: ShowLetterStage(); break;
                case 1: ShowGlyphStage(); break;
                default:
                    m_Progress.AddBones(m_Table != null ? m_Table.MapBones : 1);
                    Complete();
                    break;
            }
        }

        // --- 알파벳 ---

        private void ShowLetterStage()
        {
            bool boss = m_Progress.IsBossMap;
            m_Table.RollBundle(m_Run.Letters, m_Bundle, boss);

            m_Options.Clear();
            m_Options.Add("낱개 — 원하는 글자 1개");
            m_Options.Add("꾸러미 — 랜덤 " + m_Bundle.Count + "장");

            m_Reward.Show("알파벳", "정확도냐 양이냐", m_Options, OnLetterModePicked);
        }

        private void OnLetterModePicked(int index)
        {
            if (index == 1)
            {
                int added = 0;
                for (int i = 0; i < m_Bundle.Count; i++)
                    if (m_Run.Inventory.TryAdd(m_Bundle[i])) added++;

                if (added < m_Bundle.Count)
                    Debug.LogWarning($"[보상] 격자가 가득 차 {m_Bundle.Count - added}장 버려짐");

                Complete();
                return;
            }

            IReadOnlyList<LetterData> pool = m_Table.ResolveLetters(m_Run.Letters);

            m_Options.Clear();
            for (int i = 0; i < pool.Count; i++) m_Options.Add(pool[i].Letter.ToString());

            m_Reward.Show("낱개", "원하는 글자를 고르세요", m_Options, delegate (int pick)
            {
                IReadOnlyList<LetterData> letters = m_Table.ResolveLetters(m_Run.Letters);
                if (pick >= 0 && pick < letters.Count) Grant(letters[pick]);
                Complete();
            });
        }

        // --- 글리프 ---

        private void ShowGlyphStage()
        {
            m_Table.RollGlyphs(m_Glyphs);

            if (m_Glyphs.Count == 0)
            {
                Complete();
                return;
            }

            m_Options.Clear();
            for (int i = 0; i < m_Glyphs.Count; i++) m_Options.Add(m_Glyphs[i].Name);

            m_Reward.Show("글리프", "하나 고르세요", m_Options, delegate (int pick)
            {
                if (pick >= 0 && pick < m_Glyphs.Count) Grant(m_Glyphs[pick]);
                Complete();
            });
        }

        /// <summary>격자를 늘린다. 최대치를 넘지 않는다. 실제로 늘어난 줄 수를 돌려준다.</summary>
        private int ExpandGrid()
        {
            InventoryGrid grid = m_Run.Inventory.Grid;
            int rows = Mathf.Min(m_BossExpandRows, Mathf.Max(0, m_MaxGridHeight - grid.Height));
            if (rows <= 0) return 0;

            grid.Expand(rows);
            m_Run.Inventory.Rescan();
            return rows;
        }

        /// <summary>하나를 넘겨준다. 자리가 없거나 이미 뭔가 들고 있으면 조용히 사라지므로 알린다.</summary>
        private bool Grant(ItemData item)
        {
            if (m_Run.Inventory.TryAdd(item)) return true;

            Debug.LogWarning($"[보상] {item.Name}을(를) 받지 못했다 — 격자가 가득 찼거나 이미 무언가 들고 있음");
            return false;
        }

        // --- 마무리 ---

        private void Complete()
        {
            m_Reward.Close();
            m_Progress.AdvanceMap();
            RefreshHud();

            if (m_Progress.IsOver)
            {
                Finish();
                return;
            }

            ReviveEnemies();
            Block(false);
            m_Rewarding = false;

            // 손에 들린 게 있으면 놓을 자리를 골라야 하니 격자를 열어준다.
            if (m_Inventory != null && m_Run.Inventory.Held != null && !m_Inventory.IsOpen)
                m_Inventory.Open();
        }

        private void Finish()
        {
            m_Options.Clear();
            m_Options.Add("타이틀로");

            m_Reward.Show("런 종료", "맵 " + m_Progress.MapsCleared + "개 클리어 · 뼈 " + m_Progress.Bones,
                m_Options, delegate
                {
                    m_Reward.Close();
                    GameManager.EndRun();
                    SceneManager.Load(SceneType.Title);
                });
        }

        private void Block(bool blocked)
        {
            if (m_Player != null) m_Player.Blocked = blocked;
        }
    }
}
