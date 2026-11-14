using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Gameplay;

/// <summary>A door loader's deferred write of Samus's whole position, made by an arriving elevator's setup.</summary>
/// <param name="Elevator">The arriving elevator whose setup placed Samus; the write waits until loading has initialized it.</param>
/// <param name="Position">Where the elevator placed Samus.</param>
internal readonly record struct LoaderSamusPlacement(RoomEnemySlot Elevator, Position16<RoomPixels> Position);
