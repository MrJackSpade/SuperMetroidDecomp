namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete high-nibble dispatch values in one native room level-data word. The names
/// follow the sixteen-entry bank-$94 collision jump tables rather than describing only
/// the subset currently translated by a particular caller.
/// </summary>
public enum RoomCollisionType : byte
{
    /// <summary>Empty air handled by dispatcher entry zero.</summary>
    Air = 0x0,
    /// <summary>Slope collision whose geometry is selected by BTS.</summary>
    Slope = 0x1,
    /// <summary>Non-solid spike-air behavior.</summary>
    SpikeAir = 0x2,
    /// <summary>Special non-solid air behavior selected by BTS.</summary>
    SpecialAir = 0x3,
    /// <summary>Non-solid shootable-air behavior.</summary>
    ShootableAir = 0x4,
    /// <summary>Horizontal extension whose BTS displacement resolves another block.</summary>
    HorizontalExtension = 0x5,
    /// <summary>Unused non-solid dispatcher entry.</summary>
    UnusedAir = 0x6,
    /// <summary>Non-solid bombable-air behavior.</summary>
    BombableAir = 0x7,
    /// <summary>Ordinary fully solid block.</summary>
    SolidBlock = 0x8,
    /// <summary>Door collision block associated with a door PLM.</summary>
    DoorBlock = 0x9,
    /// <summary>Solid spike-block behavior.</summary>
    SpikeBlock = 0xa,
    /// <summary>Special solid-block behavior selected by BTS.</summary>
    SpecialBlock = 0xb,
    /// <summary>Solid shootable-block behavior.</summary>
    ShootableBlock = 0xc,
    /// <summary>Vertical extension whose BTS displacement resolves another block.</summary>
    VerticalExtension = 0xd,
    /// <summary>Solid block that accepts the grapple beam.</summary>
    GrappleBlock = 0xe,
    /// <summary>Solid bombable-block behavior.</summary>
    BombableBlock = 0xf,
}
