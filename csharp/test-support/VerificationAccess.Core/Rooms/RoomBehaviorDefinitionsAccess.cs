namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The import oracle's map between the typed room callbacks and the bank-$8F code offsets the cartridge stores; production
/// knows only the typed identities.
/// </summary>
internal static class RoomBehaviorDefinitionsAccess
{
    /// <summary>Each room-main callback's bank-$8F code offset.</summary>
    private static readonly Dictionary<RoomMainBehavior, ushort> MainPointers = new()
    {
        [RoomMainBehavior.None] = 0x0000,
        [RoomMainBehavior.ScrollingSkyLand] = 0xc116,
        [RoomMainBehavior.ScrollingSkyOcean] = 0xc11b,
        [RoomMainBehavior.ScrollingSkyLandZebesTimebombSet] = 0xc120,
        [RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions] = 0xc124,
        [RoomMainBehavior.ScrollScreenRightInDachoraRoom] = 0xc1e6,
        [RoomMainBehavior.MaridiaElevatube] = 0xe2b6,
        [RoomMainBehavior.CeresElevatorShaft] = 0xe51f,
        [RoomMainBehavior.Return] = 0xe524,
        [RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris] = 0xe525,
        [RoomMainBehavior.HandleCeresRidleyGetawayCutscene] = 0xe571,
        [RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal] = 0xe57c,
        [RoomMainBehavior.GenerateRandomExplosionEveryFourthFrame] = 0xe5a0,
        [RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal] = 0xe5a4,
        [RoomMainBehavior.CrocomireRoomShaking] = 0xe8cd,
        [RoomMainBehavior.RidleyRoomShaking] = 0xe950,
    };

    /// <summary>Each room-setup callback's bank-$8F code offset.</summary>
    private static readonly Dictionary<RoomSetupBehavior, ushort> SetupPointers = new()
    {
        [RoomSetupBehavior.None] = 0x0000,
        [RoomSetupBehavior.ClearBlocksAfterSavingAnimalsAndShakeScreen] = 0x9194,
        [RoomSetupBehavior.AutoDestroyWallDuringEscape] = 0x91a9,
        [RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape] = 0x91b2,
        [RoomSetupBehavior.ReturnAfterEscapeWallSetup] = 0x91bb,
        [RoomSetupBehavior.ReturnBeforeEscapeSkySetup] = 0x91bc,
        [RoomSetupBehavior.ShakeScreenAndCallScrollingSkyLandDuringEscape] = 0x91bd,
        [RoomSetupBehavior.ScrollingSkyLand] = 0x91c9,
        [RoomSetupBehavior.ScrollingSkyOcean] = 0x91ce,
        [RoomSetupBehavior.Return] = 0x91d3,
        [RoomSetupBehavior.ReturnAfterOceanSkySetup] = 0x91d4,
        [RoomSetupBehavior.ReturnBeforeStatueSetupA] = 0x91d5,
        [RoomSetupBehavior.ReturnBeforeStatueSetupB] = 0x91d6,
        [RoomSetupBehavior.RunStatueUnlockingAnimations] = 0x91d7,
        [RoomSetupBehavior.SharedReturn] = 0x91f4,
        [RoomSetupBehavior.SharedReturnB] = 0x91f5,
        [RoomSetupBehavior.SharedReturnC] = 0x91f6,
        [RoomSetupBehavior.SharedReturnD] = 0x91f7,
        [RoomSetupBehavior.OrdinaryReturn] = 0xc8c7,
        [RoomSetupBehavior.SpawnPrePhantoonRoomEnemyProjectile] = 0xc8c8,
        [RoomSetupBehavior.BossRoomReturn] = 0xc8d0,
        [RoomSetupBehavior.BossRoomReturnB] = 0xc8d1,
        [RoomSetupBehavior.BossRoomReturnC] = 0xc8d2,
        [RoomSetupBehavior.SetupShaktoolRoomObject] = 0xc8d3,
        [RoomSetupBehavior.ReturnBeforeDraygonSetup] = 0xc8dc,
        [RoomSetupBehavior.SetPausingCodeForDraygon] = 0xc8dd,
        [RoomSetupBehavior.SetCollectedMap] = 0xc90a,
        [RoomSetupBehavior.ReturnBeforeZebesTimebombSetup] = 0xc91e,
        [RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking] = 0xc91f,
        [RoomSetupBehavior.SetLightHorizontalRoomShaking] = 0xc933,
        [RoomSetupBehavior.SetMediumHorizontalRoomShaking] = 0xc946,
        [RoomSetupBehavior.SetupEscapeRoom4ObjectAndMediumHorizontalShaking] = 0xc953,
        [RoomSetupBehavior.TurnCeresDoorToSolidBlocksAndSpawnHaze] = 0xc96e,
        [RoomSetupBehavior.SpawnCeresHaze] = 0xc976,
        [RoomSetupBehavior.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze] = 0xc97b,
    };

    /// <summary>The room-main callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomMainBehavior> MainCallbacks =
        MainPointers.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The room-setup callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomSetupBehavior> SetupCallbacks =
        SetupPointers.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>Converts between typed room main and setup callbacks and their native dispatcher pointers.</summary>
    extension(RoomBehaviorDefinitions)
    {
        /// <summary>Returns a defined room-main callback's native pointer.</summary>
        internal static ushort PointerOf(RoomMainBehavior callback) => MainPointers.TryGetValue(callback, out ushort pointer)
            ? pointer
            : throw new ArgumentOutOfRangeException(nameof(callback));

        /// <summary>Returns a defined room-setup callback's native pointer.</summary>
        internal static ushort PointerOf(RoomSetupBehavior callback) => SetupPointers.TryGetValue(callback, out ushort pointer)
            ? pointer
            : throw new ArgumentOutOfRangeException(nameof(callback));

        /// <summary>Resolves a bank-$8F room-main pointer read from the cartridge to its typed callback.</summary>
        internal static RoomMainBehavior ResolveMain(ushort pointer) =>
            MainCallbacks.TryGetValue(pointer, out RoomMainBehavior callback)
                ? callback
                : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a retail room-main callback.");

        /// <summary>Resolves a bank-$8F room-setup pointer read from the cartridge to its typed callback.</summary>
        internal static RoomSetupBehavior ResolveSetup(ushort pointer) =>
            SetupCallbacks.TryGetValue(pointer, out RoomSetupBehavior callback)
                ? callback
                : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a retail room-setup callback.");
    }
}
