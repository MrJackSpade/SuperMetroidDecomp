namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster blocks' draws (#1994, #2034): the bombed reveal's family and the crumbling draws.</summary>
internal static class SpeedBoosterBlockObjectDraws
{
    /// <summary>The bombed Speed Booster block's reveal, pictured by its installed frame.</summary>
    internal static RoomObjectDrawFamily<SpeedBoosterBlockObjectDrawDefinitions.Draw> BombRevealFamily { get; } =
        RoomObjectDrawFamily<SpeedBoosterBlockObjectDrawDefinitions.Draw>.Pictured<InstalledRoomObjectSpeedBoosterVisuals>("Speed Booster",
            _ => SpeedBoosterBlockObjectDrawDefinitions.BombRevealShape, (visuals, draw) => visuals.Image(draw));

    /// <summary>A Speed Booster block showing <paramref name="stage"/> as it crumbles: the shared one-block breakup.</summary>
    internal static RoomObjectDraw Crumble(BlockBreakupStage stage) =>
        ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, stage);
}
