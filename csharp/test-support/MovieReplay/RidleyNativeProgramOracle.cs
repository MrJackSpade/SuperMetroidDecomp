using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using static SuperMetroid.TestSupport.MovieReplay.RidleyNativeListAddresses;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of Ridley's typed body, breakup-fragment and fireball programs (#1287): each program matched step
/// by step to its native list, yielding each typed step's native instruction address for
/// <see cref="RidleyNativeInstructionPointers"/>.
/// </summary>
internal static class RidleyNativeProgramOracle
{
    /// <summary>Compares every body program with its native list and returns each typed step's native address.</summary>
    internal static Dictionary<RidleyBodyProgram, ushort[]> BodyStepAddresses(IImportCartridgeSource cartridge) =>
        BodyEntries.ToDictionary(item => item.Program, item => CompareBodyProgram(cartridge, item.Program, item.Entry));

    /// <summary>Compares one body program with its native list and returns each typed step's native address.</summary>
    private static ushort[] CompareBodyProgram(IImportCartridgeSource cartridge, RidleyBodyProgram program, ushort entry)
    {
        var steps = RidleyAnimations.Programs.Steps(program);
        var addresses = new ushort[steps.Count];
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            string at = $"Ridley {program} step {index}";
            addresses[index] = cursor;
            var step = steps[index];
            if (cursor != entry && BodyProgramAt(cursor) is { } next)
            {
                var fallInto = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.FallIntoStep>(step, $"{at}: runs on into the next authored program");
                VerificationAssertions.AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                VerificationAssertions.AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return addresses;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var show = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.ShowStep>(step, $"{at}: frame");
                    VerificationAssertions.AssertEqual(word, show.Duration, $"{at}: duration");
                    VerificationAssertions.AssertEqual(RidleyCollisionDefinitions.BodyFrameAt(operand), (RidleyCollisionDefinitions.BodyFrame?)show.Frame, $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    VerificationAssertions.AssertTrue(step is AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.HoldStep, $"{at}: sleep");
                    VerificationAssertions.AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return addresses;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    VerificationAssertions.AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    var cueStep = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.CueStep>(step, $"{at}: instruction");
                    RidleyBodyCue cue = cueStep.Cue;
                    instruction.Cue.Check(cue, at);
                    switch (cue)
                    {
                        case RidleyBodyCue.GoToUnlessFacingLeft goToUnlessFacingLeft:
                            VerificationAssertions.AssertEqual(BodyProgramAt(operand), goToUnlessFacingLeft.Target, $"{at}: goto target");
                            break;
                        case RidleyBodyCue.GoToIfCeresAndSamusLowEnergy goToIfCeresAndSamusLowEnergy:
                            VerificationAssertions.AssertEqual(BodyProgramAt(operand), goToIfCeresAndSamusLowEnergy.Target, $"{at}: goto target");
                            break;
                        case RidleyBodyCue.SetFeetDistance setFeetDistance:
                            // The native operand is the pose's byte offset into the word-sized claw-height table.
                            VerificationAssertions.AssertEqual(operand, setFeetDistance.FeetDistanceIndex * sizeof(ushort), $"{at}: claw-height pose");
                            break;
                        case RidleyBodyCue.Move move:
                            VerificationAssertions.AssertEqual(unchecked((short)operand), move.Displacement.X, $"{at}: X move");
                            VerificationAssertions.AssertEqual(unchecked((short)cartridge.ReadCartridgeWord(address + 4)), move.Displacement.Y, $"{at}: Y move");
                            break;
                        case RidleyBodyCue.Command:
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined Ridley body cue {cue}.");
                    }
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
        throw new InvalidDataException($"Ridley {program} has steps past its native list.");
    }

    /// <summary>
    /// Compares every breakup program with its six-byte native list: one frame and a sleep. A breakup program's
    /// steps sit at the list entry and its sleep.
    /// </summary>
    internal static void BreakupStepAddresses(IImportCartridgeSource cartridge)
    {
        foreach (RidleyBreakupFrame frame in Enum.GetValues<RidleyBreakupFrame>())
        {
            ushort entry = BreakupList(frame);
            int address = (BodyBankNumber << 16) | entry;
            var steps = RidleyBreakupAnimations.Programs.Steps(frame);
            string at = $"Ridley breakup {frame}";
            VerificationAssertions.AssertEqual(2, steps.Count, $"{at}: one frame and a hold");
            var show = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue>.ShowStep>(steps[0], $"{at}: frame");
            VerificationAssertions.AssertEqual(cartridge.ReadCartridgeWord(address), show.Duration, $"{at}: duration");
            VerificationAssertions.AssertEqual(cartridge.ReadCartridgeWord(address + 2), RidleyBreakupFrameDefinitionsConstants.Spritemap(show.Frame), $"{at}: spritemap");
            VerificationAssertions.AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address + 4)), $"{at}: sleep");
            VerificationAssertions.AssertTrue(steps[1] is AnimationProgramStep<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue>.HoldStep, $"{at}: hold");
        }
    }

    /// <summary>The native list of breakup <paramref name="frame"/>.</summary>
    internal static ushort BreakupList(RidleyBreakupFrame frame) => BreakupEntry(frame);

    /// <summary>Compares every fireball program with its native bank-$86 list and returns each typed step's native address.</summary>
    internal static Dictionary<RidleyFireballProgram, ushort[]> FireballStepAddresses(IImportCartridgeSource cartridge) =>
        FireballEntries.ToDictionary(item => item.Program, item => CompareFireballProgram(cartridge, item.Program, item.Entry));

    /// <summary>Compares one fireball program with its native list and returns each typed step's native address.</summary>
    private static ushort[] CompareFireballProgram(IImportCartridgeSource cartridge, RidleyFireballProgram program, ushort entry)
    {
        var steps = RidleyFireballAnimations.Programs.Steps(program);
        var addresses = new ushort[steps.Count];
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            string at = $"Ridley fireball {program} step {index}";
            addresses[index] = cursor;
            var step = steps[index];
            if (cursor != entry && FireballProgramAt(cursor) is { } next)
            {
                var fallInto = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.FallIntoStep>(step, $"{at}: runs on into the next authored program");
                VerificationAssertions.AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                VerificationAssertions.AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return addresses;
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.ShowStep>(step, $"{at}: frame");
                VerificationAssertions.AssertEqual(word, show.Duration, $"{at}: duration");
                VerificationAssertions.AssertEqual(unchecked((ushort)(cursor + 2)), EnemyProjectileProgramFrameSources.Operand(RidleyFireballVisualDefinitions.ShownProjectileFrames[show.Frame]), $"{at}: presentation key");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            switch ((NativeEnemyProjectileInstruction)word)
            {
                case NativeEnemyProjectileInstruction.ClearFrameBehavior:
                    VerificationAssertions.AssertEqual(RidleyFireballCue.ClearFrameBehavior, VerificationAssertions.AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: cue step").Cue, $"{at}: clear pre-instruction");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyProjectileInstruction.FrameBehaviorInY:
                    VerificationAssertions.AssertEqual(EnemyProjectileFrameBehavior.RidleyFireballFlight, EnemyProjectileFrameBehaviorAccess.FromWord(operand), $"{at}: the fireball's moving pre-instruction");
                    VerificationAssertions.AssertEqual(RidleyFireballCue.BeginFlight, VerificationAssertions.AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: cue step").Cue, $"{at}: begin flight");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyProjectileInstruction.GotoY:
                    var goTo = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.GoToStep>(step, $"{at}: goto");
                    VerificationAssertions.AssertEqual(FireballProgramAt(operand), goTo.Target, $"{at}: goto target");
                    VerificationAssertions.AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return addresses;
                case NativeEnemyProjectileInstruction.Delete:
                    VerificationAssertions.AssertEqual(RidleyFireballCue.Delete, VerificationAssertions.AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: cue step").Cue, $"{at}: delete");
                    VerificationAssertions.AssertTrue(steps[index + 1] is AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.HoldStep, $"{at}: deletion ends the program");
                    VerificationAssertions.AssertEqual(steps.Count, index + 2, $"{at}: deletion ends the program");
                    addresses[index + 1] = cursor;
                    return addresses;
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
                    var cueStep = VerificationAssertions.AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: callback");
                    VerificationAssertions.AssertEqual(callback, cueStep.Cue, $"{at}: cue");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
            }
        }
        throw new InvalidDataException($"Ridley fireball {program} has steps past its native list.");
    }

}
