using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
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
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Torizo body entries ascend");
        int steps = 0;
        foreach ((TorizoBodyProgram program, ushort entry) in BodyEntries)
            steps += CompareBodyProgram(cartridge, program, entry);
        VerifyResumedSleeps(cartridge);
        var poses = new HashSet<ushort>(Enumerable.Range(0, TorizoFrameDefinitionsConstants.PoseCount)
            .Select(pose => TorizoFrameDefinitionsConstants.Spritemap(new TorizoFrame(pose))));
        AssertEqual(TorizoFrameDefinitionsConstants.PoseCount, poses.Count, "every Torizo pose shows a distinct spritemap");
        AssertTrue(poses.SetEquals(TorizoAnimationsConstants.Spritemaps(TorizoAnimationsConstants.AllPrograms)), "the body programs show every Torizo pose");
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
                var fall = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.FallIntoStep>(Next("fall-through"), $"{at}: runs on into the next authored program");
                AssertEqual(following, fall.Target, $"{at}: fall-through target");
                break;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if (ClassifyBodyWord(word) == BodyWordKind.TileTransfer)
            {
                var copyStep = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.CueStep>(Next("tile transfer"), $"{at}: tile transfer");
                var copy = AssertIs<TorizoCue.ShowArtwork>(copyStep.Cue, $"{at}: artwork swap cue");
                TorizoArtworkSwapDefinition transfer = TorizoArtworkSwapDefinitions.Entries[copy.Swap.Ordinal];
                AssertEqual(cursor, TorizoArtworkSwapDefinitions.Sites[copy.Swap.Ordinal].Instruction, $"{at}: tile transfer descriptor");
                AssertEqual(transfer.ByteCount, cartridge.ReadCartridgeWord(address + 2), $"{at}: tile transfer size");
                AssertEqual(transfer.SourceAddress, cartridge.ReadCartridgeLongFixedBank(address + 4), $"{at}: tile transfer source");
                AssertEqual(ObjCharacterNativeWords.Of(PlacedUploads.Of(new TorizoArtworkSwapCharacters(copy.Swap.Ordinal)).FirstByte / 32), cartridge.ReadCartridgeWord(address + 7), $"{at}: tile transfer destination");
                cursor = unchecked((ushort)(cursor + TileTransferBytes));
                continue;
            }
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                {
                    var show = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.ShowStep>(Next("frame"), $"{at}: frame");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(operand, TorizoFrameDefinitionsConstants.Spritemap(show.Frame), $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    _ = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.HoldStep>(Next("sleep"), $"{at}: sleep");
                    break;
                case NativeEnemyListDefinitions.WordKind.Stop:
                    _ = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.StopStep>(Next("stop"), $"{at}: stop");
                    break;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                {
                    var jump = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.GoToStep>(Next("goto"), $"{at}: goto");
                    AssertEqual(operand, Entry(jump.Target), $"{at}: goto target");
                    break;
                }
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                {
                    var load = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.SetCounterStep>(Next("counter load"), $"{at}: counter load");
                    AssertEqual(operand, load.Count, $"{at}: counter");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                {
                    var repeat = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.RepeatWhileCountingStep>(Next("counted repeat"), $"{at}: counted repeat");
                    AssertEqual(operand, Entry(repeat.Target), $"{at}: repeat target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Wait:
                {
                    var wait = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.WaitStep>(Next("wait"), $"{at}: wait");
                    AssertEqual(operand, wait.Duration, $"{at}: wait length");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    TorizoCue cue = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.CueStep>(Next("instruction"), $"{at}: instruction").Cue;
                    instruction.Cue.Check(cue, at);
                    for (int operandIndex = 0; operandIndex < instruction.Operands.Length; operandIndex++)
                    {
                        ushort value = cartridge.ReadCartridgeWord(address + 2 + 2 * operandIndex);
                        TorizoOperand role = instruction.Operands[operandIndex];
                        ushort typed = role switch
                        {
                            TorizoOperand.Value => checked((ushort)(cue switch
                            {
                                TorizoCue.Posture posture => posture.MapOffset,
                                TorizoCue.Walk walk => walk.TableOffset,
                                TorizoCue.ParameterizedSpawn spawn => spawn.Parameter,
                                _ => throw new InvalidOperationException($"{at}: {cue} carries no value."),
                            })),
                            TorizoOperand.Function => TorizoNativeWords.Functions.Word(AssertIs<TorizoCue.SetFunction>(cue, $"{at}: cue").Function),
                            TorizoOperand.PreInstruction => TorizoNativeWords.PreInstructions.Word(AssertIs<TorizoCue.SetPreInstruction>(cue, $"{at}: cue").PreInstruction),
                            TorizoOperand.Target => Entry(cue switch
                            {
                                TorizoCue.Branch branch => branch.Target,
                                TorizoCue.Choice choice => choice.Target,
                                _ => throw new InvalidOperationException($"{at}: {cue} carries no target."),
                            }),
                            TorizoOperand.Alternate => Entry(AssertIs<TorizoCue.Choice>(cue, $"{at}: cue").Alternate),
                            _ => throw new InvalidOperationException($"Undefined {nameof(TorizoOperand)} {role}."),
                        };
                        AssertEqual(value, typed, $"{at}: {role} operand");
                    }
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands.Length));
                    // Two callbacks sleep after they run; every other callback runs straight on.
                    int? sleep = cue switch
                    {
                        TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnLowHealthExplosions } => LowHealthExplosionSleep,
                        TorizoCue.Command { Kind: TorizoCueKind.SpawnDeathExplosion } => DeathExplosionSleep,
                        _ => null,
                    };
                    if (sleep is { } updates)
                    {
                        var wait = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.WaitStep>(Next("callback sleep"), $"{at}: the callback's sleep");
                        AssertEqual(updates, wait.Duration, $"{at}: the callback's sleep length");
                    }
                    // A return always redirects and saves no return point: where the native list ends, a hold closes
                    // the program, and where the next program starts, the unreached step runs on into it. A call saves
                    // the step after it, so its list always runs on.
                    if (cue is TorizoCue.Command { Kind: TorizoCueKind.Return or TorizoCueKind.ResumeInterrupted } && ProgramAt(cursor) is null)
                    {
                        _ = AssertIs<AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.HoldStep>(Next("redirect"), $"{at}: closes after its redirect");
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
            if (steps[^1] is not AnimationProgramStep<TorizoBodyProgram, TorizoFrame, TorizoCue>.HoldStep)
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
                    if (instruction.Cue.Matches(new TorizoCue.Command(TorizoCueKind.Return)) ||
                        instruction.Cue.Matches(new TorizoCue.Command(TorizoCueKind.ResumeInterrupted)))
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
