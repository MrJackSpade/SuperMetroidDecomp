namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-setup behaviors selected by a retail room state. Only the import oracle relates each to the game.</summary>
public enum RoomSetupBehavior
{
    /// <summary>No room-specific setup behaviour runs.</summary>
    None,
    /// <summary>Clears the rescued animals' exit blocks and starts shaking.</summary>
    ClearBlocksAfterSavingAnimalsAndShakeScreen,
    /// <summary>Selects automatic escape-wall destruction during room setup.</summary>
    AutoDestroyWallDuringEscape,
    /// <summary>Creates the escape wall's shootable blocks.</summary>
    TurnWallIntoShotBlocksDuringEscape,
    /// <summary>The no-op setup entry following the escape-wall setups.</summary>
    ReturnAfterEscapeWallSetup,
    /// <summary>The no-op setup entry preceding the escape-sky setup.</summary>
    ReturnBeforeEscapeSkySetup,
    /// <summary>Initializes the escape land sky and landing-area quake.</summary>
    ShakeScreenAndCallScrollingSkyLandDuringEscape,
    /// <summary>Initializes the land scrolling sky when the room loads.</summary>
    ScrollingSkyLand,
    /// <summary>Initializes the ocean scrolling sky when the room loads.</summary>
    ScrollingSkyOcean,
    /// <summary>An explicit no-op room-setup entry.</summary>
    Return,
    /// <summary>The no-op setup entry following ocean-sky initialization.</summary>
    ReturnAfterOceanSkySetup,
    /// <summary>The first no-op setup entry preceding statue initialization.</summary>
    ReturnBeforeStatueSetupA,
    /// <summary>The second no-op setup entry preceding statue initialization.</summary>
    ReturnBeforeStatueSetupB,
    /// <summary>Starts the statue-unlocking animations during room setup.</summary>
    RunStatueUnlockingAnimations,
    /// <summary>The shared no-op setup behaviour.</summary>
    SharedReturn,
    /// <summary>A distinct room-authored no-op setup behaviour.</summary>
    SharedReturnB,
    /// <summary>A distinct room-authored no-op setup behaviour.</summary>
    SharedReturnC,
    /// <summary>A distinct room-authored no-op setup behaviour.</summary>
    SharedReturnD,
    /// <summary>The ordinary shared no-op setup behaviour.</summary>
    OrdinaryReturn,
    /// <summary>Spawns the pre-Phantoon room's setup projectile.</summary>
    SpawnPrePhantoonRoomEnemyProjectile,
    /// <summary>The shared boss-room no-op setup behaviour.</summary>
    BossRoomReturn,
    /// <summary>A distinct boss-room no-op setup behaviour.</summary>
    BossRoomReturnB,
    /// <summary>A distinct boss-room no-op setup behaviour.</summary>
    BossRoomReturnC,
    /// <summary>Creates Shaktool's room-setup room object.</summary>
    SetupShaktoolRoomObject,
    /// <summary>The no-op setup entry preceding Draygon setup.</summary>
    ReturnBeforeDraygonSetup,
    /// <summary>Installs Draygon's pause-related room behaviours.</summary>
    SetPausingCodeForDraygon,
    /// <summary>Marks the room's area map as collected.</summary>
    SetCollectedMap,
    /// <summary>The no-op setup entry preceding Zebes timebomb initialization.</summary>
    ReturnBeforeZebesTimebombSetup,
    /// <summary>Sets the timebomb event and starts light horizontal shaking.</summary>
    SetZebesTimebombEventAndLightHorizontalShaking,
    /// <summary>Starts light horizontal escape-room shaking.</summary>
    SetLightHorizontalRoomShaking,
    /// <summary>Starts medium horizontal escape-room shaking.</summary>
    SetMediumHorizontalRoomShaking,
    /// <summary>Initializes escape room four's room object and medium horizontal shaking.</summary>
    SetupEscapeRoom4ObjectAndMediumHorizontalShaking,
    /// <summary>Seals the Ceres door and spawns haze.</summary>
    TurnCeresDoorToSolidBlocksAndSpawnHaze,
    /// <summary>Spawns the Ceres room haze during setup.</summary>
    SpawnCeresHaze,
    /// <summary>Points the Ceres Ridley arena's backgrounds at the sprites' artwork and spawns haze.</summary>
    ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze,
}
