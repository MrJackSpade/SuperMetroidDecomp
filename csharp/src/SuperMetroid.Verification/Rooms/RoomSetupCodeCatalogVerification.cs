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
    /// Audits every state reachable from the retail room headers and proves every nonzero
    /// setup callback is represented exactly once by the dedicated bank-$8F catalog.
    /// </summary>
    internal static void VerifyRoomSetupCodeCatalog()
    {
        ushort[] catalog = Enum.GetValues<RoomSetupCallback>()
            .Where(callback => callback != RoomSetupCallback.None)
            .Select(callback => RoomCallbackDefinitions.PointerOf(callback))
            .ToArray();
        AssertEqual(catalog.Length, catalog.Distinct().Count(),
            "room-setup callback catalog has no aliases");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  Room setup: catalog uniqueness passes; retail-state audit skipped (private inputs absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
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
                CartridgeRoomState state = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer, context).State;
                if (!statePointers.Add(state.Pointer))
                    continue;
                if (state.SetupCallback != RoomSetupCallback.None)
                    retailSetupPointers.Add(RoomCallbackDefinitions.PointerOf(state.SetupCallback));
            }
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
        AssertTrue(RoomCallbackDefinitions.SpawnsCeresHaze(
                RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze),
            "Ceres solid-door setup declares haze side effect");
        AssertTrue(!RoomCallbackDefinitions.SpawnsCeresHaze(
                RoomSetupCallback.SetMediumHorizontalRoomShaking),
            "unrelated setup does not acquire Ceres haze");

        Console.WriteLine(
            $"  Room setup: all {retailSetupPointers.Count} nonzero retail callback kinds are catalogued.");
    }
}
