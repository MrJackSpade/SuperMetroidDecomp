namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where a held beam's charge sounds, completes and arms a special beam attack, by flare counter, and how long the suit
/// glows after a charged shot.
/// </summary>
public static class BeamChargeDefinitions
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
    /// <summary>The steps an ordinary charged shot's flash starts with (#2280).</summary>
    public const int ChargedShotFlashSteps = 4;
    /// <summary>The steps the Hyper Beam's glow starts with: two for each of its ten frames (#2280).</summary>
    public const int HyperBeamGlowSteps = 20;
}
