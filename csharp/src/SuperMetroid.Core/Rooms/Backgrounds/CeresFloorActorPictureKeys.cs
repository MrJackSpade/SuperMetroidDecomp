namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The installed key of each <see cref="CeresFloorActorPicture"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CeresFloorActorPictureKeys
{
    /// <summary>Each CeresFloorActorPicture member's installed key.</summary>
    internal static InstalledKeys<CeresFloorActorPicture> Installed { get; } = new(new Dictionary<CeresFloorActorPicture, string>
    {
        [CeresFloorActorPicture.ElevatorLight] = "ElevatorLight",
        [CeresFloorActorPicture.ElevatorDark] = "ElevatorDark",
        [CeresFloorActorPicture.BabyVertical] = "BabyVertical",
        [CeresFloorActorPicture.BabyRound] = "BabyRound",
        [CeresFloorActorPicture.BabyHorizontal] = "BabyHorizontal",
        [CeresFloorActorPicture.WingsRaised] = "WingsRaised",
        [CeresFloorActorPicture.WingsLowered] = "WingsLowered",
    });
}
