using System.Globalization;
using SuperMetroid.AssetExtraction.Cartridge;
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
    /// The step at every cue-value place of every program, by pose with a record, then by frame in the program's own
    /// numbering: every frame of the program's span, from its start to the next program's, that is no step of its own. The
    /// last program laid out holds no frame past its own steps.
    /// </summary>
    internal static Dictionary<string, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>> CueValueSteps(IImportCartridgeSource cartridge)
    {
        SamusAnimationProgramLayout.LaidOutProgram[] laidOut = SamusAnimationProgramLayout.Read(cartridge);
        var byProgram = new Dictionary<SamusPoseAnimation, IReadOnlyDictionary<string, SamusOverreadDocument.CueValueStep>>();
        for (int index = 0; index < laidOut.Length; index++)
        {
            SamusAnimationProgramLayout.LaidOutProgram program = laidOut[index];
            int span = index + 1 < laidOut.Length ? laidOut[index + 1].Start - program.Start : program.Program.FrameCount;
            var steps = new Dictionary<string, SamusOverreadDocument.CueValueStep>(StringComparer.Ordinal);
            for (int frame = 0; frame < span; frame++)
            {
                if (program.Program.TryStepAt(frame) is null && Decode(cartridge, program.Start, frame) is { } step)
                    steps.Add(frame.ToString(CultureInfo.InvariantCulture), step);
            }
            byProgram.Add(program.Program, steps);
        }
        return SamusAnimationProgramLayout.PosesWithRecords()
            .ToDictionary(pose => pose.ToString(), pose => byProgram[SamusAnimationDelays.AnimationForPose(pose)]);
    }

    /// <summary>
    /// The step the animation reads at <paramref name="frame"/> of the program starting at <paramref name="start"/>, as the
    /// game's handler for its byte does; null when the step would publish a record no gameplay path gives Samus, which no
    /// pose names.
    /// </summary>
    private static SamusOverreadDocument.CueValueStep? Decode(IImportCartridgeSource cartridge, int start, int frame)
    {
        int place = start + frame;
        int Byte(int offset) => cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(SamusOverreadSourceDefinitions.SamusAnimationProgramBank, place + offset));
        int value = Byte(0);
        if (value < SamusOverreadSourceDefinitions.FirstAnimationInstructionByte)
            return new(Kind.Delay.ToString(), value, null, null, null);
        string[]? Poses(params int[] offsets) =>
            offsets.All(offset => SamusPoseNumbering.NamesPose(Byte(offset)))
                ? [.. offsets.Select(offset => SamusPoseNumbering.FromNumber(Byte(offset)).ToString())]
                : null;
        string[] Items() => ItemNames(SamusEquipmentWordCodec.FromSavedEquipmentWord(
            cartridge.ReadCartridgeWord(SnesAddressMath.AddWithinBank(SamusOverreadSourceDefinitions.SamusAnimationProgramBank, place + 1))));
        SamusOverreadDocument.CueValueStep? Transition(Kind kind, string[]? items, string[]? poses) =>
            poses is null ? null : new(kind.ToString(), null, null, items, poses);
        return (value & 0xf) switch
        {
            // The first six handlers do nothing and leave the timer run out, so the next tick moves on.
            <= 5 => new(Kind.Hold.ToString(), null, null, null, null),
            6 => new(Kind.LoopUnlessLowEnergy.ToString(), null, null, null, null),
            7 => new(Kind.InstallDrainedFall.ToString(), null, null, null, null),
            8 => Transition(Kind.TurnTransition, null, Poses(1)),
            // An operand word's bits that name no item test nothing: the equipped items hold only item bits.
            9 => Transition(Kind.ItemAirborneTransition, Items(), Poses(3, 4, 5, 6)),
            10 => Transition(Kind.VerticalSpeedTransition, null, Poses(1, 2)),
            11 => new(Kind.WallJumpSelect.ToString(), null, [frame + 1, frame + 11, frame + 21], null, null),
            12 => Transition(Kind.ItemTransition, Items(), Poses(3, 4)),
            13 => Transition(Kind.Transition, null, Poses(1)),
            14 => new(Kind.Repeat.ToString(), null, [frame - Byte(1)], null, null),
            _ => new(Kind.Restart.ToString(), null, null, null, null),
        };
    }

    /// <summary>The names of every item in <paramref name="items"/>.</summary>
    private static string[] ItemNames(SamusEquipmentFlags items) =>
        [.. Enum.GetValues<SamusEquipmentFlags>().Where(item => item != SamusEquipmentFlags.None && items.HasFlag(item)).Select(item => item.ToString())];
}
