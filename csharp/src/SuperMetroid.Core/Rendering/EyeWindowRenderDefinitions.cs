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
    /// <summary>$91 off-screen horizontal window $FF00 covers the entire scanline.</summary>
    internal const ushort FullWindow = 0xff00;
    /// <summary>Right-edge byte mask in the packed bank-$91 window word.</summary>
    internal const int FullRightEdge = 0xff00;
    /// <summary>$91 off-screen branches extend the 8.8 origin by one 256-pixel viewport.</summary>
    internal const int FixedPointSpan = 0x10000;
    /// <summary>$91:C6C1/$C822 duplicate the origin byte into both apex endpoints.</summary>
    internal const int ReplicateByte = 0x0101;
}
