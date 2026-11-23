namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The list of <see cref="RoomMainBehavior"/> that only verification reads (#2485).</summary>
internal static class RoomMainBehaviorConstants
{
    /// <summary>Every room-main behaviour the room states author, <see cref="RoomMainBehavior.None"/> first.</summary>
    public static IReadOnlyList<RoomMainBehavior> All { get; } =
    [
        RoomMainBehavior.None, RoomMainBehavior.ScrollingSkyLand, RoomMainBehavior.ScrollingSkyOcean,
        RoomMainBehavior.ScrollingSkyLandWithEscapeExplosions, RoomMainBehavior.ShakeScreenWithRandomExplosions,
        RoomMainBehavior.MaridiaElevatube, RoomMainBehavior.CeresElevatorShaft,
        RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris, RoomMainBehavior.CeresRidleyGetaway,
        RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal, RoomMainBehavior.RandomEscapeExplosions,
        RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal, RoomMainBehavior.CrocomireRoomShaking,
        RoomMainBehavior.DachoraShaftCamera,
    ];
}
