using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks the eight admitted downward-gate arguments against three native selection tables.</summary>
internal static class LookupDownwardGateSelectionVerification
{
    /// <summary>Compares selected instruction lists and rejects every unsupported word argument.</summary>
    internal static void VerifyDownwardGateListSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c70a, 0));

    /// <summary>Compares the left draw-block word for each admitted gate argument.</summary>
    internal static void VerifyDownwardGateLeftSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c71a, 1));

    /// <summary>Compares the right draw-block word for each admitted gate argument.</summary>
    internal static void VerifyDownwardGateRightSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c72a, 2));

    /// <summary>Scans all 16-bit arguments, accepting only the eight even selectors and comparing their native field.</summary>
    private static void VerifyDownwardGateSelectionField(SuperMetroidAddressSpace rom, int source, int field)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort argument = (ushort)raw;
            if (raw is not (0 or 2 or 4 or 6 or 8 or 10 or 12 or 14))
            {
                AssertThrows<InvalidDataException>(() => GatePlms.DownwardGateShotBlock(
                    PlmRoomArgumentWords.Decode(PlmHeaderId.DownwardGateShotBlock, argument) is GateTriggerArgument { Trigger: var trigger }
                        ? trigger : throw new InvalidDataException($"Argument ${argument:X4} names no trigger.")),
                    "Downward gate rejects all unsupported arguments");
                continue;
            }
            DownwardGateShotBlockDefinition action = GatePlms.DownwardGateShotBlock(argument / 2);
            ushort native = ReadSamusEaterPlmWord(rom, source + raw);
            if (field == 0)
            {
                AssertEqual(RoomPlmNativeListAddresses.DownwardGateEntries.Single(entry => entry.Entry == native).Program, action.Program,
                    $"Downward gate argument {raw} starts its trigger program");
                continue;
            }
            AssertEqual(native, field == 1 ? action.LeftBlockWord : action.RightBlockWord,
                $"Downward gate original field {field}, argument {raw}");
        }
    }
}
