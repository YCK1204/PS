using System.Collections.Generic;

namespace Utils
{
    /// <summary>취소/적용이 있는 값. Revert로 되돌리고 Commit으로 현재 값을 새 기준으로 삼는다.</summary>
    public class DirtyValue<T>
    {
        private T m_Original;
        private T m_Current;

        public DirtyValue() { }
        public DirtyValue(T original) { Init(original); }

        public bool IsDirty => !EqualityComparer<T>.Default.Equals(m_Current, m_Original);
        public T Original => m_Original;

        public T Value
        {
            get => m_Current;
            set => m_Current = value;
        }

        public void Init(T original)
        {
            m_Original = original;
            m_Current = original;
        }

        public void Revert() => m_Current = m_Original;
        public void Commit() => m_Original = m_Current;

        public static implicit operator T(DirtyValue<T> value) => value.m_Current;
    }
}
