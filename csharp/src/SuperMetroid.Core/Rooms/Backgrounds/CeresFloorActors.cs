namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Which actor each picture shown on a Ceres room's turning floor is a picture of (#1626, #2573), and the picture each shows
/// as a room loads.
/// </summary>
public static class CeresFloorActors
{
    /// <summary>The actor <paramref name="frame"/> is a picture of.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not a picture of the floor's actors.</exception>
    public static CeresFloorActor Of(CeresFloorActorPicture frame) => frame switch
    {
        CeresFloorActorPicture.ElevatorLight or CeresFloorActorPicture.ElevatorDark => CeresFloorActor.ElevatorPlatform,
        CeresFloorActorPicture.BabyVertical or CeresFloorActorPicture.BabyRound or CeresFloorActorPicture.BabyHorizontal =>
            CeresFloorActor.BabyCapsule,
        CeresFloorActorPicture.WingsRaised or CeresFloorActorPicture.WingsLowered => CeresFloorActor.RidleyWings,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a picture of the floor's actors."),
    };

    /// <summary>
    /// The picture each actor shows as a room loads, until it shows another (#2573): the platform lit, the Baby round in its
    /// capsule and Ridley's wings raised. The import checks the room's floor shows them so.
    /// </summary>
    public static IReadOnlyDictionary<CeresFloorActor, CeresFloorActorPicture> RestingPictures { get; } =
        new Dictionary<CeresFloorActor, CeresFloorActorPicture>
        {
            [CeresFloorActor.ElevatorPlatform] = CeresFloorActorPicture.ElevatorLight,
            [CeresFloorActor.BabyCapsule] = CeresFloorActorPicture.BabyRound,
            [CeresFloorActor.RidleyWings] = CeresFloorActorPicture.WingsRaised,
        };
}
