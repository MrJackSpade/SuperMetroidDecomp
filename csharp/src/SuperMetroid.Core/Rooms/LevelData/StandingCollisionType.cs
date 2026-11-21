namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// How a block a read stands for collides (#2553): every collision family but the two extensions, which a read follows to
/// the block they continue before it collides or reacts (<see cref="RoomLevelData.StandingBlockFor"/>). The values are
/// arbitrary: compare kinds by name.
/// </summary>
internal enum StandingCollisionType
{
    /// <summary>Empty air (<see cref="RoomCollisionType.Air"/>).</summary>
    Air,
    /// <summary>Slope collision whose geometry the block's behaviour selects (<see cref="RoomCollisionType.Slope"/>).</summary>
    Slope,
    /// <summary>Non-solid spike air (<see cref="RoomCollisionType.SpikeAir"/>).</summary>
    SpikeAir,
    /// <summary>Special non-solid air whose reaction the block's behaviour selects (<see cref="RoomCollisionType.SpecialAir"/>).</summary>
    SpecialAir,
    /// <summary>Non-solid shootable air (<see cref="RoomCollisionType.ShootableAir"/>).</summary>
    ShootableAir,
    /// <summary>Non-solid air the X-ray scope does not reveal (<see cref="RoomCollisionType.UnrevealedAir"/>).</summary>
    UnrevealedAir,
    /// <summary>Non-solid bombable air (<see cref="RoomCollisionType.BombableAir"/>).</summary>
    BombableAir,
    /// <summary>Ordinary fully solid block (<see cref="RoomCollisionType.SolidBlock"/>).</summary>
    SolidBlock,
    /// <summary>Door collision block (<see cref="RoomCollisionType.DoorBlock"/>).</summary>
    DoorBlock,
    /// <summary>Solid spike block (<see cref="RoomCollisionType.SpikeBlock"/>).</summary>
    SpikeBlock,
    /// <summary>Special solid block whose reaction the block's behaviour selects (<see cref="RoomCollisionType.SpecialBlock"/>).</summary>
    SpecialBlock,
    /// <summary>Solid shootable block (<see cref="RoomCollisionType.ShootableBlock"/>).</summary>
    ShootableBlock,
    /// <summary>Solid block that accepts the grapple beam (<see cref="RoomCollisionType.GrappleBlock"/>).</summary>
    GrappleBlock,
    /// <summary>Solid bombable block (<see cref="RoomCollisionType.BombableBlock"/>).</summary>
    BombableBlock,
}
