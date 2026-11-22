namespace SuperMetroid.Core.Fx;

/// <summary>How quicksand's surface moves Samus in one suit.</summary>
/// <param name="MovingDisplacement">Downward extra displacement per frame while Samus moves up or down in the surface.</param>
/// <param name="StationaryDisplacement">Downward extra displacement per frame while Samus has no vertical direction.</param>
/// <param name="UpwardSpeedLimit">The upward speed Samus is clamped to; its whole pixels and fraction replace the speed when exceeded.</param>
public readonly partial record struct QuicksandSurfacePhysics(
    SignedSixteenSixteen MovingDisplacement,
    SignedSixteenSixteen StationaryDisplacement,
    SignedSixteenSixteen UpwardSpeedLimit);
