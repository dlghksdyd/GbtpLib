using System;

namespace GbtpLib.UI
{
    // Common styles represented as ARGB ints to avoid System.Drawing dependency in the core lib.
    public static class CommonStyles
    {
        public const int PrimaryColorArgb = unchecked((int)0xFF0078D7); // #0078D7
        public const int AccentColorArgb = unchecked((int)0xFFFF8C00);  // #FF8C00
        public const int ErrorColorArgb = unchecked((int)0xFFDC143C);   // #DC143C
    }
}
