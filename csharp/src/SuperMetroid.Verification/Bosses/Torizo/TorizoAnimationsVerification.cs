using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoNativeListAddresses;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #1289: Bomb and Golden Torizo's typed body programs against the retail cartridge. Each program must match its
/// native bank-$AA list step by step: every frame's duration and spritemap, every common command, every Torizo
/// instruction's cue and operands, every tile transfer's descriptor, and the fixed sleeps two callbacks schedule.
/// </summary>
internal static class TorizoAnimationsVerification
{
    /// <summary>The updates the low-health explosion callback sleeps for.</summary>
    private const int LowHealthExplosionSleep = 40;

    /// <summary>The updates the death explosion callback sleeps for.</summary>
    private const int DeathExplosionSleep = 1;

    /// <summary>The bytes a common $814B tile transfer occupies: its opcode word and seven-byte descriptor.</summary>
    private const int TileTransferBytes = 9;

    /// <summary>Every body program matches its native list.</summary>
    internal static void VerifyTorizoAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<TorizoBodyProgram>().Length, BodyEntries.Length, "every typed Torizo body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((TorizoBodyProgram)index, BodyEntries[index].Program, "the Torizo body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1)).All(pair => pair.First.Entry < pair.Second.Entry), "native Torizo body entries ascend");
        int steps = 0;
        foreach ((TorizoBodyProgram program, ushort entry) in BodyEntries)
            steps += CompareBodyProgram(cartridge, program, entry);
        VerifyResumedSleeps(cartridge);
        var poses = new HashSet<ushort>(Enumerable.Range(0, TorizoFrameDefinitionsConstants.PoseCount)
            .Select(pose => TorizoFrameDefinitions.Spritemap(new TorizoFrame(pose))));
        AssertEqual(TorizoFrameDefinitionsConstants.PoseCount, poses.Count, "every Torizo pose shows a distinct spritemap");
        AssertTrue(poses.SetEquals(TorizoAnimations.Spritemaps(TorizoAnimations.AllPrograms)), "the body programs show every Torizo pose");
        AssertTrue(poses.SetEquals(TorizoCollisionDefinitions.FramePointers), "every Torizo pose has its physical frame");
        Console.WriteLine($"  Torizo programs: {BodyEntries.Length} body programs ({steps} steps, {poses.Count} poses) match bank $AA.");
    }

    /// <summary>Compares one body program with its native list and returns its step count.</summary>
    private static int CompareBodyProgram(IImportCartridgeSource cartridge, TorizoBodyProgram program, ushort entry)
    {
        var steps = TorizoAnimations.Programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue> Next(string what)
        {
            AssertTrue(index < steps.Count, $"Torizo {program} has a step for native {what} at $AA:{cursor:X4}");
            return steps[index++];
        }
        while (true)
        {
            string at = $"Torizo {program} at $AA:{cursor:X4}";
            if (cursor != entry && ProgramAt(cursor) is { } following)
            {
                var fall = Next("fall-through");
                AssertEqual(AnimationProgramStepKind.FallInto, fall.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(following, fall.Target, $"{at}: fall-through target");
                break;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if (ClassifyBodyWord(word) == BodyWordKind.TileTransfer)
            {
                var copy = Next("tile transfer");
                AssertEqual(AnimationProgramStepKind.Cue, copy.Kind, $"{at}: tile transfer");
                AssertEqual(TorizoCueKind.CopyTiles, copy.Cue.Kind, $"{at}: tile transfer cue");
                TorizoInstructionVramTransferDefinition transfer = TorizoInstructionVramTransferDefinitions.Entries[copy.Cue.Transfer];
                AssertEqual(cursor, TorizoInstructionVramTransferDefinitions.Sites[copy.Cue.Transfer].Instruction, $"{at}: tile transfer descriptor");
                AssertEqual(transfer.ByteCount, cartridge.ReadCartridgeWord(address + 2), $"{at}: tile transfer size");
                AssertEqual(transfer.SourceAddress, cartridge.ReadCartridgeLongFixedBank(address + 4), $"{at}: tile transfer source");
                AssertEqual(transfer.DestinationWord, cartridge.ReadCartridgeWord(address + 7), $"{at}: tile transfer destination");
                cursor = unchecked((ushort)(cursor + TileTransferBytes));
                continue;
            }
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                {
                    var show = Next("frame");
                    AssertEqual(AnimationProgramStepKind.Show, show.Kind, $"{at}: frame");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(operand, TorizoFrameDefinitions.Spritemap(show.Frame), $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertEqual(AnimationProgramStepKind.Hold, Next("sleep").Kind, $"{at}: sleep");
                    break;
                case NativeEnemyListDefinitions.WordKind.Stop:
                    AssertEqual(AnimationProgramStepKind.Stop, Next("stop").Kind, $"{at}: stop");
                    break;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                {
                    var jump = Next("goto");
                    AssertEqual(AnimationProgramStepKind.GoTo, jump.Kind, $"{at}: goto");
                    AssertEqual(operand, Entry(jump.Target), $"{at}: goto target");
                    break;
                }
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                {
                    var load = Next("counter load");
                    AssertEqual(AnimationProgramStepKind.SetCounter, load.Kind, $"{at}: counter load");
                    AssertEqual(operand, load.Duration, $"{at}: counter");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                {
                    var repeat = Next("counted repeat");
                    AssertEqual(AnimationProgramStepKind.RepeatWhileCounting, repeat.Kind, $"{at}: counted repeat");
                    AssertEqual(operand, Entry(repeat.Target), $"{at}: repeat target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Wait:
                {
                    var wait = Next("wait");
                    AssertEqual(AnimationProgramStepKind.Wait, wait.Kind, $"{at}: wait");
                    AssertEqual(operand, wait.Duration, $"{at}: wait length");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    var step = Next("instruction");
                    AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: instruction");
                    TorizoCue cue = step.Cue;
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    for (int operandIndex = 0; operandIndex < instruction.Operands.Length; operandIndex++)
                    {
                        ushort value = cartridge.ReadCartridgeWord(address + 2 + 2 * operandIndex);
                        TorizoOperand role = instruction.Operands[operandIndex];
                        ushort typed = role switch
                        {
                            TorizoOperand.Value => checked((ushort)cue.Value),
                            TorizoOperand.Function => (ushort)cue.Function,
                            TorizoOperand.PreInstruction => (ushort)cue.PreInstruction,
                            TorizoOperand.Target => Entry(cue.Target),
                            TorizoOperand.Alternate => Entry(cue.Alternate),
                            _ => throw new InvalidOperationException($"Undefined {nameof(TorizoOperand)} {role}."),
                        };
                        AssertEqual(value, typed, $"{at}: {role} operand");
                    }
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands.Length));
                    int? sleep = cue.Kind switch
                    {
                        TorizoCueKind.SpawnLowHealthExplosions => LowHealthExplosionSleep,
                        TorizoCueKind.SpawnDeathExplosion => DeathExplosionSleep,
                        TorizoCueKind.SetPreInstruction or
                            TorizoCueKind.ClearPreInstruction or
                            TorizoCueKind.SetFunction or
                            TorizoCueKind.BlowUpGut or
                            TorizoCueKind.MarkFaceBlownUp or
                            TorizoCueKind.MakeVisible or
                            TorizoCueKind.MakeInvisible or
                            TorizoCueKind.FadeToBlack or
                            TorizoCueKind.FinishDeath or
                            TorizoCueKind.AdvancePaletteFade or
                            TorizoCueKind.FadeToNormalColors or
                            TorizoCueKind.StartBombTorizoFight or
                            TorizoCueKind.NoOperation or
                            TorizoCueKind.LockAnimation or
                            TorizoCueKind.UnlockAnimation or
                            TorizoCueKind.GoToIfFaceBlownUpElseIfGolden or
                            TorizoCueKind.Link or
                            TorizoCueKind.Return or
                            TorizoCueKind.ResumeInterrupted or
                            TorizoCueKind.SpawnLandingDust or
                            TorizoCueKind.SpawnLowHealthDrool or
                            TorizoCueKind.SetTurningAround or
                            TorizoCueKind.SteppedLeftWithLeftFoot or
                            TorizoCueKind.SteppedRightWithRightFoot or
                            TorizoCueKind.SteppedLeftWithRightFoot or
                            TorizoCueKind.SteppedRightWithLeftFoot or
                            TorizoCueKind.StandUp or
                            TorizoCueKind.SitDown or
                            TorizoCueKind.WalkBombTorizo or
                            TorizoCueKind.WalkFacelessBombTorizo or
                            TorizoCueKind.GoToIfRising or
                            TorizoCueKind.CallIfSamusCloseBehind or
                            TorizoCueKind.JumpBackIfSamusCloseInFront or
                            TorizoCueKind.CallBombTorizoAttack or
                            TorizoCueKind.SpawnBombTorizoChozoOrbs or
                            TorizoCueKind.SpawnBombTorizoSonicBoom or
                            TorizoCueKind.SpawnGoldenTorizoSonicBoom or
                            TorizoCueKind.SpawnExplosiveSwipe or
                            TorizoCueKind.PlayShotSound or
                            TorizoCueKind.PlayFootstepSound or
                            TorizoCueKind.GoToIfAirborne or
                            TorizoCueKind.LoadGoldenTorizoPalettes or
                            TorizoCueKind.StartGoldenTorizoFight or
                            TorizoCueKind.ClearCaughtSuperMissile or
                            TorizoCueKind.SpawnEgg or
                            TorizoCueKind.GoToIfEyeBeamActive or
                            TorizoCueKind.DisableEyeBeamExplosions or
                            TorizoCueKind.EnableEyeBeamExplosions or
                            TorizoCueKind.UnmarkStunned or
                            TorizoCueKind.PlayEggReleasedSound or
                            TorizoCueKind.PlayLaserSound or
                            TorizoCueKind.PlaySonicBoomSound or
                            TorizoCueKind.SpawnSuperMissile or
                            TorizoCueKind.GoToIfSamusMorphedBehind or
                            TorizoCueKind.SpawnEyeBeam or
                            TorizoCueKind.CallSometimesIfSamusMorphedInFront or
                            TorizoCueKind.CallSometimesIfHealthLow or
                            TorizoCueKind.CallIfStunHealthHigh or
                            TorizoCueKind.JumpForwardIfSamusFar or
                            TorizoCueKind.SpawnGoldenTorizoChozoOrbs or
                            TorizoCueKind.JumpBackIfSamusCloseInFrontGolden or
                            TorizoCueKind.CallGoldenTorizoAttack or
                            TorizoCueKind.WalkGoldenTorizo or
                            TorizoCueKind.CopyTiles => null,
                        _ => null,
                    };
                    if (sleep is { } updates)
                    {
                        var wait = Next("callback sleep");
                        AssertEqual(AnimationProgramStepKind.Wait, wait.Kind, $"{at}: the callback's sleep");
                        AssertEqual(updates, wait.Duration, $"{at}: the callback's sleep length");
                    }
                    // A return always redirects and saves no return point: where the native list ends, a hold closes
                    // the program, and where the next program starts, the unreached step runs on into it. A call saves
                    // the step after it, so its list always runs on.
                    if (cue.Kind is TorizoCueKind.Return or TorizoCueKind.ResumeInterrupted && ProgramAt(cursor) is null)
                    {
                        AssertEqual(AnimationProgramStepKind.Hold, Next("redirect").Kind, $"{at}: closes after its redirect");
                        break;
                    }
                    continue;
                }
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
            break;
        }
        AssertEqual(steps.Count, index, $"Torizo {program} ends where its native list does");
        return steps.Count;
    }

    /// <summary>
    /// The AI resumes a sleeping list by stepping its pointer past the sleep; <see cref="TorizoAnimations.AfterSleep"/>
    /// must name the program that starts there.
    /// </summary>
    private static void VerifyResumedSleeps(IImportCartridgeSource cartridge)
    {
        int resumed = 0;
        foreach ((TorizoBodyProgram program, ushort entry) in BodyEntries)
        {
            var steps = TorizoAnimations.Programs.Steps(program);
            if (steps[^1].Kind != AnimationProgramStepKind.Hold)
                continue;
            ushort? sleep = SleepAddress(cartridge, entry);
            if (sleep is not { } address || ProgramAt(unchecked((ushort)(address + 2))) is not { } next)
                continue;
            AssertEqual(next, TorizoAnimations.AfterSleep(program), $"Torizo {program} resumes past its sleep into {next}");
            resumed++;
        }
        AssertEqual(3, resumed, "Torizo resumes three sleeping lists");
    }

    /// <summary>The native sleep that ends the list at <paramref name="entry"/>, if it ends in one.</summary>
    private static ushort? SleepAddress(IImportCartridgeSource cartridge, ushort entry)
    {
        ushort cursor = entry;
        while (true)
        {
            if (cursor != entry && ProgramAt(cursor) is not null)
                return null;
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            if (ClassifyBodyWord(word) == BodyWordKind.TileTransfer)
            {
                cursor = unchecked((ushort)(cursor + TileTransferBytes));
                continue;
            }
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    var instruction = BodyInstructions[word];
                    // A return ends its list without a sleep.
                    if (instruction.Kind is TorizoCueKind.Return or TorizoCueKind.ResumeInterrupted)
                        return null;
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands.Length));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    return cursor;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                case NativeEnemyListDefinitions.WordKind.Stop:
                    return null;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
    }
}
