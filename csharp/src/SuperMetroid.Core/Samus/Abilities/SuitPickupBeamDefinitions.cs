namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The suit pickup's light beam: when it starts, the order of its stages, its authored edges, the speeds its edges move
/// at, and its rounded sides.
/// </summary>
public static class SuitPickupBeamDefinitions
{
    /// <summary>The updates the pickup waits before the light beam's first stage runs: the beam starts on its third update.</summary>
    public const int UpdatesBeforeLightBeam = 2;
    /// <summary>The light beam's stages in the order they run (#1928).</summary>
    public static IReadOnlyList<SamusSuitPickupStage> StageOrder { get; } =
    [
        SamusSuitPickupStage.LightBeamAppears,
        SamusSuitPickupStage.UniformWidening,
        SamusSuitPickupStage.CurvedWidening,
        SamusSuitPickupStage.RevealSuit,
        SamusSuitPickupStage.LightBeamShrinks,
        SamusSuitPickupStage.LightBeamDissipates,
        SamusSuitPickupStage.Finish,
    ];
    /// <summary>Collapsed beam with both edges at X=120.</summary>
    public static readonly SuitPickupBeamEdges NarrowBeamEdges = SuitPickupBeamEdges.Columns(120, 120);
    /// <summary>The screen's rightmost column, the furthest right a beam edge reaches.</summary>
    public const int RightmostColumn = ScreenGeometry.ScreenWidthPixels - 1;
    /// <summary>Edges spanning the whole screen line.</summary>
    public static readonly SuitPickupBeamEdges FullScreenWindow = SuitPickupBeamEdges.Columns(0, RightmostColumn);
    /// <summary>Initial widening speed: one pixel per update.</summary>
    public static readonly SignedEightEight InitialWideningSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256);
    /// <summary>Per-update widening acceleration: 96/256 of a pixel per update.</summary>
    public static readonly SignedEightEight WideningAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(96);
    /// <summary>Per-update shrinking deceleration: 32/256 of a pixel per update.</summary>
    public static readonly SignedEightEight ShrinkingDeceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);
    /// <summary>Minimum shrinking speed: one pixel per update.</summary>
    public static readonly SignedEightEight MinimumShrinkingSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(256);
    /// <summary>Stage-two initial asymmetric edge pair.</summary>
    public static readonly SuitPickupBeamEdges CurvedWideningStart = SuitPickupBeamEdges.Columns(108, 132);
    /// <summary>Stage-five starting edge pair, whose left edge comes round to the screen's left on the first narrowing (<see cref="SuitPickupClosingLineWrap"/>).</summary>
    public static readonly SuitPickupBeamEdges DissipationStart = SuitPickupBeamEdges.Columns(RightmostColumn, 248);
    /// <summary>The lines the beam spans from the top of the screen; its middle line is half way down them.</summary>
    public const int BeamLines = 256;
    /// <summary>How far the curved beam's rounded sides reach beyond its column at the middle line, in pixels.</summary>
    public const int CurveWidth = 24;
}
