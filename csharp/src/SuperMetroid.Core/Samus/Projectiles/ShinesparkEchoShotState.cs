namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A shinespark crash echo's flight out from Samus (#2346), created when the echo is released.</summary>
/// <param name="angle">The echo's departure angle.</param>
/// <param name="echo">The crash's echo the shot carries (#2553).</param>
internal sealed partial class ShinesparkEchoShotState(TrigonometryAngle angle, Abilities.ShinesparkCrashEcho echo) : SamusShotKindState
{
    /// <summary>The echo's departure angle, in whole steps.</summary>
    internal TrigonometryAngle Angle { get; } = angle;

    /// <summary>The crash's echo the shot carries, which shows where the shot is and which clearing Samus's shots forgets.</summary>
    internal Abilities.ShinesparkCrashEcho Echo { get; } = Ensure.NotNull(echo);

    /// <inheritdoc/>
    internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(SignedEightEight reflectionHeadStart) => new BeamFiredOverLastShot.ReplacedShot.CrashEcho(Angle);

    /// <summary>The echo's expanding radius, in pixels: zero when it is released, growing each update.</summary>
    internal int Radius { get; set => field = Ensure.AtLeastZero(value); }
}
