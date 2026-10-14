namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-main behaviors selected by a retail room state, valued by their bank-$8F code offset.</summary>
public enum RoomMainCallback : ushort
{
    /// <summary>A zero room-main pointer; no room-specific main callback runs.</summary>
    None = 0,
    /// <summary><c>$8F:C116 MainASM_ScrollingSkyLand</c>: updates the land scrolling sky.</summary>
    ScrollingSkyLand = 0xc116,
    /// <summary><c>$8F:C11B MainASM_ScrollingSkyOcean</c>: updates the ocean scrolling sky.</summary>
    ScrollingSkyOcean = 0xc11b,
    /// <summary><c>$8F:C120 MainASM_ScrollingSkyLand_ZebesTimebombSet</c>: updates the land sky and escape effects.</summary>
    ScrollingSkyLandZebesTimebombSet = 0xc120,
    /// <summary><c>$8F:C124 MainASM_SetScreenShaking_GenerateRandomExplosions</c>: applies escape shaking and random explosions.</summary>
    SetScreenShakingAndGenerateRandomExplosions = 0xc124,
    /// <summary><c>$8F:C1E6 MainASM_ScrollScreenRightInDachoraRoom</c>: controls the Dachora room's rightward camera scroll.</summary>
    ScrollScreenRightInDachoraRoom = 0xc1e6,
    /// <summary><c>$8F:E2B6 MainASM_Elevatube</c>: advances the Maridia elevatube room behavior.</summary>
    MaridiaElevatube = 0xe2b6,
    /// <summary><c>$8F:E51F MainASM_CeresElevatorShaft</c>: advances the Ceres shaft elevator behavior.</summary>
    CeresElevatorShaft = 0xe51f,
    /// <summary><c>$8F:E524 RTS_8FE524</c>: a present room-main callback that returns without changing state.</summary>
    Return = 0xe524,
    /// <summary><c>$8F:E525 MainASM_SpawnCeresPreElevatorHallFallingDebris</c>: spawns falling debris in the Ceres pre-elevator hall.</summary>
    SpawnCeresPreElevatorHallFallingDebris = 0xe525,
    /// <summary><c>$8F:E571 MainASM_HandleCeresRidleyGetawayCutscene</c>: advances Ridley's Ceres getaway cutscene.</summary>
    HandleCeresRidleyGetawayCutscene = 0xe571,
    /// <summary><c>$8F:E57C MainASM_ShakeScreenSwitchingBetweenLightHorizAndMediumDiag</c>: alternates light horizontal and medium diagonal shaking.</summary>
    ShakeScreenLightHorizontalAndMediumDiagonal = 0xe57c,
    /// <summary><c>$8F:E5A0 MainASM_GenerateRandomExplosionOnEveryFourthFrame</c>: schedules random explosions every fourth gameplay frame.</summary>
    GenerateRandomExplosionEveryFourthFrame = 0xe5a0,
    /// <summary><c>$8F:E5A4 MainASM_ShakeScreenSwitchingBetweenMediumHorizAndStrongDiag</c>: alternates medium horizontal and strong diagonal shaking.</summary>
    ShakeScreenMediumHorizontalAndStrongDiagonal = 0xe5a4,
    /// <summary><c>$8F:E8CD MainASM_CrocomiresRoomShaking</c>: applies the Crocomire room's encounter-dependent shaking.</summary>
    CrocomireRoomShaking = 0xe8cd,
    /// <summary><c>$8F:E950 MainASM_RidleysRoomShaking</c>: selects Ridley's room-shaking behavior.</summary>
    RidleyRoomShaking = 0xe950,
}
