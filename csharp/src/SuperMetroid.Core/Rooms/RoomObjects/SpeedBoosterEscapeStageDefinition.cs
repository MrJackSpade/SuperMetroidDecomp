namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One physical lava stage used by the Speed Booster escape controller.</summary>
/// <param name="TargetSamusX">The room X, in pixels, Samus must run left to, or past, to trigger the stage.</param>
/// <param name="HighestLavaSurfaceY">The lava surface's highest allowed room Y, in pixels.</param>
/// <param name="YVelocity">The lava's rise velocity.</param>
internal readonly record struct SpeedBoosterEscapeStageDefinition(
    int TargetSamusX,
    int HighestLavaSurfaceY,
    SignedEightEight YVelocity);
