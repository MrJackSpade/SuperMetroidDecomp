namespace SuperMetroid.Core.Fx;

/// <summary>The Landing Site's scrolling sky: its bands' count and the world rows they cover (#1415).</summary>
public static class ScrollingSkyBandDefinitions
{
    /// <summary>The sky bands, from the top of the room down.</summary>
    public const int SectionCount = 23;

    /// <summary>The horizontal-scroll accumulators the bands add to.</summary>
    public const int DataSlotCount = 23;

    /// <summary>The world row below the last band.</summary>
    public const int WorldEndPosition = 1280;

    /// <summary>First gameplay scanline below the 32-pixel HUD.</summary>
    public const int GameplayFirstScanline = 32;
}
