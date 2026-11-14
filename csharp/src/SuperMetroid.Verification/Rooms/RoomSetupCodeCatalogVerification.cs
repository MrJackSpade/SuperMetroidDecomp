using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Rooms.RoomMainCodeCatalogVerification;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Audits bank-$8F room setup callback identities against reachable retail room states.</summary>
internal static class RoomSetupCodeCatalogVerification
{
    /// <summary>Runs the room-setup callback catalog check on its own.</summary>
    [VerificationCommand("--room-setup-code-catalog")]
    internal static int RoomSetupCodeCatalogCommand()
    {
        VerifyRoomSetupCodeCatalog();
        return 0;
    }

    /// <summary>
    /// Audits every state reachable from the retail room headers and proves every nonzero setup callback the cartridge
    /// stores belongs to the import's bank-$8F catalog, every catalogued routine occurs, each acting behaviour has one
    /// routine, and exactly the routines that return at once decode to <see cref="RoomSetupBehavior.None"/> (#1787).
    /// </summary>
    internal static void VerifyRoomSetupCodeCatalog()
    {
        IReadOnlyDictionary<ushort, RoomSetupBehavior> routines = RoomBehaviorDefinitions.SetupRoutines;
        ushort[] catalog = [.. routines.Keys];
        foreach (RoomSetupBehavior acting in Enum.GetValues<RoomSetupBehavior>().Where(callback => callback != RoomSetupBehavior.None))
        {
            AssertEqual(1, routines.Values.Count(callback => callback == acting),
                $"room-setup behaviour {acting} is one routine");
        }

        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  Room setup: catalog uniqueness passes; retail-state audit skipped (private inputs absent).");
            return;
        }

        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer.")))
            .Distinct()
            .ToArray();
        var statePointers = new HashSet<ushort>();
        var retailSetupPointers = new HashSet<ushort>();
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
            {
                RoomStateSettings state = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer, context).State;
                if (!statePointers.Add(state.Pointer))
                    continue;
                ushort setupPointer = SuperMetroid.AssetExtraction.Rooms.CartridgeRoomStateImporter.SetupCodePointer(bus, state.Pointer);
                if (setupPointer != 0)
                    retailSetupPointers.Add(setupPointer);
            }
        }

        foreach ((ushort pointer, RoomSetupBehavior callback) in routines)
        {
            AssertEqual(callback == RoomSetupBehavior.None,
                SuperMetroid.AssetExtraction.Rooms.CartridgeRoomStateImporter.RoutineReturnsAtOnce(bus, pointer),
                $"room-setup routine $8F:{pointer:X4} decodes to no behaviour exactly when it returns at once");
        }

        foreach (ushort pointer in retailSetupPointers)
        {
            AssertTrue(catalog.Contains(pointer),
                $"retail room-setup callback $8F:{pointer:X4} is catalogued");
        }
        foreach (ushort pointer in catalog)
        {
            AssertTrue(retailSetupPointers.Contains(pointer),
                $"catalogued room-setup callback $8F:{pointer:X4} occurs in a retail state");
        }
        AssertTrue(RoomBehaviorDefinitions.CeresHazeSetups.Contains(
                RoomSetupBehavior.TurnCeresDoorToSolidBlocksAndSpawnHaze),
            "Ceres solid-door setup declares haze side effect");
        AssertTrue(!RoomBehaviorDefinitions.CeresHazeSetups.Contains(
                RoomSetupBehavior.SetMediumHorizontalRoomShaking),
            "unrelated setup does not acquire Ceres haze");

        Console.WriteLine(
            $"  Room setup: all {retailSetupPointers.Count} nonzero retail callback kinds are catalogued.");
    }
}
