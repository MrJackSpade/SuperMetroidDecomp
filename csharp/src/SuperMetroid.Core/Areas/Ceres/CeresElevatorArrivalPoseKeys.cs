namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The installed key of each <see cref="CeresElevatorArrivalPose"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CeresElevatorArrivalPoseKeys
{
    /// <summary>Each CeresElevatorArrivalPose member's installed key.</summary>
    internal static InstalledKeys<CeresElevatorArrivalPose> Installed { get; } = new(new Dictionary<CeresElevatorArrivalPose, string>
    {
        [CeresElevatorArrivalPose.PadUnlit] = "PadUnlit",
        [CeresElevatorArrivalPose.PadLit] = "PadLit",
        [CeresElevatorArrivalPose.Platform] = "Platform",
    });

    /// <summary>The CeresElevatorArrivalPose members in the order the installation lists them (#2567), written out beside the keys so reordering the members never reorders an installed sheet or document.</summary>
    internal static IReadOnlyList<CeresElevatorArrivalPose> ListedOrder { get; } =
    [
        CeresElevatorArrivalPose.PadUnlit,
        CeresElevatorArrivalPose.PadLit,
        CeresElevatorArrivalPose.Platform,
    ];
}
