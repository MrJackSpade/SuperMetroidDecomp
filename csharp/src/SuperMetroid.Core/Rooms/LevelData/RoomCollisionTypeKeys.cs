namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="RoomCollisionType"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class RoomCollisionTypeKeys
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
}
