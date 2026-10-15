namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Named operands used by bank-$94's ordinary Samus spike-block and inside-block
/// dispatchers. These values are table identities and damage/timer operands, not flags.
/// </summary>
public static class SamusTerrainHazardRomData
{
    /// <summary>Spike-air BTS two selects the 16-energy inside-block reaction at `$94:9866`.</summary>
    public const byte DamagingSpikeAirBehavior = 2;

    /// <summary>Whole-energy contribution used by spike-block BTS zero.</summary>
    public const ushort HeavySpikeDamage = 60;

    /// <summary>Whole-energy contribution used by both light solid spikes and spike air.</summary>
    public const ushort LightSpikeDamage = 16;

    /// <summary>Invulnerability lifetime written by every damaging ordinary spike reaction.</summary>
    public const ushort InvincibilityFrames = 60;

    /// <summary>Knockback/flicker timer written by every damaging ordinary spike reaction.</summary>
    public const ushort KnockbackFrames = 10;
}
