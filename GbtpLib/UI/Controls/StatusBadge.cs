using System;

namespace GbtpLib.UI.Controls
{
    // UI control placeholders to keep library compile-only without WinForms/WPF dependency.
    // Actual implementations should live in application UI projects.
    public class StatusBadge
    {
        public enum StatusLevel { Info, Warning, Error }
        public StatusLevel Level { get; set; } = StatusLevel.Info;
        public string Text { get; set; } = "INFO";
        public int BackColorArgb { get; private set; }
        public int ForeColorArgb { get; private set; }

        public StatusBadge()
        {
            ApplyStyle();
        }
        private void ApplyStyle()
        {
            switch (Level)
            {
                case StatusLevel.Info:
                    BackColorArgb = unchecked((int)0xFF87CEFA); // LightSkyBlue
                    ForeColorArgb = unchecked((int)0xFFFFFFFF); // White
                    Text = "INFO";
                    break;
                case StatusLevel.Warning:
                    BackColorArgb = unchecked((int)0xFFDAA520); // Goldenrod
                    ForeColorArgb = unchecked((int)0xFF000000); // Black
                    Text = "WARN";
                    break;
                case StatusLevel.Error:
                    BackColorArgb = unchecked((int)0xFFB22222); // Firebrick
                    ForeColorArgb = unchecked((int)0xFFFFFFFF); // White
                    Text = "ERROR";
                    break;
            }
        }
    }
}
