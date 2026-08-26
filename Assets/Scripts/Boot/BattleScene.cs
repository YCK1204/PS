using PS.Core;
using PS.Game;
using UnityEngine;

namespace PS.Boot
{
    /// <summary>전투 씬 진입점. 런 상태를 켜고 화면에 물린다.</summary>
    public class BattleScene : BaseScene
    {
        [SerializeField] private RunState m_Run;

        public RunState Run => m_Run;

        private void Reset() => m_Type = SceneType.Battle;

        protected override void Init()
        {
            m_Type = SceneType.Battle;

            if (m_Run == null) m_Run = Object.FindFirstObjectByType<RunState>();
        }
    }
}
