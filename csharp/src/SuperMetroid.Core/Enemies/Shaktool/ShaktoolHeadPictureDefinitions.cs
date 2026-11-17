using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The pictures Shaktool's head shows as its chain orients it (#2284).</summary>
internal static class ShaktoolHeadPictureDefinitions
{
    /// <summary>The picture of the head aiming each of the eight directions, an eighth of a turn apart.</summary>
    internal static readonly Dictionary<CompassDirection, EnemyFrameId> AimedPictures = new()
    {
        [CompassDirection.Up] = EnemyFrameId.ShaktoolHeadUp,
        [CompassDirection.UpRight] = EnemyFrameId.ShaktoolHeadUpRight,
        [CompassDirection.Right] = EnemyFrameId.ShaktoolHeadRight,
        [CompassDirection.DownRight] = EnemyFrameId.ShaktoolHeadDownRight,
        [CompassDirection.Down] = EnemyFrameId.ShaktoolHeadDown,
        [CompassDirection.DownLeft] = EnemyFrameId.ShaktoolHeadDownLeft,
        [CompassDirection.Left] = EnemyFrameId.ShaktoolHeadLeft,
        [CompassDirection.UpLeft] = EnemyFrameId.ShaktoolHeadUpLeft,
    };
}
