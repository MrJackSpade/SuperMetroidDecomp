namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Varia/Gravity pickup HDMA curve and native 8.8 geometry.</summary>
public static class SuitPickupBeamDefinitions
{
    /// <summary>Collapsed beam with both window endpoints at X=120.</summary>
    public static readonly SuitPickupBeamEdges NarrowBeamEdges = new(0x78, 0x78);
    /// <summary>Inverted endpoints representing an empty window.</summary>
    public static readonly SuitPickupBeamEdges EmptyWindow = new(0xff, 0x00);
    /// <summary>Endpoints representing the complete 256-pixel window.</summary>
    public static readonly SuitPickupBeamEdges FullScreenWindow = new(0x00, 0xff);
    /// <summary>Initial unsigned 8.8 widening speed.</summary>
    public const ushort InitialWideningSpeed = 0x0100;
    /// <summary>Per-frame unsigned 8.8 widening acceleration.</summary>
    public const ushort WideningAcceleration = 0x0060;
    /// <summary>Per-frame unsigned 8.8 shrinking deceleration.</summary>
    public const ushort ShrinkingDeceleration = 0x0020;
    /// <summary>Minimum unsigned 8.8 shrinking speed.</summary>
    public const ushort MinimumShrinkingSpeed = 0x0100;
    /// <summary>Stage-two initial asymmetric endpoint pair.</summary>
    public static readonly SuitPickupBeamEdges CurvedWideningStart = new(0x6c, 0x84);
    /// <summary>Stage-five signed starting endpoint pair from <c>$88:E222</c>.</summary>
    public static readonly SuitPickupBeamEdges DissipationStart = new(0xff, 0xf8);
    /// <summary>Number of scanlines and window words produced by the HDMA builder.</summary>
    public const int WindowScanlineCount = 256;
}
