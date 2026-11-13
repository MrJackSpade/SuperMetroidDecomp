namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A beam charge as everything that tests it reads it (#1682): the updates Samus has held the charge, or the Hyper
/// Beam's flare, which stands in for a charge past every threshold, so the Hyper Beam always reads as fully charged.
/// </summary>
public readonly record struct BeamChargeLevel
{
    /// <summary>Creates a level of <paramref name="flareCounter"/> charged updates.</summary>
    private BeamChargeLevel(int flareCounter) => FlareCounter = flareCounter;

    /// <summary>No charge.</summary>
    public static BeamChargeLevel None => default;

    /// <summary>The Hyper Beam's flare, which reads as past every charge threshold.</summary>
    public static BeamChargeLevel HyperBeam { get; } = new() { HyperBeamFlare = true };

    /// <summary>A charge held for <paramref name="updates"/> updates, zero through the special-attack clamp.</summary>
    /// <param name="updates">The updates the charge has been held.</param>
    public static BeamChargeLevel Charged(int updates) =>
        new(Ensure.BetweenInclusive(0, BeamChargeDefinitions.SpecialAttackCounter, updates));

    /// <summary>The updates the charge has been held; zero for the Hyper Beam's flare.</summary>
    [IntegerQuantity("updates the charge flare has shown")]
    public int FlareCounter { get; }

    /// <summary>Whether this is the Hyper Beam's flare rather than a held charge.</summary>
    public bool HyperBeamFlare { get; private init; }

    /// <summary>Whether any charge is held: a counted charge or the Hyper Beam's flare.</summary>
    public bool IsCharging => HyperBeamFlare || FlareCounter != 0;

    /// <summary>Whether the charge has reached <paramref name="updates"/>; the Hyper Beam's flare has reached every count.</summary>
    /// <param name="updates">The charge threshold, in updates held.</param>
    public bool HasCharged(int updates) => HyperBeamFlare || FlareCounter >= updates;
}
