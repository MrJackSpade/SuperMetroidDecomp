using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianArtworkVerification;
using static SuperMetroid.Verification.Areas.Tourian.LookupTourianDescriptorsVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.PaletteFx;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<
    SuperMetroid.Core.Areas.Tourian.TourianStatueTileProgram,
    SuperMetroid.Core.Fx.Artwork.TourianStatueTilePicture,
    SuperMetroid.Core.Areas.Tourian.TourianStatueTileCue>;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// The ROM oracle for the Tourian statues' typed animated-tile programs: walks each statue's native bank-$87 list
/// beside its typed programs and requires every frame's duration and picture, every branch target and every
/// instruction's operand to match.
/// </summary>
internal static class LookupTourianProgramVerification
{
    /// <summary>Compares every statue's typed programs with its native list.</summary>
    internal static void VerifyTourianStatueProgramMappings(ISnesAddressSpace rom)
    {
        int steps = 0;
        foreach (ushort header in OriginalTourianObjects)
            steps += CompareTourianStatuePrograms(rom, header, Resolve(header));
        Console.WriteLine($"  Tourian statue programs: {OriginalTourianObjects.Length} statues ({steps} steps) match bank $87.");
    }

    /// <summary>Decodes one statue's native list into its expected typed programs, compares them, and returns the step count.</summary>
    private static int CompareTourianStatuePrograms(
        ISnesAddressSpace rom, ushort header, TourianStatueAnimatedTileProgramDefinition definition)
    {
        var instructions = OriginalTourianInstructions(rom, header).ToList();
        Dictionary<ushort, TourianStatueTileProgram> labels = ProgramLabels(rom, instructions);
        var expected = new Dictionary<TourianStatueTileProgram, List<Step>>();
        List<Step>? current = null;
        int frames = 0;
        foreach (var instruction in instructions)
        {
            string at = $"{definition.AnimatedObject} at $87:{instruction.Cursor:X4}";
            if (labels.TryGetValue(instruction.Cursor, out TourianStatueTileProgram label))
            {
                // A program that has not jumped or stopped runs on into the next authored one.
                current?.Add(Step.FallInto(label));
                current = [];
                expected.Add(label, current);
            }
            List<Step> steps = current ?? throw new InvalidOperationException($"{at}: instruction after a program end has no label.");
            ushort operand = Word(rom, instruction.Cursor + 2);
            if (instruction.Width == 4 && !Enum.IsDefined((AnimatedTileInstruction)instruction.Code))
            {
                steps.Add(Step.Show(Picture(definition, operand, at), instruction.Code));
                frames++;
                continue;
            }
            switch ((AnimatedTileInstruction)instruction.Code)
            {
                case AnimatedTileInstruction.SetTourianStatueAnimationState:
                {
                    var flags = (TourianStatueAnimationState)operand;
                    if (flags == TourianStatueAnimationState.ReleaseInProgress)
                        steps.Add(Cue(TourianStatueTileCueKind.ClaimRelease));
                    else
                    {
                        AssertEqual(definition.AnimatingFlag, flags, $"{at}: statue animating flag");
                        steps.Add(Cue(TourianStatueTileCueKind.MarkAnimating));
                    }
                    break;
                }
                case AnimatedTileInstruction.ResetTourianStatueAnimationState:
                {
                    var flags = (TourianStatueAnimationState)operand;
                    if ((flags & TourianStatueAnimationState.ReleaseInProgress) != 0)
                    {
                        AssertEqual(TourianStatueAnimationState.ReleaseInProgress | definition.AnimatingFlag, flags,
                            $"{at}: release finish clears the turn with the statue's flag");
                        steps.Add(Cue(TourianStatueTileCueKind.FinishRelease));
                    }
                    else
                    {
                        AssertEqual(definition.AnimatingFlag, flags, $"{at}: statue animating flag");
                        steps.Add(Cue(TourianStatueTileCueKind.ClearAnimating));
                    }
                    break;
                }
                case AnimatedTileInstruction.GotoIfEventSet:
                    AssertEqual(definition.GreyEvent, (EventNumber)operand, $"{at}: grey event");
                    steps.Add(Cue(TourianStatueTileCueKind.GoToIfGrey,
                        Label(labels, Word(rom, instruction.Cursor + 4), at)));
                    break;
                case AnimatedTileInstruction.GotoIfAnyBossBitsSetForArea:
                    AssertEqual(definition.BossArea, (AreaId)SnesLittleEndian.HighByte(operand), $"{at}: boss area");
                    AssertEqual(definition.DefeatedBossBits, (BossBits)SnesLittleEndian.LowByte(operand), $"{at}: boss bits");
                    steps.Add(Cue(TourianStatueTileCueKind.GoToIfBossDefeated,
                        Label(labels, Word(rom, instruction.Cursor + 4), at)));
                    break;
                case AnimatedTileInstruction.GotoIfTourianStatueBusy:
                    steps.Add(Cue(TourianStatueTileCueKind.GoToIfReleaseInProgress, Label(labels, operand, at)));
                    break;
                case AnimatedTileInstruction.ClearThreePaletteColors:
                    AssertEqual(ColourSetRunPositions.First(definition.ClearedColours) * sizeof(ushort), (int)operand, $"{at}: cleared colour byte index");
                    steps.Add(Cue(TourianStatueTileCueKind.ClearThreeColors));
                    break;
                case AnimatedTileInstruction.SpawnTourianStatueEyeGlow:
                    AssertEqual(definition.UnlockEffectParameter, operand, $"{at}: eye-glow statue selector");
                    steps.Add(Cue(TourianStatueTileCueKind.SpawnEyeGlow));
                    break;
                case AnimatedTileInstruction.SpawnTourianStatueSoul:
                    AssertEqual(definition.UnlockEffectParameter, operand, $"{at}: soul statue selector");
                    steps.Add(Cue(TourianStatueTileCueKind.SpawnSoul));
                    break;
                case AnimatedTileInstruction.SpawnPaletteFxObject:
                    AssertEqual(definition.PaletteFxDefinition, operand, $"{at}: grey palette effect");
                    steps.Add(Cue(TourianStatueTileCueKind.SpawnGreyPaletteFx));
                    break;
                case AnimatedTileInstruction.SetEvent:
                    AssertEqual(definition.GreyEvent, (EventNumber)operand, $"{at}: grey event set");
                    steps.Add(Cue(TourianStatueTileCueKind.SetGreyEvent));
                    break;
                case AnimatedTileInstruction.WriteEightTargetPaletteColors:
                    AssertEqual(ColourSetRunPositions.First(definition.GreyColours) * sizeof(ushort), (int)operand, $"{at}: grey colour byte index");
                    steps.Add(Cue(TourianStatueTileCueKind.ApplyGreyColors));
                    break;
                case AnimatedTileInstruction.Goto:
                    steps.Add(Step.GoTo(Label(labels, operand, at)));
                    current = null;
                    break;
                case AnimatedTileInstruction.Delete:
                    steps.Add(Step.Stop);
                    current = null;
                    break;
                case AnimatedTileInstruction.WaitUntilAreaBossIsDead:
                    throw new InvalidDataException($"{at}: statue lists do not wait for an area boss.");
                default:
                    throw new InvalidDataException($"{at}: undefined animated-tile instruction {instruction.Code:X4}.");
            }
        }
        AssertTrue(current is null, $"{definition.AnimatedObject}: the native list ends with a jump or a delete");
        AssertEqual(9, frames, $"{definition.AnimatedObject}: nine native timed frames");

        int compared = 0;
        AssertEqual(Enum.GetValues<TourianStatueTileProgram>().Length, expected.Count,
            $"{definition.AnimatedObject}: every typed program is a native list entry");
        foreach ((TourianStatueTileProgram program, List<Step> native) in expected)
        {
            IReadOnlyList<Step> typed = definition.Program.Steps(program);
            AssertEqual(native.Count, typed.Count, $"{definition.AnimatedObject} {program}: step count");
            for (int index = 0; index < native.Count; index++)
                AssertEqual(native[index], typed[index], $"{definition.AnimatedObject} {program}: step {index}");
            compared += native.Count;
        }
        return compared;
    }

    /// <summary>
    /// Names each native list entry: the header's program pointer is <see cref="TourianStatueTileProgram.Entry"/>, and
    /// the grey-event, boss-bit and unconditional branches enter the already-grey, release and idle programs.
    /// </summary>
    private static Dictionary<ushort, TourianStatueTileProgram> ProgramLabels(
        ISnesAddressSpace rom, List<LookupTourianArtworkInstruction> instructions)
    {
        var labels = new Dictionary<ushort, TourianStatueTileProgram> { [instructions[0].Cursor] = TourianStatueTileProgram.Entry };
        foreach (var instruction in instructions)
        {
            var command = (AnimatedTileInstruction)instruction.Code;
            if (command == AnimatedTileInstruction.GotoIfEventSet)
                labels[Word(rom, instruction.Cursor + 4)] = TourianStatueTileProgram.AlreadyGrey;
            else if (command == AnimatedTileInstruction.GotoIfAnyBossBitsSetForArea)
                labels[Word(rom, instruction.Cursor + 4)] = TourianStatueTileProgram.Release;
            else if (command == AnimatedTileInstruction.Goto)
                labels[Word(rom, instruction.Cursor + 2)] = TourianStatueTileProgram.Oscillate;
        }
        AssertEqual(Enum.GetValues<TourianStatueTileProgram>().Length, labels.Count, "four distinct native statue list entries");
        return labels;
    }

    /// <summary>The program whose native list starts at <paramref name="target"/>.</summary>
    private static TourianStatueTileProgram Label(Dictionary<ushort, TourianStatueTileProgram> labels, ushort target, string at)
    {
        AssertTrue(labels.TryGetValue(target, out TourianStatueTileProgram program), $"{at}: branch enters a named program");
        return program;
    }

    /// <summary>The statue picture whose artwork window a native frame operand selects.</summary>
    private static TourianStatueTilePicture Picture(TourianStatueAnimatedTileProgramDefinition definition, ushort operand, string at)
    {
        int source = RoomFxNativeSources.AnimatedTilesBank | operand;
        TourianStatueTilePicture[] matches = [.. TourianStatueTileStripDefinitions.Pictures.Where(picture =>
            AnimatedTileImageSources.TourianStatue(definition.AnimatedObject, picture) == source)];
        AssertEqual(1, matches.Length, $"{at}: frame source selects exactly one statue picture");
        return matches[0];
    }

    /// <summary>A cue step of <paramref name="kind"/>, continuing at <paramref name="target"/> when it branches.</summary>
    private static Step Cue(TourianStatueTileCueKind kind, TourianStatueTileProgram target = default) =>
        Step.Publish(new TourianStatueTileCue(kind, target));

    /// <summary>The bank-$87 word at <paramref name="cursor"/>.</summary>
    private static ushort Word(ISnesAddressSpace rom, int cursor) =>
        ReadVerificationWord(rom, RoomFxNativeSources.AnimatedTilesBank | cursor);
}
