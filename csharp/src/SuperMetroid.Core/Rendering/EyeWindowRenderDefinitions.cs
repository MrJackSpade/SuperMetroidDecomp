using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rendering;

/// <summary>Screen-coordinate definitions of the X-ray and scanner-eye beam's line table.</summary>
internal static class EyeWindowRenderDefinitions
{
    /// <summary>The beam's apex is one line above the body position it is traced from.</summary>
    internal const int ApexYOffset = 1;
    /// <summary>The lines the beam's table covers.</summary>
    internal const int TableRows = 230;
    /// <summary>A line the beam misses: its left edge, column 255, lies past its right edge, column zero.</summary>
    internal const ushort EmptyWindow = 0x00ff;
    /// <summary>A line the beam covers entirely: left edge 0 in the low byte, right edge 255 in the high byte.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "A beam line with its left edge at column 0 and its right edge at column 255, not a cartridge address.")]
    internal const ushort FullWindow = 0xff00;
    /// <summary>An origin off the screen is moved by one 256-pixel screen width, in 8.8 fixed point.</summary>
    internal const int FixedPointSpan = 0x10000;
    /// <summary>Copies the origin's column into both edges of the apex line.</summary>
    internal const int ReplicateByte = 0x0101;
}
