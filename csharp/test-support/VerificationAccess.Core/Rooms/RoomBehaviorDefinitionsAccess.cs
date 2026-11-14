namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The import oracle's map from the bank-$8F code offsets the cartridge stores to the typed room callbacks; production
/// knows only the typed identities. Every offset whose routine returns at once decodes to the callback's <c>None</c>,
/// as offset zero does, so the many return instructions the cartridge points at are one behaviour (#1787); an offset
/// the map does not know is refused.
/// </summary>
internal static class RoomBehaviorDefinitionsAccess
{
    /// <summary>The room-main callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomMainBehavior> MainCallbacks = new()
    {
        [0x0000] = RoomMainBehavior.None,
        [0xc116] = RoomMainBehavior.ScrollingSkyLand,
        [0xc11b] = RoomMainBehavior.ScrollingSkyOcean,
        [0xc120] = RoomMainBehavior.ScrollingSkyLandZebesTimebombSet,
        [0xc124] = RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions,
        [0xc1e6] = RoomMainBehavior.ScrollScreenRightInDachoraRoom,
        [0xe2b6] = RoomMainBehavior.MaridiaElevatube,
        [0xe51f] = RoomMainBehavior.CeresElevatorShaft,
        [0xe524] = RoomMainBehavior.None,
        [0xe525] = RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris,
        [0xe571] = RoomMainBehavior.HandleCeresRidleyGetawayCutscene,
        [0xe57c] = RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal,
        [0xe5a0] = RoomMainBehavior.GenerateRandomExplosionEveryFourthFrame,
        [0xe5a4] = RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal,
        [0xe8cd] = RoomMainBehavior.CrocomireRoomShaking,
        [0xe950] = RoomMainBehavior.RidleyRoomShaking,
    };

    /// <summary>The room-setup callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomSetupBehavior> SetupCallbacks = new()
    {
        [0x0000] = RoomSetupBehavior.None,
        [0x9194] = RoomSetupBehavior.ClearBlocksAfterSavingAnimalsAndShakeScreen,
        [0x91a9] = RoomSetupBehavior.AutoDestroyWallDuringEscape,
        [0x91b2] = RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape,
        [0x91bb] = RoomSetupBehavior.None,
        [0x91bc] = RoomSetupBehavior.None,
        [0x91bd] = RoomSetupBehavior.LandSkyWithLandingSiteQuake,
        [0x91c9] = RoomSetupBehavior.ScrollingSkyLand,
        [0x91ce] = RoomSetupBehavior.ScrollingSkyOcean,
        [0x91d3] = RoomSetupBehavior.None,
        [0x91d4] = RoomSetupBehavior.None,
        [0x91d5] = RoomSetupBehavior.None,
        [0x91d6] = RoomSetupBehavior.None,
        [0x91d7] = RoomSetupBehavior.RunStatueUnlockingAnimations,
        [0x91f4] = RoomSetupBehavior.None,
        [0x91f5] = RoomSetupBehavior.None,
        [0x91f6] = RoomSetupBehavior.None,
        [0x91f7] = RoomSetupBehavior.None,
        [0xc8c7] = RoomSetupBehavior.None,
        [0xc8c8] = RoomSetupBehavior.SpawnPrePhantoonRoomEnemyProjectile,
        [0xc8d0] = RoomSetupBehavior.None,
        [0xc8d1] = RoomSetupBehavior.None,
        [0xc8d2] = RoomSetupBehavior.None,
        [0xc8d3] = RoomSetupBehavior.SetupShaktoolRoomObject,
        [0xc8dc] = RoomSetupBehavior.None,
        [0xc8dd] = RoomSetupBehavior.KeepDraygonRoomDisplayAcrossPause,
        [0xc90a] = RoomSetupBehavior.SetCollectedMap,
        [0xc91e] = RoomSetupBehavior.None,
        [0xc91f] = RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking,
        [0xc933] = RoomSetupBehavior.SetLightHorizontalRoomShaking,
        [0xc946] = RoomSetupBehavior.SetMediumHorizontalRoomShaking,
        [0xc953] = RoomSetupBehavior.MediumHorizontalShakingWithRisingLava,
        [0xc96e] = RoomSetupBehavior.TurnCeresDoorToSolidBlocksAndSpawnHaze,
        [0xc976] = RoomSetupBehavior.SpawnCeresHaze,
        [0xc97b] = RoomSetupBehavior.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze,
    };

    /// <summary>Decodes the room main and setup callbacks the cartridge's room states point at.</summary>
    extension(RoomBehaviorDefinitions)
    {
        /// <summary>Every nonzero bank-$8F room-main code offset the import knows, each naming a routine.</summary>
        internal static IReadOnlyDictionary<ushort, RoomMainBehavior> MainRoutines =>
            MainCallbacks.Where(pair => pair.Key != 0).ToDictionary();

        /// <summary>Every nonzero bank-$8F room-setup code offset the import knows, each naming a routine.</summary>
        internal static IReadOnlyDictionary<ushort, RoomSetupBehavior> SetupRoutines =>
            SetupCallbacks.Where(pair => pair.Key != 0).ToDictionary();

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
