namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="RoomCollisionType"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class RoomCollisionTypeKeys
{
    /// <summary>Each RoomCollisionType member's installed key.</summary>
    internal static InstalledKeys<RoomCollisionType> Installed { get; } = new(new Dictionary<RoomCollisionType, string>
    {
        [RoomCollisionType.Air] = "Air",
        [RoomCollisionType.Slope] = "Slope",
        [RoomCollisionType.SpikeAir] = "SpikeAir",
        [RoomCollisionType.SpecialAir] = "SpecialAir",
        [RoomCollisionType.ShootableAir] = "ShootableAir",
        [RoomCollisionType.HorizontalExtension] = "HorizontalExtension",
        [RoomCollisionType.UnrevealedAir] = "UnrevealedAir",
        [RoomCollisionType.BombableAir] = "BombableAir",
        [RoomCollisionType.SolidBlock] = "SolidBlock",
        [RoomCollisionType.DoorBlock] = "DoorBlock",
        [RoomCollisionType.SpikeBlock] = "SpikeBlock",
        [RoomCollisionType.SpecialBlock] = "SpecialBlock",
        [RoomCollisionType.ShootableBlock] = "ShootableBlock",
        [RoomCollisionType.VerticalExtension] = "VerticalExtension",
        [RoomCollisionType.GrappleBlock] = "GrappleBlock",
        [RoomCollisionType.BombableBlock] = "BombableBlock",
    });

    /// <summary>The RoomCollisionType members in the order an installed document lists them (#2567), written out beside the keys so reordering the members never changes the bytes written.</summary>
    internal static IReadOnlyList<RoomCollisionType> ListedOrder { get; } =
    [
        RoomCollisionType.Air,
        RoomCollisionType.Slope,
        RoomCollisionType.SpikeAir,
        RoomCollisionType.SpecialAir,
        RoomCollisionType.ShootableAir,
        RoomCollisionType.HorizontalExtension,
        RoomCollisionType.UnrevealedAir,
        RoomCollisionType.BombableAir,
        RoomCollisionType.SolidBlock,
        RoomCollisionType.DoorBlock,
        RoomCollisionType.SpikeBlock,
        RoomCollisionType.SpecialBlock,
        RoomCollisionType.ShootableBlock,
        RoomCollisionType.VerticalExtension,
        RoomCollisionType.GrappleBlock,
        RoomCollisionType.BombableBlock,
    ];
}
