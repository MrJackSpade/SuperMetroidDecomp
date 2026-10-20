using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Rooms;

/// <summary>Checks generated Phantoon and Draygon BG2 frame catalogs against native stream roots.</summary>
internal static class GeneratedBossBg2CatalogsVerification
{
    /// <summary>Checks Phantoon identities, multi-component tentacles, and loaded BG2 streams.</summary>
    internal static void VerifyPhantoonGeneratedBg2Catalog(SuperMetroidAddressSpace rom)
    {
        EnemyBg2FrameDefinition[] expected =
        [
        new(0xdedd, "body_invulnerable"),
        new(0xdee7, "body_full_hitbox"),
        new(0xdef1, "body_eye_hitbox_only"),
        new(0xdefb, "eye_closed"),
        new(0xdf05, "eye_open_0"),
        new(0xdf0f, "eye_open_1"),
        new(0xdf19, "eye_open_2"),
        new(0xdf23, "eyeball_center"),
        new(0xdf2d, "eyeball_up"),
        new(0xdf37, "eyeball_down"),
        new(0xdf41, "eyeball_left"),
        new(0xdf4b, "eyeball_right"),
        new(0xdf55, "eyeball_down_left"),
        new(0xdf5f, "eyeball_down_right"),
        new(0xdf69, "eyeball_up_left"),
        new(0xdf73, "eyeball_up_right"),
        new(0xdfb3, "tentacles_0"),
        new(0xdfc5, "tentacles_1"),
        new(0xdfd7, "tentacles_2"),
        new(0xdfe9, "mouth_0"),
        new(0xdff3, "mouth_1"),
        new(0xdffd, "mouth_2"),

        ];
        byte[] json = VerifyGeneratedBg2Catalog(rom, 0xa7, "Phantoon", expected, PhantoonBg2FrameDefinitions.Frames, new Bg2CatalogAccess { IsFrame = PhantoonBg2FrameDefinitions.IsFrame, FrameAt = PhantoonBg2FrameDefinitions.Frame, Extract = () => PhantoonBg2FrameFiles.Extract(rom) });
        using var stream = new MemoryStream(json, writable: false);
        var loaded = PhantoonBg2FrameCatalog.Load(stream);
        foreach (var frame in expected)
            AssertTrue(loaded.TryGet(frame.Pointer, out var writes) && writes.Length > 0,
                "Phantoon generated catalog loads native stream");
    }
    /// <summary>Checks Draygon identities, selector coverage, and loaded BG2 streams.</summary>
    internal static void VerifyDraygonGeneratedBg2Catalog(SuperMetroidAddressSpace rom)
    {
        EnemyBg2FrameDefinition[] expected =
        [
        new(0xa31b, "draygon_bg2_A31B"),
        new(0xa325, "draygon_bg2_A325"),
        new(0xa32f, "draygon_bg2_A32F"),
        new(0xa339, "draygon_bg2_A339"),
        new(0xa343, "draygon_bg2_A343"),
        new(0xa34d, "draygon_bg2_A34D"),
        new(0xa357, "draygon_bg2_A357"),
        new(0xa361, "draygon_bg2_A361"),
        new(0xa36b, "draygon_bg2_A36B"),
        new(0xa375, "draygon_bg2_A375"),
        new(0xa37f, "draygon_bg2_A37F"),
        new(0xa389, "draygon_bg2_A389"),
        new(0xa393, "draygon_bg2_A393"),
        new(0xa39d, "draygon_bg2_A39D"),
        new(0xa3a7, "draygon_bg2_A3A7"),
        new(0xa3b1, "draygon_bg2_A3B1"),
        new(0xa3bb, "draygon_bg2_A3BB"),
        new(0xa643, "draygon_bg2_A643"),
        new(0xa64d, "draygon_bg2_A64D"),
        new(0xa657, "draygon_bg2_A657"),
        new(0xa661, "draygon_bg2_A661"),
        new(0xa66b, "draygon_bg2_A66B"),
        new(0xa675, "draygon_bg2_A675"),
        new(0xa67f, "draygon_bg2_A67F"),
        new(0xa689, "draygon_bg2_A689"),
        new(0xa693, "draygon_bg2_A693"),
        new(0xa69d, "draygon_bg2_A69D"),
        new(0xa6a7, "draygon_bg2_A6A7"),
        new(0xa6b1, "draygon_bg2_A6B1"),
        new(0xa6bb, "draygon_bg2_A6BB"),
        new(0xa6c5, "draygon_bg2_A6C5"),
        new(0xa6cf, "draygon_bg2_A6CF"),
        new(0xa6d9, "draygon_bg2_A6D9"),
        new(0xa6e3, "draygon_bg2_A6E3"),

        ];
        byte[] json = VerifyGeneratedBg2Catalog(rom, 0xa5, "Draygon", expected, DraygonBg2FrameDefinitions.Frames, new Bg2CatalogAccess { IsFrame = DraygonBg2FrameDefinitions.IsFrame, FrameAt = DraygonBg2FrameDefinitions.Frame, Extract = () => DraygonBg2FrameFiles.Extract(rom) });
        using var stream = new MemoryStream(json, writable: false);
        var loaded = DraygonBg2FrameCatalog.Load(stream);
        foreach (var frame in expected)
            AssertTrue(loaded.TryGet(frame.Pointer, out var writes) && writes.Length > 0,
                "Draygon generated catalog loads native stream");
    }
    /// <summary>Compares one family's exact frame domain, native stream markers, and extracted JSON bytes.</summary>
    private static byte[] VerifyGeneratedBg2Catalog(SuperMetroidAddressSpace rom, byte bank, string family, EnemyBg2FrameDefinition[] expected, EnemyBg2FrameDefinitionSequence frames, Bg2CatalogAccess access)
    {
        ushort Word(int address) => SnesLittleEndian.Word(GeneratedBg2CatalogRomByte(rom, address), GeneratedBg2CatalogRomByte(rom, address + 1));
        AssertEqual(expected.Length, frames.Length, $"{family} catalog count");
        for (int index = 0; index < expected.Length; index++)
        {
            AssertEqual(expected[index], access.FrameAt(index), $"{family} published identity/name");
            int root = ((int)new SnesAddress(bank, expected[index].Pointer));
            int components = family == "Phantoon" && index is >= 16 and < 19 ? 2 : 1;
            AssertEqual((ushort)components, Word(root), $"{family} native count determines stride");
            for (int component = 0; component < components; component++)
            {
                ushort target = Word(root + 6 + component * 8);
                AssertEqual((ushort)0xfffe, Word(((int)new SnesAddress(bank, target))), $"{family} native BG2 stream marker");
            }
        }
        AssertTrue(expected.SequenceEqual(frames.ToArray()), $"{family} generated enumeration");
        var originalPointers = expected.Select(frame => frame.Pointer).ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(originalPointers.Contains((ushort)pointer), access.IsFrame((ushort)pointer), $"{family} exact selector domain");
        AssertThrows<IndexOutOfRangeException>(() => access.FrameAt(-1), $"{family} negative definition index");
        AssertThrows<IndexOutOfRangeException>(() => access.FrameAt(expected.Length), $"{family} definition past end");
        byte[] originalJson = EnemyBg2FrameFiles.Extract(rom, bank, new EnemyBg2FrameDefinitionSequence(expected.Length, index => expected[index]), 1,
            family == "Phantoon" ? 2 : 1, family);
        byte[] generatedJson = access.Extract();
        AssertTrue(originalJson.SequenceEqual(generatedJson), $"{family} exact native extraction JSON");
        return generatedJson;
    }

    /// <summary>Reads native frame words while comparing generated BG2 catalogs.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for generated BG2 catalog")]
    private static byte GeneratedBg2CatalogRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
