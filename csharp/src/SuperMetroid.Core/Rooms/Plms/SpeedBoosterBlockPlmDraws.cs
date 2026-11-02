namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The crumble draw list of the Speed Booster blocks.</summary>
internal static class SpeedBoosterBlockPlmDraws
{
    /// <summary>The shared one-block crumble draw for crumble image <paramref name="frame"/> (0..3).</summary>
    internal static PlmDraw CrumbleDrawList(int frame) =>
        ShotBlockPlmDraws.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, frame);
}
