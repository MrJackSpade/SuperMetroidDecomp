using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using static SuperMetroid.TestSupport.MovieReplay.RidleyNativeListAddresses;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
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
    /// Compares every body segment with its native list and every body animation with the lists' control flow, and
    /// returns each list's beat and control addresses.
    /// </summary>
    internal static Dictionary<RidleyBodySegment, NativeEnemyList<RidleyBodySegment>> BodyLists(IImportCartridgeSource cartridge)
    {
        NativeEnemyAnimationOracle.RequireAuthoredOrder(BodyListEntries);
        Dictionary<RidleyBodySegment, NativeEnemyList<RidleyBodySegment>> lists =
            BodyEntries.ToDictionary(item => item.Program, item => ReadBodyList(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, RidleyAnimations.StartingAt);
        return lists;
    }

    /// <summary>Reads one body segment's native list, comparing each of its beats with the segment's.</summary>
    private static NativeEnemyList<RidleyBodySegment> ReadBodyList(IImportCartridgeSource cartridge, RidleyBodySegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>> beats = RidleyAnimations.Segments.Beats(segment);
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
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var frame = VerificationAssertions.AssertIs<AnimationBeat<RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.Frame>(Beat(beat, at), $"{at}: frame");
                    VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                    VerificationAssertions.AssertEqual(RidleyCollisionDefinitions.BodyFrameAt(operand), (RidleyCollisionDefinitions.BodyFrame?)frame.Shown, $"{at}: spritemap");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat, null, null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    VerificationAssertions.AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    var raised = VerificationAssertions.AssertIs<AnimationBeat<RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.Event>(Beat(beat, at), $"{at}: instruction");
                    RidleyBodyCue cue = raised.Raised;
                    instruction.Cue.Check(cue, at);
                    switch (cue)
                    {
                        case RidleyBodyCue.GoToUnlessFacingLeft goToUnlessFacingLeft:
                            VerificationAssertions.AssertEqual(BodyProgramAt(operand), goToUnlessFacingLeft.Target, $"{at}: goto target");
                            break;
                        case RidleyBodyCue.GoToIfCeresAndSamusLowEnergy goToIfCeresAndSamusLowEnergy:
                            VerificationAssertions.AssertEqual(BodyProgramAt(operand), goToIfCeresAndSamusLowEnergy.Target, $"{at}: goto target");
                            break;
                        case RidleyBodyCue.SetClawHeight setClawHeight:
                            // The native operand is the height's byte offset into the word-sized claw-height table.
                            VerificationAssertions.AssertEqual(operand, RidleyClawHeightPlaces.ByteOffset(setClawHeight.Height), $"{at}: claw height");
                            break;
                        case RidleyBodyCue.Move move:
                            VerificationAssertions.AssertEqual(unchecked((short)operand), move.Displacement.X, $"{at}: X move");
                            VerificationAssertions.AssertEqual(unchecked((short)cartridge.ReadCartridgeWord(address + 4)), move.Displacement.Y, $"{at}: Y move");
                            break;
                        case RidleyBodyCue.Signal:
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

        AnimationBeat<RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Ridley {segment}.");
    }

    /// <summary>
    /// Compares every breakup segment and animation with its six-byte native list: one frame and a sleep. A breakup
    /// animation stands at the list entry before its frame and at its sleep after.
    /// </summary>
    internal static void CompareBreakupLists(IImportCartridgeSource cartridge)
    {
        foreach (RidleyBreakupFrame frame in Enum.GetValues<RidleyBreakupFrame>())
        {
            ushort entry = BreakupList(frame);
            int address = (BodyBankNumber << 16) | entry;
            IReadOnlyList<AnimationBeat<RidleyBreakupFrame, NoAnimationCue>> beats = RidleyBreakupAnimations.Segments.Beats(frame);
            string at = $"Ridley breakup {frame}";
            VerificationAssertions.AssertEqual(1, beats.Count, $"{at}: one frame");
            var show = VerificationAssertions.AssertIs<AnimationBeat<RidleyBreakupFrame, NoAnimationCue>.Frame>(beats[0], $"{at}: frame");
            VerificationAssertions.AssertEqual(cartridge.ReadCartridgeWord(address), show.Duration, $"{at}: duration");
            VerificationAssertions.AssertEqual(cartridge.ReadCartridgeWord(address + 2), RidleyBreakupFrameDefinitionsConstants.Spritemap(show.Shown), $"{at}: spritemap");
            VerificationAssertions.AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address + 4)), $"{at}: sleep");
            Animation<RidleyBreakupFrame> animation = RidleyBreakupAnimations.StartingWith(frame);
            VerificationAssertions.AssertTrue(animation.Phases.SequenceEqual([new AnimationPhase<RidleyBreakupFrame>(frame, 0, 1, 1)]) && animation.End == AnimationEnd.Holds,
                $"{at}: the frame plays once and holds");
        }
    }

    /// <summary>
    /// The native instruction pointer of a breakup fragment's <paramref name="playback"/>: its list entry before its frame,
    /// and the sleep after the frame record once shown.
    /// </summary>
    internal static ushort BreakupPointer(AnimationPlayback<RidleyBreakupFrame> playback) =>
        checked((ushort)(BreakupList(BreakupFrameOf(playback)) + (playback.Place.Beat <= 0 ? 0 : 4)));

    /// <summary>The breakup frame <paramref name="playback"/> shows.</summary>
    internal static RidleyBreakupFrame BreakupFrameOf(AnimationPlayback<RidleyBreakupFrame> playback) =>
        playback.Animation.Phases[playback.Place.Phase].Segment;

    /// <summary>The native list of breakup <paramref name="frame"/>.</summary>
    internal static ushort BreakupList(RidleyBreakupFrame frame) => BreakupEntry(frame);

    /// <summary>
    /// Compares every fireball segment with its native bank-$86 list and every fireball animation with the lists' control
    /// flow, and returns each list's beat and control addresses.
    /// </summary>
    internal static Dictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> FireballLists(IImportCartridgeSource cartridge)
    {
        NativeEnemyAnimationOracle.RequireAuthoredOrder(FireballListEntries);
        Dictionary<RidleyFireballSegment, NativeEnemyList<RidleyFireballSegment>> lists =
            FireballEntries.ToDictionary(item => item.Program, item => ReadFireballList(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, RidleyFireballAnimations.StartingAt);
        return lists;
    }

    /// <summary>
    /// Reads one fireball segment's native list, comparing each of its beats with the segment's; the deletion ends the
    /// list, and the pointer rests on it while the animation holds.
    /// </summary>
    private static NativeEnemyList<RidleyFireballSegment> ReadFireballList(IImportCartridgeSource cartridge, RidleyFireballSegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<RidleyFireballFrame, RidleyFireballCue>> beats = RidleyFireballAnimations.Segments.Beats(segment);
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
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = VerificationAssertions.AssertIs<AnimationBeat<RidleyFireballFrame, RidleyFireballCue>.Frame>(Beat(beat, at), $"{at}: frame");
                VerificationAssertions.AssertEqual(word, show.Duration, $"{at}: duration");
                VerificationAssertions.AssertEqual(unchecked((ushort)(cursor + 2)), EnemyProjectileProgramFrameSources.Operand(RidleyFireballVisualDefinitions.ShownProjectileFrames[show.Shown]), $"{at}: presentation key");
                addresses.Add(cursor);
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            switch ((NativeEnemyProjectileInstruction)word)
            {
                case NativeEnemyProjectileInstruction.ClearFrameBehavior:
                    VerificationAssertions.AssertEqual(RidleyFireballCue.ClearFrameBehavior, Raised(beat, at), $"{at}: clear pre-instruction");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyProjectileInstruction.FrameBehaviorInY:
                    VerificationAssertions.AssertEqual(EnemyProjectileFrameBehavior.RidleyFireballFlight, EnemyProjectileFrameBehaviorAccess.FromWord(operand), $"{at}: the fireball's moving pre-instruction");
                    VerificationAssertions.AssertEqual(RidleyFireballCue.BeginFlight, Raised(beat, at), $"{at}: begin flight");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyProjectileInstruction.GotoY:
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

        AnimationBeat<RidleyFireballFrame, RidleyFireballCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Ridley fireball {segment}.");

        RidleyFireballCue Raised(int beat, string at) =>
            VerificationAssertions.AssertIs<AnimationBeat<RidleyFireballFrame, RidleyFireballCue>.Event>(Beat(beat, at), $"{at}: event").Raised;
    }
}
