using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Video;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

/// <summary>Checks escape typewriter timing and Mother Brain room door mutation during death handoff.</summary>
internal static class MotherBrainDeathHandoffVerification
{
    /// <summary>Checks visible glyph delays and clicks, then verifies the four-block escape door extension.</summary>
    internal static void VerifyMotherBrainDeathHandoff()
    {
        var vram = new SnesVram();
        // The installed program keeps the same glyphs and destination; delay two is engine-owned.
        var program = new EscapeTypewriterProgram(EscapeTypewriterProgramId.Zebes,
            [new EscapeTypewriterLine(0, "A !")]);
        var text = new EscapeTypewriterState(program);
        AssertTrue(!text.StepInto(vram), "typewriter first glyph is not completion");
        AssertEqual(EscapeTypewriterTileDefinitions.ZebesTileBase.PackedWord, vram.ReadWord(0), "text glyph reaches its cell");
        AssertTrue(!text.ClickRequested, "first glyph does not click");
        text.StepInto(vram);
        AssertEqual(2, text.Cell, "space advances cursor without a tile");
        text.StepInto(vram);
        text.StepInto(vram);
        AssertEqual(1, text.GlyphsWritten, "delay applies before next visible glyph");
        text.StepInto(vram);
        AssertEqual((ushort)(EscapeTypewriterTileDefinitions.ZebesTileBase.PackedWord + 26), vram.ReadWord(2), "exclamation uses native remap");
        AssertTrue(text.ClickRequested, "second visible glyph clicks despite intervening space");
        text.StepInto(vram); text.StepInto(vram);
        AssertTrue(text.StepInto(vram), "typewriter completes only at terminator after delay");

        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), 256).ToArray();
        var level = CreateRoom(new(16, 16), terrain, new RoomBlockBehavior[terrain.Length]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.TrySpawnMotherBrainMutation(level, new(0, 6), PlmHeaderId.MotherBrainsRoomEscapeDoor), "escape PLM allocates");
        AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.DoorBlock), level.GetCollisionBlock(new(0, 6)).Terrain, "door setup preserves the block's appearance");
        AssertEqual(1, level.GetCollisionBlock(new(0, 6)).Behavior.Value, "door setup selects second door header");
        for (int row = 7; row < 10; row++)
        {
            AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.VerticalExtension), level.GetCollisionBlock(new(0, checked((ushort)row))).Terrain, "extension preserves the block's appearance");
            AssertEqual(byte.MaxValue, level.GetCollisionBlock(new(0, checked((ushort)row))).Behavior.Value, "extension links one block upward");
        }
        AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), level.GetCollisionBlock(new(0, 10)).Terrain, "door setup does not overrun four blocks");
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
