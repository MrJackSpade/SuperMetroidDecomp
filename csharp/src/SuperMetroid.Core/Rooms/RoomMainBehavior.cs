namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-main behaviors selected by a retail room state. Only the import oracle relates each to the game.</summary>
public enum RoomMainBehavior
{
    /// <summary>
    /// No room-specific main behaviour runs: the state names no main, or one that returns at once and leaves the working
    /// value <see cref="SuperMetroid.Core.Compatibility.RoomMainCarryover"/> holds untouched (#1787).
    /// </summary>
    None,
    /// <summary>Updates the land scrolling sky.</summary>
    ScrollingSkyLand,
    /// <summary>Updates the ocean scrolling sky.</summary>
    ScrollingSkyOcean,
    /// <summary>Updates the land sky and escape effects.</summary>
    ScrollingSkyLandZebesTimebombSet,
    /// <summary>Applies escape shaking and random explosions.</summary>
    SetScreenShakingAndGenerateRandomExplosions,
    /// <summary>Controls the Dachora room's rightward camera scroll.</summary>
    ScrollScreenRightInDachoraRoom,
    /// <summary>Advances the Maridia elevatube room behavior.</summary>
    MaridiaElevatube,
    /// <summary>Advances the Ceres shaft elevator behavior.</summary>
    CeresElevatorShaft,
    /// <summary>Spawns falling debris in the Ceres pre-elevator hall.</summary>
    SpawnCeresPreElevatorHallFallingDebris,
    /// <summary>Advances Ridley's Ceres getaway cutscene.</summary>
    HandleCeresRidleyGetawayCutscene,
    /// <summary>Alternates light horizontal and medium diagonal shaking.</summary>
    ShakeScreenLightHorizontalAndMediumDiagonal,
    /// <summary>Schedules random explosions every fourth gameplay frame.</summary>
    GenerateRandomExplosionEveryFourthFrame,
    /// <summary>Alternates medium horizontal and strong diagonal shaking.</summary>
    ShakeScreenMediumHorizontalAndStrongDiagonal,
    /// <summary>Applies the Crocomire room's encounter-dependent shaking.</summary>
    CrocomireRoomShaking,
    /// <summary>Selects Ridley's room-shaking behavior.</summary>
    RidleyRoomShaking,
}
