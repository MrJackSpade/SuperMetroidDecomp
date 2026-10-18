namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-main behaviors selected by a retail room state. The bank-$8F code offset each names is known only to the import oracle.</summary>
public enum RoomMainCallback
{
    /// <summary>A zero room-main pointer; no room-specific main callback runs.</summary>
    None,
    /// <summary><c>$8F:C116 MainASM_ScrollingSkyLand</c>: updates the land scrolling sky.</summary>
    ScrollingSkyLand,
    /// <summary><c>$8F:C11B MainASM_ScrollingSkyOcean</c>: updates the ocean scrolling sky.</summary>
    ScrollingSkyOcean,
    /// <summary><c>$8F:C120 MainASM_ScrollingSkyLand_ZebesTimebombSet</c>: updates the land sky and escape effects.</summary>
    ScrollingSkyLandZebesTimebombSet,
    /// <summary><c>$8F:C124 MainASM_SetScreenShaking_GenerateRandomExplosions</c>: applies escape shaking and random explosions.</summary>
    SetScreenShakingAndGenerateRandomExplosions,
    /// <summary><c>$8F:C1E6 MainASM_ScrollScreenRightInDachoraRoom</c>: controls the Dachora room's rightward camera scroll.</summary>
    ScrollScreenRightInDachoraRoom,
    /// <summary><c>$8F:E2B6 MainASM_Elevatube</c>: advances the Maridia elevatube room behavior.</summary>
    MaridiaElevatube,
    /// <summary><c>$8F:E51F MainASM_CeresElevatorShaft</c>: advances the Ceres shaft elevator behavior.</summary>
    CeresElevatorShaft,
    /// <summary><c>$8F:E524 RTS_8FE524</c>: a present room-main callback that returns without changing state.</summary>
    Return,
    /// <summary><c>$8F:E525 MainASM_SpawnCeresPreElevatorHallFallingDebris</c>: spawns falling debris in the Ceres pre-elevator hall.</summary>
    SpawnCeresPreElevatorHallFallingDebris,
    /// <summary><c>$8F:E571 MainASM_HandleCeresRidleyGetawayCutscene</c>: advances Ridley's Ceres getaway cutscene.</summary>
    HandleCeresRidleyGetawayCutscene,
    /// <summary><c>$8F:E57C MainASM_ShakeScreenSwitchingBetweenLightHorizAndMediumDiag</c>: alternates light horizontal and medium diagonal shaking.</summary>
    ShakeScreenLightHorizontalAndMediumDiagonal,
    /// <summary><c>$8F:E5A0 MainASM_GenerateRandomExplosionOnEveryFourthFrame</c>: schedules random explosions every fourth gameplay frame.</summary>
    GenerateRandomExplosionEveryFourthFrame,
    /// <summary><c>$8F:E5A4 MainASM_ShakeScreenSwitchingBetweenMediumHorizAndStrongDiag</c>: alternates medium horizontal and strong diagonal shaking.</summary>
    ShakeScreenMediumHorizontalAndStrongDiagonal,
    /// <summary><c>$8F:E8CD MainASM_CrocomiresRoomShaking</c>: applies the Crocomire room's encounter-dependent shaking.</summary>
    CrocomireRoomShaking,
    /// <summary><c>$8F:E950 MainASM_RidleysRoomShaking</c>: selects Ridley's room-shaking behavior.</summary>
    RidleyRoomShaking,
}
