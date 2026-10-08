using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly EnemyExtendedFrameDefinition[] OriginalMotherBrainFrames =
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

    private static void VerifyMotherBrainGeneratedOamCatalog(SuperMetroidAddressSpace rom)
    {
        AssertEqual(17, MotherBrainBodyVisualDefinitions.Frames.Length, "Mother Brain OAM count");
        for (int i = 0; i < OriginalMotherBrainFrames.Length; i++)
        {
            var expected = OriginalMotherBrainFrames[i];
            AssertEqual(expected, MotherBrainBodyVisualDefinitions.Frame(i), "Mother Brain published OAM identity/name");
            ushort count = (ushort)(rom.ReadByte(0xa90000 | expected.Pointer) | rom.ReadByte(0xa90000 | (expected.Pointer + 1)) << 8);
            int expectedCount = i is >= 1 and <= 3 ? 10 : i == 9 ? 7 : i == 12 ? 1 : 9;
            AssertEqual((ushort)expectedCount, count, "Mother Brain native frame stride geometry");
        }
        AssertTrue(OriginalMotherBrainFrames.SequenceEqual(MotherBrainBodyVisualDefinitions.Frames.ToArray()), "Mother Brain OAM materialization");
        int ordinal = 0;
        foreach (var frame in MotherBrainBodyVisualDefinitions.Frames)
            AssertEqual(OriginalMotherBrainFrames[ordinal++], frame, "Mother Brain OAM iteration");
        AssertEqual(17, ordinal, "Mother Brain OAM iteration count");
        AssertThrows<IndexOutOfRangeException>(() => MotherBrainBodyVisualDefinitions.Frame(-1), "negative Mother Brain OAM index");
        AssertThrows<IndexOutOfRangeException>(() => MotherBrainBodyVisualDefinitions.Frame(17), "Mother Brain OAM past end");
    }

    private static void VerifyMotherBrainGeneratedBg2Catalog(SuperMetroidAddressSpace rom)
    {
        EnemyBg2FrameDefinition[] expected = OriginalMotherBrainFrames.Where(frame => frame.Pointer != 0xa320)
            .Select(frame => new EnemyBg2FrameDefinition(frame.Pointer,
                frame.Name.Replace("_oam_", "_bg2_", StringComparison.Ordinal))).ToArray();
        AssertEqual(16, MotherBrainBodyVisualDefinitions.Bg2Frames.Length, "Mother Brain BG2 count");
        for (int i = 0; i < expected.Length; i++)
            AssertEqual(expected[i], MotherBrainBodyVisualDefinitions.Bg2Frame(i), "Mother Brain published BG2 identity/name");
        AssertTrue(expected.SequenceEqual(MotherBrainBodyVisualDefinitions.Bg2Frames.ToArray()), "Mother Brain BG2 materialization");
        var pointers = expected.Select(frame => frame.Pointer).ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(pointers.Contains((ushort)pointer), MotherBrainBodyVisualDefinitions.HasBg2((ushort)pointer), "Mother Brain exact BG2 domain including dummy exclusion");
        AssertThrows<IndexOutOfRangeException>(() => MotherBrainBodyVisualDefinitions.Bg2Frame(-1), "negative Mother Brain BG2 index");
        AssertThrows<IndexOutOfRangeException>(() => MotherBrainBodyVisualDefinitions.Bg2Frame(16), "Mother Brain BG2 past end");
        byte[] nativeJson = EnemyBg2FrameFiles.Extract(rom, 0xa9, new EnemyBg2FrameDefinitionSequence(expected.Length, index => expected[index]), 1, 10, "Mother Brain body", allowMixedOam: true);
        byte[] generatedJson = MotherBrainBodyBg2FrameFiles.Extract(rom);
        AssertTrue(nativeJson.SequenceEqual(generatedJson), "Mother Brain exact extraction JSON");
        using var stream = new MemoryStream(generatedJson, writable: false);
        var loaded = MotherBrainBodyBg2FrameCatalog.Load(stream);
        foreach (var frame in expected)
            AssertTrue(loaded.TryGet(frame.Pointer, out var writes) && writes.Length > 0, "Mother Brain generated catalog loads all frames");
        AssertTrue(!loaded.TryGet(0xa320, out _), "Mother Brain dummy has no BG2 catalog entry");
    }
}