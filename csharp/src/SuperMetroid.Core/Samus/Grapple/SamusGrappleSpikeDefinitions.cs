using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>What the spike reactions do to the grapple beam and to a swinging Samus.</summary>
internal static class SamusGrappleSpikeDefinitions
{
    /// <summary>
    /// The spike reactions that latch the grapple beam, with the periodic damage each adds: the turret spike, which uses
    /// Draygon's broken-turret setup and adds one whole periodic-damage unit. Every other spike reaction the beam does not
    /// pass through stops it without latching.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpikeReaction, EnergyAmount> LatchingDamage = new Dictionary<SpikeReaction, EnergyAmount>
    {
        [SpikeReaction.TurretSpike] = SpikeHazardDefinitions.GrappledTurretSpikeDamage,
    };

    /// <summary>
    /// The damage each spike reaction deals a swinging body on a solid spike block: heavy spikes their heavy damage, light
    /// spikes their light damage. The turret spike, which the ground collision treats as light, costs nothing here.
    /// </summary>
    internal static readonly IReadOnlyDictionary<SpikeReaction, int> SolidBlockSwingDamage = new Dictionary<SpikeReaction, int>
    {
        [SpikeReaction.HeavySpike] = SpikeHazardDefinitions.HeavySpikeDamage,
        [SpikeReaction.LightSpike] = SpikeHazardDefinitions.LightSpikeDamage,
    };

    /// <summary>The damage each spike reaction deals a swinging body on a plain block: damaging spike air its light damage.</summary>
    internal static readonly IReadOnlyDictionary<SpikeReaction, int> PlainBlockSwingDamage = new Dictionary<SpikeReaction, int>
    {
        [SpikeReaction.DamagingSpikeAir] = SpikeHazardDefinitions.LightSpikeDamage,
    };
}
