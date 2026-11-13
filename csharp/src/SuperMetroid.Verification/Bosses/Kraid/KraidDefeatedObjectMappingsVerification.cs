using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks both defeated-room PLM requests against Kraid's inline native arguments.</summary>
internal static class KraidDefeatedObjectMappingsVerification
{
    /// <summary>Compares the native block-column bytes for both post-fight PLM operations.</summary>
    internal static void VerifyKraidDefeatedObjectColumns(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedObjectField), () => VerifyKraidDefeatedObjectField(rom, 0, request => request.Block.X));

    /// <summary>Compares the native block-row bytes for both post-fight PLM operations.</summary>
    internal static void VerifyKraidDefeatedObjectRows(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedObjectField), () => VerifyKraidDefeatedObjectField(rom, 1, request => request.Block.Y));

    /// <summary>Compares the native two-byte PLM header identities for both post-fight operations.</summary>
    internal static void VerifyKraidDefeatedObjectHeaders(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedObjectField), () => VerifyKraidDefeatedObjectField(rom, 2, request => KraidNativeWords.RoomMutations.Word(request.RoomObject)));

    /// <summary>Checks the selected field in native request order and rejects indexes outside the two operations.</summary>
    private static void VerifyKraidDefeatedObjectField(CartridgeImportAddressSpace rom, int offset,
        Func<RoomObjectSpawnRequest<KraidRoomMutation>, ushort> field)
    {
        int[] arguments = [0xa7c16c, 0xa7c175];
        var requests = KraidObjectDefinitions.DefeatedRoom;
        var enumerated = requests.ToArray();
        AssertEqual(2, requests.Count, "Two defeated-room operations");
        AssertEqual(2, enumerated.Length, "Defeated-room enumeration count");
        for (int index = 0; index < arguments.Length; index++)
        {
            int address = arguments[index] + offset;
            ushort expected = offset == 2
                ? LittleEndianBytes.Word(KraidDefeatedObjectFieldRomByte(rom, address), KraidDefeatedObjectFieldRomByte(rom, address + 1))
                : KraidDefeatedObjectFieldRomByte(rom, address);
            AssertEqual(expected, field(requests[index]), "Native defeated-room inline field");
            AssertEqual(expected, field(enumerated[index]), "Native defeated-room operation order");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = requests[invalid], "Defeated-room list bounds");
    }

    /// <summary>Reads an inline bank-$A7 argument byte from the cartridge oracle.</summary>
    private static byte KraidDefeatedObjectFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
