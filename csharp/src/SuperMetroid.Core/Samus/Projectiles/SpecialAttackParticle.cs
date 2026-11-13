namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The part one of a special beam attack's four particles plays, fixed when the attack starts (#1683): where an Ice or
/// Plasma particle starts on its orbit, where a Wave particle starts from Samus, which way a Spazer particle turns and
/// whether it is one of the Spazer attack's trailing pair, and which particles voice the attack's sounds. Each attack
/// reads only its own parts.
/// </summary>
internal sealed record SpecialAttackParticle
{
    /// <summary>The angle an Ice or Plasma particle starts its orbit of Samus at.</summary>
    public required TrigonometryAngle OrbitStart { get; init; }

    /// <summary>Where a Wave particle starts, from Samus.</summary>
    public required Offset16<RoomPixels> WaveStart { get; init; }

    /// <summary>
    /// The way a Spazer particle turns while it circles and flies towards the convergence point; it turns back the
    /// other way as it flies away. Each beam particle's partner is the trailing particle that turns the same way.
    /// </summary>
    public required RotationDirection SpazerTurn { get; init; }

    /// <summary>
    /// Whether this is one of the Spazer attack's trailing pair: drawn as its trail from the start and leaving
    /// trails as it circles, and stepping left rather than right as it falls.
    /// </summary>
    public bool SpazerTrail { get; init; }

    /// <summary>Whether this Spazer particle voices the attack's convergence sound.</summary>
    public bool VoicesSpazerConvergence { get; init; }

    /// <summary>Whether this Wave particle voices the attack's pulse each time it turns back horizontally.</summary>
    public bool VoicesWavePulse { get; init; }
}
