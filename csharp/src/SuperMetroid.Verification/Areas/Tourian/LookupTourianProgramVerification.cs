using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianArtworkVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianDescriptorsVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Animation;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<
    SuperMetroid.Core.Fx.Artwork.TourianStatueBlockPicture,
    SuperMetroid.Core.Areas.Tourian.TourianStatueBlockCue>;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// The ROM oracle for the Tourian statues' typed animated-tile animations (#1600): walks each statue's native bank-$87
/// list beside its typed segments and requires every frame's duration and picture, every branch target and every
/// instruction's operand to match, and every animation to follow the list's jumps, runs on and deletes.
/// </summary>
internal static class LookupTourianProgramVerification
{
    /// <summary>
    /// The colour index the cartridge's clear and grey instructions name for each statue look (#1512): the first colour
    /// of the three eye colours blacked out, or of the eight colours greyed, among the 256 colours, sprite rows from 128.
    /// </summary>
    private static readonly Dictionary<PictureShade, int> NativeColourIndexes = new()
    {
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.PhantoonCleared] = 128 + 2 * 16 + 12,
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.PhantoonGrey] = 128 + 2 * 16,
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.RidleyCleared] = 128 + 1 * 16 + 9,
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.RidleyGrey] = 128 + 1 * 16,
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.KraidCleared] = 7 * 16 + 12,
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.KraidGrey] = 7 * 16,
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.DraygonCleared] = 6 * 16 + 9,
        [SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueShades.DraygonGrey] = 6 * 16,
    };

    /// <summary>Compares every statue's typed segments and animations with its native list.</summary>
    internal static void VerifyTourianStatueProgramMappings(ISnesAddressSpace rom)
    {
        int beats = 0;
        foreach (ushort header in OriginalTourianObjects)
            beats += CompareTourianStatuePrograms(rom, header, Resolve(header));
        Console.WriteLine($"  Tourian statue animations: {OriginalTourianObjects.Length} statues ({beats} beats) match bank $87.");
    }

    /// <summary>
    /// Decodes one statue's native list into its expected segments and their control flow, compares them with the typed
    /// segments and animations, and returns the beat count.
    /// </summary>
    private static int CompareTourianStatuePrograms(
        ISnesAddressSpace rom, ushort header, TourianStatueAnimationDefinition definition)
    {
        var instructions = OriginalTourianInstructions(rom, header).ToList();
        Dictionary<ushort, TourianStatueBlockSegment> labels = ProgramLabels(rom, instructions);
        var expected = new Dictionary<TourianStatueBlockSegment, List<Beat>>();
        // The list each segment's list continues at when it jumps or runs on, or null where it deletes.
        var continues = new Dictionary<TourianStatueBlockSegment, TourianStatueBlockSegment?>();
        List<Beat>? current = null;
        TourianStatueBlockSegment currentLabel = default;
        int frames = 0;
        foreach (var instruction in instructions)
        {
            string at = $"{definition.Statue} statue at $87:{instruction.Cursor:X4}";
            if (labels.TryGetValue(instruction.Cursor, out TourianStatueBlockSegment label))
            {
                // A list that has not jumped or deleted runs on into the next authored one.
                if (current is not null)
                    continues.Add(currentLabel, label);
                current = [];
                currentLabel = label;
                expected.Add(label, current);
            }
            List<Beat> steps = current ?? throw new InvalidOperationException($"{at}: instruction after a program end has no label.");
            ushort operand = Word(rom, instruction.Cursor + 2);
            if (instruction.Width == 4 && !Enum.IsDefined((AnimatedTileInstruction)instruction.Code))
            {
                steps.Add(new Beat.Frame(Picture(definition, operand, at), instruction.Code));
                frames++;
                continue;
            }
            switch ((AnimatedTileInstruction)instruction.Code)
            {
                case AnimatedTileInstruction.SetTourianStatueAnimationState:
                {
                    TourianStatueNativeWords.AnimationOperand flags = TourianStatueNativeWords.AnimationState(operand, at);
                    if (flags == new TourianStatueNativeWords.AnimationOperand(TourianGreyStatues.None, AllReleased: false, ReleaseInProgress: true))
                        steps.Add(Cue(TourianStatueBlockCueKind.ClaimRelease));
                    else
                    {
                        AssertEqual(new TourianStatueNativeWords.AnimationOperand(definition.Statue, AllReleased: false, ReleaseInProgress: false), flags,
                            $"{at}: statue animating flag");
                        steps.Add(Cue(TourianStatueBlockCueKind.MarkAnimating));
                    }
                    break;
                }
                case AnimatedTileInstruction.ResetTourianStatueAnimationState:
                {
                    TourianStatueNativeWords.AnimationOperand flags = TourianStatueNativeWords.AnimationState(operand, at);
                    if (flags.ReleaseInProgress)
                    {
                        AssertEqual(new TourianStatueNativeWords.AnimationOperand(definition.Statue, AllReleased: false, ReleaseInProgress: true), flags,
                            $"{at}: release finish clears the turn with the statue's flag");
                        steps.Add(Cue(TourianStatueBlockCueKind.FinishRelease));
                    }
                    else
                    {
                        AssertEqual(new TourianStatueNativeWords.AnimationOperand(definition.Statue, AllReleased: false, ReleaseInProgress: false), flags,
                            $"{at}: statue animating flag");
                        steps.Add(Cue(TourianStatueBlockCueKind.ClearAnimating));
                    }
                    break;
                }
                case AnimatedTileInstruction.GotoIfEventSet:
                    AssertEqual(operand, definition.GreyEvent.Number(), $"{at}: grey event");
                    steps.Add(Cue(TourianStatueBlockCueKind.GoToIfGrey,
                        Label(labels, Word(rom, instruction.Cursor + 4), at)));
                    break;
                case AnimatedTileInstruction.GotoIfAnyBossBitsSetForArea:
                    AssertEqual(definition.BossArea, (AreaId)LittleEndianBytes.HighByte(operand), $"{at}: boss area");
                    AssertEqual(definition.DefeatedBosses, BossMaskCodec.Decode(LittleEndianBytes.LowByte(operand), at), $"{at}: boss bits");
                    steps.Add(Cue(TourianStatueBlockCueKind.GoToIfBossDefeated,
                        Label(labels, Word(rom, instruction.Cursor + 4), at)));
                    break;
                case AnimatedTileInstruction.GotoIfTourianStatueBusy:
                    steps.Add(Cue(TourianStatueBlockCueKind.GoToIfReleaseInProgress, Label(labels, operand, at)));
                    break;
                case AnimatedTileInstruction.ClearThreePaletteColors:
                    AssertEqual(NativeColourIndexes[definition.ClearedShade] * sizeof(ushort), (int)operand, $"{at}: cleared colour byte index");
                    steps.Add(Cue(TourianStatueBlockCueKind.ClearThreeColors));
                    break;
                case AnimatedTileInstruction.SpawnTourianStatueEyeGlow:
                    AssertEqual(definition.Eye, TourianStatueNativeWords.Eye(operand), $"{at}: eye-glow statue selector names the statue's eye");
                    steps.Add(Cue(TourianStatueBlockCueKind.SpawnEyeGlow));
                    break;
                case AnimatedTileInstruction.SpawnTourianStatueSoul:
                    AssertEqual(definition.Eye, TourianStatueNativeWords.Eye(operand), $"{at}: soul statue selector names the statue's eye");
                    steps.Add(Cue(TourianStatueBlockCueKind.SpawnSoul));
                    break;
                case AnimatedTileInstruction.SpawnPaletteFxObject:
                    AssertEqual(definition.PaletteFxDefinition, operand, $"{at}: grey palette effect");
                    steps.Add(Cue(TourianStatueBlockCueKind.SpawnGreyColourEffect));
                    break;
                case AnimatedTileInstruction.SetEvent:
                    AssertEqual(operand, definition.GreyEvent.Number(), $"{at}: grey event set");
                    steps.Add(Cue(TourianStatueBlockCueKind.SetGreyEvent));
                    break;
                case AnimatedTileInstruction.WriteEightTargetPaletteColors:
                    AssertEqual(NativeColourIndexes[definition.GreyShade] * sizeof(ushort), (int)operand, $"{at}: grey colour byte index");
                    steps.Add(Cue(TourianStatueBlockCueKind.ApplyGreyColors));
                    break;
                case AnimatedTileInstruction.Goto:
                    continues.Add(currentLabel, Label(labels, operand, at));
                    current = null;
                    break;
                case AnimatedTileInstruction.Delete:
                    continues.Add(currentLabel, null);
                    current = null;
                    break;
                case AnimatedTileInstruction.WaitUntilAreaBossIsDead:
                    throw new InvalidDataException($"{at}: statue lists do not wait for an area boss.");
                default:
                    throw new InvalidDataException($"{at}: undefined animated-tile instruction {instruction.Code:X4}.");
            }
        }
        AssertTrue(current is null, $"{definition.Statue} statue: the native list ends with a jump or a delete");
        AssertEqual(9, frames, $"{definition.Statue} statue: nine native timed frames");

        int compared = 0;
        AssertEqual(Enum.GetValues<TourianStatueBlockSegment>().Length, expected.Count,
            $"{definition.Statue} statue: every typed segment is a native list entry");
        AnimationSegments<TourianStatueBlockSegment, TourianStatueBlockPicture, TourianStatueBlockCue> segments =
            TourianStatueBlockAnimations.SegmentsFor(definition.FirstFrameDuration);
        foreach (KeyValuePair<TourianStatueBlockSegment, List<Beat>> native in expected)
        {
            IReadOnlyList<Beat> typed = segments.Beats(native.Key);
            AssertEqual(native.Value.Count, typed.Count, $"{definition.Statue} statue {native.Key}: beat count");
            for (int index = 0; index < native.Value.Count; index++)
                AssertEqual(native.Value[index], typed[index], $"{definition.Statue} statue {native.Key}: beat {index}");
            compared += native.Value.Count;
        }
        foreach (TourianStatueBlockSegment start in Enum.GetValues<TourianStatueBlockSegment>())
        {
            Animation<TourianStatueBlockSegment> typed = TourianStatueBlockAnimations.StartingWith(start);
            var phases = new List<AnimationPhase<TourianStatueBlockSegment>>();
            var entered = new Dictionary<TourianStatueBlockSegment, int> { [start] = 0 };
            TourianStatueBlockSegment segment = start;
            AnimationEnd end;
            while (true)
            {
                if (expected[segment].Count > 0)
                    phases.Add(new(segment, 0, expected[segment].Count, 1));
                if (continues[segment] is not { } next)
                {
                    end = AnimationEnd.Stops;
                    break;
                }
                if (entered.TryGetValue(next, out int loop))
                {
                    end = AnimationEnd.LoopsFrom(loop);
                    break;
                }
                entered[next] = phases.Count;
                segment = next;
            }
            AssertTrue(typed.Phases.SequenceEqual(phases), $"{definition.Statue} statue {start}: phases follow the native list");
            AssertEqual(end, typed.End, $"{definition.Statue} statue {start}: end follows the native list");
        }
        return compared;
    }

    /// <summary>
    /// Names each native list entry: the header's program pointer is <see cref="TourianStatueBlockSegment.Entry"/>, and
    /// the grey-event, boss-bit and unconditional branches enter the already-grey, release and idle programs.
    /// </summary>
    private static Dictionary<ushort, TourianStatueBlockSegment> ProgramLabels(
        ISnesAddressSpace rom, List<LookupTourianArtworkInstruction> instructions)
    {
        var labels = new Dictionary<ushort, TourianStatueBlockSegment> { [instructions[0].Cursor] = TourianStatueBlockSegment.Entry };
        foreach (var instruction in instructions)
        {
            var command = (AnimatedTileInstruction)instruction.Code;
            if (command == AnimatedTileInstruction.GotoIfEventSet)
                labels[Word(rom, instruction.Cursor + 4)] = TourianStatueBlockSegment.AlreadyGrey;
            else if (command == AnimatedTileInstruction.GotoIfAnyBossBitsSetForArea)
                labels[Word(rom, instruction.Cursor + 4)] = TourianStatueBlockSegment.Release;
            else if (command == AnimatedTileInstruction.Goto)
                labels[Word(rom, instruction.Cursor + 2)] = TourianStatueBlockSegment.Oscillate;
        }
        AssertEqual(Enum.GetValues<TourianStatueBlockSegment>().Length, labels.Count, "four distinct native statue list entries");
        return labels;
    }

    /// <summary>The program whose native list starts at <paramref name="target"/>.</summary>
    private static TourianStatueBlockSegment Label(Dictionary<ushort, TourianStatueBlockSegment> labels, ushort target, string at)
    {
        AssertTrue(labels.TryGetValue(target, out TourianStatueBlockSegment program), $"{at}: branch enters a named program");
        return program;
    }

    /// <summary>The statue picture whose artwork window a native frame operand selects.</summary>
    private static TourianStatueBlockPicture Picture(TourianStatueAnimationDefinition definition, ushort operand, string at)
    {
        int source = RoomFxNativeSources.AnimatedTilesBank | operand;
        TourianStatueBlockPicture[] matches = [.. RoomFxBlockAnimationPictures.TourianStatuePictures.Where(picture =>
            AnimatedTileImageSources.TourianStatue(definition.Statue, picture) == source)];
        AssertEqual(1, matches.Length, $"{at}: frame source selects exactly one statue picture");
        return matches[0];
    }

    /// <summary>An event beat of <paramref name="kind"/>, continuing at <paramref name="target"/> when it branches.</summary>
    private static Beat Cue(TourianStatueBlockCueKind kind, TourianStatueBlockSegment target = default) =>
        new Beat.Event(new TourianStatueBlockCue(kind, target));

    /// <summary>The bank-$87 word at <paramref name="cursor"/>.</summary>
    private static ushort Word(ISnesAddressSpace rom, int cursor) =>
        ReadVerificationWord(rom, RoomFxNativeSources.AnimatedTilesBank | cursor);
}
