using System.Buffers.Binary;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;

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
        int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(type));
        ReadOnlySpan<byte> installed = stock.ResolveBytes(type);
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

    /// <summary>Checks rain/fog domain, the stock page, an edit at every cell, and exact page bounds.</summary>
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
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(type));
            var original = new byte[0x840];
            for (int offset = 0; offset < original.Length; offset++) original[offset] = AtmosphereTilemapDomainAndEditsRomByte(rom, source + offset);
            byte[] stockPage = stock.ResolveBytes(type);
            for (int index = 0; index < 1056; index++)
            {
                int word = BinaryPrimitives.ReadUInt16LittleEndian(original.AsSpan(index * 2));
                AssertEqual((word & ~mask) | RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index),
                    (int)BinaryPrimitives.ReadUInt16LittleEndian(stockPage.AsSpan(index * 2)), "The stock page keeps the cartridge's other fields under the calculated ones");
            }
            byte[] changed = stockPage.ToArray();
            for (int index = 0; index < 1056; index++)
                changed[index * 2 + 1] ^= (byte)(type == RoomFxType.Fog && (index & 1) != 0 ? 32 : 4);
            RoomFxLayer3TilemapCatalog edited = RoomFxLayer3TilemapFixtures.WithPage(stock, type, BackgroundTileWordBytes.Tiles(changed));
            AssertTrue(changed.AsSpan().SequenceEqual(edited.ResolveBytes(type)), "A custom field edit at every cell survives");
            AssertTrue(stockPage.AsSpan().SequenceEqual(stock.ResolveBytes(type)), "An edited catalog leaves the stock catalog's page");
            foreach (int invalid in new[] { int.MinValue, -1, 1056, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, invalid),
                    "Exact33-row cell bounds");
            foreach (int length in new[] { 0, 1, 1055, 1057 })
                AssertThrows<InvalidDataException>(() => RoomFxLayer3TilemapFixtures.WithPage(stock, type, new BackgroundTile[length]),
                    "Incomplete atmosphere pages reject");
        }
    }

    /// <summary>Reads a native atmosphere page byte for editable-field comparisons.</summary>
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
