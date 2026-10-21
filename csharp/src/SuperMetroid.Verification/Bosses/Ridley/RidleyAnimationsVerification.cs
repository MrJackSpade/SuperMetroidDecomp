using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// #1287: Ridley's typed body, breakup-fragment and fireball programs against the retail cartridge. Each typed
/// program must match its native list step by step, and the body's facing and low-energy branches must redirect as
/// the native gotos do. The step-by-step walk also yields each typed step's native instruction address, which
/// <see cref="RidleyNativeInstructionPointers"/> uses to compare typed cursors with recorded native pointers.
/// </summary>
internal static class RidleyAnimationsVerification
{
    /// <summary>Every body, breakup and fireball program matches its native list, and the body's branches redirect natively.</summary>
    internal static void VerifyRidleyAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<RidleyBodyProgram>().Length, BodyEntries.Length, "every typed Ridley body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((RidleyBodyProgram)index, BodyEntries[index].Program, "the Ridley body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Ridley body entries ascend");
        AssertEqual(Enum.GetValues<RidleyFireballProgram>().Length, FireballEntries.Length, "every typed fireball program has a native entry");

        var body = BodyStepAddresses(cartridge);
        BreakupStepAddresses(cartridge);
        var fireball = FireballStepAddresses(cartridge);
        AssertEqual(RidleyFireballVisualDefinitions.ProjectileFrameCount, RidleyFireballVisualDefinitions.ProjectileFrames.Count,
            "the fireball catalog lists every frame");

        VerifyFacingBranch();
        VerifyLowEnergyBranch();
        Console.WriteLine($"  Ridley programs: {body.Count} body programs ({body.Values.Sum(steps => steps.Length)} steps), " +
            $"{Enum.GetValues<RidleyBreakupFrame>().Length} breakup programs and {fireball.Count} fireball programs match their native lists; " +
            "the facing and low-energy branches redirect natively.");
    }

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
                var fallInto = AssertIs<AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.FallIntoStep>(step, $"{at}: runs on into the next authored program");
                AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return addresses;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var show = AssertIs<AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.ShowStep>(step, $"{at}: frame");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(operand, (ushort)show.Frame, $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertTrue(step is AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.HoldStep, $"{at}: sleep");
                    AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return addresses;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    var cueStep = AssertIs<AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.CueStep>(step, $"{at}: instruction");
                    RidleyBodyCue cue = cueStep.Cue;
                    instruction.Cue.Check(cue, at);
                    switch (cue)
                    {
                        case RidleyBodyCue.GoToUnlessFacingLeft goToUnlessFacingLeft:
                            AssertEqual(BodyProgramAt(operand), goToUnlessFacingLeft.Target, $"{at}: goto target");
                            break;
                        case RidleyBodyCue.GoToIfCeresAndSamusLowEnergy goToIfCeresAndSamusLowEnergy:
                            AssertEqual(BodyProgramAt(operand), goToIfCeresAndSamusLowEnergy.Target, $"{at}: goto target");
                            break;
                        case RidleyBodyCue.SetFeetDistance setFeetDistance:
                            // The native operand is the pose's byte offset into the word-sized claw-height table.
                            AssertEqual(operand, setFeetDistance.FeetDistanceIndex * sizeof(ushort), $"{at}: claw-height pose");
                            break;
                        case RidleyBodyCue.Move move:
                            AssertEqual(unchecked((short)operand), move.Displacement.X, $"{at}: X move");
                            AssertEqual(unchecked((short)cartridge.ReadCartridgeWord(address + 4)), move.Displacement.Y, $"{at}: Y move");
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
            AssertEqual(2, steps.Count, $"{at}: one frame and a hold");
            var show = AssertIs<AnimationProgramStep<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue>.ShowStep>(steps[0], $"{at}: frame");
            AssertEqual(cartridge.ReadCartridgeWord(address), show.Duration, $"{at}: duration");
            AssertEqual(cartridge.ReadCartridgeWord(address + 2), RidleyBreakupFrameDefinitionsConstants.Spritemap(show.Frame), $"{at}: spritemap");
            AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address + 4)), $"{at}: sleep");
            AssertTrue(steps[1] is AnimationProgramStep<RidleyBreakupFrame, RidleyBreakupFrame, NoAnimationCue>.HoldStep, $"{at}: hold");
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
                var fallInto = AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.FallIntoStep>(step, $"{at}: runs on into the next authored program");
                AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return addresses;
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.ShowStep>(step, $"{at}: frame");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), EnemyProjectileFrames.ProgramFrameKey(RidleyFireballVisualDefinitions.ShownProjectileFrames[show.Frame]), $"{at}: presentation key");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            switch ((NativeEnemyProjectileInstruction)word)
            {
                case NativeEnemyProjectileInstruction.ClearPreInstruction:
                    AssertEqual(RidleyFireballCue.ClearPreInstruction, AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: cue step").Cue, $"{at}: clear pre-instruction");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyProjectileInstruction.PreInstructionInY:
                    AssertEqual(EnemyProjectilePreInstructionAccess.Word(EnemyProjectilePreInstruction.RidleyFireball), operand, $"{at}: the fireball's moving pre-instruction");
                    AssertEqual(RidleyFireballCue.BeginFlight, AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: cue step").Cue, $"{at}: begin flight");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyProjectileInstruction.GotoY:
                    var goTo = AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.GoToStep>(step, $"{at}: goto");
                    AssertEqual(FireballProgramAt(operand), goTo.Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return addresses;
                case NativeEnemyProjectileInstruction.Delete:
                    AssertEqual(RidleyFireballCue.Delete, AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: cue step").Cue, $"{at}: delete");
                    AssertTrue(steps[index + 1] is AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.HoldStep, $"{at}: deletion ends the program");
                    AssertEqual(steps.Count, index + 2, $"{at}: deletion ends the program");
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
                case NativeEnemyProjectileInstruction.PreInstructionInY_ExecuteY:
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
                    AssertTrue(FireballCallbacks.TryGetValue(word, out RidleyFireballCue callback), $"{at}: native callback ${word:X4} is known");
                    var cueStep = AssertIs<AnimationProgramStep<RidleyFireballProgram, RidleyFireballFrame, RidleyFireballCue>.CueStep>(step, $"{at}: callback");
                    AssertEqual(callback, cueStep.Cue, $"{at}: cue");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
            }
        }
        throw new InvalidDataException($"Ridley fireball {program} has steps past its native list.");
    }

    /// <summary>Creates Ridley in slot zero of a fresh enemy system, facing <paramref name="facing"/>.</summary>
    private static NewRidleyResult NewRidley(EnemyDefinitionId definition, RidleyFacing facing)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var state = new RidleyEnemyState { FacingDirection = facing };
        enemies.RidleyEncounter.Ridley = state;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(definition, body.Definition);
        return new NewRidleyResult(enemies, state, body);
    }

    /// <summary>Runs one body update, expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot body, SamusState? samus)
    {
        body.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionInterpreter.ProcessInstructions(body, samus, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>A right-facing Ridley starting a left-facing entry runs its right-facing program in the same update.</summary>
    private static void VerifyFacingBranch()
    {
        (_, RidleyEnemyState state, RoomEnemySlot body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Right);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyBodyProgram.OpeningRoarFacingRight, state.BodyProgram.Program, "a right-facing Ridley roars facing right");
        AssertEqual((ushort)RidleyCollisionDefinitions.BodyFrame.Right, body.SpritemapPointer, "the right-facing roar shows its first frame at once");
        AssertEqual((ushort)6, body.InstructionTimer.RawValue, "the right-facing roar's first frame lasts six updates");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyBodyProgram.OpeningRoar, state.BodyProgram.Program, "a left-facing Ridley keeps the left-facing roar");
        AssertEqual((ushort)RidleyCollisionDefinitions.BodyFrame.Left, body.SpritemapPointer, "the left-facing roar shows its first frame");
    }

    /// <summary>
    /// In Ceres, a Samus under 30 energy cuts the fireballing short to its closing tail and shortens the hover to eight
    /// updates; Lower Norfair Ridley ignores her energy.
    /// </summary>
    private static void VerifyLowEnergyBranch()
    {
        (_, RidleyEnemyState state, RoomEnemySlot body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        state.Roaring = true;
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState { Health = 29 });
        AssertEqual(RidleyBodyProgram.FireballingEnd, state.BodyProgram.Program, "low energy in Ceres skips to the fireballing tail");
        AssertEqual((ushort)8, state.FunctionTimer.RawValue, "low energy shortens the hover to eight updates");
        AssertTrue(!state.Roaring, "the fireballing tail clears the roaring flag");
        AssertEqual((ushort)RidleyCollisionDefinitions.BodyFrame.Left, body.SpritemapPointer, "the tail shows the standing pose");

        (_, state, body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState { Health = 30 });
        AssertEqual(RidleyBodyProgram.Fireballing, state.BodyProgram.Program, "thirty energy keeps the full fireballing");

        (_, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState { Health = 1 });
        AssertEqual(RidleyBodyProgram.Fireballing, state.BodyProgram.Program, "Lower Norfair Ridley ignores Samus's energy");
    }

    /// <summary>Runs Ridley's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--ridley-animations")]
    internal static int RidleyAnimationsCommand()
    {
        Suite(nameof(VerifyRidleyAnimations), () => VerifyRidleyAnimations(LoadRepositoryRom()));
        return 0;
    }
}
