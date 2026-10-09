using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies downward-gate instruction-list selection against every possible native argument.</summary>
    /// <param name="rom">Cartridge address space containing the gate setup table.</param>
    private static void VerifyDownwardGateListSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c70a, 0));

    /// <summary>Verifies the left block-word selection for every supported downward-gate argument.</summary>
    /// <param name="rom">Cartridge address space containing the gate setup table.</param>
    private static void VerifyDownwardGateLeftSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c71a, 1));

    /// <summary>Verifies the right block-word selection for every supported downward-gate argument.</summary>
    /// <param name="rom">Cartridge address space containing the gate setup table.</param>
    private static void VerifyDownwardGateRightSelection(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateSelectionField), () => VerifyDownwardGateSelectionField(rom, 0x84c72a, 2));

    /// <summary>Checks the selected native table field and confirms that only the eight even arguments from zero through fourteen are accepted.</summary>
    /// <param name="rom">Cartridge address space used as the native reference.</param>
    /// <param name="source">Bank-$84 table address corresponding to the selected instruction-list, left-word, or right-word field.</param>
    /// <param name="field">Resolved definition field to compare: zero for instruction list, one for left word, or two for right word.</param>
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
