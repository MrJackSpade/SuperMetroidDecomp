namespace SuperMetroid.Core.Fx;

/// <summary>
/// Immutable control skeletons for the two Wrecked Ship treadmill block animations.
/// </summary>
/// <remarks>
/// The four frame sources are presentation identities resolved to installed art.
/// This catalog owns the object definition, the wait for the boss, durations, and loop control.
/// </remarks>
public static class WreckedShipTreadmillMechanicsDefinitions
{
    /// <summary>The frames in each treadmill loop.</summary>
    public const int FrameCount = 4;

    /// <summary>The passes each treadmill frame is shown for: every pass shows the next frame.</summary>
    public const int FrameDuration = 1;

    /// <summary>The two physical conveyor directions' definitions (<see cref="WreckedShipTreadmills"/>).</summary>
    internal static readonly IReadOnlyDictionary<WreckedShipTreadmillDirection, WreckedShipTreadmillLoop> Directions =
        new Dictionary<WreckedShipTreadmillDirection, WreckedShipTreadmillLoop>
        {
            [WreckedShipTreadmillDirection.Rightwards] =
                new(WreckedShipTreadmillDirection.Rightwards, BlockAnimation.WreckedShipTreadmillRightwards),
            [WreckedShipTreadmillDirection.Leftwards] =
                new(WreckedShipTreadmillDirection.Leftwards, BlockAnimation.WreckedShipTreadmillLeftwards),
        };

    /// <summary>The direction each treadmill object runs.</summary>
    internal static readonly IReadOnlyDictionary<BlockAnimation, WreckedShipTreadmillDirection> ObjectDirections =
        new Dictionary<BlockAnimation, WreckedShipTreadmillDirection>
        {
            [BlockAnimation.WreckedShipTreadmillRightwards] = WreckedShipTreadmillDirection.Rightwards,
            [BlockAnimation.WreckedShipTreadmillLeftwards] = WreckedShipTreadmillDirection.Leftwards,
        };
}
