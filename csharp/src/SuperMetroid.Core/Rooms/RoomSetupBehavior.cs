namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-setup behaviors selected by a retail room state. Only the import oracle relates each to the game.</summary>
public enum RoomSetupBehavior
{
    /// <summary>
    /// No room-specific setup behaviour runs: the state names no setup, or one that returns at once without changing
    /// anything (#1787).
    /// </summary>
    None,
    /// <summary>Clears the rescued animals' exit blocks and starts shaking.</summary>
    ClearBlocksAfterSavingAnimalsAndShakeScreen,
    /// <summary>Selects automatic escape-wall destruction during room setup.</summary>
    AutoDestroyWallDuringEscape,
    /// <summary>Creates the escape wall's shootable blocks.</summary>
    TurnWallIntoShotBlocksDuringEscape,
    /// <summary>Starts the landing site's escape quake and sets up the land scrolling sky with its bands.</summary>
    LandSkyWithLandingSiteQuake,
    /// <summary>Initializes the land scrolling sky when the room loads.</summary>
    ScrollingSkyLand,
    /// <summary>Initializes the ocean scrolling sky when the room loads.</summary>
    ScrollingSkyOcean,
    /// <summary>Starts the statue-unlocking animations during room setup.</summary>
    RunStatueUnlockingAnimations,
    /// <summary>Spawns the pre-Phantoon room's setup projectile.</summary>
    SpawnPrePhantoonRoomEnemyProjectile,
    /// <summary>Creates Shaktool's room-setup room object.</summary>
    SetupShaktoolRoomObject,
    /// <summary>
    /// Has Draygon's room restore its own display interrupts when the game unpauses. The port draws the room without
    /// display interrupts, so nothing acts on it.
    /// </summary>
    KeepDraygonRoomDisplayAcrossPause,
    /// <summary>Marks the room's area map as collected.</summary>
    SetCollectedMap,
    /// <summary>Sets the timebomb event and starts light horizontal shaking.</summary>
    SetZebesTimebombEventAndLightHorizontalShaking,
    /// <summary>Starts light horizontal escape-room shaking.</summary>
    SetLightHorizontalRoomShaking,
    /// <summary>Starts medium horizontal escape-room shaking.</summary>
    SetMediumHorizontalRoomShaking,
    /// <summary>
    /// Starts medium horizontal shaking in the escape room before the old Tourian escape shaft and places the room object
    /// that starts its acid rising once Samus passes its trigger point (#1800).
    /// </summary>
    MediumHorizontalShakingWithRisingAcid,
    /// <summary>Seals the Ceres door and spawns haze.</summary>
    TurnCeresDoorToSolidBlocksAndSpawnHaze,
    /// <summary>Spawns the Ceres room haze during setup.</summary>
    SpawnCeresHaze,
    /// <summary>Points the Ceres Ridley arena's backgrounds at the sprites' artwork and spawns haze.</summary>
    ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze,
}
