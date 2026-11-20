namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A shinespark crash echo's flight out from Samus (#2346), created when the echo is released.</summary>
/// <param name="angle">The echo's departure angle.</param>
internal sealed class ShinesparkEchoShotState(TrigonometryAngle angle) : SamusShotKindState
{
    /// <summary>The echo's departure angle, in whole steps.</summary>
    internal TrigonometryAngle Angle { get; } = angle;

    /// <inheritdoc/>
    internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(SignedEightEight reflectionHeadStart) => new BeamFiredOverLastShot.ReplacedShot.CrashEcho(Angle);

    /// <summary>The echo's expanding radius, in pixels: zero when it is released, growing each update.</summary>
    internal int Radius { get; set => field = Ensure.AtLeastZero(value); }
}
