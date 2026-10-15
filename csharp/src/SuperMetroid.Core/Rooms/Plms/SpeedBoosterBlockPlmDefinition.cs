namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One bank-$84 collision PLM selected by the special-block tables at
/// <c>$94:9139-$92F7</c> when Samus touches Speed Booster terrain.
/// </summary>
internal readonly record struct SpeedBoosterBlockPlmDefinition(
    PlmHeaderId HeaderPointer,
    SpeedBoosterBlockProgram Program);
