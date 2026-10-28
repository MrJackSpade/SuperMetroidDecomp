using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rendering;

/// <summary>Display-coordinate definitions from the bank-$91 eye/X-ray window calculation.</summary>
internal static class EyeWindowRenderDefinitions
{
    /// <summary>$91:C5FF and sibling on-screen branches write the apex at r24-1, one line above the supplied body Y.</summary>
    internal const int ApexYOffset = 1;
    /// <summary>$91 ramp/fill routines stop at byte offset 460, covering 230 window words.</summary>
    internal const int TableRows = 230;
    /// <summary>$91 window fill $00FF: left 255 exceeds right zero, disabling the row.</summary>
    internal const ushort EmptyWindow = 0x00ff;
    /// <summary>The window row covering the entire scanline: left edge 0 in the low byte, right edge 255 in the high byte.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "A window row with its left edge at column 0 and its right edge at column 255, not a cartridge address.")]
    internal const ushort FullWindow = 0xff00;
    /// <summary>The right-edge byte of a window row, which holds its rightmost column.</summary>
    [SuppressMessage("Design", "SME6447", Justification =
        "The right-edge byte of a window row (left and right edge columns side by side), not a cartridge address.")]
    internal const int FullRightEdge = 0xff00;
    /// <summary>$91 off-screen branches extend the 8.8 origin by one 256-pixel viewport.</summary>
    internal const int FixedPointSpan = 0x10000;
    /// <summary>$91:C6C1/$C822 duplicate the origin byte into both apex endpoints.</summary>
    internal const int ReplicateByte = 0x0101;
}
