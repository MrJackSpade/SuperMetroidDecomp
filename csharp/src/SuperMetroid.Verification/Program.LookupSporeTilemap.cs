using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares the palette bits of every installed spore-tile attribute with the native transfer data.</summary>
    /// <param name="rom">Cartridge address space supplying the expected tilemap words.</param>
    /// <param name="stock">Installed room-FX tilemap catalog containing the spore page.</param>
    private static void VerifySporeTilePalette(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x1c00));
    /// <summary>Compares the priority bit of every installed spore-tile attribute with the native transfer data.</summary>
    /// <param name="rom">Cartridge address space supplying the expected tilemap words.</param>
    /// <param name="stock">Installed room-FX tilemap catalog containing the spore page.</param>
    private static void VerifySporeTilePriority(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x2000));
    /// <summary>Compares the horizontal-flip bit of every installed spore-tile attribute with the native transfer data.</summary>
    /// <param name="rom">Cartridge address space supplying the expected tilemap words.</param>
    /// <param name="stock">Installed room-FX tilemap catalog containing the spore page.</param>
    private static void VerifySporeTileHorizontalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x4000));
    /// <summary>Compares the vertical-flip bit of every installed spore-tile attribute with the native transfer data.</summary>
    /// <param name="rom">Cartridge address space supplying the expected tilemap words.</param>
    /// <param name="stock">Installed room-FX tilemap catalog containing the spore page.</param>
    private static void VerifySporeTileVerticalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x8000));

    /// <summary>Checks one masked attribute component across the complete 1,056-cell spore tilemap.</summary>
    /// <param name="rom">Cartridge address space containing the native spore attribute words.</param>
    /// <param name="stock">Installed catalog whose transfer bytes are compared with the native words.</param>
    /// <param name="mask">Attribute bits retained for this palette, priority, or flip comparison.</param>
    private static void VerifySporeTileAttribute(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock, int mask)
    {
        ReadOnlySpan<byte> installed = stock.Resolve(RoomFxType.Spores).Span;
        AssertEqual(0x840, installed.Length, "Full original spore transfer length");
        for (int index = 0; index < 1056; index++)
        {
            int expected = ReadVerificationWord(rom, 0x8a98c0 + 2 * index) & mask;
            AssertEqual(expected, RoomFxSporeTilemapDefinitions.Attributes(index) & mask, "Original spore attribute");
            AssertEqual(expected, BinaryPrimitives.ReadUInt16LittleEndian(installed[(2 * index)..]) & mask,
                "Installed spore attribute view");
        }
    }

    /// <summary>Checks that stock attributes remain uncached and custom pages preserve all edited bytes independently of input storage.</summary>
    /// <param name="rom">Cartridge address space used to seed complete custom pages from native data.</param>
    /// <param name="stock">Stock catalog whose lazy resolution must not materialize custom attribute storage.</param>
    private static void VerifySporeAttributeStorageAndEdits(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        object page = typeof(RoomFxLayer3TilemapCatalog).GetField("spores", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(stock)!;
        FieldInfo edits = typeof(RoomFxSporeTilemap).GetField("customAttributes", BindingFlags.Instance | BindingFlags.NonPublic)!;
        AssertTrue(edits.GetValue(page) is null, "Stock spore attributes are not stored");
        _ = stock.Resolve(RoomFxType.Spores);
        AssertTrue(edits.GetValue(page) is null, "Resolving spores does not cache attributes");

        var original = new byte[0x840];
        for (int offset = 0; offset < original.Length; offset++) original[offset] = rom.ReadByte(0x8a98c0 + offset);
        for (int index = 0; index < 1056; index++)
        {
            byte[] changed = original.ToArray();
            byte bit = (index % 4) switch { 0 => 4, 1 => 32, 2 => 64, _ => 128 };
            changed[index * 2 + 1] ^= bit;
            var custom = new RoomFxSporeTilemap(changed);
            byte[] transfer = custom.CreateTransfer();
            AssertTrue(changed.AsSpan().SequenceEqual(transfer), "An attribute edit at every cell survives exactly");
            Array.Clear(changed);
            AssertTrue(transfer.AsSpan().SequenceEqual(custom.CreateTransfer()), "Page owns custom values independently of input");
        }
        foreach (int index in new[] { int.MinValue, -1, 1056, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxSporeTilemapDefinitions.Attributes(index),
                "Attribute function preserves its exact cell domain");
        foreach (int length in new[] { 0, 1, 0x83f, 0x841 })
            AssertThrows<ArgumentException>(() => new RoomFxSporeTilemap(new byte[length]), "Incomplete spore page rejects");
    }

    /// <summary>Runs native parity checks for palette, priority, flips, and editable storage across the spore tilemap.</summary>
    /// <param name="rom">Cartridge address space providing the native spore transfer data.</param>
    /// <param name="stock">Installed catalog supplying the stock spore attribute page.</param>
    private static void VerifySporeTilemapAttributes(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        Suite(nameof(VerifySporeTilePalette), () => VerifySporeTilePalette(rom, stock));
        Suite(nameof(VerifySporeTilePriority), () => VerifySporeTilePriority(rom, stock));
        Suite(nameof(VerifySporeTileHorizontalFlip), () => VerifySporeTileHorizontalFlip(rom, stock));
        Suite(nameof(VerifySporeTileVerticalFlip), () => VerifySporeTileVerticalFlip(rom, stock));
        Suite(nameof(VerifySporeAttributeStorageAndEdits), () => VerifySporeAttributeStorageAndEdits(rom, stock));
    }
}
