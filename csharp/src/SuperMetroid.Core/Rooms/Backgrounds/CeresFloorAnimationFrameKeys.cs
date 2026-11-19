namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The installed key of each <see cref="CeresFloorAnimationFrame"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CeresFloorAnimationFrameKeys
{
    /// <summary>Each CeresFloorAnimationFrame member's installed key.</summary>
    internal static InstalledKeys<CeresFloorAnimationFrame> Installed { get; } = new(new Dictionary<CeresFloorAnimationFrame, string>
    {
        [CeresFloorAnimationFrame.ElevatorLight] = "ElevatorLight",
        [CeresFloorAnimationFrame.ElevatorDark] = "ElevatorDark",
        [CeresFloorAnimationFrame.BabyVertical] = "BabyVertical",
        [CeresFloorAnimationFrame.BabyRound] = "BabyRound",
        [CeresFloorAnimationFrame.BabyHorizontal] = "BabyHorizontal",
        [CeresFloorAnimationFrame.WingsRaised] = "WingsRaised",
        [CeresFloorAnimationFrame.WingsLowered] = "WingsLowered",
    });
}
