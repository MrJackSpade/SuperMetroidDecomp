namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>How a Waver flies and waves (#2156).</summary>
internal static class WaverMotionDefinitions
{
    /// <summary>How fast a Waver flies sideways, a pixel and a half an update.</summary>
    internal static SignedSixteenSixteen HorizontalSpeed { get; } = SignedSixteenSixteen.FromPixelFraction(3, 2);

    /// <summary>How far above and below its line a Waver's wave reaches, in pixels.</summary>
    internal const int WaveHeight = 4;

    /// <summary>How far a Waver's wave turns each update (#2182, #2563).</summary>
    internal static RotationAngle WaveStep { get; } = RotationAngle.FromSteps(2);

    /// <summary>The place in each half of its wave at which a Waver spins (#2182, #2563).</summary>
    internal static TrigonometryAngle SpinPlaceInHalfTurn { get; } = new(56);
}
