using UnityEngine.SceneManagement;

namespace PS.Core
{
    /// <summary>씬 이름을 한 곳에 모아둔다. 문자열을 여기저기 흩뿌리지 않게.</summary>
    public static class SceneRouter
    {
        public const string Title = "TitleScene";
        public const string Battle = "BattleScene";

        public static void Load(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return;

            PS.UI.UIStack.CloseAll();
            SceneManager.LoadScene(scene);
        }

        public static void ToTitle() => Load(Title);
        public static void ToBattle() => Load(Battle);
    }
}
