using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;
using Kind = SuperMetroid.AssetExtraction.Samus.SamusOverreadDocument.CueValueStepKind;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Decodes what Samus's animation reads at each place of a program that lands on a value one of its cues carries (#1535).
/// The game reads its animation frame as a byte of the program: a frame on a cue value's place reads that value as a step of
/// its own, a delay below the first instruction byte and otherwise the instruction its low four bits select, with that
/// instruction's operands taken from the bytes after it. The Baby's Hyper Beam freeze and rainbow stop set frames that land
/// on such places in the programs laid out after a transition's.
/// </summary>
[SuperMetroid.Core.EncodingOwner("Samus animation program bytes: a delay byte, or an instruction whose low four bits select its handler")]
internal static class SamusAnimationCueValueDecoding
{
    /// <summary>
    /// A cue value's step as the import decodes it, the frames it names counted in the game's layout of its program; the
    /// layout names those frames before anything leaves the import (<see cref="SamusAnimationLayout"/>).
    /// </summary>
    /// <param name="Kind">What the step does.</param>
    /// <param name="Ticks">A delay's ticks; null for any other kind.</param>
    /// <param name="Frames">The frames the step selects, counted from its program's start; null for a step that selects none.</param>
    /// <param name="Items">The names of the items a transition tests; null for any other kind.</param>
    /// <param name="Poses">The names of the poses a transition publishes; null for any other kind.</param>
    internal sealed record DecodedStep(Kind Kind, int? Ticks, IReadOnlyList<int>? Frames, IReadOnlyList<string>? Items, IReadOnlyList<string>? Poses);

    /// <summary>
    /// The step at every cue-value place of every program, by program, then by frame counted from the program's start: every
    /// frame of the program's span, from its start to the next program's, that is no step of its own. The last program laid
    /// out holds no frame past its own steps.
    /// </summary>
    internal static Dictionary<SamusPoseAnimation, IReadOnlyDictionary<int, DecodedStep>> CueValueSteps(IImportCartridgeSource cartridge,
        IReadOnlyList<SamusAnimationProgramLayout.LaidOutProgram> laidOut)
    {
        var byProgram = new Dictionary<SamusPoseAnimation, IReadOnlyDictionary<int, DecodedStep>>();
        for (int index = 0; index < laidOut.Count; index++)
        {
            SamusAnimationProgramLayout.LaidOutProgram program = laidOut[index];
            int span = index + 1 < laidOut.Count ? laidOut[index + 1].Start - program.Start : SamusAnimationLayout.StepCount(program.Program);
            var steps = new Dictionary<int, DecodedStep>();
            for (int frame = 0; frame < span; frame++)
            {
                if (SamusAnimationLayout.OwnStepAt(program.Program, frame) is null && Decode(cartridge, program.Start, frame) is { } step)
                    steps.Add(frame, step);
            }
            byProgram.Add(program.Program, steps);
        }
        return byProgram;
    }

    /// <summary>
    /// The step the animation reads at <paramref name="frame"/> of the program starting at <paramref name="start"/>, as the
    /// game's handler for its byte does; null when the step would publish a record no gameplay path gives Samus, which no
    /// pose names.
    /// </summary>
    private static DecodedStep? Decode(IImportCartridgeSource cartridge, int start, int frame)
    {
        int place = start + frame;
        int Byte(int offset) => cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(SamusOverreadSourceDefinitions.SamusAnimationProgramBank, place + offset));
        int value = Byte(0);
        if (value < SamusOverreadSourceDefinitions.FirstAnimationInstructionByte)
            return new(Kind.Delay, value, null, null, null);
        string[]? Poses(params int[] offsets) =>
            offsets.All(offset => SamusPoseNumbering.NamesPose(Byte(offset)))
                ? [.. offsets.Select(offset => SamusPoseIdKeys.Installed.Of(SamusPoseNumbering.FromNumber(Byte(offset))))]
                : null;
        string[] Items() => ItemNames(SamusEquipmentWordCodec.FromSavedEquipmentWord(
            cartridge.ReadCartridgeWord(SnesAddressMath.AddWithinBank(SamusOverreadSourceDefinitions.SamusAnimationProgramBank, place + 1))));
        DecodedStep? Transition(Kind kind, string[]? items, string[]? poses) =>
            poses is null ? null : new(kind, null, null, items, poses);
        return (value & 0xf) switch
        {
            // The first six handlers do nothing and leave the timer run out, so the next tick moves on.
            <= 5 => new(Kind.Hold, null, null, null, null),
            6 => new(Kind.LoopUnlessLowEnergy, null, null, null, null),
            7 => new(Kind.InstallDrainedFall, null, null, null, null),
            8 => Transition(Kind.TurnTransition, null, Poses(1)),
            // An operand word's bits that name no item test nothing: the equipped items hold only item bits.
            9 => Transition(Kind.ItemAirborneTransition, Items(), Poses(3, 4, 5, 6)),
            10 => Transition(Kind.VerticalSpeedTransition, null, Poses(1, 2)),
            11 => new(Kind.WallJumpSelect, null, [frame + 1, frame + 11, frame + 21], null, null),
            12 => Transition(Kind.ItemTransition, Items(), Poses(3, 4)),
            13 => Transition(Kind.Transition, null, Poses(1)),
            14 => new(Kind.Repeat, null, [frame - Byte(1)], null, null),
            _ => new(Kind.Restart, null, null, null, null),
        };
    }

    /// <summary>
    /// How many frames of a program the operands of <paramref name="cue"/>'s instruction take after its own byte (#2395): a
    /// transition's poses, an item transition's item word, which takes two, and a repeat's step back. The game reads each
    /// of them as a frame of its own when a frame lands there.
    /// </summary>
    /// <exception cref="InvalidDataException">The cue is no instruction a program holds.</exception>
    internal static int OperandFrames(SamusAnimationCue cue) => cue switch
    {
        SamusAnimationHold or SamusAnimationLoopUnlessLowEnergy or SamusAnimationInstallDrainedFall or SamusAnimationWallJumpSelect or
            SamusAnimationRestart => 0,
        SamusAnimationTurnTransition or SamusAnimationTransition or SamusAnimationRepeat => 1,
        SamusAnimationVerticalSpeedTransition => 2,
        SamusAnimationItemTransition => 4,
        SamusAnimationItemAirborneTransition => 6,
        _ => throw new InvalidDataException($"Samus animation cue {cue} is no instruction a program holds."),
    };

    /// <summary>The names of every item in <paramref name="items"/>.</summary>
    [InstalledKey]
    private static string[] ItemNames(SamusEquipmentFlags items) =>
        [.. SamusEquipmentFlagsKeys.ListedOrder.Where(item => items.HasFlag(item)).Select(SamusEquipmentFlagsKeys.Installed.Of)];
}
