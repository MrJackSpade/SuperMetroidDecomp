namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A beam charge as everything that tests it reads it (#1682, #2560): a closed choice of the updates Samus has held the
/// charge, none included, or the Hyper Beam's flare, which stands in for a charge past every threshold, so the Hyper Beam
/// always reads as fully charged.
/// </summary>
public abstract partial record BeamChargeLevel
{
    /// <summary>Only the two cases below are charge levels.</summary>
    private BeamChargeLevel()
    {
    }

    /// <summary>Every counted charge, by its updates held, zero through the special-attack clamp.</summary>
    private static readonly CountedCharge[] Counts =
        [.. Enumerable.Range(0, BeamChargeDefinitions.SpecialAttackCounter + 1).Select(updates => new CountedCharge(updates))];

    /// <summary>No charge.</summary>
    public static BeamChargeLevel None => Counts[0];

    /// <summary>The Hyper Beam's flare, which reads as past every charge threshold.</summary>
    public static BeamChargeLevel HyperBeam { get; } = new HyperBeamFlareLevel();

    /// <summary>A charge held for <paramref name="updates"/> updates, zero through the special-attack clamp.</summary>
    /// <param name="updates">The updates the charge has been held.</param>
    public static BeamChargeLevel Charged(int updates) =>
        Counts[Ensure.BetweenInclusive(0, BeamChargeDefinitions.SpecialAttackCounter, updates)];

    /// <summary>The updates the charge has been held; zero for the Hyper Beam's flare.</summary>
    public abstract int UpdatesHeld { get; }

    /// <summary>Whether any charge is held: a counted charge or the Hyper Beam's flare.</summary>
    public abstract bool IsCharging { get; }

    /// <summary>Whether the charge has reached <paramref name="updates"/>; the Hyper Beam's flare has reached every count.</summary>
    /// <param name="updates">The charge threshold, in updates held.</param>
    public abstract bool HasCharged(int updates);

    /// <summary>A charge held for <paramref name="Updates"/> updates.</summary>
    /// <param name="Updates">The updates the charge has been held.</param>
    internal sealed partial record CountedCharge(
        [property: IntegerQuantity("updates the charge flare has shown")] int Updates) : BeamChargeLevel
    {
        /// <inheritdoc/>
        public override int UpdatesHeld => Updates;

        /// <inheritdoc/>
        public override bool IsCharging => Updates != 0;

        /// <inheritdoc/>
        public override bool HasCharged(int updates) => Updates >= updates;
    }

    /// <summary>The Hyper Beam's flare, which reads as past every charge threshold.</summary>
    internal sealed partial record HyperBeamFlareLevel : BeamChargeLevel
    {
        /// <inheritdoc/>
        public override int UpdatesHeld => 0;

        /// <inheritdoc/>
        public override bool IsCharging => true;

        /// <inheritdoc/>
        public override bool HasCharged(int updates) => true;
    }
}
