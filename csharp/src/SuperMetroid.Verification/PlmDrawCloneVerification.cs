using static SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Checks that both native PLM draw opcodes restore and publish the same block state.</summary>
internal static class PlmDrawCloneVerification
{
    /// <summary>Executes each draw opcode from a low-WRAM probe program and checks the restored word, visible update, one-frame yield, and subsequent delete.</summary>
    internal static void VerifyPlmDrawClone()
    {
        foreach (ushort instruction in new[] { RoomPlmInstruction.DrawPlmBlock,
            RoomPlmInstruction.DrawPlmBlockClone })
        {
            var bus = new TestAddressSpace();
            const ushort probePointer = 0x1100;
            var words = new ushort[16 * 16];
            const int block = 3 * 16 + 3;
            words[block] = 0xc321;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), new byte[1024 * 8]);
            var streamer = level.CreateBackgroundStreamer();
            var plms = new RoomPlmSystem();
            AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, block, 1), "spawn restore-word PLM");
            plms.SetSoleWorkRamProgramForVerification(bus, probePointer,
                instruction, (ushort)RoomPlmInstruction.Delete);
            plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(0xc321, level.GetCollisionBlockByIndex(block).LevelWord, "draw opcode restores slot level word");
            AssertEqual(1, plms.TilemapUpdates.Count, "draw opcode publishes visible tilemap update");
            AssertEqual(1, plms.SlotTable.ActiveCount, "draw opcode yields for one frame before following Delete");
            plms.Step(bus, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            AssertEqual(0, plms.SlotTable.ActiveCount, "draw opcode resumes immediately after its operand-free word");
        }
        Console.WriteLine("  Both native PLM draw entry points restore terrain, redraw, yield, and resume identically.");
    }

    /// <summary>Runs the focused PLM draw-opcode equivalence check from the verification command line.</summary>
    [VerificationCommand("--plm-draw-clone")]
    internal static int PlmDrawCloneCommand()
    {
        Suite(nameof(VerifyPlmDrawClone), () => VerifyPlmDrawClone());
        return 0;
    }
}
