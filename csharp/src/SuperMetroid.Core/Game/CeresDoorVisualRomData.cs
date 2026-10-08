namespace SuperMetroid.Core.Game;

/// <summary>Native Ceres-door visual sources used by enemy $E23F at $A6:F6C5/$F850.</summary>
public static class CeresDoorVisualRomData
{
    /// <summary>The variant-two direct four-bit tile DMA at $B0:C400.</summary>
    public const int TileSource = 0xb0c400;
    /// <summary>Byte length of the variant-two four-bit tile transfer.</summary>
    public const int TileByteCount = 0x0400;
    /// <summary>Physical VRAM byte destination of the variant-two tile transfer.</summary>
    public const int TileVramDestination = 0xe000;

    /// <summary>Fifteen normal Ceres-door colors at $A6:F4EE.</summary>
    public const int NormalColors = 0xa6f4ee;
    /// <summary>Fifteen escape Ceres-door colors at $A6:F50E.</summary>
    public const int EscapeColors = 0xa6f50e;
    /// <summary>Number of visible RGB555 colors copied from either setup palette.</summary>
    public const int SetupColorCount = 15;
    /// <summary>First CGRAM color index receiving the normal setup palette.</summary>
    public const int NormalTargetColor = 0x142 / 2;
    /// <summary>First CGRAM color index receiving the active variant's setup palette.</summary>
    public const int ActiveTargetColor = 0x1e2 / 2;

    /// <summary>Eight six-color animation rows at $A6:F871, selected by frame bits 3..5.</summary>
    public const int AnimationColors = 0xa6f871;
    /// <summary>Number of door-glow palette rows in the native cycle.</summary>
    public const int AnimationRowCount = 8;
    /// <summary>Number of RGB555 colors copied from each animation row.</summary>
    public const int AnimationColorCount = 6;
    /// <summary>Byte distance between consecutive native animation rows.</summary>
    public const int AnimationRowByteStride = 0x10;
    /// <summary>First CGRAM color index receiving the animated six-color span.</summary>
    public const int AnimationTargetColor = 0x52 / 2;

    /// <summary>The two $A6:F900 transfer records resolve to four-byte sources at $A6:F918/F91C.</summary>
    public const int Mode7FirstFrameSource = 0xa6f918;
    /// <summary>Native address of the second four-byte Mode 7 map strip.</summary>
    public const int Mode7SecondFrameSource = 0xa6f91c;
    /// <summary>Number of alternating Mode 7 map-strip frames.</summary>
    public const int Mode7FrameCount = 2;
    /// <summary>Byte length of each Mode 7 map-strip transfer.</summary>
    public const int Mode7FrameByteCount = 4;
    /// <summary>Native VRAM word destination for both Mode 7 map strips.</summary>
    public const ushort Mode7DestinationWord = 0x060e;
}
