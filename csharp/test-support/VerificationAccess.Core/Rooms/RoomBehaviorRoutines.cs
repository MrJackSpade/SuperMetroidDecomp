using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The import oracle's map from the bank-$8F code offsets the cartridge stores to the room states' main and setup
/// behaviour objects; production knows only the behaviours. Every offset whose routine returns at once decodes to the
/// behaviour's <c>None</c>, as offset zero does, so the many return instructions the cartridge points at are one behaviour
/// (#1787), and so do Draygon's room's setup, which acts only on the room's display (#1827), and the main the port does
/// not run (#2287). The land and the ocean skies' setups are one behaviour, and so are the ordinary Ceres rooms' and Ceres
/// Ridley's arena's haze setups, the arena's display change being what the import pictures for its states (#2485); the
/// elevator shaft's escape setup, which also seals the shaft's door, is its own (#2494). An offset the map does not know
/// is refused.
/// </summary>
internal static class RoomBehaviorRoutines
{
    /// <summary>The room-main callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomMainBehavior> MainCallbacks = new()
    {
        [0x0000] = RoomMainBehavior.None,
        [0xc116] = RoomMainBehavior.ScrollingSkyLand,
        [0xc11b] = RoomMainBehavior.ScrollingSkyOcean,
        [0xc120] = RoomMainBehavior.ScrollingSkyLandWithEscapeExplosions,
        [0xc124] = RoomMainBehavior.ShakeScreenWithRandomExplosions,
        [0xc1e6] = RoomMainBehavior.DachoraShaftCamera,
        [0xe2b6] = RoomMainBehavior.MaridiaElevatube,
        [0xe51f] = RoomMainBehavior.CeresElevatorShaft,
        [0xe524] = RoomMainBehavior.None,
        [0xe525] = RoomMainBehavior.SpawnCeresPreElevatorHallFallingDebris,
        [0xe571] = RoomMainBehavior.CeresRidleyGetaway,
        [0xe57c] = RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal,
        [0xe5a0] = RoomMainBehavior.RandomExplosionEveryFourthUpdate,
        [0xe5a4] = RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal,
        [0xe8cd] = RoomMainBehavior.CrocomireRoomShaking,
        [0xe950] = RoomMainBehavior.None,
    };

    /// <summary>The room-setup callback each bank-$8F code offset names.</summary>
    private static readonly Dictionary<ushort, RoomSetupBehavior> SetupCallbacks = new()
    {
        [0x0000] = RoomSetupBehavior.None,
        [0x9194] = RoomSetupBehavior.OpenRescuedAnimalsPassageAndQuake,
        [0x91a9] = RoomSetupBehavior.AutoDestroyWallDuringEscape,
        [0x91b2] = RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape,
        [0x91bb] = RoomSetupBehavior.None,
        [0x91bc] = RoomSetupBehavior.None,
        [0x91bd] = RoomSetupBehavior.LandSkyWithLandingSiteQuake,
        [0x91c9] = RoomSetupBehavior.SpawnScrollingSkyBands,
        [0x91ce] = RoomSetupBehavior.SpawnScrollingSkyBands,
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
        [0xc8c8] = RoomSetupBehavior.HoldPrePhantoonProjectilePlace,
        [0xc8d0] = RoomSetupBehavior.None,
        [0xc8d1] = RoomSetupBehavior.None,
        [0xc8d2] = RoomSetupBehavior.None,
        [0xc8d3] = RoomSetupBehavior.SpawnShaktoolController,
        [0xc8dc] = RoomSetupBehavior.None,
        [0xc8dd] = RoomSetupBehavior.None,
        [0xc90a] = RoomSetupBehavior.SetCollectedMap,
        [0xc91e] = RoomSetupBehavior.None,
        [0xc91f] = RoomSetupBehavior.SetZebesTimebombEventAndLightHorizontalShaking,
        [0xc933] = RoomSetupBehavior.SetLightHorizontalRoomShaking,
        [0xc946] = RoomSetupBehavior.SetMediumHorizontalRoomShaking,
        [0xc953] = RoomSetupBehavior.MediumHorizontalShakingWithRisingAcid,
        [0xc96e] = RoomSetupBehavior.SealCeresElevatorDoorAndSpawnHaze,
        [0xc976] = RoomSetupBehavior.SpawnCeresHaze,
        [0xc97b] = RoomSetupBehavior.SpawnCeresHaze,
    };

    /// <summary>The setup offsets whose routine acts only on the room's display, which decode to no behaviour.</summary>
    private static readonly HashSet<ushort> DisplayOnlySetups = [0xc8dd];

    /// <summary>
    /// The main offsets whose routine does not return at once but which decode to no behaviour because the port does not
    /// run it (#2287): Ridley's room's, which only offsets the display's layers while his body's shake counter runs.
    /// </summary>
    private static readonly HashSet<ushort> UnrunMains = [0xe950];

    /// <summary>Every nonzero bank-$8F room-main code offset the import knows, each naming a routine.</summary>
    internal static IReadOnlyDictionary<ushort, RoomMainBehavior> MainRoutines =>
        MainCallbacks.Where(pair => pair.Key != 0).ToDictionary();

    /// <summary>Every nonzero bank-$8F room-setup code offset the import knows, each naming a routine.</summary>
    internal static IReadOnlyDictionary<ushort, RoomSetupBehavior> SetupRoutines =>
        SetupCallbacks.Where(pair => pair.Key != 0).ToDictionary();

    /// <summary>
    /// The bank-$8F room-main code offsets that decode to <see cref="RoomMainBehavior.None"/> although their routine does
    /// not return at once: the main the port does not run (#2287).
    /// </summary>
    internal static IReadOnlySet<ushort> UnrunMainRoutines => UnrunMains;

    /// <summary>
    /// The bank-$8F room-setup code offsets that decode to <see cref="RoomSetupBehavior.None"/> although their routine
    /// does not return at once: Draygon's room's, which only has the room restore its display effects when the game
    /// unpauses, and the port draws the room without them (#1827).
    /// </summary>
    internal static IReadOnlySet<ushort> DisplayOnlySetupRoutines => DisplayOnlySetups;

    /// <summary>Resolves a bank-$8F room-main pointer read from the cartridge to its typed callback.</summary>
    internal static RoomMainBehavior ResolveMain(ushort pointer) =>
        MainCallbacks.TryGetValue(pointer, out RoomMainBehavior? callback)
            ? callback
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a retail room-main callback.");

    /// <summary>Resolves a bank-$8F room-setup pointer read from the cartridge to its typed callback.</summary>
    internal static RoomSetupBehavior ResolveSetup(ushort pointer) =>
        SetupCallbacks.TryGetValue(pointer, out RoomSetupBehavior? callback)
            ? callback
            : throw new ArgumentOutOfRangeException(nameof(pointer), pointer, "Pointer is not a retail room-setup callback.");
}
