namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One rise of the Speed Booster escape's lava (<see cref="SpeedBoosterEscapeObject"/>): the line Samus passes and the height and speed it gives the lava.</summary>
/// <param name="TargetSamusX">The room X, in pixels, Samus must run left to, or past, to trigger the stage.</param>
/// <param name="HighestLavaSurfaceY">The lava surface's highest allowed room Y, in pixels.</param>
/// <param name="YVelocity">The lava's rise velocity.</param>
internal readonly partial record struct SpeedBoosterEscapeStageDefinition(
    int TargetSamusX,
    int HighestLavaSurfaceY,
    SignedEightEight YVelocity);
