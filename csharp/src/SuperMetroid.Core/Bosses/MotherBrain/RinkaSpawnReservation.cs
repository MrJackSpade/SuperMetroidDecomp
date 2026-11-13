namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The spawn point a special Rinka has just reserved.</summary>
/// <param name="Reserved">The point the Rinka now holds.</param>
/// <param name="MovesTo">
/// The point's centre when the Rinka could not keep its own population spawn and moves there, its population spawn
/// moving with it; null when it keeps the place it was spawned at.
/// </param>
internal readonly record struct RinkaSpawnReservation(
    RinkaSpawnPoint Reserved,
    Position16<RoomPixels>? MovesTo);
