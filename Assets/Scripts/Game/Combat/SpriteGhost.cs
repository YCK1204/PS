using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>한 프레임을 복사해 떠 있다가 사라지는 잔상 한 장.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteGhost : MonoBehaviour
    {
        private SpriteRenderer m_Renderer;
        private Color m_Start;
        private float m_Life;
        private float m_Age;

        public static SpriteGhost Spawn(SpriteRenderer source, Color tint, float life, int sortingOffset)
            => Spawn(source, source != null ? source.transform.position : Vector3.zero, tint, life, sortingOffset);

        public static SpriteGhost Spawn(SpriteRenderer source, Vector3 position, Color tint, float life, int sortingOffset)
        {
            if (source == null || source.sprite == null) return null;

            var go = new GameObject("Ghost");
            go.transform.SetPositionAndRotation(position, source.transform.rotation);
            go.transform.localScale = source.transform.lossyScale;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = source.sprite;
            renderer.flipX = source.flipX;
            renderer.flipY = source.flipY;
            renderer.sharedMaterial = source.sharedMaterial;
            renderer.sortingLayerID = source.sortingLayerID;
            renderer.sortingOrder = source.sortingOrder + sortingOffset;
            renderer.color = tint;

            var ghost = go.AddComponent<SpriteGhost>();
            ghost.m_Renderer = renderer;
            ghost.m_Start = tint;
            ghost.m_Life = Mathf.Max(0.01f, life);
            return ghost;
        }

        private void Update()
        {
            m_Age += Time.deltaTime;

            float t = m_Age / m_Life;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            Color c = m_Start;
            c.a = m_Start.a * (1f - t);
            m_Renderer.color = c;
        }
    }
}
