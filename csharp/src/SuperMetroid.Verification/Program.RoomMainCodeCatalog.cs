using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Enumerates every state reachable from every named retail room header and proves
    /// every nonzero main callback belongs to the dedicated bank-$8F catalog.
    /// </summary>
    static void VerifyRoomMainCodeCatalog()
    {
        ushort[] catalog = typeof(RoomMainCodePointers)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
            .Select(field => (ushort)field.GetRawConstantValue()!)
            .ToArray();
        AssertEqual(catalog.Length, catalog.Distinct().Count(),
            "room-main callback catalog has no aliases");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        if (!File.Exists(romPath) || !File.Exists(symbolPath))
        {
            Console.WriteLine(
                "  Room main: catalog uniqueness passes; retail-state audit skipped (private inputs absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .ToArray();
        RoomStateSelectionContext[] contexts = BuildRoomStateAuditContexts();
        var statePointers = new HashSet<ushort>();
        var retailMainPointers = new HashSet<ushort>();
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in contexts)
            {
                CartridgeRoomState state = CartridgeRoomHeader.Load(bus, roomPointer, context).State;
                if (!statePointers.Add(state.Pointer))
                    continue;
                if (state.MainCodePointer != 0)
                    retailMainPointers.Add(state.MainCodePointer);
            }
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

    private static ushort? TryParseRoomHeaderPointer(string line)
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
        return pointer == 0xe82c ? null : pointer;
    }

    private static RoomStateSelectionContext[] BuildRoomStateAuditContexts()
    {
        var contexts = new List<RoomStateSelectionContext>
        {
            default,
            new(Array.Empty<byte>(), 0, HasMorphBallAndMissiles: true, HasPowerBombs: false),
            new(Array.Empty<byte>(), 0, HasMorphBallAndMissiles: false, HasPowerBombs: true),
        };
        for (int bit = 0; bit < 8; bit++)
        {
            contexts.Add(new RoomStateSelectionContext(
                Array.Empty<byte>(),
                unchecked((ushort)(1 << bit)),
                false,
                false));
        }
        for (int eventIndex = 0; eventIndex < Bank80SystemState.EventByteCount * 8; eventIndex++)
        {
            var events = new byte[Bank80SystemState.EventByteCount];
            events[eventIndex >> 3] = unchecked((byte)(1 << (eventIndex & 7)));
            contexts.Add(new RoomStateSelectionContext(events, 0, false, false));
        }
        return contexts.ToArray();
    }
}
