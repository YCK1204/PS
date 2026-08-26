using PS.Core;
using UnityEngine;

namespace PS.Boot
{
    /// <summary>타이틀 씬 진입점.</summary>
    public class TitleScene : BaseScene
    {
        private void Reset() => m_Type = SceneType.Title;

        protected override void Init()
        {
            m_Type = SceneType.Title;
            GameSettings.Apply();
        }
    }
}
