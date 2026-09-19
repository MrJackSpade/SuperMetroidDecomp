using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledRoomStateDefinitions()
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

        var statePointers = new HashSet<ushort>();
        foreach (ushort roomPointer in roomPointers)
        foreach (RoomStateSelectionContext context in BuildRoomStateAuditContexts())
        {
            CartridgeRoomState native = CartridgeRoomHeader.Load(bus, roomPointer, context).State;
            CartridgeRoomState compiled = RoomStateDefinitions.Get(native.Pointer);
            AssertEqual(native, compiled,
                $"compiled room-state payload $8F:{native.Pointer:X4}");
            statePointers.Add(native.Pointer);
        }

        AssertEqual(RoomStateDefinitions.RetailStateCount, statePointers.Count,
            "compiled payload catalog contains every selected retail state");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomStateDefinitions.Get(0xffff),
            "compiled state payloads reject arbitrary pointers");

        ushort ceresPointer = LoadStationDefinitions.Get(
            SuperMetroid.Core.Game.AreaId.Ceres, 0).RoomPointer;
        ushort ceresStatePointer = CartridgeRoomHeader.Load(bus, ceresPointer).State.Pointer;
        var guardedRuntime = new SuperMetroidRuntime(new RoomStateReadGuard(
            bus,
            RoomHeaderRomData.BankAddress | ceresStatePointer,
            RoomHeaderRomData.BankAddress | ceresStatePointer + RoomHeaderRomData.StateByteCount));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertEqual(RoomStateDefinitions.Get(ceresStatePointer),
            guardedRuntime.ActiveRoom!.State,
            "production Ceres entry uses its compiled room-state payload");

        Console.WriteLine(
            "Room state payloads: all 323 typed records match cartridge data; " +
            "production Ceres entry rejects selected-state record reads.");
    }

    private sealed class RoomStateReadGuard(
        ISnesAddressSpace source,
        int blockedStart,
        int blockedEnd) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= blockedStart && address < blockedEnd)
            {
                throw new InvalidOperationException(
                    $"Production runtime read native room-state data at ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
