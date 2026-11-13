using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Areas.Ceres.CeresBabyNativeListAddresses;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// #1290, #1600: the Ceres Baby's typed cutscene animations, each segment matched beat by beat to its private bank-$A6
/// draw list (every pose's duration and spritemap, every cue's branch target and shade) and each animation to the lists'
/// control flow: the resting list's run on into the expressive list and that list's own goto.
/// </summary>
internal static class CeresBabyAnimationsVerification
{
    /// <summary>Both segments match their native lists, and both animations the lists' control flow.</summary>
    internal static void VerifyCeresBabyAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CeresBabySegment>().Length, Entries.Length, "every Ceres Baby segment has a native entry");
        Dictionary<CeresBabySegment, NativeEnemyList<CeresBabySegment>> lists =
            Entries.ToDictionary(item => item.Program, item => Read(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, CeresBabyAnimations.StartingAt);
        Console.WriteLine($"  Ceres Baby animations: {Entries.Length} segments ({lists.Values.Sum(list => list.BeatAddresses.Length)} beats) match bank $A6.");
    }

    /// <summary>Reads one segment's native list, comparing each of its beats with the segment's.</summary>
    private static NativeEnemyList<CeresBabySegment> Read(IImportCartridgeSource cartridge, CeresBabySegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<CeresBabyPose, CeresBabyCue>> beats = CeresBabyAnimations.Segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<CeresBabySegment>>();
        ushort cursor = entry;
        while (true)
        {
            int beat = addresses.Count;
            string at = $"Ceres Baby {segment} at $A6:{cursor:X4}";
            if (cursor != entry && Entries.Any(item => item.Entry == cursor))
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, Entries.Single(item => item.Entry == cursor).Program, null, cursor));
                break;
            }
            int address = (Bank << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = AssertIs<AnimationBeat<CeresBabyPose, CeresBabyCue>.Frame>(Beat(beat, at), $"{at}: pose");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(operand, CeresBabyFrameDefinitionsConstants.Spritemap(show.Shown), $"{at}: spritemap");
                addresses.Add(cursor);
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            AssertTrue(Instructions.TryGetValue(word, out Type? cueType), $"{at}: native instruction ${word:X4} is known");
            if (cueType is null)
            {
                controls.Add(new(NativeEnemyControlKind.Goto, beat, Entries.Single(item => item.Entry == operand).Program, null, cursor));
                break;
            }
            var raised = AssertIs<AnimationBeat<CeresBabyPose, CeresBabyCue>.Event>(Beat(beat, at), $"{at}: instruction");
            AssertEqual(cueType, raised.Raised.GetType(), $"{at}: cue");
            switch (raised.Raised)
            {
                case CeresBabyCue.UpdateColors updateColors:
                    AssertEqual(operand, ShadeOperands.Single(item => item.Shade == updateColors.Shade).Operand, $"{at}: shade");
                    break;
                case CeresBabyCue.CryOrGoTo cryOrGoTo:
                    AssertEqual(operand, Entries.Single(item => item.Program == cryOrGoTo.Target).Entry, $"{at}: branch target");
                    break;
                case CeresBabyCue.GoToIfMoving goToIfMoving:
                    AssertEqual(operand, Entries.Single(item => item.Program == goToIfMoving.Target).Entry, $"{at}: branch target");
                    break;
                default:
                    throw new InvalidOperationException($"{at}: undefined Ceres Baby cue {raised.Raised}.");
            }
            addresses.Add(cursor);
            cursor = unchecked((ushort)(cursor + 4));
        }
        AssertEqual(beats.Count, addresses.Count, $"Ceres Baby {segment} ends where its native list does");
        return new NativeEnemyList<CeresBabySegment>([.. addresses], [.. controls]);

        AnimationBeat<CeresBabyPose, CeresBabyCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Ceres Baby {segment}.");
    }
}
