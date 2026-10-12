using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static KraidSmallProgramsVerification;
using static SuiteTimingVerification;

/// <summary>Checks Kraid rock projectile control words and presentation slots against their native bank-$86 lists.</summary>
internal static class KraidRockGeneratedProgramsVerification
{
    /// <summary>Compares every compiled rock-list mechanics word and byte boundary with the cartridge oracle.</summary>
    internal static void VerifyKraidRockMechanicsMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidSmallMechanics), () => VerifyKraidSmallMechanics(rom,
            [0x9c7d, 0x9c81, 0x9c83, 0x9c87, 0x9c89, 0x9c8b, 0x9c8d, 0x9c91, 0x9c95, 0x9c99, 0x9c9d, 0x9ca1],
            KraidRockProjectileInstructionProgramDefinitions.MechanicsWordCount,
            index => { var word = KraidRockProjectileInstructionProgramDefinitions.MechanicsWord(index); return (word.Address, word.Value); },
            KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord,
            KraidRockProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte, bank: 0x86));

    /// <summary>Checks that all seven rock spritemap operands stay separate from compiled mechanics words.</summary>
    internal static void VerifyKraidRockPresentationMapping() =>
        Suite(nameof(VerifyKraidSmallPresentation), () => VerifyKraidSmallPresentation([0x9c7f, 0x9c85, 0x9c8f, 0x9c93, 0x9c97, 0x9c9b, 0x9c9f],
            KraidRockProjectileInstructionProgramDefinitions.PresentationWordCount,
            KraidRockProjectileInstructionProgramDefinitions.PresentationWordAddress,
            KraidRockProjectileInstructionProgramDefinitions.ReadMechanicsWord));
}
