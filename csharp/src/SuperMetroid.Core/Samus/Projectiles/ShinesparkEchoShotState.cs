namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A shinespark crash echo's flight out from Samus (#2346), created when the echo is released.</summary>
/// <param name="angle">The echo's departure angle.</param>
/// <param name="echo">The crash's echo the shot carries (#2553).</param>
internal sealed class ShinesparkEchoShotState(TrigonometryAngle angle, Abilities.ShinesparkCrashEcho echo) : SamusShotKindState
{
    /// <summary>The echo's departure angle, in whole steps.</summary>
    internal TrigonometryAngle Angle { get; } = angle;

    /// <summary>The echo's expanding radius, in pixels: zero when it is released, growing each update.</summary>
    internal int Radius { get; set => field = Ensure.AtLeastZero(value); }

    /// <inheritdoc/>
    internal override bool AwaitsIgnitionAsFired => false;

    /// <inheritdoc/>
    internal override int? NamedLinkPlace => null;

    /// <inheritdoc/>
    internal override bool DeletesItselfAfterEnemyHit => false;

    /// <inheritdoc/>
    internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam => new BeamFiredOverLastShot.ReplacedShot.CrashEcho(Angle);
}
