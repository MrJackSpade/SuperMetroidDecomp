using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySporeTilePalette(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        VerifySporeTileAttribute(rom, stock, 0x1c00);
    private static void VerifySporeTilePriority(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        VerifySporeTileAttribute(rom, stock, 0x2000);
    private static void VerifySporeTileHorizontalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        VerifySporeTileAttribute(rom, stock, 0x4000);
    private static void VerifySporeTileVerticalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        VerifySporeTileAttribute(rom, stock, 0x8000);

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

    private static void VerifySporeTilemapAttributes(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        VerifySporeTilePalette(rom, stock);
        VerifySporeTilePriority(rom, stock);
        VerifySporeTileHorizontalFlip(rom, stock);
        VerifySporeTileVerticalFlip(rom, stock);
        VerifySporeAttributeStorageAndEdits(rom, stock);
    }
}
