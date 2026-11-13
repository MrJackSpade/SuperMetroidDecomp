using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Areas.Ceres.CeresSteamNativeListAddresses;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>#1290: the Ceres steam vents' typed programs, each matched step by step to its native bank-$A6 list.</summary>
internal static class CeresSteamAnimationsVerification
{
    /// <summary>Every segment matches its native list and every animation the lists' control flow.</summary>
    internal static void VerifyCeresSteamAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries, CeresSteamAnimations.Segments, CeresSteamAnimations.StartingAt,
            CeresSteamFrameDefinitionsConstants.Spritemap,
            new NativeEnemyCueRules<CeresSteamCue>(word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null, (word, operands, cue, at) =>
            {
                var instruction = Instructions[word];
                instruction.Check(cue, at);
                for (int index = 0; index < operands.Length; index++)
                    AssertEqual(operands[index], instruction.Operands[index] switch
                {
                    'T' => NativeEnemyAnimationOracle.EntryOf(Entries, AssertIs<CeresSteamCue.CountDownToActivation>(cue, $"{at}: cue").Target),
                    'A' => NativeEnemyAnimationOracle.EntryOf(Entries, AssertIs<CeresSteamCue.CountDownToActivation>(cue, $"{at}: cue").Alternate),
                    _ => throw new InvalidOperationException($"Undefined operand role {instruction.Operands[index]}."),
                }, $"{at}: operand {index}");
            }));
        Console.WriteLine($"  Ceres steam animations: {Entries.Length} segments ({beats} beats) match bank $A6.");
    }
}
