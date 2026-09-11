using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBotwoonPlmIdentity()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CartridgeRoomHeader.Load(bus, RoomHeaderPointers.Botwoon);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var plms = new RoomPlmSystem();
        for (int i = 0; i < 40; i++)
        {
            ushort header = (i & 1) == 0 ? RoomPlmHeaders.ClearBotwoonWall : RoomPlmHeaders.CrumbleBotwoonWall;
            AssertTrue(plms.TrySpawnBotwoonWall(assets.LevelData, header), "Botwoon native slot allocation succeeds until full");
            var slot = plms.PopulationSlots.Single(slot => slot.NativeSlotIndex == 39 - i);
            AssertEqual(header, slot.HeaderPointer, "hardcoded Botwoon header identity survives allocation");
            AssertEqual(assets.LevelData.GetBlockIndex(15, 4), slot.BlockIndex, "hardcoded Botwoon placement");
            AssertEqual((ushort)((i & 1) == 0 ? 1 : 64), slot.InstructionTimer, "header-specific Botwoon delay");
            AssertEqual((i & 1) == 0 ? RoomPlmInstructionLists.ClearBotwoonWall : RoomPlmInstructionLists.CrumbleBotwoonWall,
                slot.InstructionPointer, "header-specific Botwoon program");
        }
        var before = plms.PopulationSlots.ToArray();
        AssertTrue(!plms.TrySpawnBotwoonWall(assets.LevelData, RoomPlmHeaders.ClearBotwoonWall), "full native PLM pool declines spawn");
        AssertTrue(before.SequenceEqual(plms.PopulationSlots), "failed spawn preserves every owner and timer");
        plms.Reset();
        AssertTrue(!plms.HasActiveHeader(RoomPlmHeaders.ClearBotwoonWall) && !plms.HasActiveHeader(RoomPlmHeaders.CrumbleBotwoonWall),
            "room reset retires both Botwoon header identities");
        Console.WriteLine("Botwoon hardcoded PLMs: both header identities, slot order, placement, programs, timers and full-pool/reset behavior pass.");
    }
}
