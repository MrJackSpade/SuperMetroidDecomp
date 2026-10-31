namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One collision PLM selected by the special-block tables
/// when Samus touches Speed Booster terrain.
/// </summary>
internal readonly record struct SpeedBoosterBlockPlmDefinition(
    PlmHeaderId Header,
    SpeedBoosterBlockProgram Program);
