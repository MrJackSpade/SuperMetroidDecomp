namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The suit pickup's light beam: when it starts, the order of its stages, its authored edges, the speeds its edges move
/// at, and its rounded sides.
/// </summary>
public static partial class SuitPickupBeamDefinitions
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
    /// <summary>Collapsed beam with both edges together at the beam's centre.</summary>
    public static readonly SuitPickupBeamEdges NarrowBeamEdges = new(120, 120);
    /// <summary>The screen's rightmost column, the furthest right a beam edge reaches.</summary>
    public const int RightmostColumn = ScreenGeometry.ScreenWidthPixels - 1;
    /// <summary>Edges spanning the whole screen line.</summary>
    public static readonly SuitPickupBeamEdges FullScreenEdges = new(0, RightmostColumn);
    /// <summary>Initial widening speed: one pixel per update.</summary>
    public static readonly SignedEightEight InitialWideningSpeed = SignedEightEight.FromPixels(1);
    /// <summary>Per-update widening acceleration: three eighths of a pixel per update.</summary>
    public static readonly SignedEightEight WideningAcceleration = SignedEightEight.FromPixelFraction(3, 8);
    /// <summary>Per-update shrinking deceleration: an eighth of a pixel per update.</summary>
    public static readonly SignedEightEight ShrinkingDeceleration = SignedEightEight.FromPixelFraction(1, 8);
    /// <summary>Minimum shrinking speed: one pixel per update.</summary>
    public static readonly SignedEightEight MinimumShrinkingSpeed = SignedEightEight.FromPixels(1);
    /// <summary>The curved widening's initial asymmetric edge pair.</summary>
    public static readonly SuitPickupBeamEdges CurvedWideningStart = new(108, 132);
    /// <summary>The final narrowing's starting edge pair, whose left edge comes round to the screen's left on the first narrowing (<see cref="SuitPickupClosingLineWrap"/>).</summary>
    public static readonly SuitPickupBeamEdges DissipationStart = new(RightmostColumn, 248);
    /// <summary>The lines the beam spans from the top of the screen; its middle line is half way down them.</summary>
    public const int BeamLines = 256;
    /// <summary>The lines the appearing and the shrinking beam cover from each end before the next stage: half the beam's lines.</summary>
    public const int CoveredLines = BeamLines / 2;
    /// <summary>The lines the appearing beam covers from each end on each update.</summary>
    public const int AppearingLinesPerUpdate = 8;
    /// <summary>The uniform widening ends once its left edge moves left of this column.</summary>
    public const int UniformWideningEndColumn = 97;
    /// <summary>The dissipating line ends once its left edge reaches this column.</summary>
    public const int DissipatedColumn = 112;
    /// <summary>How far the curved beam's rounded sides reach beyond its column at the middle line, in pixels.</summary>
    public const int CurveWidth = 24;
}
