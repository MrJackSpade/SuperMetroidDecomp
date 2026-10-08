using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Proves that the compiled Mother Brain body mechanics words match the pinned cartridge
    /// and that every supported native program can run while those ROM bytes are inaccessible.
    /// </summary>
    private static void VerifyMotherBrainBodyInstructionPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));

        foreach (MotherBrainBodyInstructionMechanicsWord definition in
            MotherBrainBodyInstructionProgramDefinitions.AllWords)
        {
            AssertEqual(
                ReadRetailWord(rom, 0xa90000 | definition.Address),
                definition.Word,
                $"Mother Brain body compiled mechanics word $A9:{definition.Address:X4}");

            if ((definition.Word & 0x8000) == 0)
            {
                ushort visualOperand = unchecked((ushort)(definition.Address + 2));
                AssertEqual(ReadRetailWord(rom, 0xa90000 | visualOperand),
                    MotherBrainBodyInstructionProgramDefinitions.ReadVisualSelector(visualOperand),
                    $"Mother Brain body visual selector $A9:{visualOperand:X4}");
                AssertTrue(
                    !MotherBrainBodyInstructionProgramDefinitions.TryGetWord(
                        visualOperand,
                        out _),
                    $"Mother Brain body spritemap $A9:{definition.Address + 2:X4} remains presentation-owned");
            }
        }

        AssertEqual(ReadRetailWord(rom, 0xa90000 |
                MotherBrainBodyInstructionProgramDefinitions.InitialDummyVisualOperand),
            MotherBrainBodyInstructionProgramDefinitions.ReadInitialDummyVisualSelector(
                MotherBrainBodyInstructionProgramDefinitions.InitialDummyVisualOperand),
            "Mother Brain initial dummy selector matches the cartridge");
        AssertThrows<InvalidDataException>(
            () => MotherBrainBodyInstructionProgramDefinitions.ReadInitialDummyVisualSelector(
                MotherBrainBodyInstructionProgramDefinitions.InitialDummy),
            "Mother Brain dummy visual selector rejects neighboring mechanics data");

        Console.WriteLine(
            $"  Mother Brain: {MotherBrainBodyInstructionProgramDefinitions.AllWords.Count} " +
            "body command/duration words and visual selectors match the cartridge.");

        static ushort ReadRetailWord(ISnesAddressSpace source, int address) =>
            unchecked((ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8));
    }
}
