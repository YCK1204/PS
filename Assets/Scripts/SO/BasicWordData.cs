using PS.Game.Inventory;
using UnityEngine;

namespace SO
{
    /// <summary>효과 없는 단어. 켜지고 꺼지는 것만 로그로 확인하는 프로토타입용.</summary>
    [CreateAssetMenu(menuName = "PS/Word/Basic", fileName = "Word_")]
    public class BasicWordData : WordData
    {
        [Tooltip("켜고 꺼질 때 콘솔에 남길지")]
        [SerializeField] private bool m_LogToggle = true;

        public override void OnEnableWordEffect(in WordMatch match)
        {
            if (m_LogToggle) Debug.Log($"[워드 ON ] {Word} {match.Enhancement}강  @{match.Origin} 방향{match.Direction}", this);
        }

        public override void OnDisableWordEffect(in WordMatch match)
        {
            if (m_LogToggle) Debug.Log($"[워드 OFF] {Word} {match.Enhancement}강  @{match.Origin} 방향{match.Direction}", this);
        }
    }
}
