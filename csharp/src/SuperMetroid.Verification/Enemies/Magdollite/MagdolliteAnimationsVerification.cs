using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteNativeListAddresses;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>#641, #1600: the Magdollite's typed animations, each segment matched to its native bank-$A8 list and each animation to the lists' control flow.</summary>
internal static class MagdolliteAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMagdolliteAnimations(CartridgeImportAddressSpace rom)
    {
        int beats = NativeEnemyAnimationOracle.CompareAll(CartridgeImportSource.Require(rom), Bank, Entries,
            MagdolliteAnimations.Segments, MagdolliteAnimations.StartingAt, MagdolliteFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<MagdolliteCue>(
                word => Instructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    var instruction = Instructions[word];
                    if (word == EndsPartWait)
                    {
                        // The routine ending a part's wait finishes the animation whose list it ends (#2439).
                        MagdolliteCueKind finishing = at.Contains("PillarRaised", StringComparison.Ordinal) ? MagdolliteCueKind.FinishSubmerge
                            : at.Contains("Surfacing", StringComparison.Ordinal) ? MagdolliteCueKind.FinishEmerge
                            : at.Contains("ThrowFinish", StringComparison.Ordinal) ? MagdolliteCueKind.FinishThrow
                            : throw new InvalidOperationException($"{at}: no part waits for this list's animation.");
                        AssertEqual<MagdolliteCue>(new MagdolliteCue.Signal(finishing), cue, $"{at}: cue");
                        return;
                    }
                    // The sound routine's cue is its operand decoded to the cue's sound; every other routine's cue is fixed.
                    MagdolliteCue expected = instruction.Operands switch
                    {
                        "V" => new MagdolliteCue.PlaySoundIfOnScreen(SoundEffectCommands.DecodeCartridge(SoundEffectLibrary.Enemies, operands[0])),
                        "" => instruction.Cue ?? throw new InvalidOperationException($"{at}: the routine names no cue."),
                        _ => throw new InvalidOperationException($"Undefined operand roles {instruction.Operands}."),
                    };
                    AssertEqual(expected, cue, $"{at}: cue");
                },
                Omitted: (word, _) => OmittedInstructions.Contains(word)),
            // The head is free again, or surfaces, only after its pillar's counted rise or descent (#2337), and a throw
            // finishes only after its counted flings (#2439).
            onlyRunOnInto: new HashSet<MagdolliteSegment>
            {
                MagdolliteSegment.LeftLavaFling, MagdolliteSegment.LeftThrowFinish, MagdolliteSegment.LeftPillarRaised, MagdolliteSegment.LeftSurfacing,
                MagdolliteSegment.RightLavaFling, MagdolliteSegment.RightThrowFinish, MagdolliteSegment.RightPillarRaised, MagdolliteSegment.RightSurfacing,
            });
        Console.WriteLine($"  Magdollite animations: {Entries.Length} segments ({beats} beats) and {MagdolliteAnimations.StartingAt.Count} animations match bank $A8.");
    }
}
