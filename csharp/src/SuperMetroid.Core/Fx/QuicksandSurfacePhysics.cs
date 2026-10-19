namespace SuperMetroid.Core.Fx;

/// <summary>One suit-dependent row of the fixed quicksand-surface physics table.</summary>
/// <param name="MovingDisplacement">Downward extra displacement per frame while Samus moves up or down in the surface.</param>
/// <param name="StationaryDisplacement">Downward extra displacement per frame while Samus has no vertical direction.</param>
/// <param name="UpwardSpeedLimit">The upward speed Samus is clamped to; its whole pixels and fraction replace the speed when exceeded.</param>
public readonly record struct QuicksandSurfacePhysics(
    SnesSignedSixteenSixteen MovingDisplacement,
    SnesSignedSixteenSixteen StationaryDisplacement,
    SnesSignedSixteenSixteen UpwardSpeedLimit);
