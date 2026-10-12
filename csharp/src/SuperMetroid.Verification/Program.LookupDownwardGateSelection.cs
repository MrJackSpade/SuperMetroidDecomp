using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SamusEaterPlmDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Represents lookup downward gate selection verification.</summary>
internal static class LookupDownwardGateSelectionVerification
{
    /// <summary>Verifies downward gate list selection.</summary>
    internal static void VerifyDownwardGateListSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c70a, 0));

    /// <summary>Verifies downward gate left selection.</summary>
    internal static void VerifyDownwardGateLeftSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c71a, 1));

    /// <summary>Verifies downward gate right selection.</summary>
    internal static void VerifyDownwardGateRightSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c72a, 2));

    /// <summary>Verifies downward gate selection field.</summary>
    private static void VerifyDownwardGateSelectionField(SuperMetroidAddressSpace rom, int source, int field)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort argument = (ushort)raw;
            if (raw is not (0 or 2 or 4 or 6 or 8 or 10 or 12 or 14))
            {
                AssertThrows<InvalidDataException>(() => DownwardGateShotBlockDefinitions.Resolve(argument),
                    "Downward gate rejects all unsupported arguments");
                continue;
            }
            DownwardGateShotBlockDefinition action = DownwardGateShotBlockDefinitions.Resolve(argument);
            ushort actual = field switch
            {
                0 => action.InstructionList,
                1 => action.LeftBlockWord,
                _ => action.RightBlockWord,
            };
            AssertEqual(ReadSamusEaterPlmWord(rom, source + raw), actual,
                $"Downward gate original field {field}, argument {raw}");
        }
    }
}
