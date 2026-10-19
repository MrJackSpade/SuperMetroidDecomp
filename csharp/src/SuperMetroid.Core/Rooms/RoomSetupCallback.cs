namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-setup behaviors selected by a retail room state. The bank-$8F code offset each names is known only to the import oracle.</summary>
public enum RoomSetupCallback
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
