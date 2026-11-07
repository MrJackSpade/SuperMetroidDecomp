namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One collision PLM selected by the special-block tables
/// when Samus touches Speed Booster terrain.
/// </summary>
internal readonly record struct SpeedBoosterBlockObjectDefinition(
    RoomObjectHeaderId Header,
    SpeedBoosterBlockProgram Program);
