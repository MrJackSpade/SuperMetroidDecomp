namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The crumble draw list of the Speed Booster blocks.</summary>
internal static class SpeedBoosterBlockObjectDraws
{
    /// <summary>The shared one-block crumble draw for crumble image <paramref name="frame"/> (0..3).</summary>
    internal static RoomObjectDraw CrumbleDrawList(int frame) =>
        ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, frame);
}
