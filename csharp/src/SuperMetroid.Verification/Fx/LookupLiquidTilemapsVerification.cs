using System.Buffers.Binary;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks calculated liquid layer-3 pages against retail words and uncached stock artwork.</summary>
internal static class LookupLiquidTilemapsVerification
{
    /// <summary>Compares the tile index field of every lava, acid, and water cell.</summary>
    private static void VerifyLiquidTileCharacters(ISnesAddressSpace rom, RoomFxLayer3Pages catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x03ff));
    /// <summary>Compares the palette selection field of every liquid cell.</summary>
    private static void VerifyLiquidTilePalettes(ISnesAddressSpace rom, RoomFxLayer3Pages catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x1c00));
    /// <summary>Compares the priority bit of every liquid cell.</summary>
    private static void VerifyLiquidTilePriority(ISnesAddressSpace rom, RoomFxLayer3Pages catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x2000));
    /// <summary>Compares the horizontal flip bit of every liquid cell.</summary>
    private static void VerifyLiquidTileHorizontalFlip(ISnesAddressSpace rom, RoomFxLayer3Pages catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x4000));
    /// <summary>Compares the vertical flip bit of every liquid cell.</summary>
    private static void VerifyLiquidTileVerticalFlip(ISnesAddressSpace rom, RoomFxLayer3Pages catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x8000));

    /// <summary>Checks one native tile-word field against calculated and installed transfers across all three liquids.</summary>
    private static void VerifyLiquidTileField(ISnesAddressSpace rom, RoomFxLayer3Pages catalog, int mask)
    {
        foreach (RoomFxType type in new[] { RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water })
        {
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(type));
            byte[] transfer = RoomFxLiquidTilemapDefinitions.ComposePage(type);
            ReadOnlySpan<byte> installed = catalog.ResolveBytes(type);
            AssertEqual(0x840, transfer.Length, "Calculated full native transfer length");
            AssertEqual(0x840, installed.Length, "Installed full native transfer length");
            for (int index = 0; index < 1056; index++)
            {
                int expected = ReadVerificationWord(rom, source + 2 * index) & mask;
                AssertEqual(expected, RoomFxLiquidTilemapDefinitions.Cell(type, index).Raw & mask, "Original liquid cell field");
                AssertEqual(expected, BinaryPrimitives.ReadUInt16LittleEndian(transfer.AsSpan(2 * index)) & mask,
                    "Calculated transfer field view");
                AssertEqual(expected, BinaryPrimitives.ReadUInt16LittleEndian(installed[(2 * index)..]) & mask,
                    "Installed stock field view");
            }
        }
    }

    /// <summary>Rejects unknown liquid identities and partial pages, and confirms the stock pages are the cartridge's pages.</summary>
    private static void VerifyLiquidTilemapDomainAndStorage(ISnesAddressSpace rom, RoomFxLayer3Pages stock)
    {
        var valid = new HashSet<RoomFxType> { RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water };
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var type = (RoomFxType)value;
            if (valid.Contains(type)) continue;
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxLiquidTilemapDefinitions.Cell(type, 0),
                "Only three liquid identities have stock layout algorithms");
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxLiquidTilemapDefinitions.ComposePage(type),
                "Transfer rejects unsupported type without expanding domain");
        }
        foreach (RoomFxType type in valid)
        {
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(type));
            var original = new byte[0x840];
            for (int offset = 0; offset < original.Length; offset++) original[offset] = LiquidTilemapDomainAndStorageRomByte(rom, source + offset);
            AssertTrue(RoomFxLiquidTilemapDefinitionsConstants.Matches(type, original), "Full original page is recognized");
            for (int index = 0; index < 1056; index++)
            {
                original[index * 2] ^= 1;
                AssertTrue(!RoomFxLiquidTilemapDefinitionsConstants.Matches(type, original), "An edit at any cell prevents stock substitution");
                original[index * 2] ^= 1;
            }
            AssertTrue(!RoomFxLiquidTilemapDefinitionsConstants.Matches(type, original.AsSpan(0, original.Length - 1)),
                "Partial page cannot become stock");
            foreach (int invalid in new[] { int.MinValue, -1, 1056, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxLiquidTilemapDefinitions.Cell(type, invalid),
                    "Calculated cell rejects outside full33-row domain");
            AssertTrue(original.AsSpan().SequenceEqual(stock.ResolveBytes(type)), "The stock catalog's page is the cartridge's page");
        }
    }

    /// <summary>Reads one retail liquid page byte for the exact stock-page comparison.</summary>
    private static byte LiquidTilemapDomainAndStorageRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs field-by-field page comparisons and the identity, bounds, and storage checks.</summary>
    internal static void VerifyLiquidTilemaps(ISnesAddressSpace rom, RoomFxLayer3Pages stock)
    {
        Suite(nameof(VerifyLiquidTileCharacters), () => VerifyLiquidTileCharacters(rom, stock));
        Suite(nameof(VerifyLiquidTilePalettes), () => VerifyLiquidTilePalettes(rom, stock));
        Suite(nameof(VerifyLiquidTilePriority), () => VerifyLiquidTilePriority(rom, stock));
        Suite(nameof(VerifyLiquidTileHorizontalFlip), () => VerifyLiquidTileHorizontalFlip(rom, stock));
        Suite(nameof(VerifyLiquidTileVerticalFlip), () => VerifyLiquidTileVerticalFlip(rom, stock));
        Suite(nameof(VerifyLiquidTilemapDomainAndStorage), () => VerifyLiquidTilemapDomainAndStorage(rom, stock));
    }

    /// <summary>Guards the converted bank-$8A liquid-page range against extraction reads.</summary>
    internal static CartridgeReadGuard NewLiquidTilemapSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(0x8a8000, 0x8a98c0 - 0x8a8000,
            _ => "Liquid tilemap extraction read a converted stock page.");
}
