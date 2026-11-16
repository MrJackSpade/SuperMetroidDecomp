namespace SuperMetroid.Core.World;

/// <summary>Ceres Ridley's arena's main (#2287): his getaway with the Baby, which his encounter runs.</summary>
internal sealed class CeresRidleyGetawayMain : IRoomMain
{
    /// <inheritdoc/>
    public void Run(RoomMainUpdate update) =>
        update.Room.Enemies.Ceres.RunCeresRidleyGetawayRoomMain(update.Player.IntroducedSamus, update.FrameCounter);
}
