using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Bosses;
using static SuperMetroid.Verification.Bosses.Kraid.KraidContourCaseMappingsVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// The ROM oracle for <see cref="KraidHeadAnimations"/>: walks the cartridge's four head lists independently and checks
/// every typed step, every cursor's mouth and growth reads, and the entry points the AI installs.
/// </summary>
internal static class KraidHeadCommandMappingVerification
{
    /// <summary>Checks poses, durations, cries and ends; the cursor-relative mouth reads; growth resumes; and entry cursors.</summary>
    internal static void VerifyKraidHeadCommandMapping(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var native = new KraidHeadNativeCursors(cartridge);
        ushort Word(int pointer) => (ushort)(KraidHeadCommandMappingRomByte(rom, KraidHeadNativeCursors.Bank | pointer) |
            (KraidHeadCommandMappingRomByte(rom, KraidHeadNativeCursors.Bank | (pointer + 1)) << 8));
        AssertEqual(19, native.Records.Count, "the started native head lists hold 19 records");
        AssertEqual(21, native.AllPoses.Count, "the four native head lists hold 21 poses, the unstarted roar's seven included");

        // A pose's place in the import's head order is its tilemap's rank among the head's distinct tilemaps: the closed
        // mouth is the lowest.
        List<ushort> stageTilemaps = [.. native.Records.Where(record => record.IsPose)
            .Select(record => record.Tilemap).Distinct().Order()];
        AssertEqual(KraidHeadTilemapFormat.HeadsByMouthStage.Count, stageTilemaps.Count, "one native tilemap per head pose");

        foreach (KraidHeadSegment program in KraidHeadAnimations.StartingAt.Keys)
        {
            Animation<KraidHeadSegment> animation = KraidHeadAnimations.StartingWith(program);
            // The animation's steps are its segments' beats, each segment played once (#2399).
            AnimationBeat<KraidHeadFrame, KraidHeadCue>[] beats = [.. animation.Phases.SelectMany(phase => KraidHeadAnimations.Segments.Beats(phase.Segment))];
            KraidHeadNativeRecord[] records = [.. native.Records.Where(record => KraidHeadAnimations.PlaceOf(record.Playback).Animation == program)];
            AssertEqual(records.Length, beats.Length + 1, $"{program} has one typed beat per native record before its terminator");
            AssertTrue(animation.Phases.All(phase => phase == KraidHeadAnimations.Segments.Whole(phase.Segment)) && animation.End == AnimationEnd.Holds,
                $"{program}'s animation plays its list once and holds at its terminator");
            for (int position = 0; position < records.Length; position++)
            {
                KraidHeadNativeRecord record = records[position];
                string at = $"{program}[{position}] at $A7:{record.Pointer:X4}";
                AssertEqual(new KraidHeadPlace(program, position), KraidHeadAnimations.PlaceOf(record.Playback), $"{at} stands at its animation step");
                if (record.IsPose)
                {
                    var show = AssertIs<AnimationBeat<KraidHeadFrame, KraidHeadCue>.Frame>(beats[position], $"{at} shows a pose");
                    AssertEqual(record.Opcode, show.Duration, $"{at} pose duration");
                    AssertEqual(KraidHeadTilemapFormat.HeadsByMouthStage[stageTilemaps.IndexOf(record.Tilemap)], show.Shown, $"{at} head pose");
                    AssertEqual(record.Tilemap, KraidHeadFrameDefinitions.PresentationKey(show.Shown), $"{at} head image");
                    AssertEqual(record.OuterShape, KraidNativeWords.MouthHitboxes.Word(show.Shown.VulnerableShape), $"{at} vulnerable shape");
                    AssertEqual(record.InnerShape,
                        show.Shown.InvulnerableShape is { } inner ? KraidNativeWords.MouthHitboxes.Word(inner) : ushort.MaxValue,
                        $"{at} invulnerable shape");
                }
                else if (record.IsTerminator)
                {
                    AssertEqual(beats.Length, position, $"{at} ends the list after its last beat");
                    AssertEqual(AnimationPlace.HoldingIn(animation.Phases.Count - 1, animation.Phases[^1].BeatCount), record.Playback.Place, $"{at} holds the animation");
                }
                else
                {
                    var cueStep = AssertIs<AnimationBeat<KraidHeadFrame, KraidHeadCue>.Event>(beats[position], $"{at} cries");
                    (KraidHeadCue cue, SoundEffectId sound) = record.Opcode switch
                    {
                        KraidHeadNativeCursors.RoarCryCall => new VerifyKraidHeadCommandMappingCase(KraidHeadCue.Roar, KraidSoundDefinitions.HeadRoar),
                        KraidHeadNativeCursors.DyingCryCall => new VerifyKraidHeadCommandMappingCase(KraidHeadCue.DyingCry, KraidSoundDefinitions.HeadDyingCry),
                        _ => throw new InvalidDataException($"{at}: unknown native head command ${record.Opcode:X4}."),
                    };
                    AssertEqual(cue, cueStep.Raised, $"{at} cry");
                    AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, (byte)record.Sound), sound,
                        $"{at} cry sound: the import decodes the native byte to the definition's sound");
                }

                KraidHeadAnimationReads reads = KraidHeadAnimations.ReadsAt(record.Playback);
                AssertEqual(NativeMouthRead(native, record.Pointer - 4), reads.Outer, $"{at} outer mouth read four bytes back");
                AssertEqual(NativeMouthRead(native, record.Pointer - 2), reads.Inner, $"{at} inner mouth read two bytes back");
                (ushort resumePointer, ushort resumeTimer) = NativeKraidGrowthResume(rom, Word(record.Pointer + 2));
                KraidHeadEntry resumed = KraidHeadAnimations.GrowthResume(reads.Growth);
                AssertEqual(resumePointer, native.PointerAt(resumed.Playback), $"{at} growth resume cursor");
                AssertEqual(resumeTimer, resumed.Delay, $"{at} growth resume delay");
            }
        }

        // The AI installs each program one pose in, with the entry pose's own duration as the head delay: starting an
        // animation enters its opening pose (#2399).
        AssertEqual((ushort)0x96da, native.PointerAt(KraidHeadAnimations.Starting(KraidHeadSegment.Roar).Playback), "roar entry cursor");
        AssertEqual((ushort)0x9752, native.PointerAt(KraidHeadAnimations.Starting(KraidHeadSegment.EyeGlow).Playback), "eye-glow entry cursor");
        AssertEqual((ushort)0x976c, native.PointerAt(KraidHeadAnimations.Starting(KraidHeadSegment.Death).Playback), "death entry cursor");
        AssertEqual(Word(0x96d2), KraidHeadAnimations.Starting(KraidHeadSegment.Roar).Delay, "roar entry duration");
        AssertEqual(Word(0x974a), KraidHeadAnimations.Starting(KraidHeadSegment.EyeGlow).Delay, "eye-glow entry duration");
        AssertEqual(Word(0x9764), KraidHeadAnimations.Starting(KraidHeadSegment.Death).Delay, "death entry duration");
        AssertThrows<InvalidDataException>(() => KraidHeadAnimations.ReadsAt(null), "an unstarted head has no reads");
    }

    /// <summary>
    /// What a native mouth read at <paramref name="address"/> finds: the outer shape four bytes into a pose, the inner
    /// shape six bytes in ($FFFF being the closed mouth's missing shape), or no pose at all.
    /// </summary>
    internal static KraidMouthShapeRead NativeMouthRead(KraidHeadNativeCursors native, int address)
    {
        foreach (KraidHeadNativePose record in native.AllPoses)
        {
            if (address == record.Pointer + 4)
                return global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Bosses.Kraid.KraidMouthShapeRead>([typeof(global::SuperMetroid.Core.Bosses.Kraid.KraidMouthShapeReadKind), typeof(global::SuperMetroid.Core.Bosses.Kraid.KraidMouthHitbox)], new object?[] { KraidMouthShapeReadKind.Shape, KraidNativeWords.MouthHitboxes.Decode(record.OuterShape, "Kraid mouth hitbox") });
            if (address == record.Pointer + 6)
                return record.InnerShape == ushort.MaxValue
                    ? global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Bosses.Kraid.KraidMouthShapeRead>([typeof(global::SuperMetroid.Core.Bosses.Kraid.KraidMouthShapeReadKind), typeof(global::SuperMetroid.Core.Bosses.Kraid.KraidMouthHitbox)], new object?[] { KraidMouthShapeReadKind.NoShape, (global::SuperMetroid.Core.Bosses.Kraid.KraidMouthHitbox)(default) })
                    : global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Bosses.Kraid.KraidMouthShapeRead>([typeof(global::SuperMetroid.Core.Bosses.Kraid.KraidMouthShapeReadKind), typeof(global::SuperMetroid.Core.Bosses.Kraid.KraidMouthHitbox)], new object?[] { KraidMouthShapeReadKind.Shape, KraidNativeWords.MouthHitboxes.Decode(record.InnerShape, "Kraid mouth hitbox") });
        }
        return KraidMouthShapeRead.NoPose;
    }

    /// <summary>Reads one native bank-$A7 byte solely for the head-program oracle.</summary>
    private static byte KraidHeadCommandMappingRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
