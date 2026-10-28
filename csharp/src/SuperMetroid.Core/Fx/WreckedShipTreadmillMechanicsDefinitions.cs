namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable control skeletons for the two Wrecked Ship treadmill animated-tile objects.
/// </summary>
/// <remarks>
/// The four frame-source operands are presentation identities resolved to installed art.
/// This catalog owns the object header, boss-wait command, durations, and loop control.
/// </remarks>
public static class WreckedShipTreadmillMechanicsDefinitions
{
    /// <summary>The frames in each treadmill loop.</summary>
    public const int FrameCount = 4;

    /// <summary>The bytes of characters every treadmill frame shows: two characters.</summary>
    public const ushort FrameByteCount = 32;

    /// <summary>The passes each treadmill frame is shown for: every pass shows the next frame.</summary>
    public const int FrameDuration = 1;

    /// <summary>The two physical conveyor directions' native headers (<see cref="WreckedShipTreadmills"/>).</summary>
    internal static readonly IReadOnlyDictionary<WreckedShipTreadmillDirection, WreckedShipTreadmillLoop> Directions =
        new Dictionary<WreckedShipTreadmillDirection, WreckedShipTreadmillLoop>
        {
            [WreckedShipTreadmillDirection.Rightwards] =
                new(WreckedShipTreadmillDirection.Rightwards, AnimatedTileObject.WreckedShipTreadmillRightwards),
            [WreckedShipTreadmillDirection.Leftwards] =
                new(WreckedShipTreadmillDirection.Leftwards, AnimatedTileObject.WreckedShipTreadmillLeftwards),
        };

    /// <summary>The direction each treadmill object runs.</summary>
    internal static readonly IReadOnlyDictionary<AnimatedTileObject, WreckedShipTreadmillDirection> ObjectDirections =
        new Dictionary<AnimatedTileObject, WreckedShipTreadmillDirection>
        {
            [AnimatedTileObject.WreckedShipTreadmillRightwards] = WreckedShipTreadmillDirection.Rightwards,
            [AnimatedTileObject.WreckedShipTreadmillLeftwards] = WreckedShipTreadmillDirection.Leftwards,
        };
}
