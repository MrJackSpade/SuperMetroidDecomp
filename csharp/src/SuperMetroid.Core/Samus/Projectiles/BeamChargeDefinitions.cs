namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where a held beam's charge sounds, completes and arms a special beam attack, by flare counter.</summary>
public static class BeamChargeDefinitions
{
    /// <summary>Flare-counter value at which <c>$90:BAFC</c> starts SFX `$08`.</summary>
    public const ushort ChargeSoundStartCounter = 16;
    /// <summary>Flare-counter value at which release selects the charged beam family.</summary>
    public const ushort FullyChargedCounter = 60;
    /// <summary>Flare-counter clamp at which bank $90 begins testing special beam attacks.</summary>
    public const ushort SpecialAttackCounter = 120;
}
