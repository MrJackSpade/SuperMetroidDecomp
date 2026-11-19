namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The turns, orbit and head directions Shaktool's segments move and aim by.</summary>
internal static class ShaktoolOrbitDefinitions
{
    /// <summary>The turn a straightening chain's segments and target make each update: one whole step.</summary>
    internal static RotationAngle StraighteningTurn { get; } = RotationAngle.FromSteps(1);

    /// <summary>
    /// The share of the whole steps left to its target a converging segment turns by each update: a sixty-fourth of a
    /// step for each step left.
    /// </summary>
    internal const int ConvergenceShares = 64;

    /// <summary>
    /// The direction the centre segment's head aims for each eighth of the turn its neighbours' midpoint lies in, in turn
    /// order: each direction's eighth opens eight steps before it.
    /// </summary>
    internal static TurnArcs<CompassDirection> Orientations { get; } = new(TrigonometryAngle.Wrapped(-8),
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft);

    /// <summary>The curl progress installed when the centre reaches its target angle: 120 whole steps.</summary>
    internal static RotationAngle StraighteningStart { get; } = RotationAngle.FromSteps(120);

    /// <summary>Once the curl progress reaches 240 whole steps, the chain starts straightening.</summary>
    internal static RotationAngle StraighteningThreshold { get; } = RotationAngle.FromSteps(240);

    /// <summary>The fraction middle segments reset to on an endpoint swap: half a pixel.</summary>
    internal static PixelFraction SwapFraction => PixelFraction.Half;

    /// <summary>
    /// How far across a segment sits from the one before it at each step of its orbit angle: the orbit's authored sine,
    /// which peaks at just short of twelve pixels a quarter turn on and is not quite symmetric about its peaks. The value a
    /// quarter turn behind an angle is how far down the segment sits.
    /// </summary>
    internal static TurnArcs<SignedSixteenSixteen> OrbitSine { get; } = new(TrigonometryAngle.Zero,
        SignedSixteenSixteen.Zero, SignedSixteenSixteen.FromPixelFraction(75, 256), SignedSixteenSixteen.FromPixelFraction(75, 128), SignedSixteenSixteen.FromPixelFraction(225, 256),
        SignedSixteenSixteen.FromPixelFraction(301, 256), SignedSixteenSixteen.FromPixelFraction(47, 32), SignedSixteenSixteen.FromPixelFraction(225, 128), SignedSixteenSixteen.FromPixelFraction(525, 256),
        SignedSixteenSixteen.FromPixelFraction(599, 256), SignedSixteenSixteen.FromPixelFraction(673, 256), SignedSixteenSixteen.FromPixelFraction(373, 128), SignedSixteenSixteen.FromPixelFraction(819, 256),
        SignedSixteenSixteen.FromPixelFraction(891, 256), SignedSixteenSixteen.FromPixelFraction(963, 256), SignedSixteenSixteen.FromPixelFraction(517, 128), SignedSixteenSixteen.FromPixelFraction(1105, 256),
        SignedSixteenSixteen.FromPixelFraction(1175, 256), SignedSixteenSixteen.FromPixelFraction(311, 64), SignedSixteenSixteen.FromPixelFraction(1313, 256), SignedSixteenSixteen.FromPixelFraction(1381, 256),
        SignedSixteenSixteen.FromPixelFraction(181, 32), SignedSixteenSixteen.FromPixelFraction(757, 128), SignedSixteenSixteen.FromPixelFraction(1579, 256), SignedSixteenSixteen.FromPixelFraction(1643, 256),
        SignedSixteenSixteen.FromPixelFraction(853, 128), SignedSixteenSixteen.FromPixelFraction(221, 32), SignedSixteenSixteen.FromPixelFraction(1829, 256), SignedSixteenSixteen.FromPixelFraction(1889, 256),
        SignedSixteenSixteen.FromPixelFraction(487, 64), SignedSixteenSixteen.FromPixelFraction(1003, 128), SignedSixteenSixteen.FromPixelFraction(2063, 256), SignedSixteenSixteen.FromPixelFraction(1059, 128),
        SignedSixteenSixteen.FromPixelFraction(543, 64), SignedSixteenSixteen.FromPixelFraction(139, 16), SignedSixteenSixteen.FromPixelFraction(569, 64), SignedSixteenSixteen.FromPixelFraction(1163, 128),
        SignedSixteenSixteen.FromPixelFraction(1187, 128), SignedSixteenSixteen.FromPixelFraction(2421, 256), SignedSixteenSixteen.FromPixelFraction(2467, 256), SignedSixteenSixteen.FromPixelFraction(2511, 256),
        SignedSixteenSixteen.FromPixelFraction(1277, 128), SignedSixteenSixteen.FromPixelFraction(2595, 256), SignedSixteenSixteen.FromPixelFraction(1317, 128), SignedSixteenSixteen.FromPixelFraction(167, 16),
        SignedSixteenSixteen.FromPixelFraction(2709, 256), SignedSixteenSixteen.FromPixelFraction(2743, 256), SignedSixteenSixteen.FromPixelFraction(2777, 256), SignedSixteenSixteen.FromPixelFraction(351, 32),
        SignedSixteenSixteen.FromPixelFraction(1419, 128), SignedSixteenSixteen.FromPixelFraction(1433, 128), SignedSixteenSixteen.FromPixelFraction(723, 64), SignedSixteenSixteen.FromPixelFraction(729, 64),
        SignedSixteenSixteen.FromPixelFraction(2939, 256), SignedSixteenSixteen.FromPixelFraction(185, 16), SignedSixteenSixteen.FromPixelFraction(2979, 256), SignedSixteenSixteen.FromPixelFraction(2997, 256),
        SignedSixteenSixteen.FromPixelFraction(753, 64), SignedSixteenSixteen.FromPixelFraction(1513, 128), SignedSixteenSixteen.FromPixelFraction(1519, 128), SignedSixteenSixteen.FromPixelFraction(381, 32),
        SignedSixteenSixteen.FromPixelFraction(3057, 256), SignedSixteenSixteen.FromPixelFraction(3063, 256), SignedSixteenSixteen.FromPixelFraction(767, 64), SignedSixteenSixteen.FromPixelFraction(3071, 256),
        SignedSixteenSixteen.FromPixelFraction(3071, 256), SignedSixteenSixteen.FromPixelFraction(3071, 256), SignedSixteenSixteen.FromPixelFraction(767, 64), SignedSixteenSixteen.FromPixelFraction(3063, 256),
        SignedSixteenSixteen.FromPixelFraction(3057, 256), SignedSixteenSixteen.FromPixelFraction(381, 32), SignedSixteenSixteen.FromPixelFraction(1519, 128), SignedSixteenSixteen.FromPixelFraction(1513, 128),
        SignedSixteenSixteen.FromPixelFraction(753, 64), SignedSixteenSixteen.FromPixelFraction(2997, 256), SignedSixteenSixteen.FromPixelFraction(2979, 256), SignedSixteenSixteen.FromPixelFraction(185, 16),
        SignedSixteenSixteen.FromPixelFraction(2939, 256), SignedSixteenSixteen.FromPixelFraction(729, 64), SignedSixteenSixteen.FromPixelFraction(723, 64), SignedSixteenSixteen.FromPixelFraction(1433, 128),
        SignedSixteenSixteen.FromPixelFraction(1419, 128), SignedSixteenSixteen.FromPixelFraction(351, 32), SignedSixteenSixteen.FromPixelFraction(2777, 256), SignedSixteenSixteen.FromPixelFraction(2743, 256),
        SignedSixteenSixteen.FromPixelFraction(2709, 256), SignedSixteenSixteen.FromPixelFraction(167, 16), SignedSixteenSixteen.FromPixelFraction(1317, 128), SignedSixteenSixteen.FromPixelFraction(2595, 256),
        SignedSixteenSixteen.FromPixelFraction(1277, 128), SignedSixteenSixteen.FromPixelFraction(2511, 256), SignedSixteenSixteen.FromPixelFraction(2467, 256), SignedSixteenSixteen.FromPixelFraction(2421, 256),
        SignedSixteenSixteen.FromPixelFraction(1187, 128), SignedSixteenSixteen.FromPixelFraction(1163, 128), SignedSixteenSixteen.FromPixelFraction(569, 64), SignedSixteenSixteen.FromPixelFraction(139, 16),
        SignedSixteenSixteen.FromPixelFraction(543, 64), SignedSixteenSixteen.FromPixelFraction(1059, 128), SignedSixteenSixteen.FromPixelFraction(2063, 256), SignedSixteenSixteen.FromPixelFraction(1003, 128),
        SignedSixteenSixteen.FromPixelFraction(487, 64), SignedSixteenSixteen.FromPixelFraction(1889, 256), SignedSixteenSixteen.FromPixelFraction(1829, 256), SignedSixteenSixteen.FromPixelFraction(221, 32),
        SignedSixteenSixteen.FromPixelFraction(853, 128), SignedSixteenSixteen.FromPixelFraction(1643, 256), SignedSixteenSixteen.FromPixelFraction(1579, 256), SignedSixteenSixteen.FromPixelFraction(757, 128),
        SignedSixteenSixteen.FromPixelFraction(181, 32), SignedSixteenSixteen.FromPixelFraction(1381, 256), SignedSixteenSixteen.FromPixelFraction(1313, 256), SignedSixteenSixteen.FromPixelFraction(311, 64),
        SignedSixteenSixteen.FromPixelFraction(1175, 256), SignedSixteenSixteen.FromPixelFraction(1105, 256), SignedSixteenSixteen.FromPixelFraction(517, 128), SignedSixteenSixteen.FromPixelFraction(963, 256),
        SignedSixteenSixteen.FromPixelFraction(891, 256), SignedSixteenSixteen.FromPixelFraction(819, 256), SignedSixteenSixteen.FromPixelFraction(373, 128), SignedSixteenSixteen.FromPixelFraction(673, 256),
        SignedSixteenSixteen.FromPixelFraction(599, 256), SignedSixteenSixteen.FromPixelFraction(525, 256), SignedSixteenSixteen.FromPixelFraction(225, 128), SignedSixteenSixteen.FromPixelFraction(47, 32),
        SignedSixteenSixteen.FromPixelFraction(301, 256), SignedSixteenSixteen.FromPixelFraction(225, 256), SignedSixteenSixteen.FromPixelFraction(75, 128), SignedSixteenSixteen.FromPixelFraction(75, 256),
        SignedSixteenSixteen.Zero, SignedSixteenSixteen.FromPixelFraction(-75, 256), SignedSixteenSixteen.FromPixelFraction(-75, 128), SignedSixteenSixteen.FromPixelFraction(-225, 256),
        SignedSixteenSixteen.FromPixelFraction(-301, 256), SignedSixteenSixteen.FromPixelFraction(-47, 32), SignedSixteenSixteen.FromPixelFraction(-225, 128), SignedSixteenSixteen.FromPixelFraction(-525, 256),
        SignedSixteenSixteen.FromPixelFraction(-599, 256), SignedSixteenSixteen.FromPixelFraction(-673, 256), SignedSixteenSixteen.FromPixelFraction(-373, 128), SignedSixteenSixteen.FromPixelFraction(-819, 256),
        SignedSixteenSixteen.FromPixelFraction(-891, 256), SignedSixteenSixteen.FromPixelFraction(-963, 256), SignedSixteenSixteen.FromPixelFraction(-517, 128), SignedSixteenSixteen.FromPixelFraction(-1105, 256),
        SignedSixteenSixteen.FromPixelFraction(-1175, 256), SignedSixteenSixteen.FromPixelFraction(-311, 64), SignedSixteenSixteen.FromPixelFraction(-1313, 256), SignedSixteenSixteen.FromPixelFraction(-1381, 256),
        SignedSixteenSixteen.FromPixelFraction(-181, 32), SignedSixteenSixteen.FromPixelFraction(-757, 128), SignedSixteenSixteen.FromPixelFraction(-1579, 256), SignedSixteenSixteen.FromPixelFraction(-1643, 256),
        SignedSixteenSixteen.FromPixelFraction(-853, 128), SignedSixteenSixteen.FromPixelFraction(-221, 32), SignedSixteenSixteen.FromPixelFraction(-1829, 256), SignedSixteenSixteen.FromPixelFraction(-1889, 256),
        SignedSixteenSixteen.FromPixelFraction(-487, 64), SignedSixteenSixteen.FromPixelFraction(-1003, 128), SignedSixteenSixteen.FromPixelFraction(-2063, 256), SignedSixteenSixteen.FromPixelFraction(-1059, 128),
        SignedSixteenSixteen.FromPixelFraction(-543, 64), SignedSixteenSixteen.FromPixelFraction(-139, 16), SignedSixteenSixteen.FromPixelFraction(-569, 64), SignedSixteenSixteen.FromPixelFraction(-1163, 128),
        SignedSixteenSixteen.FromPixelFraction(-1187, 128), SignedSixteenSixteen.FromPixelFraction(-2421, 256), SignedSixteenSixteen.FromPixelFraction(-2467, 256), SignedSixteenSixteen.FromPixelFraction(-2511, 256),
        SignedSixteenSixteen.FromPixelFraction(-1277, 128), SignedSixteenSixteen.FromPixelFraction(-2595, 256), SignedSixteenSixteen.FromPixelFraction(-1317, 128), SignedSixteenSixteen.FromPixelFraction(-167, 16),
        SignedSixteenSixteen.FromPixelFraction(-2709, 256), SignedSixteenSixteen.FromPixelFraction(-2743, 256), SignedSixteenSixteen.FromPixelFraction(-2777, 256), SignedSixteenSixteen.FromPixelFraction(-351, 32),
        SignedSixteenSixteen.FromPixelFraction(-1419, 128), SignedSixteenSixteen.FromPixelFraction(-1433, 128), SignedSixteenSixteen.FromPixelFraction(-723, 64), SignedSixteenSixteen.FromPixelFraction(-729, 64),
        SignedSixteenSixteen.FromPixelFraction(-2939, 256), SignedSixteenSixteen.FromPixelFraction(-185, 16), SignedSixteenSixteen.FromPixelFraction(-2979, 256), SignedSixteenSixteen.FromPixelFraction(-2997, 256),
        SignedSixteenSixteen.FromPixelFraction(-753, 64), SignedSixteenSixteen.FromPixelFraction(-1513, 128), SignedSixteenSixteen.FromPixelFraction(-1519, 128), SignedSixteenSixteen.FromPixelFraction(-381, 32),
        SignedSixteenSixteen.FromPixelFraction(-3057, 256), SignedSixteenSixteen.FromPixelFraction(-3063, 256), SignedSixteenSixteen.FromPixelFraction(-767, 64), SignedSixteenSixteen.FromPixelFraction(-3071, 256),
        SignedSixteenSixteen.FromPixelFraction(-3071, 256), SignedSixteenSixteen.FromPixelFraction(-3071, 256), SignedSixteenSixteen.FromPixelFraction(-767, 64), SignedSixteenSixteen.FromPixelFraction(-3063, 256),
        SignedSixteenSixteen.FromPixelFraction(-3057, 256), SignedSixteenSixteen.FromPixelFraction(-381, 32), SignedSixteenSixteen.FromPixelFraction(-1519, 128), SignedSixteenSixteen.FromPixelFraction(-1513, 128),
        SignedSixteenSixteen.FromPixelFraction(-753, 64), SignedSixteenSixteen.FromPixelFraction(-2997, 256), SignedSixteenSixteen.FromPixelFraction(-2979, 256), SignedSixteenSixteen.FromPixelFraction(-185, 16),
        SignedSixteenSixteen.FromPixelFraction(-2939, 256), SignedSixteenSixteen.FromPixelFraction(-729, 64), SignedSixteenSixteen.FromPixelFraction(-723, 64), SignedSixteenSixteen.FromPixelFraction(-1433, 128),
        SignedSixteenSixteen.FromPixelFraction(-1419, 128), SignedSixteenSixteen.FromPixelFraction(-351, 32), SignedSixteenSixteen.FromPixelFraction(-2777, 256), SignedSixteenSixteen.FromPixelFraction(-2743, 256),
        SignedSixteenSixteen.FromPixelFraction(-2709, 256), SignedSixteenSixteen.FromPixelFraction(-167, 16), SignedSixteenSixteen.FromPixelFraction(-1317, 128), SignedSixteenSixteen.FromPixelFraction(-2595, 256),
        SignedSixteenSixteen.FromPixelFraction(-1277, 128), SignedSixteenSixteen.FromPixelFraction(-2511, 256), SignedSixteenSixteen.FromPixelFraction(-2467, 256), SignedSixteenSixteen.FromPixelFraction(-2421, 256),
        SignedSixteenSixteen.FromPixelFraction(-1187, 128), SignedSixteenSixteen.FromPixelFraction(-1163, 128), SignedSixteenSixteen.FromPixelFraction(-569, 64), SignedSixteenSixteen.FromPixelFraction(-139, 16),
        SignedSixteenSixteen.FromPixelFraction(-543, 64), SignedSixteenSixteen.FromPixelFraction(-1059, 128), SignedSixteenSixteen.FromPixelFraction(-2063, 256), SignedSixteenSixteen.FromPixelFraction(-1003, 128),
        SignedSixteenSixteen.FromPixelFraction(-487, 64), SignedSixteenSixteen.FromPixelFraction(-945, 128), SignedSixteenSixteen.FromPixelFraction(-915, 128), SignedSixteenSixteen.FromPixelFraction(-221, 32),
        SignedSixteenSixteen.FromPixelFraction(-853, 128), SignedSixteenSixteen.FromPixelFraction(-1643, 256), SignedSixteenSixteen.FromPixelFraction(-1579, 256), SignedSixteenSixteen.FromPixelFraction(-757, 128),
        SignedSixteenSixteen.FromPixelFraction(-181, 32), SignedSixteenSixteen.FromPixelFraction(-1381, 256), SignedSixteenSixteen.FromPixelFraction(-1313, 256), SignedSixteenSixteen.FromPixelFraction(-311, 64),
        SignedSixteenSixteen.FromPixelFraction(-1175, 256), SignedSixteenSixteen.FromPixelFraction(-1105, 256), SignedSixteenSixteen.FromPixelFraction(-517, 128), SignedSixteenSixteen.FromPixelFraction(-963, 256),
        SignedSixteenSixteen.FromPixelFraction(-891, 256), SignedSixteenSixteen.FromPixelFraction(-819, 256), SignedSixteenSixteen.FromPixelFraction(-373, 128), SignedSixteenSixteen.FromPixelFraction(-673, 256),
        SignedSixteenSixteen.FromPixelFraction(-599, 256), SignedSixteenSixteen.FromPixelFraction(-525, 256), SignedSixteenSixteen.FromPixelFraction(-225, 128), SignedSixteenSixteen.FromPixelFraction(-47, 32),
        SignedSixteenSixteen.FromPixelFraction(-301, 256), SignedSixteenSixteen.FromPixelFraction(-113, 128), SignedSixteenSixteen.FromPixelFraction(-75, 128), SignedSixteenSixteen.FromPixelFraction(-75, 256));
}
