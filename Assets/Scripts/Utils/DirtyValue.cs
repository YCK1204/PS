namespace Utils
{
    public struct DirtyValue<T>
    {
        private T m_Original;
        private T m_Current;

        public bool IsDirty { get; private set; }

        public T Value
        {
            get => m_Current;
            set { m_Current = value; IsDirty = true; }
        }

        public void Init(T original)
        {
            m_Original = original;
            m_Current = original;
            IsDirty = false;
        }

        public void Clear()
        {
            if (!IsDirty) return;
            m_Current = m_Original;
            IsDirty = false;
        }
    }
}
