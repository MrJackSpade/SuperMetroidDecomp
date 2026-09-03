using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Audits every state reachable from the retail room headers and proves every nonzero
    /// setup callback is represented exactly once by the dedicated bank-$8F catalog.
    /// </summary>
    static void VerifyRoomSetupCodeCatalog()
    {
        ushort[] catalog = typeof(RoomSetupCodePointers)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(ushort))
            .Select(field => (ushort)field.GetRawConstantValue()!)
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

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .ToArray();
        var statePointers = new HashSet<ushort>();
        var retailSetupPointers = new HashSet<ushort>();
        foreach (ushort roomPointer in roomPointers)
        {
            foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
            {
                CartridgeRoomState state = CartridgeRoomHeader.Load(bus, roomPointer, context).State;
                if (!statePointers.Add(state.Pointer))
                    continue;
                if (state.SetupCodePointer != 0)
                    retailSetupPointers.Add(state.SetupCodePointer);
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
        AssertTrue(RoomSetupCodePointers.SpawnsCeresHaze(
                RoomSetupCodePointers.TurnCeresDoorToSolidBlocksAndSpawnHaze),
            "Ceres solid-door setup declares haze side effect");
        AssertTrue(!RoomSetupCodePointers.SpawnsCeresHaze(
                RoomSetupCodePointers.SetMediumHorizontalRoomShaking),
            "unrelated setup does not acquire Ceres haze");

        Console.WriteLine(
            $"  Room setup: all {retailSetupPointers.Count} nonzero retail callback kinds are catalogued.");
    }
}
