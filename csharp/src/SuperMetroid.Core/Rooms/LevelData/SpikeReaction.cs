namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a spike block or spike air does with its behaviour (#1722): the damage Samus's contact and the grapple swing take,
/// whether the grapple beam connects, whether an enemy breaks the block and whether the X-ray scope reveals it. Its values
/// are arbitrary; a behaviour written for another collision kind is read as one of these through
/// <see cref="RoomBlockBehavior"/>, or as none when the spike kinds have no reaction for it.
/// </summary>
public enum SpikeReaction
{
    /// <summary>A spike block taking heavy damage where the room's heavy spikes are live.</summary>
    HeavySpike,

    /// <summary>A spike block taking light damage.</summary>
    LightSpike,

    /// <summary>Spike air whose interior takes light damage; as a spike block, harmless.</summary>
    DamagingSpikeAir,

    /// <summary>
    /// The spike block Draygon's destroyed turret leaves: Samus's contact takes light damage, and the grapple beam connects
    /// to it at the cost of a periodic-damage unit.
    /// </summary>
    TurretSpike,

    /// <summary>A harmless spike.</summary>
    Harmless,

    /// <summary>A harmless spike block the X-ray scope reveals.</summary>
    XrayRevealed,

    /// <summary>A harmless spike block an enemy breaks.</summary>
    EnemyBreakable,

    /// <summary>A harmless spike the grapple beam passes through (<see cref="SpikeReactions"/>).</summary>
    HarmlessLetsGrappleThrough,

    /// <summary>A harmless spike block an enemy breaks, as <see cref="EnemyBreakable"/>; the grapple beam passes through it.</summary>
    EnemyBreakableLetsGrappleThrough,
}
