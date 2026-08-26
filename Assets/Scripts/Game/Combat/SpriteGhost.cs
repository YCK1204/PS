using PS.Core;
using UnityEngine;

namespace PS.Game.Combat
{
    /// <summary>한 프레임을 복사해 떠 있다가 사라지는 잔상 한 장. 풀에서 나온다.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteGhost : MonoBehaviour, IPoolable
    {
        private static SpriteGhost s_Template;

        private SpriteRenderer m_Renderer;
        private Color m_Start;
        private float m_Life;
        private float m_Age;

        public static SpriteGhost Spawn(SpriteRenderer source, Color tint, float life, int sortingOffset)
            => Spawn(source, source != null ? source.transform.position : Vector3.zero, tint, life, sortingOffset);

        public static SpriteGhost Spawn(SpriteRenderer source, Vector3 position, Color tint, float life, int sortingOffset)
        {
            if (source == null || source.sprite == null) return null;

            SpriteGhost ghost = PoolManager.Get(Template(), position, source.transform.rotation);
            ghost.transform.localScale = source.transform.lossyScale;

            SpriteRenderer renderer = ghost.m_Renderer;
            renderer.sprite = source.sprite;
            renderer.flipX = source.flipX;
            renderer.flipY = source.flipY;
            renderer.sharedMaterial = source.sharedMaterial;
            renderer.sortingLayerID = source.sortingLayerID;
            renderer.sortingOrder = source.sortingOrder + sortingOffset;
            renderer.color = tint;

            ghost.m_Start = tint;
            ghost.m_Life = Mathf.Max(0.01f, life);
            ghost.m_Age = 0f;
            return ghost;
        }

        /// <summary>잔상은 프리팹 에셋이 없다. 꺼둔 원본을 하나 만들어 풀의 씨앗으로 쓴다.</summary>
        private static SpriteGhost Template()
        {
            if (s_Template != null) return s_Template;

            var go = new GameObject("Ghost");
            go.SetActive(false);
            Object.DontDestroyOnLoad(go);
            go.AddComponent<SpriteRenderer>();
            s_Template = go.AddComponent<SpriteGhost>();

            return s_Template;
        }

        private void Awake() => m_Renderer = GetComponent<SpriteRenderer>();

        public void OnGet()
        {
            if (m_Renderer == null) m_Renderer = GetComponent<SpriteRenderer>();
            m_Age = 0f;
        }

        public void OnRelease() { }

        private void Update()
        {
            m_Age += Time.deltaTime;

            float t = m_Age / m_Life;
            if (t >= 1f)
            {
                PoolManager.Release(this);
                return;
            }

            Color c = m_Start;
            c.a = m_Start.a * (1f - t);
            m_Renderer.color = c;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => s_Template = null;
    }
}
