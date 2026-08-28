using PS.Core;
using PS.Game;
using PS.Game.Actors;
using PS.Game.Combat;
using PS.Game.Inventory;
using SO;
using UnityEngine;

namespace PS.Boot
{
    /// <summary>개발용 치트 패널. F1로 여닫는다.
    /// 게임 UI를 건드리지 않게 IMGUI로 그린다 — 배선이 필요 없고 실물과 헷갈리지 않는다.</summary>
    public class DevPanel : MonoBehaviour
    {
        [SerializeField] private KeyCode m_Toggle = KeyCode.F1;
        [SerializeField] private bool m_OpenOnStart;

        [Tooltip("패널 위치와 폭. 게임 UI와 겹치지 않게 조절한다")]
        [SerializeField] private Rect m_Area = new Rect(12f, 108f, 250f, 0f);

        private RunState m_Run;
        private BattleFlow m_Flow;
        private PS.UI.Inventory m_Inventory;

        private bool m_Open;
        private string m_Message = string.Empty;
        private float m_MessageUntil;
        private Vector2 m_Scroll;

        private bool Enabled => Application.isEditor || Debug.isDebugBuild;

        private void Start()
        {
            m_Open = m_OpenOnStart;
            Rebind();
        }

        private void Rebind()
        {
            m_Run = Object.FindFirstObjectByType<RunState>();
            m_Flow = Object.FindFirstObjectByType<BattleFlow>();
            m_Inventory = Object.FindFirstObjectByType<PS.UI.Inventory>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            if (!Enabled) return;
            if (Input.GetKeyDown(m_Toggle)) m_Open = !m_Open;
        }

        private void OnGUI()
        {
            if (!Enabled) return;

            if (!m_Open)
            {
                GUI.Label(new Rect(12f, Screen.height - 26f, 300f, 22f), "F1 — 개발 패널");
                return;
            }

            float height = m_Area.height > 0f ? m_Area.height : Screen.height - m_Area.y - 12f;
            GUILayout.BeginArea(new Rect(m_Area.x, m_Area.y, m_Area.width, height), GUI.skin.box);
            m_Scroll = GUILayout.BeginScrollView(m_Scroll);

            GUILayout.Label("개발 패널 (F1)");
            GUILayout.Label(Status());
            GUILayout.Space(6f);

            DrawRunSection();
            DrawGridSection();
            DrawCombatSection();
            DrawTimeSection();

            if (Time.time < m_MessageUntil)
            {
                GUILayout.Space(6f);
                GUILayout.Label(m_Message);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private string Status()
        {
            RunProgress run = GameManager.Run;
            string grid = m_Run != null && m_Run.Inventory != null
                ? m_Run.Inventory.Grid.Width + "x" + m_Run.Inventory.Grid.Height
                : "-";

            return (run != null ? run.Label + " · 뼈 " + run.Bones : "런 없음") + "\n격자 " + grid + " · 적 " + AliveEnemies();
        }

        // ------------------------------------------------------------ 런

        private void DrawRunSection()
        {
            GUILayout.Label("── 런");

            if (GUILayout.Button("적 전멸 (맵 클리어)")) KillAll();
            if (GUILayout.Button("적 부활")) ReviveAll();

            if (GUILayout.Button("뼈 +1"))
            {
                GameManager.Run?.AddBones(1);
                Toast("뼈 +1");
            }

            if (GUILayout.Button("맵 건너뛰기 (보상 없이)"))
            {
                GameManager.Run?.AdvanceMap();
                ReviveAll();
                Toast("다음 맵");
            }

            if (GUILayout.Button("타이틀로"))
            {
                GameManager.EndRun();
                SceneManager.Load(SceneType.Title);
            }
        }

        // ------------------------------------------------------------ 격자

        private void DrawGridSection()
        {
            GUILayout.Space(6f);
            GUILayout.Label("── 격자");

            if (m_Run == null || m_Run.Inventory == null)
            {
                if (GUILayout.Button("RunState 다시 찾기")) Rebind();
                return;
            }

            if (GUILayout.Button("세로 +1줄"))
            {
                m_Run.Inventory.Grid.Expand(1);
                m_Run.Inventory.Rescan();
                Toast("격자 " + m_Run.Inventory.Grid.Width + "x" + m_Run.Inventory.Grid.Height);
            }

            if (GUILayout.Button("랜덤 글자 +5")) AddLetters(5);
            if (GUILayout.Button("랜덤 글자 +20")) AddLetters(20);
            if (GUILayout.Button("격자 비우기")) ClearGrid();

            if (GUILayout.Button(m_Inventory != null && m_Inventory.IsOpen ? "인벤 닫기" : "인벤 열기"))
            {
                if (m_Inventory == null) Rebind();
                if (m_Inventory == null) return;

                if (m_Inventory.IsOpen) m_Inventory.Close();
                else m_Inventory.Open();
            }
        }

        // ------------------------------------------------------------ 전투

        private void DrawCombatSection()
        {
            GUILayout.Space(6f);
            GUILayout.Label("── 전투");

            Character player = Object.FindFirstObjectByType<Character>();
            if (player == null) return;

            Health health = player.Health;
            if (health != null && GUILayout.Button(health.Immortal ? "플레이어 무적 끄기" : "플레이어 무적 켜기"))
            {
                health.Immortal = !health.Immortal;
                Toast("플레이어 무적 " + (health.Immortal ? "켜짐" : "꺼짐"));
            }

            if (GUILayout.Button("허수아비 무적 토글")) ToggleEnemyImmortal();
        }

        // ------------------------------------------------------------ 시간

        private void DrawTimeSection()
        {
            GUILayout.Space(6f);
            GUILayout.Label("── 시간  x" + Time.timeScale.ToString("0.##"));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.25")) Time.timeScale = 0.25f;
            if (GUILayout.Button("1")) Time.timeScale = 1f;
            if (GUILayout.Button("2")) Time.timeScale = 2f;
            if (GUILayout.Button("4")) Time.timeScale = 4f;
            GUILayout.EndHorizontal();
        }

        // ------------------------------------------------------------ 동작

        private int AliveEnemies()
        {
            int alive = 0;
            foreach (Dummy dummy in Object.FindObjectsByType<Dummy>(FindObjectsSortMode.None))
                if (dummy.IsAlive) alive++;

            return alive;
        }

        private void KillAll()
        {
            foreach (Dummy dummy in Object.FindObjectsByType<Dummy>(FindObjectsSortMode.None))
            {
                Health health = dummy.Health;
                if (health == null || !health.IsAlive) continue;

                bool immortal = health.Immortal;
                health.Immortal = false;
                health.TakeDamage(new DamageInfo { Amount = health.Max + 1f, Source = gameObject });
                health.Immortal = immortal;
            }

            Toast("적 전멸");
        }

        private void ReviveAll()
        {
            foreach (Dummy dummy in Object.FindObjectsByType<Dummy>(FindObjectsSortMode.None))
                dummy.Respawn(dummy.transform.position);

            Toast("적 부활");
        }

        private void ToggleEnemyImmortal()
        {
            bool next = true;
            foreach (Dummy dummy in Object.FindObjectsByType<Dummy>(FindObjectsSortMode.None))
            {
                if (dummy.Health == null) continue;

                next = !dummy.Health.Immortal;
                break;
            }

            foreach (Dummy dummy in Object.FindObjectsByType<Dummy>(FindObjectsSortMode.None))
                if (dummy.Health != null) dummy.Health.Immortal = next;

            Toast("허수아비 무적 " + (next ? "켜짐" : "꺼짐"));
        }

        private void AddLetters(int count)
        {
            if (m_Run == null || m_Run.Letters == null) return;

            var pool = m_Run.Letters.Letters;
            if (pool == null || pool.Count == 0) return;

            int added = 0;
            for (int i = 0; i < count; i++)
                if (m_Run.Inventory.TryAdd(pool[Random.Range(0, pool.Count)])) added++;

            Toast("글자 +" + added + (added < count ? " (자리 부족)" : string.Empty));
        }

        private void ClearGrid()
        {
            InventoryGrid grid = m_Run.Inventory.Grid;

            for (int y = 0; y < grid.Height; y++)
                for (int x = 0; x < grid.Width; x++)
                    while (grid[new Vector2Int(x, y)].Count > 0)
                        grid.Take(new Vector2Int(x, y));

            m_Run.Inventory.Rescan();
            Toast("격자 비움");
        }

        private void Toast(string message)
        {
            m_Message = message;
            m_MessageUntil = Time.time + 2f;
        }
    }
}
