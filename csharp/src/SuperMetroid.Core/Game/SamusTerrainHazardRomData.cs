namespace SuperMetroid.Core.Game;

/// <summary>
/// Named operands used by bank-$94's ordinary Samus spike-block and inside-block
/// dispatchers. These values are table identities and damage/timer operands, not flags.
/// </summary>
public static class SamusTerrainHazardRomData
{
    /// <summary>Spike-block BTS zero selects the conditional 60-energy reaction at `$94:8E83`.</summary>
    public const byte HeavySpikeBlockBehavior = 0;

    /// <summary>Spike-block BTS one selects the 16-energy reaction at `$94:8ECF`.</summary>
    public const byte LightSpikeBlockBehavior = 1;

    /// <summary>Spike-block BTS three selects the alternate 16-energy reaction at `$94:8F0A`.</summary>
    public const byte AlternateLightSpikeBlockBehavior = 3;

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
