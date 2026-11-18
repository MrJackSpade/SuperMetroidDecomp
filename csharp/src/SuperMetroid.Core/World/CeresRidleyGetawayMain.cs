using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>Ceres Ridley's arena's main (#2287): his getaway with the Baby, which his encounter runs over the arena's floor.</summary>
/// <param name="floor">
/// The arena's turning floor, whose capsule and wings the getaway animates (#2382); null in a state of the arena that shows
/// none, whose Ridley never flies the getaway.
/// </param>
internal sealed class CeresRidleyGetawayMain(RoomFloor? floor) : IRoomMain
{
    /// <inheritdoc/>
    public void Run(RoomMainUpdate update) =>
        update.Room.Enemies.Ceres.RunCeresRidleyGetawayRoomMain(update.Player.IntroducedSamus, floor, update.FrameCounter);
}
