using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The cycles of Ceres floor animation frames the elevator platform, the Baby's capsule and Ridley's wings show (#1467,
/// #1747). Each frame is a picture drawn over the turning floor (<see cref="CeresFloorAnimation"/>); the import pictures it.
/// </summary>
public static class CeresFloorAnimationDefinitions
{
    /// <summary>The capsule's four phases: the Baby squishes vertically, rounds, squishes horizontally and rounds again.</summary>
    internal static readonly CeresFloorAnimationFrame[] CapsuleCycle =
    [
        CeresFloorAnimationFrame.BabyVertical, CeresFloorAnimationFrame.BabyRound,
        CeresFloorAnimationFrame.BabyHorizontal, CeresFloorAnimationFrame.BabyRound,
    ];

    /// <summary>The elevator platform's two phases: its light strip, then its dark strip.</summary>
    internal static readonly CeresFloorAnimationFrame[] ElevatorPlatformCycle =
        [CeresFloorAnimationFrame.ElevatorLight, CeresFloorAnimationFrame.ElevatorDark];

    /// <summary>The wings' two beats: raised, then lowered.</summary>
    internal static readonly CeresFloorAnimationFrame[] WingCycle =
        [CeresFloorAnimationFrame.WingsRaised, CeresFloorAnimationFrame.WingsLowered];
}
