namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-main behaviors selected by a retail room state.</summary>
public enum RoomMainCallback : byte
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

/// <summary>Mutually exclusive room-setup behaviors selected by a retail room state.</summary>
public enum RoomSetupCallback : byte
{
    /// <summary>A zero room-setup pointer; no room-specific setup callback runs.</summary>
    None,
    /// <summary><c>$8F:9194 SetupASM_ClearBlocksAfterSavingAnimalsAndShakeScreen</c>: clears the rescued animals' exit blocks and starts shaking.</summary>
    ClearBlocksAfterSavingAnimalsAndShakeScreen,
    /// <summary><c>$8F:91A9 SetupASM_AutoDestroyWallDuringEscape</c>: selects automatic escape-wall destruction during room setup.</summary>
    AutoDestroyWallDuringEscape,
    /// <summary><c>$8F:91B2 SetupASM_TurnWallIntoShotBlocksDuringEscape</c>: creates the escape wall's shootable blocks.</summary>
    TurnWallIntoShotBlocksDuringEscape,
    /// <summary><c>$8F:91BB RTS_8F91BB</c>: the no-op setup entry following the escape-wall routines.</summary>
    ReturnAfterEscapeWallSetup,
    /// <summary><c>$8F:91BC RTS_8F91BC</c>: the no-op setup entry preceding the escape-sky routine.</summary>
    ReturnBeforeEscapeSkySetup,
    /// <summary><c>$8F:91BD SetupASM_ShakeScreenAndCall88A7D8DuringEscape</c>: initializes the escape land sky and landing-area quake.</summary>
    ShakeScreenAndCallScrollingSkyLandDuringEscape,
    /// <summary><c>$8F:91C9 SetupASM_ScrollingSkyLand</c>: initializes the land scrolling sky when the room loads.</summary>
    ScrollingSkyLand,
    /// <summary><c>$8F:91CE SetupASM_ScrollingSkyOcean</c>: initializes the ocean scrolling sky when the room loads.</summary>
    ScrollingSkyOcean,
    /// <summary><c>$8F:91D3 RTS_8F91D3</c>: an explicit no-op room-setup entry.</summary>
    Return,
    /// <summary><c>$8F:91D4 RTS_8F91D4</c>: the no-op setup entry following ocean-sky initialization.</summary>
    ReturnAfterOceanSkySetup,
    /// <summary><c>$8F:91D5 RTS_8F91D5</c>: the first no-op setup entry preceding statue initialization.</summary>
    ReturnBeforeStatueSetupA,
    /// <summary><c>$8F:91D6 RTS_8F91D6</c>: the second no-op setup entry preceding statue initialization.</summary>
    ReturnBeforeStatueSetupB,
    /// <summary><c>$8F:91D7 SetupASM_RunStatueUnlockingAnimations</c>: starts the statue-unlocking animations during room setup.</summary>
    RunStatueUnlockingAnimations,
    /// <summary><c>$8F:91F4 RTS_8F91F4</c>: the shared no-op setup callback.</summary>
    SharedReturn,
    /// <summary><c>$8F:91F5 RTS_8F91F5</c>: a distinct room-authored no-op setup callback.</summary>
    SharedReturnB,
    /// <summary><c>$8F:91F6 RTS_8F91F6</c>: a distinct room-authored no-op setup callback.</summary>
    SharedReturnC,
    /// <summary><c>$8F:91F7 RTS_8F91F7</c>: a distinct room-authored no-op setup callback.</summary>
    SharedReturnD,
    /// <summary><c>$8F:C8C7 RTS_8FC8C7</c>: the ordinary shared no-op setup callback.</summary>
    OrdinaryReturn,
    /// <summary><c>$8F:C8C8 SetupASM_SpawnPrePhantoonRoomEnemyProjectile</c>: spawns the pre-Phantoon room's setup projectile.</summary>
    SpawnPrePhantoonRoomEnemyProjectile,
    /// <summary><c>$8F:C8D0 RTS_8FC8D0</c>: the shared boss-room no-op setup callback.</summary>
    BossRoomReturn,
    /// <summary><c>$8F:C8D1 RTS_8FC8D1</c>: a distinct boss-room no-op setup callback.</summary>
    BossRoomReturnB,
    /// <summary><c>$8F:C8D2 RTS_8FC8D2</c>: a distinct boss-room no-op setup callback.</summary>
    BossRoomReturnC,
    /// <summary><c>$8F:C8D3 SetupASM_SetupShaktoolsRoomPLM</c>: creates Shaktool's room-setup PLM.</summary>
    SetupShaktoolRoomPlm,
    /// <summary><c>$8F:C8DC RTS_8FC8DC</c>: the no-op setup entry preceding Draygon setup.</summary>
    ReturnBeforeDraygonSetup,
    /// <summary><c>$8F:C8DD SetupASM_SetPausingCodeForDraygon</c>: installs Draygon's pause-related room callbacks.</summary>
    SetPausingCodeForDraygon,
    /// <summary><c>$8F:C90A SetupASM_SetCollectedMap</c>: marks the room's area map as collected.</summary>
    SetCollectedMap,
    /// <summary><c>$8F:C91E RTS_8FC91E</c>: the no-op setup entry preceding Zebes timebomb initialization.</summary>
    ReturnBeforeZebesTimebombSetup,
    /// <summary><c>$8F:C91F SetupASM_SetZebesTimebombEvent_SetLightHorizontalRoomShaking</c>: sets the timebomb event and starts light horizontal shaking.</summary>
    SetZebesTimebombEventAndLightHorizontalShaking,
    /// <summary><c>$8F:C933 SetupASM_SetLightHorizontalRoomShaking</c>: starts light horizontal escape-room shaking.</summary>
    SetLightHorizontalRoomShaking,
    /// <summary><c>$8F:C946 SetupASM_SetMediumHorizontalRoomShaking</c>: starts medium horizontal escape-room shaking.</summary>
    SetMediumHorizontalRoomShaking,
    /// <summary><c>$8F:C953 SetupASM_SetupEscapeRoom4sPLM_SetMediumHorizontalRoomShaking</c>: initializes escape room four's PLM and medium horizontal shaking.</summary>
    SetupEscapeRoom4PlmAndMediumHorizontalShaking,
    /// <summary><c>$8F:C96E SetupASM_TurnCeresDoorToSolidBlocks_SpawnCeresHaze</c>: seals the Ceres door and spawns haze.</summary>
    TurnCeresDoorToSolidBlocksAndSpawnHaze,
    /// <summary><c>$8F:C976 SetupASM_SpawnCeresHaze</c>: spawns the Ceres room haze during setup.</summary>
    SpawnCeresHaze,
    /// <summary><c>$8F:C97B SetupASM_SetBG1_2_TilesBaseAddress_SpawnCeresHaze</c>: sets Ceres Ridley's background character base and spawns haze.</summary>
    SetCeresRidleyBgCharacterBaseAndSpawnHaze,
}

/// <summary>Typed dispatch identities for every main/setup pointer used by retail room states.</summary>
public static class RoomCallbackDefinitions
{
    /// <summary>Resolves a bank-$8F room-main pointer to its exclusive typed dispatch identity.</summary>
    /// <param name="pointer">The room state's main-code offset; zero selects no callback.</param>
    /// <returns>The callback selected for subsequent room-main updates.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pointer does not identify a retail room-main callback.</exception>
    public static RoomMainCallback ResolveMain(ushort pointer) => pointer switch
    {
        0 => RoomMainCallback.None,
        RoomMainCodePointers.ScrollingSkyLand => RoomMainCallback.ScrollingSkyLand,
        RoomMainCodePointers.ScrollingSkyOcean => RoomMainCallback.ScrollingSkyOcean,
        RoomMainCodePointers.ScrollingSkyLandZebesTimebombSet => RoomMainCallback.ScrollingSkyLandZebesTimebombSet,
        RoomMainCodePointers.SetScreenShakingAndGenerateRandomExplosions => RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions,
        RoomMainCodePointers.ScrollScreenRightInDachoraRoom => RoomMainCallback.ScrollScreenRightInDachoraRoom,
        RoomMainCodePointers.MaridiaElevatube => RoomMainCallback.MaridiaElevatube,
        RoomMainCodePointers.CeresElevatorShaft => RoomMainCallback.CeresElevatorShaft,
        RoomMainCodePointers.Return => RoomMainCallback.Return,
        RoomMainCodePointers.SpawnCeresPreElevatorHallFallingDebris => RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris,
        RoomMainCodePointers.HandleCeresRidleyGetawayCutscene => RoomMainCallback.HandleCeresRidleyGetawayCutscene,
        RoomMainCodePointers.ShakeScreenLightHorizontalAndMediumDiagonal => RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal,
        RoomMainCodePointers.GenerateRandomExplosionEveryFourthFrame => RoomMainCallback.GenerateRandomExplosionEveryFourthFrame,
        RoomMainCodePointers.ShakeScreenMediumHorizontalAndStrongDiagonal => RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal,
        RoomMainCodePointers.CrocomireRoomShaking => RoomMainCallback.CrocomireRoomShaking,
        RoomMainCodePointers.RidleyRoomShaking => RoomMainCallback.RidleyRoomShaking,
        _ => throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
            "Pointer is not a retail room-main callback."),
    };

    /// <summary>Resolves a bank-$8F room-setup pointer to its exclusive typed dispatch identity.</summary>
    /// <param name="pointer">The room state's setup-code offset; zero selects no callback.</param>
    /// <returns>The callback selected to run after PLM creation and door ASM, before elevator finalization.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The pointer does not identify a retail room-setup callback.</exception>
    public static RoomSetupCallback ResolveSetup(ushort pointer) => pointer switch
    {
        0 => RoomSetupCallback.None,
        RoomSetupCodePointers.ClearBlocksAfterSavingAnimalsAndShakeScreen => RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen,
        RoomSetupCodePointers.AutoDestroyWallDuringEscape => RoomSetupCallback.AutoDestroyWallDuringEscape,
        RoomSetupCodePointers.TurnWallIntoShotBlocksDuringEscape => RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape,
        RoomSetupCodePointers.ReturnAfterEscapeWallSetup => RoomSetupCallback.ReturnAfterEscapeWallSetup,
        RoomSetupCodePointers.ReturnBeforeEscapeSkySetup => RoomSetupCallback.ReturnBeforeEscapeSkySetup,
        RoomSetupCodePointers.ShakeScreenAndCallScrollingSkyLandDuringEscape => RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape,
        RoomSetupCodePointers.ScrollingSkyLand => RoomSetupCallback.ScrollingSkyLand,
        RoomSetupCodePointers.ScrollingSkyOcean => RoomSetupCallback.ScrollingSkyOcean,
        RoomSetupCodePointers.Return => RoomSetupCallback.Return,
        RoomSetupCodePointers.ReturnAfterOceanSkySetup => RoomSetupCallback.ReturnAfterOceanSkySetup,
        RoomSetupCodePointers.ReturnBeforeStatueSetupA => RoomSetupCallback.ReturnBeforeStatueSetupA,
        RoomSetupCodePointers.ReturnBeforeStatueSetupB => RoomSetupCallback.ReturnBeforeStatueSetupB,
        RoomSetupCodePointers.RunStatueUnlockingAnimations => RoomSetupCallback.RunStatueUnlockingAnimations,
        RoomSetupCodePointers.SharedReturn => RoomSetupCallback.SharedReturn,
        RoomSetupCodePointers.SharedReturnB => RoomSetupCallback.SharedReturnB,
        RoomSetupCodePointers.SharedReturnC => RoomSetupCallback.SharedReturnC,
        RoomSetupCodePointers.SharedReturnD => RoomSetupCallback.SharedReturnD,
        RoomSetupCodePointers.OrdinaryReturn => RoomSetupCallback.OrdinaryReturn,
        RoomSetupCodePointers.SpawnPrePhantoonRoomEnemyProjectile => RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile,
        RoomSetupCodePointers.BossRoomReturn => RoomSetupCallback.BossRoomReturn,
        RoomSetupCodePointers.BossRoomReturnB => RoomSetupCallback.BossRoomReturnB,
        RoomSetupCodePointers.BossRoomReturnC => RoomSetupCallback.BossRoomReturnC,
        RoomSetupCodePointers.SetupShaktoolRoomPlm => RoomSetupCallback.SetupShaktoolRoomPlm,
        RoomSetupCodePointers.ReturnBeforeDraygonSetup => RoomSetupCallback.ReturnBeforeDraygonSetup,
        RoomSetupCodePointers.SetPausingCodeForDraygon => RoomSetupCallback.SetPausingCodeForDraygon,
        RoomSetupCodePointers.SetCollectedMap => RoomSetupCallback.SetCollectedMap,
        RoomSetupCodePointers.ReturnBeforeZebesTimebombSetup => RoomSetupCallback.ReturnBeforeZebesTimebombSetup,
        RoomSetupCodePointers.SetZebesTimebombEventAndLightHorizontalShaking => RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking,
        RoomSetupCodePointers.SetLightHorizontalRoomShaking => RoomSetupCallback.SetLightHorizontalRoomShaking,
        RoomSetupCodePointers.SetMediumHorizontalRoomShaking => RoomSetupCallback.SetMediumHorizontalRoomShaking,
        RoomSetupCodePointers.SetupEscapeRoom4PlmAndMediumHorizontalShaking => RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking,
        RoomSetupCodePointers.TurnCeresDoorToSolidBlocksAndSpawnHaze => RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze,
        RoomSetupCodePointers.SpawnCeresHaze => RoomSetupCallback.SpawnCeresHaze,
        RoomSetupCodePointers.SetCeresRidleyBgCharacterBaseAndSpawnHaze => RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze,
        _ => throw new ArgumentOutOfRangeException(nameof(pointer), pointer,
            "Pointer is not a retail room-setup callback."),
    };

    /// <summary>Determines whether the selected setup callback initializes the Ceres haze effect.</summary>
    /// <param name="callback">The room's resolved setup callback.</param>
    /// <returns>Whether room loading should enable Ceres haze for this callback.</returns>
    public static bool SpawnsCeresHaze(RoomSetupCallback callback) => callback is
        RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze or
        RoomSetupCallback.SpawnCeresHaze or
        RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
}
