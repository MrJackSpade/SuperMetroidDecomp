using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyLiquidTileCharacters(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x03ff));
    private static void VerifyLiquidTilePalettes(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x1c00));
    private static void VerifyLiquidTilePriority(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x2000));
    private static void VerifyLiquidTileHorizontalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x4000));
    private static void VerifyLiquidTileVerticalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog catalog) =>
        Suite(nameof(VerifyLiquidTileField), () => VerifyLiquidTileField(rom, catalog, 0x8000));

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

    private static void VerifyLiquidTilemaps(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        Suite(nameof(VerifyLiquidTileCharacters), () => VerifyLiquidTileCharacters(rom, stock));
        Suite(nameof(VerifyLiquidTilePalettes), () => VerifyLiquidTilePalettes(rom, stock));
        Suite(nameof(VerifyLiquidTilePriority), () => VerifyLiquidTilePriority(rom, stock));
        Suite(nameof(VerifyLiquidTileHorizontalFlip), () => VerifyLiquidTileHorizontalFlip(rom, stock));
        Suite(nameof(VerifyLiquidTileVerticalFlip), () => VerifyLiquidTileVerticalFlip(rom, stock));
        Suite(nameof(VerifyLiquidTilemapDomainAndStorage), () => VerifyLiquidTilemapDomainAndStorage(rom, stock));
    }

    private sealed class LiquidTilemapSourceGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadByte(int address) => address is >= 0x8a8000 and < 0x8a98c0
            ? throw new InvalidOperationException("Liquid tilemap extraction read a converted stock page.")
            : source.ReadByte(address);
        public byte ReadCartridgeByte(int address) => ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
