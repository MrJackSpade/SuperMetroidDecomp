namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The suit pickup's light beam: its authored edges, the speeds its edges move at, and its rounded sides.</summary>
public static class SuitPickupBeamDefinitions
{
    /// <summary>Collapsed beam with both edges at X=120.</summary>
    public static readonly SuitPickupBeamEdges NarrowBeamEdges = SuitPickupBeamEdges.Columns(120, 120);
    /// <summary>Edges spanning the whole 256-pixel screen line.</summary>
    public static readonly SuitPickupBeamEdges FullScreenWindow = SuitPickupBeamEdges.Columns(0, 255);
    /// <summary>Initial unsigned 8.8 widening speed.</summary>
    public const int InitialWideningSpeed = 256;
    /// <summary>Per-frame unsigned 8.8 widening acceleration.</summary>
    public const int WideningAcceleration = 96;
    /// <summary>Per-frame unsigned 8.8 shrinking deceleration.</summary>
    public const int ShrinkingDeceleration = 32;
    /// <summary>Minimum unsigned 8.8 shrinking speed.</summary>
    public const int MinimumShrinkingSpeed = 256;
    /// <summary>Stage-two initial asymmetric edge pair.</summary>
    public static readonly SuitPickupBeamEdges CurvedWideningStart = SuitPickupBeamEdges.Columns(108, 132);
    /// <summary>Stage-five starting edge pair, whose left edge wraps to the screen's left on the first narrowing.</summary>
    public static readonly SuitPickupBeamEdges DissipationStart = SuitPickupBeamEdges.Columns(255, 248);
    /// <summary>The lines the beam spans from the top of the screen; its middle line is half way down them.</summary>
    public const int BeamLines = 256;
    /// <summary>How far the curved beam's rounded sides reach beyond its column at the middle line, in pixels.</summary>
    public const int CurveWidth = 24;
}
