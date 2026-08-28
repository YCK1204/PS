using System;
using UnityEngine;

namespace PS.Game
{
    /// <summary>런 하나를 소유한다. 씬이 바뀌어도 여기 남는다.</summary>
    public static class GameManager
    {
        public static RunProgress Run { get; private set; }

        public static bool InRun => Run != null && !Run.IsOver;

        public static event Action<RunProgress> RunStarted;
        public static event Action<RunProgress> RunEnded;

        public static RunProgress StartRun()
        {
            Run = new RunProgress();
            RunStarted?.Invoke(Run);
            return Run;
        }

        /// <summary>없으면 새로 시작한다. 전투 씬에 바로 들어와도 동작하게.</summary>
        public static RunProgress EnsureRun() => Run != null ? Run : StartRun();

        public static void EndRun()
        {
            if (Run == null) return;

            RunProgress finished = Run;
            Run = null;
            RunEnded?.Invoke(finished);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Run = null;
            RunStarted = null;
            RunEnded = null;
        }
    }
}
