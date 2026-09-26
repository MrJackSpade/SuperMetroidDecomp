namespace SuperMetroid.Core.Game;

/// <summary>Native Ceres-door visual sources used by enemy $E23F at $A6:F6C5/$F850.</summary>
public static class CeresDoorVisualRomData
{
    /// <summary>The variant-two direct four-bit tile DMA at $B0:C400.</summary>
    public const int TileSource = 0xb0c400;
    public const int TileByteCount = 0x0400;
    public const int TileVramDestination = 0xe000;

    /// <summary>Fifteen normal Ceres-door colors at $A6:F4EE.</summary>
    public const int NormalColors = 0xa6f4ee;
    /// <summary>Fifteen escape Ceres-door colors at $A6:F50E.</summary>
    public const int EscapeColors = 0xa6f50e;
    public const int SetupColorCount = 15;
    public const int NormalTargetColor = 0x142 / 2;
    public const int ActiveTargetColor = 0x1e2 / 2;

    /// <summary>Eight six-color animation rows at $A6:F871, selected by frame bits 3..5.</summary>
    public const int AnimationColors = 0xa6f871;
    public const int AnimationRowCount = 8;
    public const int AnimationColorCount = 6;
    public const int AnimationRowByteStride = 0x10;
    public const int AnimationTargetColor = 0x52 / 2;

    /// <summary>The two $A6:F900 transfer records resolve to four-byte sources at $A6:F918/F91C.</summary>
    public const int Mode7FirstFrameSource = 0xa6f918;
    public const int Mode7SecondFrameSource = 0xa6f91c;
    public const int Mode7FrameCount = 2;
    public const int Mode7FrameByteCount = 4;
    public const ushort Mode7DestinationWord = 0x060e;
}
