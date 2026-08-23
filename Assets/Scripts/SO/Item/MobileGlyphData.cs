namespace SO
{
    /// <summary>이동형. 칸 하나를 차지하고 남아서 주변 칸에 효과를 건다. 다시 집어서 옮길 수 있다.</summary>
    public abstract class MobileGlyphData : GlyphData
    {
        public sealed override ItemType Type => ItemType.MobileGlyph;
    }
}
