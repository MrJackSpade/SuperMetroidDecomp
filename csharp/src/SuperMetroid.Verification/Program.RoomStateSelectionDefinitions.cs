using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledRoomStateSelectionDefinitions()
    {
        string symbolPath = Path.GetFullPath(
            Path.Combine("upstream-sm", "assets", "names.txt"));
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] roomPointers = File.ReadLines(symbolPath)
            .Select(TryParseRoomHeaderPointer)
            .Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value)
            .Distinct()
            .Order()
            .ToArray();
        AssertEqual(RoomStateSelectionDefinitions.RetailRoomCount, roomPointers.Length,
            "compiled room-state catalog contains every retail room");

        RoomStateSelectionContext[] contexts = BuildRoomStateAuditContexts();
        var statePointers = new HashSet<ushort>();
        int comparisons = 0;
        foreach (ushort roomPointer in roomPointers)
        foreach (RoomStateSelectionContext context in contexts)
        {
            CartridgeRoomHeader native = CartridgeRoomHeader.Load(bus, roomPointer, context);
            CartridgeRoomHeader compiled = CartridgeRoomHeader.LoadUsingCompiledSelection(
                bus, roomPointer, context);
            AssertEqual(native, compiled,
                $"compiled room-state selection for $8F:{roomPointer:X4}");
            statePointers.Add(compiled.State.Pointer);
            comparisons++;
        }

        AssertEqual(323, statePointers.Count, "compiled selectors reach all retail states");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomStateSelectionDefinitions.Select(0xe82c, default),
            "compiled selectors reject the developer area-seven room");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomStateSelectionDefinitions.Select(0xffff, default),
            "compiled selectors reject arbitrary pointers");

        ushort ceresPointer = LoadStationDefinitions.Get(
            SuperMetroid.Core.Game.AreaId.Ceres, 0).RoomPointer;
        ushort ceresDefaultState = CartridgeRoomHeader.Load(bus, ceresPointer).State.Pointer;
        var guardedRuntime = new SuperMetroidRuntime(new RoomSelectorReadGuard(
            bus,
            RoomHeaderRomData.BankAddress |
                unchecked((ushort)(ceresPointer + RoomHeaderRomData.FixedHeaderByteCount)),
            RoomHeaderRomData.BankAddress | ceresDefaultState));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertEqual(ceresDefaultState, guardedRuntime.ActiveRoom!.State.Pointer,
            "production Ceres entry chooses its compiled default state");

        Console.WriteLine(
            $"Room state selectors: {comparisons} context comparisons reach " +
            $"{statePointers.Count} states; production Ceres entry rejects selector-ROM reads.");
    }

    private sealed class RoomSelectorReadGuard(
        ISnesAddressSpace source,
        int blockedStart,
        int blockedEnd) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= blockedStart && address < blockedEnd)
            {
                throw new InvalidOperationException(
                    $"Production runtime read native room-selector data at ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
