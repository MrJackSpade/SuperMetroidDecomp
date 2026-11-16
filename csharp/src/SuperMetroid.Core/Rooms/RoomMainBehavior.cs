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
    /// <summary>Updates the land sky and sets off the escape's random explosions.</summary>
    ScrollingSkyLandWithEscapeExplosions,
    /// <summary>Applies escape shaking and random explosions.</summary>
    ShakeScreenWithRandomExplosions,
    /// <summary>Controls the Dachora room's rightward camera scroll.</summary>
    ScrollScreenRightInDachoraRoom,
    /// <summary>Advances the Maridia elevatube room behavior.</summary>
    MaridiaElevatube,
    /// <summary>Advances the Ceres shaft elevator behavior.</summary>
    CeresElevatorShaft,
    /// <summary>Spawns falling debris in the Ceres pre-elevator hall.</summary>
    SpawnCeresPreElevatorHallFallingDebris,
    /// <summary>Advances Ridley's Ceres getaway cutscene.</summary>
    CeresRidleyGetaway,
    /// <summary>Alternates light horizontal and medium diagonal shaking.</summary>
    ShakeScreenLightHorizontalAndMediumDiagonal,
    /// <summary>Sets off a random explosion every fourth gameplay update.</summary>
    RandomExplosionEveryFourthUpdate,
    /// <summary>Alternates medium horizontal and strong diagonal shaking.</summary>
    ShakeScreenMediumHorizontalAndStrongDiagonal,
    /// <summary>Applies the Crocomire room's encounter-dependent shaking.</summary>
    CrocomireRoomShaking,
    /// <summary>Selects Ridley's room-shaking behavior.</summary>
    RidleyRoomShaking,
}
