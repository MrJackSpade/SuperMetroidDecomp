namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What a spike block or spike air does with its behaviour (#1722): the damage Samus's contact and the grapple swing take,
/// whether the grapple beam connects, whether an enemy breaks the block and whether the X-ray scope reveals it. Its values
/// are arbitrary; a behaviour written for another collision kind is read as one of these through
/// <see cref="RoomBlockBehavior"/>.
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

    /// <summary>A harmless spike past the end of the enemy-breakable table: an enemy reaching it rejects it.</summary>
    PastEnemyTable,

    /// <summary>One of the reactions the room's area gives spikes: harmless, and the grapple beam passes through it.</summary>
    AreaHarmless,

    /// <summary>An area reaction an enemy breaks, as <see cref="EnemyBreakable"/>; otherwise as <see cref="AreaHarmless"/>.</summary>
    AreaEnemyBreakable,

    /// <summary>An area reaction past the end of the enemy-breakable table; otherwise as <see cref="AreaHarmless"/>.</summary>
    AreaPastEnemyTable,
}
