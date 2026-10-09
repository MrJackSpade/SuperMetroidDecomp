using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Verifies every liquid tile's character index against native, calculated, and installed pages.</summary>
    /// <param name="rom">The address space containing the original liquid tilemap pages.</param>
    /// <param name="catalog">The stock catalog whose transfer pages are checked.</param>
    private static void VerifyLiquidTileCharacters(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x03ff));

    /// <summary>Verifies every liquid tile's palette bits against native, calculated, and installed pages.</summary>
    /// <param name="rom">The address space containing the original liquid tilemap pages.</param>
    /// <param name="catalog">The stock catalog whose transfer pages are checked.</param>
    private static void VerifyLiquidTilePalettes(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x1c00));

    /// <summary>Verifies every liquid tile's priority bit against native, calculated, and installed pages.</summary>
    /// <param name="rom">The address space containing the original liquid tilemap pages.</param>
    /// <param name="catalog">The stock catalog whose transfer pages are checked.</param>
    private static void VerifyLiquidTilePriority(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x2000));

    /// <summary>Verifies every liquid tile's horizontal-flip bit against native, calculated, and installed pages.</summary>
    /// <param name="rom">The address space containing the original liquid tilemap pages.</param>
    /// <param name="catalog">The stock catalog whose transfer pages are checked.</param>
    private static void VerifyLiquidTileHorizontalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x4000));

    /// <summary>Verifies every liquid tile's vertical-flip bit against native, calculated, and installed pages.</summary>
    /// <param name="rom">The address space containing the original liquid tilemap pages.</param>
    /// <param name="catalog">The stock catalog whose transfer pages are checked.</param>
    private static void VerifyLiquidTileVerticalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x8000));

    /// <summary>Compares one masked tilemap field across cartridge data, calculated cells, and full-page transfers.</summary>
    /// <param name="rom">The address space containing the native page for each supported liquid effect.</param>
    /// <param name="catalog">The catalog that supplies the installed stock transfer page.</param>
    /// <param name="mask">The tile-word bits selected for this field comparison.</param>
    private static void VerifyLiquidTileField(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog, int mask)
    {
        foreach (RoomFxType type in new[] { RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water })
        {
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type);
            byte[] transfer = RoomFxLiquidTilemapDefinitions.CreateTransfer(type);
            ReadOnlySpan<byte> installed = catalog.Resolve(type).Span;
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

    /// <summary>Checks unsupported effect identities, exact stock-page recognition, and the absence of cached page storage.</summary>
    /// <param name="rom">The address space containing the original liquid pages used for recognition checks.</param>
    /// <param name="stock">The stock catalog inspected for retained or cached liquid page bytes.</param>
    private static void VerifyLiquidTilemapDomainAndStorage(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        var valid = new HashSet<RoomFxType> { RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water };
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var type = (RoomFxType)value;
            if (valid.Contains(type)) continue;
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxLiquidTilemapDefinitions.Cell(type, 0),
                "Only three liquid identities have stock layout algorithms");
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxLiquidTilemapDefinitions.CreateTransfer(type),
                "Transfer rejects unsupported type without expanding domain");
        }
        foreach (RoomFxType type in valid)
        {
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type);
            var original = new byte[0x840];
            for (int offset = 0; offset < original.Length; offset++) original[offset] = rom.ReadByte(source + offset);
            AssertTrue(RoomFxLiquidTilemapDefinitions.Matches(type, original), "Full original page is recognized");
            for (int index = 0; index < 1056; index++)
            {
                original[index * 2] ^= 1;
                AssertTrue(!RoomFxLiquidTilemapDefinitions.Matches(type, original), "An edit at any cell prevents stock substitution");
                original[index * 2] ^= 1;
            }
            AssertTrue(!RoomFxLiquidTilemapDefinitions.Matches(type, original.AsSpan(0, original.Length - 1)),
                "Partial page cannot become stock");
            foreach (int invalid in new[] { int.MinValue, -1, 1056, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxLiquidTilemapDefinitions.Cell(type, invalid),
                    "Calculated cell rejects outside full33-row domain");
            FieldInfo field = typeof(RoomFxLayer3TilemapCatalog).GetField(type.ToString().ToLowerInvariant(),
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            AssertTrue(field.GetValue(stock) is null, "Stock catalog retains no liquid page bytes");
            _ = stock.Resolve(type);
            AssertTrue(field.GetValue(stock) is null, "Transfer does not create a cached page");
        }
    }

    /// <summary>Runs the liquid tile field, identity-domain, and storage verification suites.</summary>
    /// <param name="rom">The address space containing the native liquid tilemap data.</param>
    /// <param name="stock">The stock catalog supplying calculated and installed tilemap pages.</param>
    private static void VerifyLiquidTilemaps(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        Suite(nameof(VerifyLiquidTileCharacters), () => VerifyLiquidTileCharacters(rom, stock));
        Suite(nameof(VerifyLiquidTilePalettes), () => VerifyLiquidTilePalettes(rom, stock));
        Suite(nameof(VerifyLiquidTilePriority), () => VerifyLiquidTilePriority(rom, stock));
        Suite(nameof(VerifyLiquidTileHorizontalFlip), () => VerifyLiquidTileHorizontalFlip(rom, stock));
        Suite(nameof(VerifyLiquidTileVerticalFlip), () => VerifyLiquidTileVerticalFlip(rom, stock));
        Suite(nameof(VerifyLiquidTilemapDomainAndStorage), () => VerifyLiquidTilemapDomainAndStorage(rom, stock));
    }

    /// <summary>Rejects cartridge reads from the converted stock liquid-page range while forwarding other bus operations.</summary>
    /// <param name="source">The underlying address space used for allowed reads and all writes.</param>
    private sealed class LiquidTilemapSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Rejects reads of converted liquid pages and delegates all other byte reads.</summary>
        /// <param name="address">The bus address to inspect and read.</param>
        /// <returns>The byte supplied by the underlying address space when the address is allowed.</returns>
        /// <exception cref="InvalidOperationException">The address is within the converted stock liquid-page range.</exception>
        public byte ReadByte(int address) => address is >= 0x8a8000 and < 0x8a98c0
            ? throw new InvalidOperationException("Liquid tilemap extraction read a converted stock page.")
            : source.ReadByte(address);

        /// <summary>Routes cartridge reads through the converted-page guard.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The byte returned by the guarded address-space read.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Forwards byte writes unchanged to the underlying address space.</summary>
        /// <param name="address">The bus address to write.</param>
        /// <param name="value">The byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
