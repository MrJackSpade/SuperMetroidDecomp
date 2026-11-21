namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Which actor each picture shown on a Ceres room's turning floor is a picture of (#1626, #2573).
/// </summary>
public static partial class CeresFloorActors
{
    /// <summary>
    /// The floor's actors in the order their pictures lie over it (#2567): the elevator platform, the Baby's capsule, then
    /// Ridley's wings.
    /// </summary>
    public static IReadOnlyList<CeresFloorActor> DrawingOrder { get; } =
        [CeresFloorActor.ElevatorPlatform, CeresFloorActor.BabyCapsule, CeresFloorActor.RidleyWings];

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

}
