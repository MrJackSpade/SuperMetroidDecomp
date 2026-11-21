using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Enemies.Magdollite.MagdolliteNativeListAddresses;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>
/// #641, #1600: the Magdollite's typed animations, its head's, body's and overlay's, each segment matched to its native
/// bank-$A8 list and each animation to the lists' control flow.
/// </summary>
internal static class MagdolliteAnimationsVerification
{
    /// <summary>Every segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyMagdolliteAnimations(CartridgeImportAddressSpace rom)
    {
        int headBeats = NativeEnemyAnimationOracle.CompareAll(rom, Bank, HeadEntries,
            MagdolliteHeadAnimations.Segments, MagdolliteHeadAnimations.StartingAt, MagdolliteFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<MagdolliteHeadCue>(
                word => HeadInstructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    if (word == EndsPartWait)
                    {
                        // The routine ending a part's wait finishes the animation whose list it ends (#2439).
                        MagdolliteHeadCueKind finishing = at.Contains("PillarRaised", StringComparison.Ordinal) ? MagdolliteHeadCueKind.FinishSubmerge
                            : at.Contains("Surfacing", StringComparison.Ordinal) ? MagdolliteHeadCueKind.FinishEmerge
                            : throw new InvalidOperationException($"{at}: the head waits for no such list's animation.");
                        AssertEqual<MagdolliteHeadCue>(new MagdolliteHeadCue.Signal(finishing), cue, $"{at}: cue");
                        return;
                    }
                    MagdolliteNativeListAddressesInstruction<MagdolliteHeadCue> instruction = HeadInstructions[word];
                    AssertEqual(instruction.Cue ?? throw new InvalidOperationException($"{at}: the routine names no cue."), cue, $"{at}: cue");
                },
                Omitted: (word, _) => OmittedInstructions.Contains(word)),
            // The head is free again, or surfaces, only after its pillar's counted rise or descent (#2337).
            onlyRunOnInto: new HashSet<MagdolliteHeadSegment>
            {
                MagdolliteHeadSegment.LeftPillarRaised, MagdolliteHeadSegment.LeftSurfacing,
                MagdolliteHeadSegment.RightPillarRaised, MagdolliteHeadSegment.RightSurfacing,
            });
        int bodyBeats = NativeEnemyAnimationOracle.CompareAll(rom, Bank, BodyEntries,
            MagdolliteBodyAnimations.Segments, MagdolliteBodyAnimations.StartingAt, MagdolliteFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<NoAnimationCue>(
                _ => null,
                (_, _, _, at) => throw new InvalidOperationException($"{at}: the body's lists call no routine.")));
        int overlayBeats = NativeEnemyAnimationOracle.CompareAll(rom, Bank, OverlayEntries,
            MagdolliteOverlayAnimations.Segments, MagdolliteOverlayAnimations.StartingAt, MagdolliteFrameToolingDefinitions.Spritemap,
            new NativeEnemyCueRules<MagdolliteOverlayCue>(
                word => OverlayInstructions.TryGetValue(word, out var instruction) ? instruction.Operands.Length : null,
                (word, operands, cue, at) =>
                {
                    if (word == EndsPartWait)
                    {
                        // The routine ending a part's wait finishes the throw whose list it ends (#2439).
                        if (!at.Contains("ThrowFinish", StringComparison.Ordinal))
                            throw new InvalidOperationException($"{at}: the overlay waits for no such list's animation.");
                        AssertEqual<MagdolliteOverlayCue>(new MagdolliteOverlayCue.Signal(MagdolliteOverlayCueKind.FinishThrow), cue, $"{at}: cue");
                        return;
                    }
                    MagdolliteNativeListAddressesInstruction<MagdolliteOverlayCue> instruction = OverlayInstructions[word];
                    // The sound routine's cue is its operand decoded to the cue's sound; every other routine's cue is fixed.
                    MagdolliteOverlayCue expected = instruction.Operands switch
                    {
                        "V" => new MagdolliteOverlayCue.PlaySoundIfOnScreen(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[0])),
                        "" => instruction.Cue ?? throw new InvalidOperationException($"{at}: the routine names no cue."),
                        _ => throw new InvalidOperationException($"Undefined operand roles {instruction.Operands}."),
                    };
                    AssertEqual(expected, cue, $"{at}: cue");
                },
                Omitted: (word, _) => OmittedInstructions.Contains(word)),
            // A throw finishes only after its counted flings (#2439).
            onlyRunOnInto: new HashSet<MagdolliteOverlaySegment>
            {
                MagdolliteOverlaySegment.LeftLavaFling, MagdolliteOverlaySegment.LeftThrowFinish,
                MagdolliteOverlaySegment.RightLavaFling, MagdolliteOverlaySegment.RightThrowFinish,
            });
        int segments = HeadEntries.Length + BodyEntries.Length + OverlayEntries.Length;
        int animations = MagdolliteHeadAnimations.StartingAt.Count + MagdolliteBodyAnimations.StartingAt.Count +
            MagdolliteOverlayAnimations.StartingAt.Count;
        Console.WriteLine($"  Magdollite animations: {segments} segments ({headBeats + bodyBeats + overlayBeats} beats) and {animations} animations match bank $A8.");
    }
}
