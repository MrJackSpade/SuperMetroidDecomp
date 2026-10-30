namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// How a room block collides. The kinds are the game's sixteen collision families, named whether or not a particular
/// caller handles them. The values are arbitrary: compare kinds by name, and ask families through
/// <see cref="RoomCollisionTypeExtensions"/>, never by ordering.
/// </summary>
public enum RoomCollisionType
{
    /// <summary>Empty air; the default kind.</summary>
    Air,
    /// <summary>Slope collision whose geometry the block's behaviour selects.</summary>
    Slope,
    /// <summary>Non-solid spike-air behavior.</summary>
    SpikeAir,
    /// <summary>Special non-solid air whose reaction the block's behaviour selects.</summary>
    SpecialAir,
    /// <summary>Non-solid shootable-air behavior.</summary>
    ShootableAir,
    /// <summary>Horizontal extension: the block collides as the block its behaviour's displacement names.</summary>
    HorizontalExtension,
    /// <summary>Unused non-solid kind.</summary>
    UnusedAir,
    /// <summary>Non-solid bombable-air behavior.</summary>
    BombableAir,
    /// <summary>Ordinary fully solid block.</summary>
    SolidBlock,
    /// <summary>Door collision block associated with a door PLM.</summary>
    DoorBlock,
    /// <summary>Solid spike-block behavior.</summary>
    SpikeBlock,
    /// <summary>Special solid block whose reaction the block's behaviour selects.</summary>
    SpecialBlock,
    /// <summary>Solid shootable-block behavior.</summary>
    ShootableBlock,
    /// <summary>Vertical extension: the block collides as the block its behaviour's displacement names.</summary>
    VerticalExtension,
    /// <summary>Solid block that accepts the grapple beam.</summary>
    GrappleBlock,
    /// <summary>Solid bombable-block behavior.</summary>
    BombableBlock,
}
