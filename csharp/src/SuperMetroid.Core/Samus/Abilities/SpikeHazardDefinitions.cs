using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>What touching a damaging spike costs Samus: the energy, the invulnerability and the knockback.</summary>
public static class SpikeHazardDefinitions
{
    /// <summary>The spike-air reaction whose interior damages Samus like a light spike.</summary>
    public static readonly BlockReaction DamagingSpikeAirReaction = BlockReaction.Common(2);

    /// <summary>The spike-air block behaviour whose interior damages Samus like a light spike.</summary>
    public static readonly RoomBlockBehavior DamagingSpikeAirBehavior = RoomBlockBehavior.Reacting(DamagingSpikeAirReaction);

    /// <summary>The solid spike reaction that takes heavy damage where the room's heavy spikes are live.</summary>
    public static readonly BlockReaction HeavySpikeReaction = BlockReaction.Common(0);

    /// <summary>The solid spike reaction that takes light damage.</summary>
    public static readonly BlockReaction LightSpikeReaction = BlockReaction.Common(1);

    /// <summary>The damaging solid spikes, by reaction; every other spike-block reaction is harmless.</summary>
    public static readonly IReadOnlyDictionary<BlockReaction, SpikeBlockBehavior> DamagingSpikeBlocks = new Dictionary<BlockReaction, SpikeBlockBehavior>
    {
        [HeavySpikeReaction] = SpikeBlockBehavior.Heavy,
        [LightSpikeReaction] = SpikeBlockBehavior.Light,
        [BlockReaction.Common(3)] = SpikeBlockBehavior.AlternateLight,
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
