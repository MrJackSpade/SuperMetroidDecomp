using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms;

/// <summary>Checks the room artwork sources decoded from the tileset table against the retail cartridge's.</summary>
internal static class RoomTilesetSourcesVerification
{
    /// <summary>Every set the tileset table names decodes to its retail source, and no other set appears.</summary>
    internal static void VerifyRoomTilesetSources()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        RoomTilesetSourceMap sources = RoomTilesetSources.Read(CartridgeImportSource.Require(rom));
        Check(RoomTilesetNative.RetailCharacters, sources.Characters, "character sheet");
        Check(RoomTilesetNative.RetailBlocks, sources.Blocks, "block table");
        Check(RoomTilesetNative.RetailPalettes, sources.Palettes, "palette");
    }

    [VerificationCommand("--room-tileset-sources")]
    internal static int RoomTilesetSourcesCommand()
    {
        Suite(nameof(VerifyRoomTilesetSources), () => VerifyRoomTilesetSources());
        Console.WriteLine("Room tileset sources: every graphics set's sheet, blocks and palette decode to the retail sources.");
        return 0;
    }

    /// <summary>Compares one kind of decoded source with the retail sources.</summary>
    private static void Check<TSet>(IReadOnlyDictionary<TSet, int> retail, IReadOnlyDictionary<TSet, SnesAddress> decoded, string kind)
        where TSet : struct, Enum
    {
        AssertEqual(retail.Count, decoded.Count, $"every {kind} the tileset table names");
        foreach ((TSet set, int source) in retail)
            AssertEqual(source, (int)decoded[set], $"{set} {kind} source");
    }
}
