using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks stock and edited Spore Spawn layer-three tile attributes.</summary>
internal static class LookupSporeTilemapVerification
{
    /// <summary>Compares palette bits throughout the 1056-cell page.</summary>
    private static void VerifySporeTilePalette(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x1c00));
    /// <summary>Compares priority bits throughout the page.</summary>
    private static void VerifySporeTilePriority(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x2000));
    /// <summary>Compares horizontal-flip bits throughout the page.</summary>
    private static void VerifySporeTileHorizontalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x4000));
    /// <summary>Compares vertical-flip bits throughout the page.</summary>
    private static void VerifySporeTileVerticalFlip(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x8000));

    /// <summary>Checks both compiled and installed attribute words against the selected retail bit mask.</summary>
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

    /// <summary>Confirms stock values stay computed while each edited cell survives an owned transfer.</summary>
    private static void VerifySporeAttributeStorageAndEdits(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        object page = stock.spores;
        AssertTrue(((RoomFxSporeTilemap)page).customAttributes is null, "Stock spore attributes are not stored");
        _ = stock.Resolve(RoomFxType.Spores);
        AssertTrue(((RoomFxSporeTilemap)page).customAttributes is null, "Resolving spores does not cache attributes");

        var original = new byte[0x840];
        for (int offset = 0; offset < original.Length; offset++) original[offset] = SporeAttributeStorageAndEditsRomByte(rom, 0x8a98c0 + offset);
        for (int index = 0; index < 1056; index++)
        {
            byte[] changed = original.ToArray();
            byte bit = (index % 4) switch { 0 => 4, 1 => 32, 2 => 64, _ => 128 };
            changed[index * 2 + 1] ^= bit;
            var custom = new RoomFxSporeTilemap(changed);
            byte[] transfer = custom.CreateTransfer();
            AssertTrue(changed.SequenceEqual(transfer), "An attribute edit at every cell survives exactly");
            Array.Clear(changed);
            AssertTrue(transfer.SequenceEqual(custom.CreateTransfer()), "Page owns custom values independently of input");
        }
        foreach (int index in new[] { int.MinValue, -1, 1056, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxSporeTilemapDefinitions.Attributes(index),
                "Attribute function preserves its exact cell domain");
        foreach (int length in new[] { 0, 1, 0x83f, 0x841 })
            AssertThrows<ArgumentException>(() => new RoomFxSporeTilemap(new byte[length]), "Incomplete spore page rejects");
    }

    /// <summary>Reads a retail tilemap byte when constructing the independent edit source.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for spore attribute storage and edits")]
    private static byte SporeAttributeStorageAndEditsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs all four attribute-bit comparisons and storage ownership checks.</summary>
    internal static void VerifySporeTilemapAttributes(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        Suite(nameof(VerifySporeTilePalette), () => VerifySporeTilePalette(rom, stock));
        Suite(nameof(VerifySporeTilePriority), () => VerifySporeTilePriority(rom, stock));
        Suite(nameof(VerifySporeTileHorizontalFlip), () => VerifySporeTileHorizontalFlip(rom, stock));
        Suite(nameof(VerifySporeTileVerticalFlip), () => VerifySporeTileVerticalFlip(rom, stock));
        Suite(nameof(VerifySporeAttributeStorageAndEdits), () => VerifySporeAttributeStorageAndEdits(rom, stock));
    }
}
