namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where a held beam's charge sounds, completes and arms a special beam attack, by flare counter.</summary>
public static class BeamChargeDefinitions
{
    /// <summary>Flare-counter value at which the flare draw starts the charging sound.</summary>
    public const int ChargeSoundStartCounter = 16;
    /// <summary>Flare-counter value at which release selects the charged beam family.</summary>
    public const int FullyChargedCounter = 60;
    /// <summary>Flare-counter clamp at which the charge handler begins testing special beam attacks.</summary>
    public const int SpecialAttackCounter = 120;
    /// <summary>
    /// The flare counter a Hyper Beam shot stores: past every charge threshold, so the Hyper Beam always reads as fully
    /// charged and never advances it; the largest value the counter holds.
    /// </summary>
    public const int HyperBeamFlareCounter = 32768;
}
