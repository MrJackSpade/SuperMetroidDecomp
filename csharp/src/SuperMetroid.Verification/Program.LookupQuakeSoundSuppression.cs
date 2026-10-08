using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyQuakeSoundSuppression(SuperMetroidAddressSpace rom)
    {
        var nativeRooms = new HashSet<ushort>();
        for (int instruction = 0x8882cd; instruction <= 0x8882e6; instruction += 5)
        {
            AssertEqual((byte)0xc9, rom.ReadByte(instruction), "Native CMP immediate");
            AssertEqual((byte)0xf0, rom.ReadByte(instruction + 3), "Native BEQ suppression");
            int branchTarget = instruction + 5 + unchecked((sbyte)rom.ReadByte(instruction + 4));
            AssertEqual(0x8882ed, branchTarget, "All native room matches share suppression branch");
            nativeRooms.Add(ReadVerificationWord(rom, instruction + 1));
        }
        AssertEqual(6, nativeRooms.Count, "Six distinct native suppressed rooms");
        for (int room = 0; room <= ushort.MaxValue; room++)
            AssertEqual(nativeRooms.Contains((ushort)room),
                RoomFxRomData.Earthquake.SoundSuppressedRooms.Contains((ushort)room),
                "Quake suppression complete room identity domain");

        var state = new RoomLayer3FxState();
        var bus = new TestAddressSpace();
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        foreach (ushort room in nativeRooms.Append((ushort)0))
        {
            state.Load(bus, vram, cgram, fxPointer: 0, doorPointer: 0, randomNumber: 0,
                roomHeaderPointer: room);
            state.PublishStatueEarthquakeSound(0, mainGameLoopCarry: true);
            AssertEqual(nativeRooms.Contains(room) ? 0 : 1, state.SoundRequests.Count,
                "Actual room initialization and quake emission honor native suppression");
            state.Load(bus, vram, cgram, fxPointer: 0, doorPointer: 0, randomNumber: 0,
                roomHeaderPointer: 0);
            state.PublishStatueEarthquakeSound(0, mainGameLoopCarry: true);
            AssertEqual(1, state.SoundRequests.Count, "Unsuppressed room resets previous sentinel");
        }
    }
}