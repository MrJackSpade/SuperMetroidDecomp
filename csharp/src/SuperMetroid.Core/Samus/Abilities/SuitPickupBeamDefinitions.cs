using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Varia/Gravity pickup HDMA curve and native 8.8 geometry.</summary>
public static class SuitPickupBeamDefinitions
{
    /// <summary>Collapsed beam with both window endpoints at X=120.</summary>
    public const ushort NarrowBeamEndpoints = 0x7878;
    /// <summary>Inverted endpoints representing an empty window.</summary>
    public const ushort EmptyWindowEndpoints = 0x00ff;
    /// <summary>Endpoints representing the complete 256-pixel window.</summary>
    public const ushort FullScreenEndpoints = 0xff00;
    /// <summary>Initial unsigned 8.8 widening speed.</summary>
    public const ushort InitialWideningSpeed = 0x0100;
    /// <summary>Per-frame unsigned 8.8 widening acceleration.</summary>
    public const ushort WideningAcceleration = 0x0060;
    /// <summary>Per-frame unsigned 8.8 shrinking deceleration.</summary>
    public const ushort ShrinkingDeceleration = 0x0020;
    /// <summary>Minimum unsigned 8.8 shrinking speed.</summary>
    public const ushort MinimumShrinkingSpeed = 0x0100;
    /// <summary>Stage-two initial asymmetric endpoint pair.</summary>
    public const ushort CurvedWideningStart = 0x846c;
    /// <summary>Stage-five signed starting endpoint pair from <c>$88:E222</c>.</summary>
    public const ushort DissipationStart = 0xf8ff;
    /// <summary>Number of scanlines and window words produced by the HDMA builder.</summary>
    public const int WindowScanlineCount = 256;
}
