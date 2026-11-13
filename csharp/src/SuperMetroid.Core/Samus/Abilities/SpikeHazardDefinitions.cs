using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What touching a damaging spike costs Samus: the energy, the invulnerability and the knockback.</summary>
public static class SpikeHazardDefinitions
{
    /// <summary>The spike-air block behaviour whose interior damages Samus like a light spike.</summary>
    public static readonly RoomBlockBehavior DamagingSpikeAirBehavior = RoomBlockBehavior.Reacting(SpikeReaction.DamagingSpikeAir);

    /// <summary>The damaging solid spikes, by reaction; every other spike-block reaction is harmless.</summary>
    public static readonly IReadOnlyDictionary<SpikeReaction, SpikeBlockBehavior> DamagingSpikeBlocks = new Dictionary<SpikeReaction, SpikeBlockBehavior>
    {
        [SpikeReaction.HeavySpike] = SpikeBlockBehavior.Heavy,
        [SpikeReaction.LightSpike] = SpikeBlockBehavior.Light,
        [SpikeReaction.TurretSpike] = SpikeBlockBehavior.AlternateLight,
    };

    /// <summary>Energy a heavy solid spike takes.</summary>
    public const int HeavySpikeDamage = 60;

    /// <summary>Energy a light solid spike, or damaging spike air, takes.</summary>
    public const int LightSpikeDamage = 16;

    /// <summary>Frames of invulnerability after any damaging spike contact.</summary>
    public const int InvincibilityFrames = 60;

    /// <summary>Frames of knockback after any damaging spike contact.</summary>
    public const int KnockbackFrames = 10;
}
