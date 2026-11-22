using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Samus's whole position, which an arriving elevator's setup places her at once the door loader finishes.</summary>
/// <param name="Elevator">The arriving elevator whose setup placed Samus; the write waits until loading has initialized it.</param>
/// <param name="Position">Where the elevator placed Samus.</param>
internal readonly partial record struct LoaderSamusPlacement(RoomEnemyPlace Elevator, Position16<RoomPixels> Position);
