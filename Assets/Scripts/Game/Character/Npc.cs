using UnityEngine;
using UnityEngine.Events;

namespace PS.Game.Actors
{
    /// <summary>말 걸거나 여는 것. 체력도 전투도 없다.</summary>
    public class Npc : Actor
    {
        [SerializeField] private string m_Label;
        [SerializeField] private float m_Radius = 1.2f;
        [SerializeField] private UnityEvent m_Interacted;

        public string Label => m_Label;
        public float Radius => m_Radius;

        public bool InRange(Vector2 point) => ((Vector2)Center.position - point).sqrMagnitude <= m_Radius * m_Radius;

        public void Interact() => m_Interacted?.Invoke();

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Center.position, m_Radius);
        }
    }
}
