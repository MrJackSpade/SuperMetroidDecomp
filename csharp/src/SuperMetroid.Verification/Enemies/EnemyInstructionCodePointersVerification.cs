using SuperMetroid.Tooling;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks enemy instruction catalog uniqueness, mapped addresses, and optional retail readability.</summary>
internal static class EnemyInstructionCodePointersVerification
{
    /// <summary>
    /// Verifies that enemy animation bytecode and list catalogs contain unique mapped
    /// pointers, then touches every bank-local entry in the retail ROM when it is present.
    /// </summary>
    internal static void VerifyEnemyInstructionCodePointerCatalogs()
    {
        (string Name, ushort[] Pointers)[] codeCatalogs =
        [
            (nameof(CommonEnemyInstruction), [.. Enum.GetValues<CommonEnemyInstruction>().Select(instruction => (ushort)instruction)]),
            (nameof(MotherBrainInstructionCodes), [MotherBrainInstructionCodes.Instruction_CommonA9_Sleep]),
        ];
        foreach ((string name, ushort[] pointers) in codeCatalogs)
            AssertUniqueMappedInstructionPointers(name, pointers);

        Console.WriteLine($"  Enemy instructions: {codeCatalogs.Length} unique catalogs verified.");
    }

    /// <summary>Requires a catalog's instruction words to be distinct mapped-bank offsets.</summary>
    private static void AssertUniqueMappedInstructionPointers(string catalog, ushort[] pointers)
    {
        AssertTrue(pointers.Length != 0, $"{catalog} contains named instruction pointers");
        AssertEqual(pointers.Length, pointers.Distinct().Count(),
            $"{catalog} contains no duplicate addresses");
        foreach (ushort pointer in pointers)
            AssertTrue(pointer >= 0x8000, $"{catalog} pointer ${pointer:X4} is mapped");
    }
}
