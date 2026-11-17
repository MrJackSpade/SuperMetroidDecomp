using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using static SuperMetroid.TestSupport.MovieReplay.RidleyNativeListAddresses;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of Ridley's typed body, breakup-fragment and fireball animations (#1287, #1600): each segment matched
/// beat by beat to its native list and each animation to the lists' control flow, yielding each beat's native
/// instruction address for <see cref="RidleyNativeInstructionPointers"/>.
/// </summary>
internal static class RidleyNativeProgramOracle
{
    /// <summary>Each body segment's native entry, in authored order.</summary>
    internal static NativeProgramEntry<RidleyBodySegment>[] BodyListEntries =>
        [.. BodyEntries.Select(item => new NativeProgramEntry<RidleyBodySegment>(item.Program, item.Entry))];

    /// <summary>Each fireball segment's native entry, in authored order.</summary>
    internal static NativeProgramEntry<RidleyFireballSegment>[] FireballListEntries =>
        [.. FireballEntries.Select(item => new NativeProgramEntry<RidleyFireballSegment>(item.Program, item.Entry))];

    /// <summary>
    /// The animations Ridley's owner starts for the way he faces (#2175). The cartridge starts each at its left-facing list,
    /// which opens by going on to the right-facing list unless he faces left.
    /// </summary>
    internal static readonly RidleyFacedAnimation[] OwnerStartedFacedAnimations =
    [
        RidleyAnimations.Initial, RidleyAnimations.CeresLunge, RidleyAnimations.RetrieveBabyMetroid, RidleyAnimations.OpeningRoar,
        RidleyAnimations.DeathRoar, RidleyAnimations.Fireballing, RidleyAnimations.TransitionToFlying,
    ];

    /// <summary>
    /// Compares every body segment with its native list and every body animation with the lists' control flow, and
    /// returns each list's beat and control addresses.
    /// </summary>
    internal static Dictionary<RidleyBodySegment, NativeEnemyList<RidleyBodySegment>> BodyLists(IImportCartridgeSource cartridge)
    {
        NativeEnemyAnimationOracle.RequireAuthoredOrder(BodyListEntries);
        Dictionary<RidleyBodySegment, NativeEnemyList<RidleyBodySegment>> lists =
            BodyEntries.ToDictionary(item => item.Program, item => ReadBodyList(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, RidleyAnimations.StartingAt);
        // Exactly the owner's faced animations open with the facing branch, each naming its right-facing counterpart.
        foreach (BodyEntry entry in BodyEntries)
        {
            int address = (BodyBankNumber << 16) | entry.Entry;
            RidleyFacedAnimation? faced = OwnerStartedFacedAnimations.Where(animation => animation.FacingLeft == entry.Program)
                .Select(animation => (RidleyFacedAnimation?)animation).SingleOrDefault();
            bool branches = cartridge.ReadCartridgeWord(address) == GoToUnlessFacingLeft;
            VerificationAssertions.AssertEqual(faced is not null, branches, $"Ridley {entry.Program}: its native list opens with the facing branch only when the owner starts it by facing");
            if (faced is { } opened)
                VerificationAssertions.AssertEqual(EntryOf(opened.FacingRight), cartridge.ReadCartridgeWord(address + 2),
                    $"Ridley {entry.Program}: the facing branch goes on to {opened.FacingRight}");
        }
        return lists;
    }

    /// <summary>The native entry of body <paramref name="segment"/>.</summary>
    private static ushort EntryOf(RidleyBodySegment segment) => BodyEntries.Single(item => item.Program == segment).Entry;

    /// <summary>
    /// The native pointer of an owner-started faced animation before its first play (#2175): the entry of its left-facing
    /// list, which the cartridge starts for either facing; null for any other animation.
    /// </summary>
    internal static ushort? FacedStartPointer(Animation<RidleyBodySegment> animation) =>
        OwnerStartedFacedAnimations
            .Where(faced => ReferenceEquals(RidleyAnimations.StartingWith(faced.FacingLeft), animation)
                || ReferenceEquals(RidleyAnimations.StartingWith(faced.FacingRight), animation))
            .Select(faced => (ushort?)EntryOf(faced.FacingLeft))
            .SingleOrDefault();

    /// <summary>
    /// The playback before the first play of the owner-started faced animation whose left-facing list starts at
    /// <paramref name="address"/>, for a Ridley facing <paramref name="facing"/> (#2175); null when no such list starts there.
    /// </summary>
    internal static AnimationPlayback<RidleyBodySegment>? FacedStartAt(ushort address, RidleyFacing facing) =>
        OwnerStartedFacedAnimations
            .Where(faced => EntryOf(faced.FacingLeft) == address)
            .Select(faced => (AnimationPlayback<RidleyBodySegment>?)new AnimationPlayback<RidleyBodySegment>(
                RidleyAnimations.StartingWith(faced.For(facing)), AnimationPlace.StartOfAnimation))
            .SingleOrDefault();

    /// <summary>The way a Ridley playing fireballing <paramref name="segment"/> faces.</summary>
    /// <exception cref="InvalidDataException"><paramref name="segment"/> is no fireballing.</exception>
    private static RidleyFacing FireballingFacing(RidleyBodySegment segment) =>
        segment == RidleyAnimations.Fireballing.FacingLeft ? RidleyFacing.Left
        : segment == RidleyAnimations.Fireballing.FacingRight ? RidleyFacing.Right
        : throw new InvalidDataException($"Ridley {segment} is no fireballing, but may cut its fireballing short.");

    /// <summary>Reads one body segment's native list, comparing each of its beats with the segment's.</summary>
    private static NativeEnemyList<RidleyBodySegment> ReadBodyList(IImportCartridgeSource cartridge, RidleyBodySegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<EnemyFrameId, RidleyBodyCue>> beats = RidleyAnimations.Segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<RidleyBodySegment>>();
        ushort cursor = entry;
        while (true)
        {
            int beat = addresses.Count;
            string at = $"Ridley {segment} beat {beat}";
            if (cursor != entry && BodyProgramAt(cursor) is { } next)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, next, null, cursor));
                return Finish();
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if (cursor == entry && word == GoToUnlessFacingLeft)
            {
                // The owner starts the right-facing counterpart this branch names instead (#2175, checked in BodyLists): the
                // branch is no beat of the left-facing segment.
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var frame = VerificationAssertions.AssertIs<AnimationBeat<EnemyFrameId, RidleyBodyCue>.Frame>(Beat(beat, at), $"{at}: frame");
                    VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                    VerificationAssertions.AssertEqual(RidleyCollisionDefinitions.BodyFrameAt(operand), (EnemyFrameId?)frame.Shown, $"{at}: spritemap");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat, null, null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    VerificationAssertions.AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    var raised = VerificationAssertions.AssertIs<AnimationBeat<EnemyFrameId, RidleyBodyCue>.Event>(Beat(beat, at), $"{at}: instruction");
                    RidleyBodyCue cue = raised.Raised;
                    instruction.Cue.Check(cue, at);
                    switch (cue)
                    {
                        case RidleyBodyCue.Signal { Kind: RidleyBodyCueKind.MayCutFireballingShort }:
                            // The native branch names the end the owner continues in for the way the fireballing faces (#2175).
                            VerificationAssertions.AssertEqual(BodyProgramAt(operand), (RidleyBodySegment?)RidleyAnimations.FireballingEnd.For(FireballingFacing(segment)),
                                $"{at}: goto target");
                            break;
                        case RidleyBodyCue.SetClawHeight setClawHeight:
                            // The native operand is the height's byte offset into the word-sized claw-height table.
                            VerificationAssertions.AssertEqual(operand, RidleyClawHeightPlaces.ByteOffset(setClawHeight.Height), $"{at}: claw height");
                            break;
                        case RidleyBodyCue.Move move:
                            VerificationAssertions.AssertEqual(unchecked((short)operand), move.Displacement.X, $"{at}: X move");
                            VerificationAssertions.AssertEqual(unchecked((short)cartridge.ReadCartridgeWord(address + 4)), move.Displacement.Y, $"{at}: Y move");
                            break;
                        case RidleyBodyCue.Signal or RidleyBodyCue.Flip:
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined Ridley body cue {cue}.");
                    }
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.GoTo:
                case NativeEnemyListDefinitions.WordKind.Stop:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    throw new InvalidDataException($"{at}: Ridley's body lists use no common gotos, stops, counters or waits.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }

        NativeEnemyList<RidleyBodySegment> Finish()
        {
            VerificationAssertions.AssertEqual(beats.Count, addresses.Count, $"Ridley {segment}: its native list holds each of its beats");
            return new NativeEnemyList<RidleyBodySegment>([.. addresses], [.. controls]);
        }

        AnimationBeat<EnemyFrameId, RidleyBodyCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Ridley {segment}.");
    }

    /// <summary>
    /// Compares every breakup picture with its six-byte native list (#2235): one frame and a sleep, so the fragment shows
    /// the picture for good; the frame's duration only times a sleep that shows nothing else. A fragment stands at the list
    /// entry before it shows its picture and at its sleep after.
    /// </summary>
    internal static void CompareBreakupLists(IImportCartridgeSource cartridge)
    {
        foreach (EnemyFrameId frame in BreakupEntries.Select(entry => entry.Frame))
        {
            int address = (BodyBankNumber << 16) | BreakupList(frame);
            string at = $"Ridley breakup {frame}";
            VerificationAssertions.AssertEqual(NativeEnemyListDefinitions.WordKind.Frame, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address)), $"{at}: frame");
            VerificationAssertions.AssertEqual(RidleyBreakupFrameDefinitions.PictureShowingUpdates, (int)cartridge.ReadCartridgeWord(address), $"{at}: frame duration");
            VerificationAssertions.AssertEqual(cartridge.ReadCartridgeWord(address + 2), RidleyBreakupFrameDefinitionsConstants.Spritemap(frame), $"{at}: spritemap");
            VerificationAssertions.AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address + 4)), $"{at}: sleep");
        }
    }

    /// <summary>
    /// The native instruction pointer of breakup <paramref name="fragment"/>: its list entry before it shows its picture,
    /// and the sleep after the frame record once it has.
    /// </summary>
    internal static ushort BreakupPointer(RoomEnemyPlace fragment)
    {
        EnemyFrameId picture = Ensure.NotNull(fragment).RequireSpeciesState<RidleyExplosionFragmentState>().Picture;
        return checked((ushort)(BreakupList(picture) + (fragment.Frame == picture ? 4 : 0)));
    }

    /// <summary>The native list of breakup <paramref name="frame"/>.</summary>
    internal static ushort BreakupList(EnemyFrameId frame) => BreakupEntry(frame);

    /// <summary>
    /// The puff every afterburn run plays after its spark (#2331), which the cartridge repeats at the end of each afterburn
    /// list: it has no native entry of its own.
    /// </summary>
    private const RidleyFireballSegment Puff = RidleyFireballSegment.AfterburnPuff;

    /// <summary>
    /// Compares every fireball segment with its native bank-$86 list and every fireball animation with the lists' control
    /// flow, and returns each list's beat and control addresses. Each afterburn list's spark runs on into the shared puff,
    /// which is compared with that list's own tail (#2331): every tail plays the puff's beats and ends as the others do.
    /// </summary>
    internal static RidleyFireballNativeLists FireballLists(IImportCartridgeSource cartridge)
    {
        VerificationAssertions.AssertTrue(
            FireballEntries.Select(item => item.Program).SequenceEqual(Enum.GetValues<RidleyFireballSegment>().Where(segment => segment != Puff)),
            "every typed fireball segment but the shared puff has a native entry, in authored order");
        Dictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> lists =
            FireballEntries.ToDictionary(item => item.Program, item => ReadFireballList(cartridge, item.Program, item.Entry));
        Dictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> puffs = lists
            .Where(pair => pair.Value.Controls.Any(control => control is { Kind: NativeEnemyControlKind.RunOnIntoNext, Target: Puff }))
            .ToDictionary(pair => pair.Key, pair => ReadFireballList(cartridge, Puff,
                pair.Value.Controls.Single(control => control is { Kind: NativeEnemyControlKind.RunOnIntoNext, Target: Puff }).Address));
        VerificationAssertions.AssertEqual(4, puffs.Count, "the four afterburn lists each end with the shared puff");
        NativeEnemyList<RidleyFireballSegment> firstPuff = puffs.Values.First();
        foreach ((RidleyFireballSegment opener, NativeEnemyList<RidleyFireballSegment> puff) in puffs)
            VerificationAssertions.AssertTrue(puff.Controls.Select(Shape).SequenceEqual(firstPuff.Controls.Select(Shape)),
                $"Ridley fireball {opener}'s puff ends as every afterburn list's does");
        lists.Add(Puff, firstPuff);
        // The flight is the part of the fireball's list its goto loops back to, which the launch runs on into (#2235); the
        // puff is the part of every afterburn list its spark runs on into (#2331).
        NativeEnemyAnimationOracle.CompareAnimations(lists, RidleyFireballAnimations.StartingAt,
            onlyRunOnInto: new HashSet<RidleyFireballSegment> { RidleyFireballSegment.FireballFlight, Puff });
        return new RidleyFireballNativeLists(lists, puffs);

        static NativeEnemyControl<RidleyFireballSegment> Shape(NativeEnemyControl<RidleyFireballSegment> control) => control with { Address = 0 };
    }

    /// <summary>
    /// Reads one fireball segment's native list, comparing each of its beats with the segment's; the deletion ends the
    /// list, and the pointer rests on it while the animation holds.
    /// </summary>
    private static NativeEnemyList<RidleyFireballSegment> ReadFireballList(IImportCartridgeSource cartridge, RidleyFireballSegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>> beats = RidleyFireballAnimations.Segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<RidleyFireballSegment>>();
        ushort cursor = entry;
        while (true)
        {
            int beat = addresses.Count;
            string at = $"Ridley fireball {segment} beat {beat}";
            if (cursor != entry && FireballProgramAt(cursor) is { } next)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, next, null, cursor));
                return Finish();
            }
            if (beat == beats.Count && PlaysPuffAfter(segment))
            {
                // An afterburn run's spark ends its own segment; its list goes on with the puff every afterburn list repeats,
                // which the run plays next (#2331).
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, Puff, null, cursor));
                return Finish();
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = VerificationAssertions.AssertIs<AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Frame>(Beat(beat, at), $"{at}: frame");
                VerificationAssertions.AssertEqual(word, show.Duration, $"{at}: duration");
                // A pose an afterburn run shows again from an operand of its own is its one installed frame (#2234).
                VerificationAssertions.AssertEqual(EnemyProjectileProgramFrameOperands.OwnOperand(unchecked((ushort)(cursor + 2))),
                    EnemyProjectileProgramFrameSources.Operand(show.Shown), $"{at}: presentation key");
                addresses.Add(cursor);
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            switch ((NativeEnemyProjectileInstruction)word)
            {
                case NativeEnemyProjectileInstruction.ClearFrameBehavior:
                    VerificationAssertions.AssertEqual(RidleyFireballCue.StopMoving, Raised(beat, at), $"{at}: clear pre-instruction");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyProjectileInstruction.FrameBehaviorInY:
                    VerificationAssertions.AssertEqual<Enum?>(RidleyFireballMotion.FireballFlying, EnemyProjectileMotionAccess.FromWord(operand), $"{at}: the fireball's moving pre-instruction");
                    VerificationAssertions.AssertEqual(RidleyFireballCue.BeginFlight, Raised(beat, at), $"{at}: begin flight");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyProjectileInstruction.GotoY:
                    if (operand != entry && addresses.IndexOf(operand) is var loopBeat and > 0)
                    {
                        // A goto back to a beat its own list already showed (#2166) loops the fireball's flight from there: the
                        // launch before it plays once.
                        controls.Insert(controls.FindLastIndex(control => control.AfterBeat <= loopBeat) + 1,
                            new(NativeEnemyControlKind.LoopStart, loopBeat, segment, null, operand));
                        controls.Add(new(NativeEnemyControlKind.Goto, beat, segment, null, cursor, loopBeat));
                        return Finish();
                    }
                    controls.Add(new(NativeEnemyControlKind.Goto, beat,
                        FireballProgramAt(operand) ?? throw new InvalidDataException($"{at}: ${operand:X4} is no segment's native entry."), null, cursor));
                    return Finish();
                case NativeEnemyProjectileInstruction.Delete:
                    VerificationAssertions.AssertEqual(RidleyFireballCue.Delete, Raised(beat, at), $"{at}: delete");
                    addresses.Add(cursor);
                    // The deletion ends the list; the pointer stays on it while the animation holds.
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat + 1, null, null, cursor));
                    return Finish();
                case NativeEnemyProjectileInstruction.Properties_OrY:
                case NativeEnemyProjectileInstruction.Properties_AndY:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProj_EnableCollisionWithSamusProj_868248:
                case NativeEnemyProjectileInstruction.DisableCollisionWIthSamusProj:
                case NativeEnemyProjectileInstruction.DisableCollisionWithSamus:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_EnableCollisionWithSamus_868266:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_SetToNotDieOnContact_868270:
                case NativeEnemyProjectileInstruction.UNUSED_Instruction_EnemyProjectile_SetToDieOnContact_86827A:
                case NativeEnemyProjectileInstruction.SetHighPriority:
                case NativeEnemyProjectileInstruction.UNUSED_Instruction_EnemyProjectile_SetLowPriority_86828E:
                case NativeEnemyProjectileInstruction.XYRadiusInY:
                case NativeEnemyProjectileInstruction.UNUSED_Instruction_EnemyProjectile_XYRadius_0:
                case NativeEnemyProjectileInstruction.UNUSED_Instruction_EnemyProjectile_QueueMusicTrackInY:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max6_868309:
                case NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6:
                case NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max6:
                case NativeEnemyProjectileInstruction.QueueSoundInY_Lib1_Max15:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max15_86832D:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max15_868336:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max3_86833F:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max3_868348:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max3_868351:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86835A:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max9_868363:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86836C:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max1_868375:
                case NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max1:
                case NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max1:
                case NativeEnemyProjectileInstruction.Sleep:
                case NativeEnemyProjectileInstruction.CalculateDirectionTowardsSamus:
                case NativeEnemyProjectileInstruction.DownwardGateSetYVelocityInstruction:
                case NativeEnemyProjectileInstruction.CallExternalFunctionInY:
                case NativeEnemyProjectileInstruction.SetPreInst_DraygonsWallTurretProjectile_Fired:
                case NativeEnemyProjectileInstruction.DraygonGoop_SamusCollision:
                case NativeEnemyProjectileInstruction.GotoY_Y:
                case NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero:
                case NativeEnemyProjectileInstruction.UNUSED_Inst_EnemyProj_DecrementTimer_GotoY_YIfNonZero_8681CE:
                case NativeEnemyProjectileInstruction.TimerInY:
                case NativeEnemyProjectileInstruction.NoobTubeShardAssignFallingAngle:
                case NativeEnemyProjectileInstruction.NoobTubeBubbleAssignFallingAngle:
                case NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker:
                case NativeEnemyProjectileInstruction.NoobTubeShardFlicker:
                case NativeEnemyProjectileInstruction.RTS_8681DE:
                case NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius:
                case NativeEnemyProjectileInstruction.FrameBehaviorInY_ExecuteY:
                case NativeEnemyProjectileInstruction.Torizo_ResetPosition:
                case NativeEnemyProjectileInstruction.UNUSED_Instruction_EnemyProj_MoveHorizontally_GotoY_86AD92:
                case NativeEnemyProjectileInstruction.GoldenTorizoEgg_GoToHatched:
                case NativeEnemyProjectileInstruction.AimSuperMissile_Rightwards:
                case NativeEnemyProjectileInstruction.AimSuperMissile_Leftwards:
                case NativeEnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled:
                case NativeEnemyProjectileInstruction.UNUSED_Instruction_ResetPosition_86B436:
                case NativeEnemyProjectileInstruction.MotherBrainsTurretBullets_GotoY:
                case NativeEnemyProjectileInstruction.GotoY_Probability_1_4:
                case NativeEnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances:
                case NativeEnemyProjectileInstruction.Spawn_HorizontalAfterburn_EnemyProjectiles:
                case NativeEnemyProjectileInstruction.Spawn_VerticalAfterburn_EnemyProjectiles:
                case NativeEnemyProjectileInstruction.SpawnNext_Afterburn_EnemyProjectile:
                case NativeEnemyProjectileInstruction.SpawnPhantoonDrop:
                case NativeEnemyProjectileInstruction.SpawnEnemyDropsWithDraygonEyeChances:
                case NativeEnemyProjectileInstruction.SpawnEnemyDropsWithCrocomireChances:
                case NativeEnemyProjectileInstruction.KagoBugStartJumpInstruction:
                case NativeEnemyProjectileInstruction.KagoBugStartIdleInstruction:
                case NativeEnemyProjectileInstruction.KagoBugUsePaletteZeroInstruction:
                case NativeEnemyProjectileInstruction.KagoBugSpawnDropInstruction:
                case NativeEnemyProjectileInstruction.MagdolliteFlame_SpawnDrops:
                case NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_20:
                case NativeEnemyProjectileInstruction.EnemyDeathExpl_SpawnSpriteObjectInY_10:
                case NativeEnemyProjectileInstruction.EnemyDeathExpl_QueueEnemyKilledSoundFX:
                case NativeEnemyProjectileInstruction.EDeathExplo_QueueSmallExplosionSoundFX:
                case NativeEnemyProjectileInstruction.EDeathExplo_QueueContactKilledSoundFX:
                case NativeEnemyProjectileInstruction.Spores_SetProperties3000:
                case NativeEnemyProjectileInstruction.Spores_SpawnEnemyDrops:
                case NativeEnemyProjectileInstruction.SporeSpawner_SpawnSpore:
                case NativeEnemyProjectileInstruction.TorizoLandingDustClouds:
                case NativeEnemyProjectileInstruction.EnemyDeathExplosion_BecomePickup:
                case NativeEnemyProjectileInstruction.Pickup_HandleRespawningEnemy:
                case NativeEnemyProjectileInstruction.MotherBrainPurpleBreath_Inactive:
                case NativeEnemyProjectileInstruction.UsePalette0:
                case NativeEnemyProjectileInstruction.UsePalette0_Duplicate:
                case NativeEnemyProjectileInstruction.MotherBrainsDrool_MoveDownCPixels:
                case NativeEnemyProjectileInstruction.TourianStatueResetDustPosition:
                default:
                    VerificationAssertions.AssertTrue(FireballCallbacks.TryGetValue(word, out RidleyFireballCue callback), $"{at}: native callback ${word:X4} is known");
                    VerificationAssertions.AssertEqual(callback, Raised(beat, at), $"{at}: cue");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
            }
        }

        NativeEnemyList<RidleyFireballSegment> Finish()
        {
            VerificationAssertions.AssertEqual(beats.Count, addresses.Count, $"Ridley fireball {segment}: its native list holds each of its beats");
            return new NativeEnemyList<RidleyFireballSegment>([.. addresses], [.. controls]);
        }

        AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Ridley fireball {segment}.");

        RidleyFireballCue Raised(int beat, string at) =>
            VerificationAssertions.AssertIs<AnimationBeat<EnemyProjectileFrameId, RidleyFireballCue>.Event>(Beat(beat, at), $"{at}: event").Raised;
    }

    /// <summary>Whether the animation starting at <paramref name="segment"/> plays the shared puff straight after it (#2331).</summary>
    private static bool PlaysPuffAfter(RidleyFireballSegment segment) =>
        RidleyFireballAnimations.StartingAt.TryGetValue(segment, out Animation<RidleyFireballSegment>? animation)
        && animation.Phases is [{ Segment: var first }, { Segment: Puff }, ..] && first == segment;
}
