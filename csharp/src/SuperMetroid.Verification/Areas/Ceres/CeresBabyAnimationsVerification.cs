using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;
using static SuperMetroid.Verification.Areas.Ceres.CeresBabyNativeListAddresses;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// #1290: the Ceres Baby's typed cutscene programs, each matched step by step to its private bank-$A6 draw list: every
/// pose's duration and spritemap, every cue's branch target and palette row, and the list's own goto.
/// </summary>
internal static class CeresBabyAnimationsVerification
{
    /// <summary>Both programs match their native lists.</summary>
    internal static void VerifyCeresBabyAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CeresBabyProgram>().Length, Entries.Length, "every Ceres Baby program has a native entry");
        int steps = 0;
        foreach ((CeresBabyProgram program, ushort entry) in Entries)
            steps += Compare(cartridge, program, entry);
        Console.WriteLine($"  Ceres Baby programs: {Entries.Length} programs ({steps} steps) match bank $A6.");
    }

    /// <summary>Compares one program with its native list and returns its step count.</summary>
    private static int Compare(IImportCartridgeSource cartridge, CeresBabyProgram program, ushort entry)
    {
        var steps = CeresBabyAnimations.Programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        while (true)
        {
            string at = $"Ceres Baby {program} at $A6:{cursor:X4}";
            if (cursor != entry && Entries.Any(item => item.Entry == cursor))
            {
                var fall = steps[index++];
                var fallInto = AssertIs<AnimationProgramStep<CeresBabyProgram, CeresBabyPose, CeresBabyCue>.FallIntoStep>(fall, $"{at}: runs on into the next program");
                AssertEqual(Entries.Single(item => item.Entry == cursor).Program, fallInto.Target, $"{at}: fall-through target");
                break;
            }
            int address = (Bank << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            AssertTrue(index < steps.Count, $"{at}: has a step");
            var step = steps[index++];
            cursor = unchecked((ushort)(cursor + 4));
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = AssertIs<AnimationProgramStep<CeresBabyProgram, CeresBabyPose, CeresBabyCue>.ShowStep>(step, $"{at}: pose");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(operand, CeresBabyFrameDefinitionsConstants.Spritemap(show.Frame), $"{at}: spritemap");
                continue;
            }
            AssertTrue(Instructions.TryGetValue(word, out Type? cueType), $"{at}: native instruction ${word:X4} is known");
            if (cueType is null)
            {
                var goTo = AssertIs<AnimationProgramStep<CeresBabyProgram, CeresBabyPose, CeresBabyCue>.GoToStep>(step, $"{at}: goto");
                AssertEqual(operand, Entries.Single(item => item.Program == goTo.Target).Entry, $"{at}: goto target");
                break;
            }
            var cueStep = AssertIs<AnimationProgramStep<CeresBabyProgram, CeresBabyPose, CeresBabyCue>.CueStep>(step, $"{at}: instruction");
            AssertEqual(cueType, cueStep.Cue.GetType(), $"{at}: cue");
            switch (cueStep.Cue)
            {
                case CeresBabyCue.UpdateColors updateColors:
                    AssertEqual(operand, PaletteRows.Single(item => item.Row == updateColors.PaletteRow).Operand, $"{at}: palette row");
                    break;
                case CeresBabyCue.CryOrGoTo cryOrGoTo:
                    AssertEqual(operand, Entries.Single(item => item.Program == cryOrGoTo.Target).Entry, $"{at}: branch target");
                    break;
                case CeresBabyCue.GoToIfMoving goToIfMoving:
                    AssertEqual(operand, Entries.Single(item => item.Program == goToIfMoving.Target).Entry, $"{at}: branch target");
                    break;
                default:
                    throw new InvalidOperationException($"{at}: undefined Ceres Baby cue {cueStep.Cue}.");
            }
        }
        AssertEqual(steps.Count, index, $"Ceres Baby {program} ends where its native list does");
        return steps.Count;
    }
}
