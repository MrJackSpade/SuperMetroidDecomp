namespace SuperMetroid.Core.Rooms;

/// <summary>Native bank-$8F room-setup callback offsets present in retail room states.</summary>
/// <remarks>
/// Setup callbacks run after PLM creation and door ASM but before elevator finalization.
/// Keeping this catalog separate from <see cref="DoorCodes"/> preserves that ordering and
/// prevents two unrelated bank-$8F callback domains from being compared interchangeably.
/// </remarks>
public static class RoomSetupCodePointers
{
    /// <summary><c>$8F:9194 SetupASM_ClearBlocksAfterSavingAnimalsAndShakeScreen</c>.</summary>
    public const ushort ClearBlocksAfterSavingAnimalsAndShakeScreen = 0x9194;
    /// <summary><c>$8F:91A9 SetupASM_AutoDestroyWallDuringEscape</c>.</summary>
    public const ushort AutoDestroyWallDuringEscape = 0x91a9;
    /// <summary><c>$8F:91B2 SetupASM_TurnWallIntoShotBlocksDuringEscape</c>.</summary>
    public const ushort TurnWallIntoShotBlocksDuringEscape = 0x91b2;
    /// <summary><c>$8F:91BB RTS_8F91BB</c>: an explicit no-op room setup.</summary>
    public const ushort ReturnAfterEscapeWallSetup = 0x91bb;
    /// <summary><c>$8F:91BC RTS_8F91BC</c>: an explicit no-op room setup.</summary>
    public const ushort ReturnBeforeEscapeSkySetup = 0x91bc;
    /// <summary><c>$8F:91BD SetupASM_ShakeScreenAndCall88A7D8DuringEscape</c>.</summary>
    public const ushort ShakeScreenAndCallScrollingSkyLandDuringEscape = 0x91bd;
    /// <summary><c>$8F:91C9 SetupASM_ScrollingSkyLand</c>.</summary>
    public const ushort ScrollingSkyLand = 0x91c9;
    /// <summary><c>$8F:91CE SetupASM_ScrollingSkyOcean</c>.</summary>
    public const ushort ScrollingSkyOcean = 0x91ce;
    /// <summary><c>$8F:91D3 RTS_8F91D3</c>: an explicit no-op room setup.</summary>
    public const ushort Return = 0x91d3;
    /// <summary><c>$8F:91D4 RTS_8F91D4</c>: the adjacent explicit no-op entry.</summary>
    public const ushort ReturnAfterOceanSkySetup = 0x91d4;
    /// <summary><c>$8F:91D5 RTS_8F91D5</c>: an explicit no-op setup entry.</summary>
    public const ushort ReturnBeforeStatueSetupA = 0x91d5;
    /// <summary><c>$8F:91D6 RTS_8F91D6</c>: an explicit no-op setup entry.</summary>
    public const ushort ReturnBeforeStatueSetupB = 0x91d6;
    /// <summary><c>$8F:91D7 SetupASM_RunStatueUnlockingAnimations</c>.</summary>
    public const ushort RunStatueUnlockingAnimations = 0x91d7;
    /// <summary><c>$8F:91F4 RTS_8F91F4</c>: the shared explicit no-op setup.</summary>
    public const ushort SharedReturn = 0x91f4;
    /// <summary><c>$8F:91F5 RTS_8F91F5</c>: a distinct room-authored no-op entry.</summary>
    public const ushort SharedReturnB = 0x91f5;
    /// <summary><c>$8F:91F6 RTS_8F91F6</c>: a distinct room-authored no-op entry.</summary>
    public const ushort SharedReturnC = 0x91f6;
    /// <summary><c>$8F:91F7 RTS_8F91F7</c>: a distinct room-authored no-op entry.</summary>
    public const ushort SharedReturnD = 0x91f7;
    /// <summary><c>$8F:C8C7 RTS_8FC8C7</c>: the ordinary shared no-op setup.</summary>
    public const ushort OrdinaryReturn = 0xc8c7;
    /// <summary><c>$8F:C8C8 SetupASM_SpawnPrePhantoonRoomEnemyProjectile</c>.</summary>
    public const ushort SpawnPrePhantoonRoomEnemyProjectile = 0xc8c8;
    /// <summary><c>$8F:C8D0 RTS_8FC8D0</c>: a shared boss-room no-op setup.</summary>
    public const ushort BossRoomReturn = 0xc8d0;
    /// <summary><c>$8F:C8D1 RTS_8FC8D1</c>: a distinct boss-room no-op entry.</summary>
    public const ushort BossRoomReturnB = 0xc8d1;
    /// <summary><c>$8F:C8D2 RTS_8FC8D2</c>: a distinct boss-room no-op entry.</summary>
    public const ushort BossRoomReturnC = 0xc8d2;
    /// <summary><c>$8F:C8D3 SetupASM_SetupShaktoolsRoomPLM</c>.</summary>
    public const ushort SetupShaktoolRoomPlm = 0xc8d3;
    /// <summary><c>$8F:C8DC RTS_8FC8DC</c>: the no-op entry before Draygon setup.</summary>
    public const ushort ReturnBeforeDraygonSetup = 0xc8dc;
    /// <summary><c>$8F:C8DD SetupASM_SetPausingCodeForDraygon</c>.</summary>
    public const ushort SetPausingCodeForDraygon = 0xc8dd;
    /// <summary><c>$8F:C90A SetupASM_SetCollectedMap</c>.</summary>
    public const ushort SetCollectedMap = 0xc90a;
    /// <summary><c>$8F:C91E RTS_8FC91E</c>: the no-op entry before timebomb setup.</summary>
    public const ushort ReturnBeforeZebesTimebombSetup = 0xc91e;
    /// <summary><c>$8F:C91F SetupASM_SetZebesTimebombEvent_SetLightHorizontalRoomShaking</c>.</summary>
    public const ushort SetZebesTimebombEventAndLightHorizontalShaking = 0xc91f;
    /// <summary><c>$8F:C933 SetupASM_SetLightHorizontalRoomShaking</c>.</summary>
    public const ushort SetLightHorizontalRoomShaking = 0xc933;
    /// <summary><c>$8F:C946 SetupASM_SetMediumHorizontalRoomShaking</c>.</summary>
    public const ushort SetMediumHorizontalRoomShaking = 0xc946;
    /// <summary><c>$8F:C953 SetupASM_SetupEscapeRoom4sPLM_SetMediumHorizontalRoomShaking</c>.</summary>
    public const ushort SetupEscapeRoom4PlmAndMediumHorizontalShaking = 0xc953;
    /// <summary><c>$8F:C96E SetupASM_TurnCeresDoorToSolidBlocks_SpawnCeresHaze</c>.</summary>
    public const ushort TurnCeresDoorToSolidBlocksAndSpawnHaze = 0xc96e;
    /// <summary><c>$8F:C976 SetupASM_SpawnCeresHaze</c>.</summary>
    public const ushort SpawnCeresHaze = 0xc976;
    /// <summary><c>$8F:C97B SetupASM_SetBG1_2_TilesBaseAddress_SpawnCeresHaze</c>.</summary>
    public const ushort SetCeresRidleyBgCharacterBaseAndSpawnHaze = 0xc97b;

    /// <summary>Whether a translated presentation path consumes Ceres haze from this setup.</summary>
    public static bool SpawnsCeresHaze(ushort pointer) => pointer is
        TurnCeresDoorToSolidBlocksAndSpawnHaze or
        SpawnCeresHaze or
        SetCeresRidleyBgCharacterBaseAndSpawnHaze;
}
