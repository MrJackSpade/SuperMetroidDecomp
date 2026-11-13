using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoNativeListAddresses;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #1289, #1600: Bomb and Golden Torizo's typed body animations against the retail cartridge. Each segment must match its
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

    /// <summary>Every body segment matches its native list, and every animation its lists' control flow.</summary>
    internal static void VerifyTorizoAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<TorizoBodySegment>().Length, BodyEntries.Length, "every typed Torizo body segment has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((TorizoBodySegment)index, BodyEntries[index].Program, "the Torizo body segments are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Torizo body entries ascend");
        Dictionary<TorizoBodySegment, NativeEnemyList<TorizoBodySegment>> lists = BodyEntries.ToDictionary(entry => entry.Program,
            entry => ReadBodyList(cartridge, entry.Program, entry.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, TorizoAnimations.StartingAt);
        int beats = lists.Values.Sum(list => list.BeatAddresses.Length);
        VerifyResumedSleeps(cartridge, lists);
        var poses = new HashSet<ushort>(TorizoAnimations.Segments.Frames.Select(TorizoFrameDefinitionsConstants.Spritemap));
        AssertEqual(TorizoFrameDefinitionsConstants.PoseCount, poses.Count, "every Torizo pose shows a distinct spritemap");
        AssertTrue(poses.SetEquals(TorizoAnimationsConstants.Spritemaps(TorizoAnimationsConstants.AllPrograms)), "the body segments show every Torizo pose");
        AssertTrue(poses.SetEquals(TorizoCollisionDefinitions.FramePointers), "every Torizo pose has its physical frame");
        Console.WriteLine($"  Torizo animations: {BodyEntries.Length} body segments ({beats} beats, {poses.Count} poses) and {TorizoAnimations.StartingAt.Count} animations match bank $AA.");
    }

    /// <summary>
    /// Reads one body segment's native list, comparing its beats with the segment's: each frame, wait, tile transfer and
    /// Torizo instruction (with the fixed sleep two callbacks schedule, a wait beat at the instruction after the callback),
    /// and each control instruction with its address and the beat it follows.
    /// </summary>
    private static NativeEnemyList<TorizoBodySegment> ReadBodyList(IImportCartridgeSource cartridge, TorizoBodySegment program, ushort entry)
    {
        IReadOnlyList<AnimationBeat<EnemyFrameId, TorizoCue>> beats = TorizoAnimations.Segments.Beats(program);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<TorizoBodySegment>>();
        ushort cursor = entry;
        AnimationBeat<EnemyFrameId, TorizoCue> Next(string what)
        {
            AssertTrue(addresses.Count < beats.Count, $"Torizo {program} has a beat for native {what} at $AA:{cursor:X4}");
            addresses.Add(cursor);
            return beats[addresses.Count - 1];
        }
        while (true)
        {
            string at = $"Torizo {program} at $AA:{cursor:X4}";
            if (cursor != entry && ProgramAt(cursor) is { } following)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, addresses.Count, following, null, cursor));
                break;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if (ClassifyBodyWord(word) == BodyWordKind.TileTransfer)
            {
                var copyBeat = AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Event>(Next("tile transfer"), $"{at}: tile transfer");
                var copy = AssertIs<TorizoCue.ShowArtwork>(copyBeat.Raised, $"{at}: artwork swap cue");
                TorizoArtworkSwapDefinition transfer = TorizoArtworkSwapPageDefinitions.Entries[copy.Swap.Ordinal];
                AssertEqual(cursor, TorizoArtworkSwapDefinitions.Sites[copy.Swap.Ordinal].Instruction, $"{at}: tile transfer descriptor");
                AssertEqual(transfer.ByteCount, cartridge.ReadCartridgeWord(address + 2), $"{at}: tile transfer size");
                AssertEqual(transfer.SourceAddress, cartridge.ReadCartridgeLongFixedBank(address + 4), $"{at}: tile transfer source");
                AssertEqual(ObjCharacterNativeWords.Of(EncounterCharacterPlacementDefinitions.TorizoDestinations[copy.Swap.Ordinal].FirstCharacter), cartridge.ReadCartridgeWord(address + 7), $"{at}: tile transfer destination");
                cursor = unchecked((ushort)(cursor + TileTransferBytes));
                continue;
            }
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                {
                    var show = AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Frame>(Next("frame"), $"{at}: frame");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(operand, TorizoFrameDefinitionsConstants.Spritemap(show.Shown), $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, addresses.Count, null, null, cursor));
                    break;
                case NativeEnemyListDefinitions.WordKind.Stop:
                    controls.Add(new(NativeEnemyControlKind.Stop, addresses.Count, null, null, cursor));
                    break;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    controls.Add(new(NativeEnemyControlKind.Goto, addresses.Count, SegmentAt(operand, at), null, cursor));
                    break;
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                    controls.Add(new(NativeEnemyControlKind.SetCounter, addresses.Count, null, operand, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                {
                    TorizoBodySegment repeated = SegmentAt(operand, at);
                    AssertEqual(program, repeated, $"{at}: a counted repeat replays its own list");
                    controls.Add(new(NativeEnemyControlKind.Repeat, addresses.Count, repeated, null, cursor));
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Wait:
                {
                    var wait = AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Pause>(Next("wait"), $"{at}: wait");
                    AssertEqual(operand, wait.Duration, $"{at}: wait length");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    TorizoCue cue = AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Event>(Next("instruction"), $"{at}: instruction").Raised;
                    instruction.Cue.Check(cue, at);
                    for (int operandIndex = 0; operandIndex < instruction.Operands.Length; operandIndex++)
                    {
                        ushort value = cartridge.ReadCartridgeWord(address + 2 + 2 * operandIndex);
                        TorizoOperand role = instruction.Operands[operandIndex];
                        ushort typed = role switch
                        {
                            TorizoOperand.Value => checked((ushort)(cue switch
                            {
                                // The cartridge's operand is the step's byte offset into its word table.
                                TorizoCue.Posture posture => posture.Step * sizeof(ushort),
                                TorizoCue.Walk walk => walk.Step * sizeof(ushort),
                                // A swipe's operand is its placement's byte offset; a low-health explosion's selects
                                // the gut's or the face's rows, six bytes apart.
                                TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnExplosiveSwipe } swipe => swipe.Parameter * sizeof(ushort),
                                TorizoCue.LowHealthExplosions explosions => (explosions.Site == BombTorizoExplosionSite.Gut ? 0 : 1) * 6,
                                TorizoCue.ParameterizedSpawn spawn => spawn.Parameter,
                                _ => throw new InvalidOperationException($"{at}: {cue} carries no value."),
                            })),
                            TorizoOperand.Function => TorizoNativeWords.Functions.Word(AssertIs<TorizoCue.SetFunction>(cue, $"{at}: cue").Function),
                            TorizoOperand.FrameBehavior => TorizoNativeWords.FrameBehaviors.Word(AssertIs<TorizoCue.SetFrameBehavior>(cue, $"{at}: cue").FrameBehavior),
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
                        TorizoCue.LowHealthExplosions => LowHealthExplosionSleep,
                        TorizoCue.Signal { Kind: TorizoCueKind.SpawnDeathExplosion } => DeathExplosionSleep,
                        _ => null,
                    };
                    if (sleep is { } updates)
                    {
                        var wait = AssertIs<AnimationBeat<EnemyFrameId, TorizoCue>.Pause>(Next("callback sleep"), $"{at}: the callback's sleep");
                        AssertEqual(updates, wait.Duration, $"{at}: the callback's sleep length");
                    }
                    // A return always redirects and saves no return point: where the native list ends, the animation
                    // holds there, and where the next list starts, the unreached end runs on into it. A call saves the
                    // place after it, so its list always runs on.
                    if (cue is TorizoCue.Signal { Kind: TorizoCueKind.Return or TorizoCueKind.ResumeInterrupted } && ProgramAt(cursor) is null)
                    {
                        controls.Add(new(NativeEnemyControlKind.Sleep, addresses.Count, null, null, cursor));
                        break;
                    }
                    continue;
                }
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
            break;
        }
        AssertEqual(beats.Count, addresses.Count, $"Torizo {program} holds each of its beats in its native list");
        return new NativeEnemyList<TorizoBodySegment>([.. addresses], [.. controls]);
    }

    /// <summary>The body segment whose native list starts at <paramref name="address"/>, failing when none does.</summary>
    private static TorizoBodySegment SegmentAt(ushort address, string at) =>
        ProgramAt(address) ?? throw new InvalidDataException($"{at}: ${address:X4} is no body segment's native entry.");

    /// <summary>
    /// The AI resumes a sleeping list by stepping its pointer past the sleep; <see cref="TorizoAnimations.AfterSleep"/>
    /// must name the segment that starts there.
    /// </summary>
    private static void VerifyResumedSleeps(IImportCartridgeSource cartridge, Dictionary<TorizoBodySegment, NativeEnemyList<TorizoBodySegment>> lists)
    {
        int resumed = 0;
        foreach ((TorizoBodySegment program, ushort entry) in BodyEntries)
        {
            if (lists[program].Controls.LastOrDefault() is not { Kind: NativeEnemyControlKind.Sleep })
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
                    if (instruction.Cue.Matches(new TorizoCue.Signal(TorizoCueKind.Return)) ||
                        instruction.Cue.Matches(new TorizoCue.Signal(TorizoCueKind.ResumeInterrupted)))
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
