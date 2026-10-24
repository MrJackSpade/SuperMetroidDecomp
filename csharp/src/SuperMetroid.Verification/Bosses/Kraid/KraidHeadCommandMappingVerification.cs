using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Bosses.Kraid.KraidContourCaseMappingsVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// The ROM oracle for <see cref="KraidHeadAnimations"/>: walks the cartridge's four head lists independently and checks
/// every typed step, every cursor's mouth and growth reads, and the entry points the AI installs.
/// </summary>
internal static class KraidHeadCommandMappingVerification
{
    /// <summary>Checks poses, durations, cries and ends; the cursor-relative mouth reads; growth resumes; and entry cursors.</summary>
    internal static void VerifyKraidHeadCommandMapping(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var native = new KraidHeadNativeCursors(cartridge);
        ushort Word(int pointer) => (ushort)(KraidHeadCommandMappingRomByte(rom, KraidHeadNativeCursors.Bank | pointer) |
            (KraidHeadCommandMappingRomByte(rom, KraidHeadNativeCursors.Bank | (pointer + 1)) << 8));
        AssertEqual(28, native.Records.Count, "the native head lists hold 28 records");

        // A pose's mouth stage is its tilemap's rank among the head's distinct tilemaps: the closed mouth is the lowest.
        List<ushort> stageTilemaps = [.. native.Records.Where(record => record.IsPose)
            .Select(record => record.Tilemap).Distinct().Order()];
        AssertEqual(KraidHeadFrame.FullyOpenMouthStage + 1, stageTilemaps.Count, "one native tilemap per mouth stage");

        foreach (KraidHeadProgram program in Enum.GetValues<KraidHeadProgram>())
        {
            IReadOnlyList<AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>> steps =
                KraidHeadAnimations.Programs.Steps(program);
            KraidHeadNativeRecord[] records = [.. native.Records.Where(record => record.Cursor.Program == program)];
            AssertEqual(records.Length, steps.Count, $"{program} has one typed step per native record");
            for (int position = 0; position < records.Length; position++)
            {
                KraidHeadNativeRecord record = records[position];
                AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue> step = steps[position];
                string at = $"{program}[{position}] at $A7:{record.Pointer:X4}";
                if (record.IsPose)
                {
                    var show = AssertIs<AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>.ShowStep>(step, $"{at} shows a pose");
                    AssertEqual(record.Opcode, show.Duration, $"{at} pose duration");
                    AssertEqual(stageTilemaps.IndexOf(record.Tilemap), show.Frame.MouthStage, $"{at} mouth stage");
                    AssertEqual(record.Tilemap, KraidHeadFrameDefinitions.PresentationKey(show.Frame), $"{at} head image");
                    AssertEqual(record.OuterShape, (ushort)show.Frame.VulnerableShape, $"{at} vulnerable shape");
                    AssertEqual(record.InnerShape, show.Frame.InvulnerableShape is { } inner ? (ushort)inner : ushort.MaxValue,
                        $"{at} invulnerable shape");
                }
                else if (record.IsTerminator)
                    AssertTrue(step is AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>.HoldStep, $"{at} ends the program");
                else
                {
                    var cueStep = AssertIs<AnimationProgramStep<KraidHeadProgram, KraidHeadFrame, KraidHeadCue>.CueStep>(step, $"{at} cries");
                    (KraidHeadCue cue, SoundEffectId sound) = record.Opcode switch
                    {
                        KraidHeadNativeCursors.RoarCryCall => new VerifyKraidHeadCommandMappingCase(KraidHeadCue.Roar, KraidSoundDefinitions.HeadRoar),
                        KraidHeadNativeCursors.DyingCryCall => new VerifyKraidHeadCommandMappingCase(KraidHeadCue.DyingCry, KraidSoundDefinitions.HeadDyingCry),
                        _ => throw new InvalidDataException($"{at}: unknown native head command ${record.Opcode:X4}."),
                    };
                    AssertEqual(cue, cueStep.Cue, $"{at} cry");
                    AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Library2, (byte)record.Sound), sound,
                        $"{at} cry sound: the import decodes the native byte to the definition's sound");
                }

                KraidHeadCursorReads reads = KraidHeadAnimations.ReadsAt(record.Cursor);
                AssertEqual(NativeMouthRead(native, record.Pointer - 4), reads.Outer, $"{at} outer mouth read four bytes back");
                AssertEqual(NativeMouthRead(native, record.Pointer - 2), reads.Inner, $"{at} inner mouth read two bytes back");
                (ushort resumePointer, ushort resumeTimer) = NativeKraidGrowthResume(rom, Word(record.Pointer + 2));
                AssertEqual(native.CursorAt(resumePointer), KraidHeadAnimations.GrowthResumeCursor(reads.Growth),
                    $"{at} growth resume cursor");
                AssertEqual(resumeTimer, KraidHeadAnimations.GrowthResumeDelay(reads.Growth), $"{at} growth resume delay");
            }
        }

        // The AI installs each program one pose in, with the entry pose's own duration as the head delay.
        AssertEqual((ushort)0x96da, native.PointerAt(KraidHeadAnimations.RoarAfterEntryFrame), "roar entry cursor");
        AssertEqual((ushort)0x9752, native.PointerAt(KraidHeadAnimations.EyeGlowAfterEntryFrame), "eye-glow entry cursor");
        AssertEqual((ushort)0x976c, native.PointerAt(KraidHeadAnimations.DeathAfterEntryFrame), "death entry cursor");
        AssertEqual(Word(0x96d2), KraidHeadAnimations.RoarEntryDuration, "roar entry duration");
        AssertEqual(Word(0x974a), KraidHeadAnimations.EyeGlowEntryDuration, "eye-glow entry duration");
        AssertEqual(Word(0x9764), KraidHeadAnimations.DeathEntryDuration, "death entry duration");
        AssertThrows<InvalidDataException>(() => KraidHeadAnimations.ReadsAt(null), "an unstarted head has no reads");
    }

    /// <summary>
    /// What a native mouth read at <paramref name="address"/> finds: the outer shape four bytes into a pose, the inner
    /// shape six bytes in ($FFFF being the closed mouth's missing shape), or no pose at all.
    /// </summary>
    internal static KraidMouthShapeRead NativeMouthRead(KraidHeadNativeCursors native, int address)
    {
        foreach (KraidHeadNativeRecord record in native.Records.Where(record => record.IsPose))
        {
            if (address == record.Pointer + 4)
                return new(KraidMouthShapeReadKind.Shape, (KraidMouthHitbox)record.OuterShape);
            if (address == record.Pointer + 6)
                return record.InnerShape == ushort.MaxValue
                    ? new(KraidMouthShapeReadKind.NoShape, default)
                    : new(KraidMouthShapeReadKind.Shape, (KraidMouthHitbox)record.InnerShape);
        }
        return KraidMouthShapeRead.OutsideHeadFrames;
    }

    /// <summary>Reads one native bank-$A7 byte solely for the head-program oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid head command mapping")]
    private static byte KraidHeadCommandMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
