namespace SuperMetroid.Core.Runtime;

/// <summary>Auditable host placement chosen by the verification grounded-Samus initializer.</summary>
internal readonly record struct DebugGroundedSamusPlacement(
    Position16<RoomPixels> Position,
    ushort DesiredScreenY,
    Position16<RoomBlocks> FloorBlockPosition,
    SuperMetroid.Core.Rooms.LevelData.RoomCollisionBlock FloorBlock,
    byte FloorHeight);
