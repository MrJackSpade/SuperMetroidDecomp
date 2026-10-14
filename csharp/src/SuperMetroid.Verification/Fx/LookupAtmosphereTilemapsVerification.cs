using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks calculated rain and fog tilemap attributes against native pages and editable fields.</summary>
internal static class LookupAtmosphereTilemapsVerification
{
    /// <summary>Compares rain palette bits with the original atmosphere page.</summary>
    private static void VerifyRainTilePalette(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifyAtmosphereTileField), () => VerifyAtmosphereTileField(rom, stock, RoomFxType.Rain, 0x1c00));
    /// <summary>Compares fog palette bits with the original atmosphere page.</summary>
    private static void VerifyFogTilePalette(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifyAtmosphereTileField), () => VerifyAtmosphereTileField(rom, stock, RoomFxType.Fog, 0x1c00));
    /// <summary>Compares fog priority bits with the original atmosphere page.</summary>
    private static void VerifyFogTilePriority(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifyAtmosphereTileField), () => VerifyAtmosphereTileField(rom, stock, RoomFxType.Fog, 0x2000));

    /// <summary>Checks every page cell's selected attribute mask against both calculation and installed output.</summary>
    private static void VerifyAtmosphereTileField(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock,
        RoomFxType type, int mask)
    {
        int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type);
        ReadOnlySpan<byte> installed = stock.Resolve(type).Span;
        AssertEqual(0x840, installed.Length, "Complete original atmosphere page");
        for (int index = 0; index < 1056; index++)
        {
            int expected = ReadVerificationWord(rom, source + 2 * index) & mask;
            AssertEqual(expected, RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index) & mask,
                "Original calculated atmosphere field");
            AssertEqual(expected, BinaryPrimitives.ReadUInt16LittleEndian(installed[(2 * index)..]) & mask,
                "Installed atmosphere field view");
        }
    }

    /// <summary>Checks rain/fog domain, uncached calculated fields, independent edits, and exact page bounds.</summary>
    private static void VerifyAtmosphereTilemapDomainAndEdits(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var type = (RoomFxType)value;
            if (type is RoomFxType.Rain or RoomFxType.Fog) continue;
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, 0),
                "Only rain/fog have these field rules");
        }
        foreach (RoomFxType type in new[] { RoomFxType.Rain, RoomFxType.Fog })
        {
            int mask = type == RoomFxType.Rain ? 0x1c00 : 0x3c00;
            AssertEqual(mask, (int)RoomFxAtmosphereTilemapDefinitions.CalculatedMask(type), "Exact converted field mask");
            RoomFxAtmosphereTilemap page = type == RoomFxType.Rain ? stock.rain : stock.fog;
            var retained = page.preservedBits;
            AssertTrue(page.customFields is null && retained.All(word => (word & mask) == 0),
                "Calculated stock fields absent from both stores");
            _ = stock.Resolve(type);
            AssertTrue(((RoomFxAtmosphereTilemap)page).customFields is null && retained.All(word => (word & mask) == 0),
                "Transfer never caches stock fields");
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type);
            var original = new byte[0x840];
            for (int offset = 0; offset < original.Length; offset++) original[offset] = AtmosphereTilemapDomainAndEditsRomByte(rom, source + offset);
            for (int index = 0; index < 1056; index++)
            {
                byte[] changed = original.ToArray();
                changed[index * 2 + 1] ^= (byte)(type == RoomFxType.Fog && (index & 1) != 0 ? 32 : 4);
                var custom = new RoomFxAtmosphereTilemap(type, changed);
                byte[] transfer = custom.CreateTransfer();
                AssertTrue(changed.SequenceEqual(transfer), "A custom field edit at every cell survives");
                Array.Clear(changed);
                AssertTrue(transfer.SequenceEqual(custom.CreateTransfer()), "Custom input ownership is independent");
            }
            foreach (int invalid in new[] { int.MinValue, -1, 1056, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, invalid),
                    "Exact33-row cell bounds");
            foreach (int length in new[] { 0, 1, 0x83f, 0x841 })
                AssertThrows<ArgumentException>(() => new RoomFxAtmosphereTilemap(type, new byte[length]),
                    "Incomplete atmosphere pages reject");
        }
    }

    /// <summary>Reads a native atmosphere page byte for editable-field comparisons.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for atmosphere tilemap domain and edits")]
    private static byte AtmosphereTilemapDomainAndEditsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs rain and fog palette, priority, and edit ownership checks.</summary>
    internal static void VerifyAtmosphereTilemapFields(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        Suite(nameof(VerifyRainTilePalette), () => VerifyRainTilePalette(rom, stock));
        Suite(nameof(VerifyFogTilePalette), () => VerifyFogTilePalette(rom, stock));
        Suite(nameof(VerifyFogTilePriority), () => VerifyFogTilePriority(rom, stock));
        Suite(nameof(VerifyAtmosphereTilemapDomainAndEdits), () => VerifyAtmosphereTilemapDomainAndEdits(rom, stock));
    }
}
