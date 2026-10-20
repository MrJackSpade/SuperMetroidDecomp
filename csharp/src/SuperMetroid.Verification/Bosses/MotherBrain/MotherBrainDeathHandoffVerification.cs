using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Areas.Escape.Artwork;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks escape typewriter timing and Mother Brain room door mutation during death handoff.</summary>
internal static class MotherBrainDeathHandoffVerification
{
    /// <summary>Checks visible glyph delays and clicks, then verifies the four-block escape door extension.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyMotherBrainDeathHandoff()
    {
        var vram = new SnesVram();
        const int textAddress = 0xa68000;
        // The installed program keeps the same glyphs and destination; delay two is engine-owned.
        var program = new EscapeTypewriterProgram(EscapeTypewriterProgramId.Zebes, textAddress,
            [new EscapeTypewriterLine(0x4800, "A !")]);
        var text = new EscapeTypewriterState(program, EscapeTypewriterRomData.ZebesTileBase);
        AssertTrue(!text.StepInto(vram), "typewriter first glyph is not completion");
        AssertEqual(EscapeTypewriterRomData.ZebesTileBase, vram.ReadWord(0x4800), "text glyph reaches actual VRAM");
        AssertTrue(!text.ClickRequested, "first glyph does not click");
        text.StepInto(vram);
        AssertEqual(0x4802, text.Destination, "space advances cursor without a tile");
        text.StepInto(vram);
        text.StepInto(vram);
        AssertEqual(1, text.GlyphsWritten, "delay applies before next visible glyph");
        text.StepInto(vram);
        AssertEqual((ushort)(EscapeTypewriterRomData.ZebesTileBase + 26), vram.ReadWord(0x4802), "exclamation uses native remap");
        AssertTrue(text.ClickRequested, "second visible glyph clicks despite intervening space");
        text.StepInto(vram); text.StepInto(vram);
        AssertTrue(text.StepInto(vram), "typewriter completes only at terminator after delay");

        ushort[] foreground = Enumerable.Repeat((ushort)0x8123, 256).ToArray();
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(foreground, new byte[256], new ushort[256]), new byte[8192]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.TrySpawnMotherBrainMutation(level, new(0, 6), PlmHeaderId.MotherBrainsRoomEscapeDoor), "escape PLM allocates");
        AssertEqual((ushort)0x9123, level.GetCollisionBlock(new(0, 6)).PackedWord.Raw, "door setup preserves visual bits");
        AssertEqual(1, level.GetCollisionBlock(new(0, 6)).Bts.Value, "door setup selects second door header");
        for (int row = 7; row < 10; row++)
        {
            AssertEqual((ushort)0xd123, level.GetCollisionBlock(new(0, checked((ushort)row))).PackedWord.Raw, "extension preserves visual bits");
            AssertEqual(byte.MaxValue, level.GetCollisionBlock(new(0, checked((ushort)row))).Bts.Value, "extension links one block upward");
        }
        AssertEqual((ushort)0x8123, level.GetCollisionBlock(new(0, 10)).PackedWord.Raw, "door setup does not overrun four blocks");
        Console.WriteLine("  Mother Brain escape: typewriter glyph/delay/click semantics and native door setup agree.");
    }

    /// <summary>Runs the focused Mother Brain escape handoff check.</summary>
    [VerificationCommand("--mother-brain-death-handoff")]
    internal static int MotherBrainDeathHandoffCommand()
    {
        Suite(nameof(VerifyMotherBrainDeathHandoff), () => VerifyMotherBrainDeathHandoff());
        return 0;
    }
}
