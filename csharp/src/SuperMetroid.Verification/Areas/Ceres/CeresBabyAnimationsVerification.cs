using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Fx;
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
                AssertEqual(AnimationProgramStepKind.FallInto, fall.Kind, $"{at}: runs on into the next program");
                AssertEqual(Entries.Single(item => item.Entry == cursor).Program, fall.Target, $"{at}: fall-through target");
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
                AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: pose");
                AssertEqual((int)word, step.Duration, $"{at}: duration");
                AssertEqual(operand, CeresBabyFrameDefinitions.Spritemap(step.Frame), $"{at}: spritemap");
                continue;
            }
            AssertTrue(Instructions.TryGetValue(word, out CeresBabyCueKind? kind), $"{at}: native instruction ${word:X4} is known");
            if (kind is not { } cueKind)
            {
                AssertEqual(AnimationProgramStepKind.GoTo, step.Kind, $"{at}: goto");
                AssertEqual(operand, Entries.Single(item => item.Program == step.Target).Entry, $"{at}: goto target");
                break;
            }
            AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: instruction");
            AssertEqual(cueKind, step.Cue.Kind, $"{at}: cue");
            if (cueKind == CeresBabyCueKind.UpdateColors)
                AssertEqual(operand, PaletteRows.Single(item => item.Row == step.Cue.PaletteRow).Operand, $"{at}: palette row");
            else
                AssertEqual(operand, Entries.Single(item => item.Program == step.Cue.Target).Entry, $"{at}: branch target");
        }
        AssertEqual(steps.Count, index, $"Ceres Baby {program} ends where its native list does");
        return steps.Count;
    }
}
