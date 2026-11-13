namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Which part of a Ceres room's turning floor each floor animation frame shows (#1626).</summary>
public static class CeresFloorParts
{
    /// <summary>The part of the floor <paramref name="frame"/> animates.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a floor animation frame.</exception>
    public static CeresFloorPart Of(CeresFloorAnimationFrame frame) => frame switch
    {
        CeresFloorAnimationFrame.ElevatorLight or CeresFloorAnimationFrame.ElevatorDark => CeresFloorPart.ElevatorPlatform,
        CeresFloorAnimationFrame.BabyFrame0 or CeresFloorAnimationFrame.BabyFrame1 or CeresFloorAnimationFrame.BabyFrame2 =>
            CeresFloorPart.BabyCapsule,
        CeresFloorAnimationFrame.WingFrame0 or CeresFloorAnimationFrame.WingFrame1 => CeresFloorPart.RidleyWings,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a floor animation frame."),
    };
}
