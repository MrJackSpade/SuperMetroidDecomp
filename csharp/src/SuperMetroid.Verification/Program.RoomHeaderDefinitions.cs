using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCompiledRoomHeaderDefinitions()
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
        AssertEqual(RoomHeaderDefinitions.RetailRoomCount, roomPointers.Length,
            "compiled fixed-header catalog contains every retail room");

        foreach (ushort roomPointer in roomPointers)
        {
            CartridgeRoomHeader native = CartridgeRoomHeader.Load(bus, roomPointer);
            RoomHeaderDefinition compiled = RoomHeaderDefinitions.Get(roomPointer);
            AssertEqual(native.Pointer, compiled.Pointer,
                $"room $8F:{roomPointer:X4} header pointer");
            AssertEqual(native.RoomIndex, compiled.RoomIndex,
                $"room $8F:{roomPointer:X4} room index");
            AssertEqual(native.AreaIndex, compiled.AreaIndex,
                $"room $8F:{roomPointer:X4} area");
            AssertEqual(native.MapX, compiled.MapX,
                $"room $8F:{roomPointer:X4} map X");
            AssertEqual(native.MapY, compiled.MapY,
                $"room $8F:{roomPointer:X4} map Y");
            AssertEqual(native.WidthInScreens, compiled.WidthInScreens,
                $"room $8F:{roomPointer:X4} width");
            AssertEqual(native.HeightInScreens, compiled.HeightInScreens,
                $"room $8F:{roomPointer:X4} height");
            AssertEqual(native.UpScroller, compiled.UpScroller,
                $"room $8F:{roomPointer:X4} up scroller");
            AssertEqual(native.DownScroller, compiled.DownScroller,
                $"room $8F:{roomPointer:X4} down scroller");
            AssertEqual(native.CreBitset, compiled.CreBitset,
                $"room $8F:{roomPointer:X4} CRE bitset");
            AssertEqual(native.DoorListPointer, compiled.DoorListPointer,
                $"room $8F:{roomPointer:X4} door-list pointer");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomHeaderDefinitions.Get(0xe82c),
            "compiled headers reject the developer area-seven room");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomHeaderDefinitions.Get(0xffff),
            "compiled headers reject arbitrary pointers");

        ushort ceresPointer = LoadStationDefinitions.Get(
            SuperMetroid.Core.Game.AreaId.Ceres, 0).RoomPointer;
        ushort ceresDefaultState = CartridgeRoomHeader.Load(bus, ceresPointer).State.Pointer;
        var guardedRuntime = new SuperMetroidRuntime(new RoomHeaderReadGuard(
            bus,
            RoomHeaderRomData.BankAddress | ceresPointer,
            RoomHeaderRomData.BankAddress | ceresDefaultState));
        guardedRuntime.InitializeStartingCeresRoom();
        AssertEqual(ceresPointer, guardedRuntime.ActiveRoom!.Pointer,
            "production Ceres entry uses its compiled fixed header");

        Console.WriteLine(
            "Room fixed headers: 262 typed records match every cartridge field; " +
            "production Ceres entry rejects fixed-header and selector reads.");
    }

    private sealed class RoomHeaderReadGuard(
        ISnesAddressSpace source,
        int blockedStart,
        int blockedEnd) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= blockedStart && address < blockedEnd)
            {
                throw new InvalidOperationException(
                    $"Production runtime read native fixed-header/selector data at ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
