namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The crumbling draws of the Speed Booster blocks (#1994).</summary>
internal static class SpeedBoosterBlockObjectDraws
{
    /// <summary>A Speed Booster block showing <paramref name="stage"/> as it crumbles: the shared one-block breakup.</summary>
    internal static RoomObjectDraw Crumble(BlockBreakupStage stage) =>
        ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, stage);
}
