using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid's nine growth callback PLM placements against the native bank-$A7 callback table.</summary>
internal static class KraidGrowthObjectMappingsVerification
{
    /// <summary>Compares the compiled growth PLM column coordinates with callback inline bytes.</summary>
    internal static void VerifyKraidGrowthObjectColumns(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthObjectField), () => VerifyKraidGrowthObjectField(rom, 4, request => request.Block.X));

    /// <summary>Compares the compiled growth PLM row coordinates with callback inline bytes.</summary>
    internal static void VerifyKraidGrowthObjectRows(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthObjectField), () => VerifyKraidGrowthObjectField(rom, 5, request => request.Block.Y));

    /// <summary>Compares the compiled growth PLM headers with callback inline words.</summary>
    internal static void VerifyKraidGrowthObjectHeaders(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthObjectField), () => VerifyKraidGrowthObjectField(rom, 6, request => KraidNativeWords.RoomMutations.Word(request.RoomObject)));

    /// <summary>Reads each callback field at the requested native offset and checks indexed and enumerated PLM requests.</summary>
    private static void VerifyKraidGrowthObjectField(CartridgeImportAddressSpace rom, int offset,
        Func<RoomObjectSpawnRequest<KraidRoomMutation>, ushort> field)
    {
        ushort Word(int address) => LittleEndianBytes.Word(KraidGrowthObjectFieldRomByte(rom, address), KraidGrowthObjectFieldRomByte(rom, address + 1));
        // Each ceiling rock carries the block it breaks, in the order the rocks fall (#2400).
        RoomObjectSpawnRequest<KraidRoomMutation>[] requests = [.. KraidCeilingRockDefinitions.Rocks.Select(rock => rock.Crumble)];
        AssertEqual(9, requests.Length, "Nine native growth callbacks");
        for (int index = 0; index < 9; index++)
        {
            int callback = 0xa70000 | Word(0xa7acc5 + 2 * index);
            ushort expected = offset == 6 ? Word(callback + offset) : KraidGrowthObjectFieldRomByte(rom, callback + offset);
            AssertEqual(expected, field(requests[index]), "Native callback inline PLM field");
        }
    }

    /// <summary>Reads a native Kraid growth callback byte for the focused cartridge comparison.</summary>
    private static byte KraidGrowthObjectFieldRomByte(CartridgeImportAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
