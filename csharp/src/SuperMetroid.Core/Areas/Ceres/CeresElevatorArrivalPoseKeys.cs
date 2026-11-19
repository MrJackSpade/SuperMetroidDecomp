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
}
