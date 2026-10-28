using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks generated Mother Brain body OAM and BG2 frame catalogs.</summary>
internal static class MotherBrainGeneratedVisualCatalogsVerification
{
    /// <summary>Pinned bank A9 body frame addresses and names, including the initial dummy pose.</summary>
    private static readonly NativeEnemySpritemapDefinition[] OriginalMotherBrainFrames =
    [
        new(0xa9, 0x9fa0, "mother_brain_body_oam_standing"),
        new(0xa9, 0x9fea, "mother_brain_body_oam_walking_0"),
        new(0xa9, 0xa03c, "mother_brain_body_oam_walking_1"),
        new(0xa9, 0xa08e, "mother_brain_body_oam_walking_2"),
        new(0xa9, 0xa0e0, "mother_brain_body_oam_walking_3"),
        new(0xa9, 0xa12a, "mother_brain_body_oam_walking_4"),
        new(0xa9, 0xa174, "mother_brain_body_oam_walking_5"),
        new(0xa9, 0xa1be, "mother_brain_body_oam_walking_6"),
        new(0xa9, 0xa208, "mother_brain_body_oam_walking_7"),
        new(0xa9, 0xa252, "mother_brain_body_oam_crouched"),
        new(0xa9, 0xa28c, "mother_brain_body_oam_uncrouching"),
        new(0xa9, 0xa2d6, "mother_brain_body_oam_leaning_down"),
        new(0xa9, 0xa320, "mother_brain_body_oam_initial_dummy"),
        new(0xa9, 0xa384, "mother_brain_body_oam_death_beam_0"),
        new(0xa9, 0xa3ce, "mother_brain_body_oam_death_beam_1"),
        new(0xa9, 0xa418, "mother_brain_body_oam_death_beam_2"),
        new(0xa9, 0xa462, "mother_brain_body_oam_death_beam_3"),

    ];

    /// <summary>Checks 17 OAM identities, native part counts, iteration, and bounds.</summary>
    internal static void VerifyMotherBrainGeneratedOamCatalog(SuperMetroidAddressSpace rom)
    {
        AssertEqual(17, MotherBrainBodyVisualDefinitions.Frames.Length, "Mother Brain OAM count");
        for (int i = 0; i < OriginalMotherBrainFrames.Length; i++)
        {
            var expected = OriginalMotherBrainFrames[i];
            AssertEqual(expected, MotherBrainBodyVisualDefinitionsConstants.Frame(i), "Mother Brain published OAM identity/name");
            ushort count = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | expected.Pointer);
            int expectedCount = i is >= 1 and <= 3 ? 10 : i == 9 ? 7 : i == 12 ? 1 : 9;
            AssertEqual((ushort)expectedCount, count, "Mother Brain native frame stride geometry");
        }
        AssertTrue(OriginalMotherBrainFrames.SequenceEqual(MotherBrainBodyVisualDefinitions.Frames.ToArray()), "Mother Brain OAM materialization");
        int ordinal = 0;
        foreach (var frame in MotherBrainBodyVisualDefinitions.Frames)
            AssertEqual(OriginalMotherBrainFrames[ordinal++], frame, "Mother Brain OAM iteration");
        AssertEqual(17, ordinal, "Mother Brain OAM iteration count");
        AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainBodyVisualDefinitionsConstants.Frame(-1), "negative Mother Brain OAM index");
        AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainBodyVisualDefinitionsConstants.Frame(17), "Mother Brain OAM past end");
    }

    /// <summary>Checks 16 BG2 frames, excluding the dummy, and parity of generated extraction JSON.</summary>
    internal static void VerifyMotherBrainGeneratedBg2Catalog(SuperMetroidAddressSpace rom)
    {
        SpritemapFrameDefinition[] expected = OriginalMotherBrainFrames.Where(frame => frame.Pointer != 0xa320)
            .Select(frame => new SpritemapFrameDefinition(frame.Pointer,
                frame.Name.Replace("_oam_", "_bg2_", StringComparison.Ordinal))).ToArray();
        AssertEqual(16, MotherBrainBodyVisualDefinitions.Bg2Frames.Length, "Mother Brain BG2 count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], new SpritemapFrameDefinition(MotherBrainBodyVisualDefinitions.Bg2Frames[i].Pointer, MotherBrainBodyVisualDefinitions.Bg2Frames[i].Name),
                "Mother Brain published BG2 identity/name");
        var pointers = expected.Select(frame => frame.Pointer).ToHashSet();
        for (int pointer = 1; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(pointers.Contains((ushort)pointer),
                RoomEnemySlotFrameAccess.TryFrameAt(0xa9, (ushort)pointer) is { } frame && MotherBrainBodyVisualDefinitions.Bg2FrameIds.Contains(frame),
                "Mother Brain exact BG2 domain including dummy exclusion");
        byte[] generatedJson = MotherBrainBodyBg2FrameFiles.Extract(rom);
        using var stream = new MemoryStream(generatedJson, writable: false);
        var loaded = MotherBrainBodyBg2FrameCatalog.Load(stream);
        foreach (var frame in expected)
            AssertTrue(loaded.TryGet(frame.Pointer, out var writes) && writes.Length > 0, "Mother Brain generated catalog loads all frames");
        AssertTrue(!loaded.TryGet(0xa320, out _), "Mother Brain dummy has no BG2 catalog entry");
    }
}
