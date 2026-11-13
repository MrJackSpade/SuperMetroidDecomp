using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
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
        var program = new EscapeTypewriterMessage(EscapeTypewriterMessageId.Zebes,
            [new EscapeTypewriterLine(default, "A !")]);
        var text = new EscapeTypewriterState(program);
        AssertTrue(!text.StepInto(vram), "typewriter first glyph is not completion");
        AssertEqual(EscapeTypewriterTileDefinitions.ZebesTileBase.PackedWord, vram.ReadWord(0), "text glyph reaches its cell");
        AssertTrue(!text.ClickRequested, "first glyph does not click");
        text.StepInto(vram);
        AssertEqual(new Position16<PlanePixels>(2 * EscapeTypewriterDefinitions.GlyphAdvance, 0), text.Corner, "space advances cursor a glyph's width without a tile");
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
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.BossRoomMutations.MotherBrainRoom.TrySpawn(level, new(new(0, 6), MotherBrainRoomMutation.EscapeDoor)), "escape PLM allocates");
        AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.DoorBlock), level.GetCollisionBlock(new(0, 6)).Terrain, "door setup preserves the block's appearance");
        AssertEqual(1, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(0, 6)).Behavior), "door setup selects second door header");
        for (int row = 7; row < 10; row++)
        {
            AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.VerticalExtension), level.GetCollisionBlock(new(0, checked((ushort)row))).Terrain, "extension preserves the block's appearance");
            AssertEqual(byte.MaxValue, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(0, checked((ushort)row))).Behavior), "extension links one block upward");
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
