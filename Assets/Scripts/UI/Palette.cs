using UnityEngine;

namespace PS.UI
{
    /// <summary>프로토타입 팔레트. 아트 톤이 정해지면 여기만 바꾼다.</summary>
    public static class Palette
    {
        public static readonly Color Bg        = Hex("#0D0F16");
        public static readonly Color Panel     = Hex("#171A24");
        public static readonly Color PanelAlt  = Hex("#232838");
        public static readonly Color Row       = Hex("#1E2330");
        public static readonly Color RowAlt    = Hex("#252B3A");
        public static readonly Color Border    = Hex("#333A4D");
        public static readonly Color Text      = Hex("#E6E9F0");
        public static readonly Color TextMuted = Hex("#8B93A7");
        public static readonly Color TextDim   = Hex("#5F677C");
        public static readonly Color Accent    = Hex("#F0C674");
        public static readonly Color AccentInk = Hex("#12141C");
        public static readonly Color Danger    = Hex("#E56C6C");

        public static Color Hex(string hex)
            => ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.magenta;
    }
}
