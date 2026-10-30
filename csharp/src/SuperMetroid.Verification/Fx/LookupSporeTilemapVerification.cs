using System.Buffers.Binary;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks stock and edited Spore Spawn layer-three tile attributes.</summary>
internal static class LookupSporeTilemapVerification
{
    /// <summary>Compares palette bits throughout the 1056-cell page.</summary>
    private static void VerifySporeTilePalette(ISnesAddressSpace rom, RoomFxLayer3Pages stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x1c00));
    /// <summary>Compares priority bits throughout the page.</summary>
    private static void VerifySporeTilePriority(ISnesAddressSpace rom, RoomFxLayer3Pages stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x2000));
    /// <summary>Compares horizontal-flip bits throughout the page.</summary>
    private static void VerifySporeTileHorizontalFlip(ISnesAddressSpace rom, RoomFxLayer3Pages stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x4000));
    /// <summary>Compares vertical-flip bits throughout the page.</summary>
    private static void VerifySporeTileVerticalFlip(ISnesAddressSpace rom, RoomFxLayer3Pages stock) =>
        Suite(nameof(VerifySporeTileAttribute), () => VerifySporeTileAttribute(rom, stock, 0x8000));

    /// <summary>Checks both compiled and installed attribute words against the selected retail bit mask.</summary>
    private static void VerifySporeTileAttribute(ISnesAddressSpace rom, RoomFxLayer3Pages stock, int mask)
    {
        ReadOnlySpan<byte> installed = stock.ResolveBytes(RoomFxType.Spores);
        AssertEqual(0x840, installed.Length, "Full original spore transfer length");
        for (int index = 0; index < 1056; index++)
        {
            int expected = ReadVerificationWord(rom, 0x8a98c0 + 2 * index) & mask;
            AssertEqual(expected, RoomFxSporeTilemapDefinitions.Attributes(index) & mask, "Original spore attribute");
            AssertEqual(expected, BinaryPrimitives.ReadUInt16LittleEndian(installed[(2 * index)..]) & mask,
                "Installed spore attribute view");
        }
    }

    /// <summary>Confirms the stock page is the cartridge's page and an edit at every cell survives a reload.</summary>
    private static void VerifySporeAttributeStorageAndEdits(ISnesAddressSpace rom, RoomFxLayer3Pages stock)
    {
        var original = new byte[0x840];
        for (int offset = 0; offset < original.Length; offset++) original[offset] = SporeAttributeStorageAndEditsRomByte(rom, 0x8a98c0 + offset);
        AssertTrue(original.AsSpan().SequenceEqual(stock.ResolveBytes(RoomFxType.Spores)), "The stock spore page is the cartridge's page");
        byte[] changed = original.ToArray();
        for (int index = 0; index < 1056; index++)
        {
            byte bit = (index % 4) switch { 0 => 4, 1 => 32, 2 => 64, _ => 128 };
            changed[index * 2 + 1] ^= bit;
        }
        RoomFxLayer3Pages edited = RoomFxLayer3TilemapFixtures.WithPage(stock, RoomFxType.Spores, BackgroundTileWordBytes.Tiles(changed));
        AssertTrue(changed.AsSpan().SequenceEqual(edited.ResolveBytes(RoomFxType.Spores)), "An attribute edit at every cell survives exactly");
        AssertTrue(original.AsSpan().SequenceEqual(stock.ResolveBytes(RoomFxType.Spores)), "An edited catalog leaves the stock catalog's page");
        foreach (int index in new[] { int.MinValue, -1, 1056, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RoomFxSporeTilemapDefinitions.Attributes(index),
                "Attribute function preserves its exact cell domain");
        foreach (int length in new[] { 0, 1, 1055, 1057 })
            AssertThrows<InvalidDataException>(() => RoomFxLayer3TilemapFixtures.WithPage(stock, RoomFxType.Spores, new BackgroundTile[length]),
                "Incomplete spore page rejects");
    }

    /// <summary>Reads a retail tilemap byte when constructing the independent edit source.</summary>
    private static byte SporeAttributeStorageAndEditsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs all four attribute-bit comparisons and storage ownership checks.</summary>
    internal static void VerifySporeTilemapAttributes(ISnesAddressSpace rom, RoomFxLayer3Pages stock)
    {
        Suite(nameof(VerifySporeTilePalette), () => VerifySporeTilePalette(rom, stock));
        Suite(nameof(VerifySporeTilePriority), () => VerifySporeTilePriority(rom, stock));
        Suite(nameof(VerifySporeTileHorizontalFlip), () => VerifySporeTileHorizontalFlip(rom, stock));
        Suite(nameof(VerifySporeTileVerticalFlip), () => VerifySporeTileVerticalFlip(rom, stock));
        Suite(nameof(VerifySporeAttributeStorageAndEdits), () => VerifySporeAttributeStorageAndEdits(rom, stock));
    }
}
