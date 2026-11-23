namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Speed Booster blocks' draws (#1994, #2034): the bombed reveal's family and the crumbling draws.</summary>
internal static partial class SpeedBoosterBlockObjectDraws
{
    /// <summary>The bombed Speed Booster block's reveal, pictured by its installed frame.</summary>
    internal static RoomObjectDrawFamily<SpeedBoosterBlockObjectDrawDefinitions.Draw> BombRevealFamily { get; } =
        new RoomObjectDrawFamily<SpeedBoosterBlockObjectDrawDefinitions.Draw>("Speed Booster",
            _ => SpeedBoosterBlockObjectDrawDefinitions.BombRevealShape);

    /// <summary>A Speed Booster block showing <paramref name="stage"/> as it crumbles: the shared one-block breakup.</summary>
    internal static RoomObjectDraw Crumble(BlockBreakupStage stage) =>
        ShotBlockObjectDraws.Breakup(BlockArrangement.Single, stage);
}
