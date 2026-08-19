#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Managers
{
    using Opsive.BehaviorDesigner.Editor;
    using Opsive.GraphDesigner.Editor;
    using Opsive.Shared.Editor.Managers;

    /// <summary>
    /// Draws the Behavior Designer settings.
    /// </summary>
    [OrderedEditorItem("Settings", 11)]
    public class SettingsManager : GraphSettingsManager
    {
        private BehaviorDesignerGraphDesignerSettingsProvider m_SettingsProvider;

        /// <summary>
        /// The settings provider used to draw the settings UI.
        /// </summary>
        protected override GraphDesignerSettingsProvider SettingsProvider => m_SettingsProvider ?? (m_SettingsProvider = new BehaviorDesignerGraphDesignerSettingsProvider("Preferences/Behavior Designer"));

        /// <summary>
        /// The graph settings instance.
        /// </summary>
        protected override GraphSettings Settings => BehaviorDesignerSettings.Instance;

        /// <summary>
        /// Is the graph a tree?
        /// </summary>
        protected override bool IsTree => true;
    }
}
#endif