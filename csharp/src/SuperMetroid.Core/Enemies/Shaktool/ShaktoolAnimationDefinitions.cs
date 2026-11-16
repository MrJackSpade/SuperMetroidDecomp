namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The fixed animation selections Shaktool's mechanics state machine makes.</summary>
internal static class ShaktoolAnimationDefinitions
{
    /// <summary>
    /// The head animation of each of the eight directions, an eighth of a turn apart, the head aims in.
    /// </summary>
    internal static readonly Dictionary<CompassDirection, ShaktoolSegment> OrientationAnimations = new()
    {
        [CompassDirection.Up] = ShaktoolSegment.HeadAimingUp,
        [CompassDirection.UpRight] = ShaktoolSegment.HeadAimingUpRight,
        [CompassDirection.Right] = ShaktoolSegment.HeadAimingRight,
        [CompassDirection.DownRight] = ShaktoolSegment.HeadAimingDownRight,
        [CompassDirection.Down] = ShaktoolSegment.HeadAimingDown,
        [CompassDirection.DownLeft] = ShaktoolSegment.HeadAimingDownLeft,
        [CompassDirection.Left] = ShaktoolSegment.HeadAimingLeft,
        [CompassDirection.UpLeft] = ShaktoolSegment.HeadAimingUpLeft,
    };
}
