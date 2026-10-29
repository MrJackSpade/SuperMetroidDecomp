namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The import oracle's map between the typed room callbacks and the bank-$8F code offsets the cartridge stores; production
/// knows only the typed identities.
/// </summary>
internal static class RoomCallbackDefinitionsAccess
{
    /// <summary>Each room-main callback's bank-$8F code offset.</summary>
    private static readonly Dictionary<RoomMainCallback, ushort> MainPointers = new()
    {
        [RoomMainCallback.None] = 0x0000,
        [RoomMainCallback.ScrollingSkyLand] = 0xc116,
        [RoomMainCallback.ScrollingSkyOcean] = 0xc11b,
        [RoomMainCallback.ScrollingSkyLandZebesTimebombSet] = 0xc120,
        [RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions] = 0xc124,
        [RoomMainCallback.ScrollScreenRightInDachoraRoom] = 0xc1e6,
        [RoomMainCallback.MaridiaElevatube] = 0xe2b6,
        [RoomMainCallback.CeresElevatorShaft] = 0xe51f,
        [RoomMainCallback.Return] = 0xe524,
        [RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris] = 0xe525,
        [RoomMainCallback.HandleCeresRidleyGetawayCutscene] = 0xe571,
        [RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal] = 0xe57c,
        [RoomMainCallback.GenerateRandomExplosionEveryFourthFrame] = 0xe5a0,
        [RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal] = 0xe5a4,
        [RoomMainCallback.CrocomireRoomShaking] = 0xe8cd,
        [RoomMainCallback.RidleyRoomShaking] = 0xe950,
    };

    /// <summary>Each room-setup callback's bank-$8F code offset.</summary>
    private static readonly Dictionary<RoomSetupCallback, ushort> SetupPointers = new()
    {
        [RoomSetupCallback.None] = 0x0000,
        [RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen] = 0x9194,
        [RoomSetupCallback.AutoDestroyWallDuringEscape] = 0x91a9,
        [RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape] = 0x91b2,
        [RoomSetupCallback.ReturnAfterEscapeWallSetup] = 0x91bb,
        [RoomSetupCallback.ReturnBeforeEscapeSkySetup] = 0x91bc,
        [RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape] = 0x91bd,
        [RoomSetupCallback.ScrollingSkyLand] = 0x91c9,
        [RoomSetupCallback.ScrollingSkyOcean] = 0x91ce,
        [RoomSetupCallback.Return] = 0x91d3,
        [RoomSetupCallback.ReturnAfterOceanSkySetup] = 0x91d4,
        [RoomSetupCallback.ReturnBeforeStatueSetupA] = 0x91d5,
        [RoomSetupCallback.ReturnBeforeStatueSetupB] = 0x91d6,
        [RoomSetupCallback.RunStatueUnlockingAnimations] = 0x91d7,
        [RoomSetupCallback.SharedReturn] = 0x91f4,
        [RoomSetupCallback.SharedReturnB] = 0x91f5,
        [RoomSetupCallback.SharedReturnC] = 0x91f6,
        [RoomSetupCallback.SharedReturnD] = 0x91f7,
        [RoomSetupCallback.OrdinaryReturn] = 0xc8c7,
        [RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile] = 0xc8c8,
        [RoomSetupCallback.BossRoomReturn] = 0xc8d0,
        [RoomSetupCallback.BossRoomReturnB] = 0xc8d1,
        [RoomSetupCallback.BossRoomReturnC] = 0xc8d2,
        [RoomSetupCallback.SetupShaktoolRoomPlm] = 0xc8d3,
        [RoomSetupCallback.ReturnBeforeDraygonSetup] = 0xc8dc,
        [RoomSetupCallback.SetPausingCodeForDraygon] = 0xc8dd,
        [RoomSetupCallback.SetCollectedMap] = 0xc90a,
        [RoomSetupCallback.ReturnBeforeZebesTimebombSetup] = 0xc91e,
        [RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking] = 0xc91f,
        [RoomSetupCallback.SetLightHorizontalRoomShaking] = 0xc933,
        [RoomSetupCallback.SetMediumHorizontalRoomShaking] = 0xc946,
        [RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking] = 0xc953,
        [RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze] = 0xc96e,
        [RoomSetupCallback.SpawnCeresHaze] = 0xc976,
        [RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze] = 0xc97b,
    };

    /// <summary>The room-main callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomMainCallback> MainCallbacks =
        MainPointers.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The room-setup callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomSetupCallback> SetupCallbacks =
        SetupPointers.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>Converts between typed room main and setup callbacks and their native dispatcher pointers.</summary>
    extension(RoomCallbackDefinitions)
    {
        /// <summary>Returns a defined room-main callback's native pointer.</summary>
        internal static ushort PointerOf(RoomMainCallback callback) => MainPointers.TryGetValue(callback, out ushort pointer)
            ? pointer
            : throw new ArgumentOutOfRangeException(nameof(callback));

        /// <summary>Returns a defined room-setup callback's native pointer.</summary>
        internal static ushort PointerOf(RoomSetupCallback callback) => SetupPointers.TryGetValue(callback, out ushort pointer)
            ? pointer
            : throw new ArgumentOutOfRangeException(nameof(callback));

        /// <summary>Resolves a bank-$8F room-main pointer read from the cartridge to its typed callback.</summary>
        internal static RoomMainCallback ResolveMain(ushort pointer) =>
            MainCallbacks.TryGetValue(pointer, out RoomMainCallback callback)
                ? callback
                : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a retail room-main callback.");

        /// <summary>Resolves a bank-$8F room-setup pointer read from the cartridge to its typed callback.</summary>
        internal static RoomSetupCallback ResolveSetup(ushort pointer) =>
            SetupCallbacks.TryGetValue(pointer, out RoomSetupCallback callback)
                ? callback
                : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a retail room-setup callback.");
    }
}
