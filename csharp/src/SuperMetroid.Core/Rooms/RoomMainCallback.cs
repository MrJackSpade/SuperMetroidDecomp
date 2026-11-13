namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-main behaviors selected by a retail room state. Only the import oracle relates each to the game.</summary>
public enum RoomMainCallback
{
    /// <summary>No room-specific main behaviour runs.</summary>
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
    /// <summary>A present room-main behaviour that returns without changing state.</summary>
    Return,
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
