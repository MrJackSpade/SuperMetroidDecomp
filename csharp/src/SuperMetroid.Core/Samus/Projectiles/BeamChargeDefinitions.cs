namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where a held beam's charge sounds, completes and arms a special beam attack, by flare counter.
/// </summary>
public static partial class BeamChargeDefinitions
{
    /// <summary>Flare-counter value at which the flare draw starts the charging sound.</summary>
    public const int ChargeSoundStartCounter = 16;
    /// <summary>
    /// Flare-counter value at which Samus is fully charged: release selects the charged beam family, and a bomb spread
    /// may be released.
    /// </summary>
    public const int FullyChargedCounter = 60;
    /// <summary>Flare-counter clamp at which the charge begins testing special beam attacks.</summary>
    public const int SpecialAttackCounter = 120;
}
