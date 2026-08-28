using System;

namespace PS.Game
{
    /// <summary>런 1회분의 진행 상태. 씬과 무관한 순수 데이터다.</summary>
    public class RunProgress
    {
        public const int RoundCount = 5;
        public const int MapsPerRound = 5;

        /// <summary>1 ~ RoundCount.</summary>
        public int Round { get; private set; } = 1;

        /// <summary>라운드 안에서 몇 번째 맵인가. 0 ~ MapsPerRound-1.</summary>
        public int MapIndex { get; private set; }

        public int Bones { get; private set; }
        public int Gold { get; private set; }
        public int MapsCleared { get; private set; }

        /// <summary>라운드의 마지막 맵은 보스.</summary>
        public bool IsBossMap => MapIndex == MapsPerRound - 1;

        /// <summary>마지막 라운드의 보스는 보상이 없다. 런이 거기서 끝난다.</summary>
        public bool GivesReward => !(IsBossMap && Round >= RoundCount);

        public bool IsOver { get; private set; }

        public event Action Changed;

        public void AddBones(int amount)
        {
            if (amount == 0) return;

            Bones += amount;
            Changed?.Invoke();
        }

        public void AddGold(int amount)
        {
            if (amount == 0) return;

            Gold += amount;
            Changed?.Invoke();
        }

        public bool SpendGold(int amount)
        {
            if (amount <= 0 || Gold < amount) return false;

            Gold -= amount;
            Changed?.Invoke();
            return true;
        }

        /// <summary>맵 하나를 끝냈다. 라운드 끝이면 다음 라운드로 넘어간다.</summary>
        public void AdvanceMap()
        {
            if (IsOver) return;

            MapsCleared++;

            if (IsBossMap)
            {
                if (Round >= RoundCount)
                {
                    IsOver = true;
                    Changed?.Invoke();
                    return;
                }

                Round++;
                MapIndex = 0;
            }
            else
            {
                MapIndex++;
            }

            Changed?.Invoke();
        }

        public string Label => IsOver
            ? "런 종료"
            : $"라운드 {Round}/{RoundCount} · 맵 {MapIndex + 1}/{MapsPerRound}" + (IsBossMap ? " (보스)" : string.Empty);
    }
}
