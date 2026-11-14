using SuperMetroid.AssetExtraction.Progression;
using System.Globalization;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks bank-$8F room-main callbacks against every reachable retail room state.</summary>
internal static class RoomMainCodeCatalogVerification
{
    /// <summary>Runs the room-main callback catalog check on its own.</summary>
    [VerificationCommand("--room-main-code-catalog")]
    internal static int RoomMainCodeCatalogCommand()
    {
        VerifyRoomMainCodeCatalog();
        return 0;
    }

    /// <summary>
    /// Enumerates every state reachable from every named retail room header and proves every nonzero main callback the
    /// cartridge stores belongs to the import's bank-$8F catalog, every catalogued routine occurs, each acting
    /// behaviour has one routine, and exactly the routines that return at once decode to <see cref="RoomMainBehavior.None"/>
    /// (#1787).
    /// </summary>
    internal static void VerifyRoomMainCodeCatalog()
    {
        IReadOnlyDictionary<ushort, RoomMainBehavior> routines = RoomBehaviorDefinitions.MainRoutines;
        ushort[] catalog = [.. routines.Keys];
        foreach (RoomMainBehavior acting in Enum.GetValues<RoomMainBehavior>().Where(callback => callback != RoomMainBehavior.None))
        {
            AssertEqual(1, routines.Values.Count(callback => callback == acting),
                $"room-main behaviour {acting} is one routine");
        }

        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  Room main: catalog uniqueness passes; retail-state audit skipped (private inputs absent).");
            return;
        }

        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => (pointer ?? throw new InvalidOperationException("Verification requires pointer.")))
            .Distinct()
            .ToArray();
        RoomStateSelectionContext[] contexts = BuildRoomStateAuditContexts();
        var statePointers = new HashSet<ushort>();
        var retailMainPointers = new HashSet<ushort>();
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in contexts)
            {
                RoomStateSettings state = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, roomPointer, context).State;
                if (!statePointers.Add(state.Pointer))
                    continue;
                ushort mainPointer = SuperMetroid.AssetExtraction.Rooms.CartridgeRoomStateImporter.MainCodePointer(bus, state.Pointer);
                if (mainPointer != 0)
                    retailMainPointers.Add(mainPointer);
            }
        }

        foreach ((ushort pointer, RoomMainBehavior callback) in routines)
        {
            AssertEqual(callback == RoomMainBehavior.None,
                SuperMetroid.AssetExtraction.Rooms.CartridgeRoomStateImporter.RoutineReturnsAtOnce(bus, pointer),
                $"room-main routine $8F:{pointer:X4} decodes to no behaviour exactly when it returns at once");
        }

        foreach (ushort pointer in retailMainPointers)
        {
            AssertTrue(catalog.Contains(pointer),
                $"retail room-main callback $8F:{pointer:X4} is catalogued");
        }
        foreach (ushort pointer in catalog)
        {
            AssertTrue(retailMainPointers.Contains(pointer),
                $"catalogued room-main callback $8F:{pointer:X4} occurs in a retail state");
        }

        Console.WriteLine(
            $"  Room main: {roomPointers.Length} rooms expose {statePointers.Count} states and " +
            $"all {retailMainPointers.Count} nonzero callback kinds are catalogued.");
    }

    /// <summary>Extracts a retail room header pointer from a named symbol, excluding the debug room and the room no door reaches.</summary>
    internal static ushort? TryParseRoomHeaderPointer(string line)
    {
        string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 2 || !fields[0].StartsWith("0x8f", StringComparison.Ordinal) ||
            !fields[1].StartsWith("kRoom_", StringComparison.Ordinal) ||
            fields[1].Contains("_DoorOuts", StringComparison.Ordinal))
        {
            return null;
        }

        string suffix = fields[1]["kRoom_".Length..];
        if (!ushort.TryParse(
            suffix,
            NumberStyles.AllowHexSpecifier,
            CultureInfo.InvariantCulture,
            out ushort pointer))
        {
            return null;
        }

        // $E82C is the cartridge's developer debug room and deliberately uses area ID 7,
        // outside the seven retail save/map areas accepted by the production room loader.
        // $B3E1 is a Norfair room no door list names (#1686), so it is no room of the game.
        return pointer is 0xe82c or 0xb3e1 ? null : pointer;
    }

    /// <summary>Constructs boss, event, and item contexts that expose conditional room states.</summary>
    internal static RoomStateSelectionContext[] BuildRoomStateAuditContexts()
    {
        var contexts = new List<RoomStateSelectionContext>
        {
            default,
            new(null, DefeatedBosses.None, HasMorphBallAndMissiles: true, HasPowerBombs: false),
            new(null, DefeatedBosses.None, HasMorphBallAndMissiles: false, HasPowerBombs: true),
        };
        for (int bit = 0; bit < 8; bit++)
        {
            contexts.Add(new RoomStateSelectionContext(
                null,
                ProgressionTableCodec.StoredBosses(1 << bit),
                false,
                false));
        }
        foreach (StoryEvent storyEvent in Enum.GetValues<StoryEvent>())
        {
            ProgressSet<StoryEvent> events = new PersistentProgress().Events;
            events.Add(storyEvent);
            contexts.Add(new RoomStateSelectionContext(events, DefeatedBosses.None, false, false));
        }
        return contexts.ToArray();
    }
}
