using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks Kraid's small-rock instruction mechanics words and bank-byte ownership against native addresses.</summary>
    /// <param name="rom">Cartridge address space used to compare compiled mechanics words with retail data.</param>
    private static void VerifyKraidRockMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom,
            [0x9c7d, 0x9c81, 0x9c83, 0x9c87, 0x9c89, 0x9c8b, 0x9c8d, 0x9c91, 0x9c95, 0x9c99, 0x9c9d, 0x9ca1],
            KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord,
            KraidRockProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte, bank: 0x86));

    /// <summary>Checks the visual-operand addresses interleaved with Kraid's small-rock instruction stream.</summary>
    private static void VerifyKraidRockPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x9c7f, 0x9c85, 0x9c8f, 0x9c93, 0x9c97, 0x9c9b, 0x9c9f],
            KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount,
            KraidRockProjectileInstructionProgramDefinitions.PresentationWordAddress,
            KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord));
}
