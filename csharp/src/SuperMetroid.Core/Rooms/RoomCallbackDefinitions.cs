namespace SuperMetroid.Core.Rooms;

/// <summary>Mutually exclusive room-main behaviors selected by a retail room state.</summary>
public enum RoomMainCallback : byte
{
    None,
    ScrollingSkyLand,
    ScrollingSkyOcean,
    ScrollingSkyLandZebesTimebombSet,
    SetScreenShakingAndGenerateRandomExplosions,
    ScrollScreenRightInDachoraRoom,
    MaridiaElevatube,
    CeresElevatorShaft,
    Return,
    SpawnCeresPreElevatorHallFallingDebris,
    HandleCeresRidleyGetawayCutscene,
    ShakeScreenLightHorizontalAndMediumDiagonal,
    GenerateRandomExplosionEveryFourthFrame,
    ShakeScreenMediumHorizontalAndStrongDiagonal,
    CrocomireRoomShaking,
    RidleyRoomShaking,
}

/// <summary>Mutually exclusive room-setup behaviors selected by a retail room state.</summary>
public enum RoomSetupCallback : byte
{
    None,
    ClearBlocksAfterSavingAnimalsAndShakeScreen,
    AutoDestroyWallDuringEscape,
    TurnWallIntoShotBlocksDuringEscape,
    ReturnAfterEscapeWallSetup,
    ReturnBeforeEscapeSkySetup,
    ShakeScreenAndCallScrollingSkyLandDuringEscape,
    ScrollingSkyLand,
    ScrollingSkyOcean,
    Return,
    ReturnAfterOceanSkySetup,
    ReturnBeforeStatueSetupA,
    ReturnBeforeStatueSetupB,
    RunStatueUnlockingAnimations,
    SharedReturn,
    SharedReturnB,
    SharedReturnC,
    SharedReturnD,
    OrdinaryReturn,
    SpawnPrePhantoonRoomEnemyProjectile,
    BossRoomReturn,
    BossRoomReturnB,
    BossRoomReturnC,
    SetupShaktoolRoomPlm,
    ReturnBeforeDraygonSetup,
    SetPausingCodeForDraygon,
    SetCollectedMap,
    ReturnBeforeZebesTimebombSetup,
    SetZebesTimebombEventAndLightHorizontalShaking,
    SetLightHorizontalRoomShaking,
    SetMediumHorizontalRoomShaking,
    SetupEscapeRoom4PlmAndMediumHorizontalShaking,
    TurnCeresDoorToSolidBlocksAndSpawnHaze,
    SpawnCeresHaze,
    SetCeresRidleyBgCharacterBaseAndSpawnHaze,
}

/// <summary>Typed dispatch identities for every main/setup pointer used by retail room states.</summary>
public static class RoomCallbackDefinitions
{
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

    public static bool SpawnsCeresHaze(RoomSetupCallback callback) => callback is
        RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze or
        RoomSetupCallback.SpawnCeresHaze or
        RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
}
