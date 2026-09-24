using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyPlmDrawClone()
    {
        foreach (ushort instruction in new[] { RoomPlmInstructionCodes.DrawPlmBlock,
            RoomPlmInstructionCodes.DrawPlmBlockClone })
        {
            var bus = new TestAddressSpace();
            // The explicit probe cursor keeps this constructed opcode stream away
            // from immutable retail PLM programs, including future compiled families.
            const ushort probePointer = 0xf100;
            int program = 0x840000 | probePointer;
            WriteWord(bus, program, instruction);
            WriteWord(bus, program + 2, RoomPlmInstructionCodes.Delete);
            var words = new ushort[16 * 16];
            const int block = 3 * 16 + 3;
            words[block] = 0xc321;
            var level = new RoomLevelData(16, 16, words, new byte[words.Length],
                new ushort[words.Length], new byte[1024 * 8]);
            var streamer = level.CreateBackgroundStreamer();
            var plms = new RoomPlmSystem();
            AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, block, 1), "spawn restore-word PLM");
            plms.SetSoleInstructionPointerForVerification(probePointer);
            plms.Step(bus, level, streamer, 0, 0, 0);
            AssertEqual(0xc321, level.GetCollisionBlockByIndex(block).LevelWord, "draw opcode restores slot level word");
            AssertEqual(1, plms.TilemapUpdates.Count, "draw opcode publishes visible tilemap update");
            AssertEqual(1, plms.ActiveCount, "draw opcode yields for one frame before following Delete");
            plms.Step(bus, level, streamer, 0, 0, 0);
            AssertEqual(0, plms.ActiveCount, "draw opcode resumes immediately after its operand-free word");
        }
        Console.WriteLine("  Both native PLM draw entry points restore terrain, redraw, yield, and resume identically.");
    }
}
