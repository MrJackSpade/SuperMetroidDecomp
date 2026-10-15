using static SuperMetroid.Verification.Areas.Ceres.CeresSteamNativeListAddresses;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>#1290: the Ceres steam vents' typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class CeresSteamAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyCeresSteamAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, CeresSteamAnimations.Programs,
            CeresSteamFrameDefinitions.Spritemap,
            word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
            (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'T' => NativeEnemyProgramOracle.EntryOf(Entries, cue.Target),
                    'A' => NativeEnemyProgramOracle.EntryOf(Entries, cue.Alternate),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            });
        Console.WriteLine($"  Ceres steam programs: {Entries.Length} programs ({steps} steps) match bank $A6.");
    }
}
