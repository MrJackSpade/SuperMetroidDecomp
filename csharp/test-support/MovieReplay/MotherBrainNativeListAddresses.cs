using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodyCue;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadCue;
using K = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadCueKind;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;

/// <summary>
/// The native lists Mother Brain's ROM oracle (#1285, #1600) reads: every typed segment's entry point, the bank-$A9
/// opcodes with the events each must raise, and the reader that compares a typed segment with its native list and the
/// typed animations with the lists' control flow. Only verification reads them; the port plays typed animations. The
/// lists it reads also decode a native snapshot's list pointers into typed playbacks.
/// </summary>
internal static class MotherBrainNativeListAddresses
{
    /// <summary>Mother Brain's bank, $A9.</summary>
    internal const int MotherBrainBank = 0xa90000;
    /// <summary>The Zebetites' bank, $A6.</summary>
    internal const int ZebetiteBank = 0xa60000;

    /// <summary>Each body program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<MotherBrainBodySegment>[] BodyEntries =
    [
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkForwardReallyFast, 0x9730),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkForwardFast, 0x976a),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkForwardMedium, 0x97a4),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkForwardSlow, 0x97de),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkForwardReallySlow, 0x9818),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkBackwardSlow, 0x9852),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkBackwardReallyFast, 0x988c),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkBackwardFast, 0x98c6),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkBackwardMedium, 0x9900),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.WalkBackwardReallySlow, 0x993a),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.CrouchAndThenStandUp, 0x9974),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.StandAfterCrouchSlow, 0x99aa),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.StandAfterCrouchFast, 0x99c6),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.StandAfterLeaning, 0x99e2),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.LeanDown, 0x99f2),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.Crouched, 0x9a02),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.CrouchSlow, 0x9a0a),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.CrouchFast, 0x9a26),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.HandBeam, 0x9a42),
        new NativeProgramEntry<MotherBrainBodySegment>(MotherBrainBodySegment.InitialDummy, 0x9c13),
    ];

    /// <summary>Each brain program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<MotherBrainHeadSegment>[] HeadEntries =
    [
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.StretchingPhase2, 0x9b7f),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.StretchingPhase2Loop, 0x9bab),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.StretchingPhase3, 0x9bb3),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.StretchingPhase3Loop, 0x9bdf),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.HyperBeamRecoil, 0x9be7),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.HyperBeamRecoilLoop, 0x9c0b),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.Initial, 0x9c21),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.Decapitated, 0x9c29),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.DecapitatedLoop, 0x9c31),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.DyingDrool, 0x9c39),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.DyingDroolBurst, 0x9c47),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.DyingDroolLoop, 0x9c5f),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.FiringRainbowBeam, 0x9c77),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.NeutralPhase2, 0x9c87),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.NeutralPhase2Hold, 0x9c9f),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.NeutralPhase3, 0x9cb9),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.NeutralPhase3Hold, 0x9cd1),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.Corpse, 0x9d25),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.CorpseLoop, 0x9d35),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.FourOnionRingsPhase2, 0x9d3d),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.TwoOnionRingsPhase2, 0x9d7f),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.AttackingBabyMetroid, 0x9db1),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.AttackingSamusFourOnionRingsPhase3, 0x9dbb),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.FourOnionRingsPhase3, 0x9dc1),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.BombPhase2, 0x9ecc),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.BombPhase3, 0x9f00),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.Laser, 0x9f34),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.ChargingRainbowBeam, 0x9f6c),
        new NativeProgramEntry<MotherBrainHeadSegment>(MotherBrainHeadSegment.ChargingRainbowBeamLoop, 0x9f7a),
    ];

    /// <summary>Each cutscene Baby program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<MotherBrainBabySegment>[] BabyEntries =
    [
        new NativeProgramEntry<MotherBrainBabySegment>(MotherBrainBabySegment.Initial, 0xcfa2),
        new NativeProgramEntry<MotherBrainBabySegment>(MotherBrainBabySegment.DrainingMotherBrain, 0xcfb8),
        new NativeProgramEntry<MotherBrainBabySegment>(MotherBrainBabySegment.TakingFatalBlow, 0xcfce),
    ];

    /// <summary>Each Shitroid program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<ShitroidSegment>[] ShitroidEntries =
    [
        new NativeProgramEntry<ShitroidSegment>(ShitroidSegment.FinishDraining, 0xf906),
        new NativeProgramEntry<ShitroidSegment>(ShitroidSegment.Normal, 0xf90e),
        new NativeProgramEntry<ShitroidSegment>(ShitroidSegment.LatchedOn, 0xf924),
        new NativeProgramEntry<ShitroidSegment>(ShitroidSegment.Remorse, 0xf93a),
    ];

    /// <summary>The bank-$A9 opcodes of body programs and the body steps each becomes, with its operand words.</summary>
    private static readonly Dictionary<ushort, BodyOp> BodyOps = new()
    {
        [0x95fc] = new BodyOp(0, _ => new B.Move(new(1, -2))),
        [0x95b6] = new BodyOp(0, _ => new B.MoveWithScrollBias(-10, 4)),
        [0x95c0] = new BodyOp(0, _ => new B.MoveWithScrollBias(-16, 4)),
        [0x95ca] = new BodyOp(0, _ => new B.MoveWithScrollBias(-12, -2)),
        [0x95de] = new BodyOp(0, _ => new B.MoveWithScrollBias(12, 4)),
        [0x95e8] = new BodyOp(0, _ => new B.MoveWithScrollBias(16, -2)),
        [0x95f2] = new BodyOp(0, _ => new B.MoveWithScrollBias(10, -2)),
        [0x960c] = new BodyOp(0, _ => new B.Move(new(2, 0))),
        [0x961c] = new BodyOp(0, _ => new B.Move(new(0, 1))),
        [0x9622] = new BodyOp(0, _ => new B.StepAndMove(new(3, 1))),
        [0x9638] = new BodyOp(0, _ => new B.Move(new(15, -2))),
        [0x9648] = new BodyOp(0, _ => new B.Move(new(6, -4))),
        [0x9658] = new BodyOp(0, _ => new B.Move(new(-2, 4))),
        [0x9668] = new BodyOp(0, _ => new B.StepAndMove(new(-1, 2))),
        [0x967e] = new BodyOp(0, _ => new B.StepAndMove(new(-1, 2))),
        [0x9694] = new BodyOp(0, _ => new B.Move(new(-2, 0))),
        [0x96a4] = new BodyOp(0, _ => new B.Move(new(0, -1))),
        [0x96aa] = new BodyOp(0, _ => new B.Move(new(-3, -1))),
        [0x96ba] = new BodyOp(0, _ => new B.StepAndMove(new(-15, 2))),
        [0x96d0] = new BodyOp(0, _ => new B.Move(new(-6, 4))),
        [0x96e0] = new BodyOp(0, _ => new B.Move(new(2, -4))),
        [0x96f0] = new BodyOp(0, _ => new B.Move(new(1, -2))),
        [0x9700] = new BodyOp(0, _ => new B.SetPose(MotherBrainBodyPose.Standing)),
        [0x9708] = new BodyOp(0, _ => new B.SetPose(MotherBrainBodyPose.Walking)),
        [0x9710] = new BodyOp(0, _ => new B.SetPose(MotherBrainBodyPose.Crouched)),
        [0x9718] = new BodyOp(0, _ => new B.SetPose(MotherBrainBodyPose.CrouchingTransition)),
        [0x9720] = new BodyOp(0, _ => new B.SetPose(MotherBrainBodyPose.DeathBeam)),
        [0x9728] = new BodyOp(0, _ => new B.SetPose(MotherBrainBodyPose.LeaningDown)),
        [0x9ac8] = new BodyOp(3, operands => new B.SpawnHandDust(new(unchecked((short)operands[0]), unchecked((short)operands[1])), (MiscDustAnimation)operands[2])),
        [0x9aef] = new BodyOp(0, _ => B.FireHandBeam),
        [0x9b05] = new BodyOp(0, _ => B.AdvanceHandBeamPhase),
    };

    /// <summary>The bank-$A9 brain opcodes that become a single cue, with their operand words.</summary>
    private static readonly Dictionary<ushort, HeadCueOp> HeadCueOps = new()
    {
        [0x9ea3] = new HeadCueOp(0, _ => H.Of(K.CountBabyMetroidAttack)),
        [0x9eb5] = new HeadCueOp(0, _ => H.Of(K.ResetBabyMetroidAttacks)),
        [0x9b20] = new HeadCueOp(0, _ => H.Of(K.DisableNeckMovement)),
        [0x9e37] = new HeadCueOp(0, _ => H.Of(K.AimOnionRingsAtBabyMetroid)),
        [0x9e5b] = new HeadCueOp(0, _ => H.Of(K.AimOnionRingsAtSamus)),
        [0x9df7] = new HeadCueOp(0, _ => H.Of(K.QueueBabyMetroidAttackSound)),
        [0x9e29] = new HeadCueOp(0, _ => H.Of(K.SpawnOnionRing)),
        [0x9b32] = new HeadCueOp(1, operands => H.Sound(SoundEffectCommands.Decode(SoundEffectLibrary.Room, operands[0]))),
        [0x9b28] = new HeadCueOp(1, operands => H.Sound(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[0]))),
        [0x9ebd] = new HeadCueOp(1, operands => H.Bomb(operands[0])),
        [0x9b6d] = new HeadCueOp(0, _ => H.Of(K.SpawnBigPurpleBreath)),
        [0x9d0d] = new HeadCueOp(0, _ => H.Of(K.MaybeHoldNeutralPhase3)),
        [0x9b3c] = new HeadCueOp(0, _ => H.Of(K.SpawnDrool)),
        [0x9b77] = new HeadCueOp(0, _ => H.Of(K.ShakeBrain)),
        [0x9cad] = new HeadCueOp(0, _ => H.Of(K.MaybeHoldNeutralPhase2)),
        [0x9f46] = new HeadCueOp(0, _ => H.Of(K.SpawnLaser)),
        [0x9f84] = new HeadCueOp(0, _ => H.Of(K.SpawnRainbowBeamChargeParticle)),
        [0x9f8e] = new HeadCueOp(0, _ => H.Of(K.PrepareRainbowBeamCharge)),
    };

    /// <summary>Each Zebetite health program's native entry point in bank $A6, six bytes apart.</summary>
    internal static readonly NativeProgramEntry<ZebetiteSegment>[] ZebetiteEntries =
    [
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.LargeAtLeast800, 0xfdcc),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.LargeAtLeast600, 0xfdd2),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.LargeAtLeast400, 0xfdd8),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.LargeAtLeast200, 0xfdde),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.LargeBelow200, 0xfde4),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.SplitAtLeast800, 0xfdea),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.SplitAtLeast600, 0xfdf0),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.SplitAtLeast400, 0xfdf6),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.SplitAtLeast200, 0xfdfc),
        new NativeProgramEntry<ZebetiteSegment>(ZebetiteSegment.SplitBelow200, 0xfe02),
    ];

    /// <summary>Each falling tube's pose list in bank $A9, six bytes apart.</summary>
    internal static readonly NativeProgramEntry<MotherBrainTubePiece>[] TubeEntries =
    [
        new NativeProgramEntry<MotherBrainTubePiece>(MotherBrainTubePiece.BottomLeft, 0x8c69),
        new NativeProgramEntry<MotherBrainTubePiece>(MotherBrainTubePiece.BottomRight, 0x8c6f),
        new NativeProgramEntry<MotherBrainTubePiece>(MotherBrainTubePiece.BottomMiddleLeft, 0x8c75),
        new NativeProgramEntry<MotherBrainTubePiece>(MotherBrainTubePiece.BottomMiddleRight, 0x8c7b),
        new NativeProgramEntry<MotherBrainTubePiece>(MotherBrainTubePiece.Main, 0x8c81),
    ];

    /// <summary>Each falling tube's sixteen-byte population record, as a long address.</summary>
    private static readonly Dictionary<MotherBrainTubePiece, int> TubePopulationRecords = new()
    {
        [MotherBrainTubePiece.BottomLeft] = 0xa98ae5,
        [MotherBrainTubePiece.BottomRight] = 0xa98af5,
        [MotherBrainTubePiece.BottomMiddleLeft] = 0xa98b05,
        [MotherBrainTubePiece.BottomMiddleRight] = 0xa98b15,
        [MotherBrainTubePiece.Main] = 0xa98b25,
    };

    /// <summary>The bank-$A9 population record of <paramref name="piece"/>, as a long address.</summary>
    internal static int TubePopulationRecord(MotherBrainTubePiece piece) => TubePopulationRecords[piece];

    /// <summary>
    /// What a native opcode of these lists becomes (#1600): its operand words, the events it raises, and the list it
    /// continues at when it ends its own with a goto.
    /// </summary>
    /// <param name="Operands">The opcode's operand words.</param>
    /// <param name="Effect">What the opcode does, given its operands and the entry-to-segment lookup.</param>
    internal readonly record struct NativeListOp<TSegment, TEvent>(
        int Operands, Func<ushort[], Func<ushort, TSegment>, NativeListOpEffect<TSegment, TEvent>> Effect)
        where TSegment : struct, Enum
        where TEvent : notnull;

    /// <summary>The events a native opcode raises, in order, and the list it then continues at, if it ends its own.</summary>
    /// <param name="Events">The events raised.</param>
    /// <param name="GoesTo">The list a goto continues at, or null when the list runs on past the opcode.</param>
    internal sealed record NativeListOpEffect<TSegment, TEvent>(TEvent[] Events, TSegment? GoesTo)
        where TSegment : struct, Enum
        where TEvent : notnull;

    /// <summary>
    /// Reads each segment's native list beside its typed beats (#1600): frames must show the native spritemap for the
    /// native duration, opcodes must raise the events <paramref name="ops"/> names, and the lists' sleeps, gotos and runs on
    /// into the next authored entry are recorded as their control instructions. Every typed animation must then play the
    /// phases the lists' control flow plays. Returns each segment's native list, which also maps a native snapshot's list
    /// pointers to typed playbacks.
    /// </summary>
    internal static Dictionary<TSegment, NativeEnemyList<TSegment>> Lists<TSegment, TFrame, TEvent>(
        IImportCartridgeSource cartridge, int bank, AnimationSegments<TSegment, TFrame, TEvent> segments,
        IReadOnlyDictionary<TSegment, Animation<TSegment>> startingAt,
        NativeProgramEntry<TSegment>[] entries, Func<TFrame, ushort> spritemap,
        IReadOnlyDictionary<ushort, NativeListOp<TSegment, TEvent>> ops)
        where TSegment : struct, Enum
        where TFrame : struct
        where TEvent : notnull
    {
        VerificationAssertions.AssertEqual(Enum.GetValues<TSegment>().Length, entries.Length, $"every typed {typeof(TSegment).Name} has a native entry");
        Dictionary<ushort, TSegment> segmentAt = entries.ToDictionary(entry => entry.Entry, entry => entry.Program);
        var lists = new Dictionary<TSegment, NativeEnemyList<TSegment>>();
        foreach ((TSegment segment, ushort entry) in entries)
        {
            IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats = segments.Beats(segment);
            var addresses = new List<ushort>();
            var controls = new List<NativeEnemyControl<TSegment>>();
            ushort cursor = entry;
            for (int record = 0; controls.Count == 0; record++)
            {
                if (record > 256)
                    throw new InvalidDataException($"{segment} ran past its native list.");
                string at = $"{segment} beat {addresses.Count} (native record {record})";
                if (record > 0 && segmentAt.TryGetValue(cursor, out TSegment next))
                {
                    controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, addresses.Count, next, null, cursor));
                    continue;
                }
                ushort word = cartridge.ReadCartridgeWord(bank | cursor);
                switch (NativeEnemyListDefinitions.Classify(word))
                {
                    case NativeEnemyListDefinitions.WordKind.Frame:
                        var frame = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Frame>(Beat(beats, addresses.Count, at), $"{at}: frame");
                        VerificationAssertions.AssertEqual(word, frame.Duration, $"{at}: duration");
                        VerificationAssertions.AssertEqual(cartridge.ReadCartridgeWord(bank | (cursor + 2)), spritemap(frame.Shown), $"{at}: spritemap");
                        addresses.Add(cursor);
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case NativeEnemyListDefinitions.WordKind.Sleep:
                        controls.Add(new(NativeEnemyControlKind.Sleep, addresses.Count, null, null, cursor));
                        continue;
                    case NativeEnemyListDefinitions.WordKind.Callback:
                    {
                        if (!ops.TryGetValue(word, out NativeListOp<TSegment, TEvent> op))
                            throw new InvalidDataException($"{at}: opcode ${word:X4} is outside the compared set.");
                        ushort[] operands = [.. Enumerable.Range(0, op.Operands)
                            .Select(operand => cartridge.ReadCartridgeWord(bank | (cursor + 2 + 2 * operand)))];
                        NativeListOpEffect<TSegment, TEvent> effect = op.Effect(operands, address => segmentAt[address]);
                        foreach (TEvent raised in effect.Events)
                        {
                            var typed = VerificationAssertions.AssertIs<AnimationBeat<TFrame, TEvent>.Event>(Beat(beats, addresses.Count, at), $"{at}: opcode ${word:X4} event");
                            VerificationAssertions.AssertEqual(raised, typed.Raised, $"{at}: opcode ${word:X4} event");
                            addresses.Add(cursor);
                        }
                        ushort after = unchecked((ushort)(cursor + 2 + 2 * op.Operands));
                        // A goto that raises nothing is where the pointer rests after the beat before it; one that raises an
                        // event first is never a resting place, so it stands after its operands.
                        if (effect.GoesTo is { } target)
                            controls.Add(new(NativeEnemyControlKind.Goto, addresses.Count, target, null, effect.Events.Length == 0 ? cursor : after));
                        cursor = after;
                        continue;
                    }
                    case NativeEnemyListDefinitions.WordKind.GoTo:
                    case NativeEnemyListDefinitions.WordKind.Stop:
                    case NativeEnemyListDefinitions.WordKind.SetCounter:
                    case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                    case NativeEnemyListDefinitions.WordKind.Wait:
                        throw new InvalidDataException($"{at}: these lists use no common gotos, stops, counters or waits.");
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
                }
            }
            VerificationAssertions.AssertEqual(beats.Count, addresses.Count, $"{segment}: its native list holds each of its beats");
            lists[segment] = new NativeEnemyList<TSegment>([.. addresses], [.. controls]);
        }
        NativeEnemyAnimationOracle.CompareAnimations(lists, startingAt);
        return lists;
    }

    /// <summary>Beat <paramref name="beat"/> of <paramref name="beats"/>, which the native list must not outrun.</summary>
    private static AnimationBeat<TFrame, TEvent> Beat<TFrame, TEvent>(IReadOnlyList<AnimationBeat<TFrame, TEvent>> beats, int beat, string at)
        where TFrame : struct
        where TEvent : notnull =>
        beat < beats.Count ? beats[beat] : throw new InvalidDataException($"{at}: the native list holds more beats than its segment.");

    /// <summary>The body opcodes, each a single event.</summary>
    private static Dictionary<ushort, NativeListOp<MotherBrainBodySegment, B>> BodyNativeOps { get; } =
        BodyOps.ToDictionary(pair => pair.Key, pair => new NativeListOp<MotherBrainBodySegment, B>(
            pair.Value.Operands, (operands, _) => new([pair.Value.Cue(operands)], null)));

    /// <summary>The brain opcodes: single events, and the gotos that end a list.</summary>
    private static Dictionary<ushort, NativeListOp<MotherBrainHeadSegment, H>> HeadNativeOps { get; } = BuildHeadNativeOps();

    /// <summary>Adds <c>MotherBrain_GotoX</c>, the neck-enabling goto and the dying-drool pick to the brain's event opcodes.</summary>
    private static Dictionary<ushort, NativeListOp<MotherBrainHeadSegment, H>> BuildHeadNativeOps()
    {
        var ops = HeadCueOps.ToDictionary(pair => pair.Key, pair => new NativeListOp<MotherBrainHeadSegment, H>(
            pair.Value.Operands, (operands, _) => new([pair.Value.Cue(operands)], null)));
        // $A9:9B0F, MotherBrain_GotoX.
        ops[0x9b0f] = new(1, (operands, segmentAt) => new([], segmentAt(operands[0])));
        // $A9:9B14, MotherBrainHead_EnableNeckMovement_GotoX.
        ops[0x9b14] = new(1, (operands, segmentAt) => new([H.Of(K.EnableNeckMovement)], segmentAt(operands[0])));
        // $A9:9C65, MotherBrainHead_GotoDyingDroolInstList: it picks the burst or the loop itself.
        ops[0x9c65] = new(0, (_, _) => new([H.Of(K.MaybeDroolAgain)], MotherBrainHeadSegment.DyingDroolLoop));
        return ops;
    }

    /// <summary>The cutscene Baby's opcodes: its two same-bank gotos.</summary>
    private static Dictionary<ushort, NativeListOp<MotherBrainBabySegment, NoAnimationCue>> BabyNativeOps { get; } = new()
    {
        [0xcfb4] = new(0, (_, _) => new([], MotherBrainBabySegment.Initial)),
        [0xcfca] = new(0, (_, _) => new([], MotherBrainBabySegment.DrainingMotherBrain)),
    };

    /// <summary>The Shitroid's opcodes: three gotos and the random remorse cry, whose goto operand is the remorse start; without the cry it goes to that operand.</summary>
    private static Dictionary<ushort, NativeListOp<ShitroidSegment, ShitroidCue>> ShitroidNativeOps { get; } = new()
    {
        [0xf920] = new(0, (_, _) => new([], ShitroidSegment.Normal)),
        [0xf936] = new(0, (_, _) => new([], ShitroidSegment.LatchedOn)),
        [0xf990] = new(0, (_, _) => new([], ShitroidSegment.Remorse)),
        [0xf994] = new(1, (operands, segmentAt) => segmentAt(operands[0]) == ShitroidSegment.Remorse
            ? new([ShitroidCue.MaybeRemorseCry], null)
            : throw new InvalidDataException("The remorse cry's goto operand is not the remorse start.")),
    };

    /// <summary>Compares every typed body segment and animation with its native list; returns each body segment's native list.</summary>
    internal static Dictionary<MotherBrainBodySegment, NativeEnemyList<MotherBrainBodySegment>> CompareBody(IImportCartridgeSource cartridge) =>
        Lists(cartridge, MotherBrainBank, MotherBrainAnimations.BodySegments, MotherBrainAnimations.BodyStartingAt, BodyEntries,
            frame => MotherBrainBodyVisualDefinitionsConstants.Frame(frame.Index).Pointer, BodyNativeOps);

    /// <summary>Compares every typed brain segment and animation with its native list; returns each brain segment's native list.</summary>
    internal static Dictionary<MotherBrainHeadSegment, NativeEnemyList<MotherBrainHeadSegment>> CompareHead(IImportCartridgeSource cartridge) =>
        Lists(cartridge, MotherBrainBank, MotherBrainAnimations.HeadSegments, MotherBrainAnimations.HeadStartingAt, HeadEntries,
            frame => MotherBrainVisualDefinitionsConstants.Frame(frame.Index).Pointer, HeadNativeOps);

    /// <summary>Compares the cutscene Baby's typed segments and animations with their native lists.</summary>
    internal static void CompareBaby(IImportCartridgeSource cartridge) =>
        Lists(cartridge, MotherBrainBank, MotherBrainBabyAnimations.Segments, MotherBrainBabyAnimations.StartingAt, BabyEntries,
            frame => BabyMetroidVisualToolingDefinitions.Spritemap(frame.Index), BabyNativeOps);

    /// <summary>Compares the Shitroid's typed segments and animations with their native lists.</summary>
    internal static void CompareShitroid(IImportCartridgeSource cartridge) =>
        Lists(cartridge, MotherBrainBank, ShitroidAnimations.Segments, ShitroidAnimations.StartingAt, ShitroidEntries,
            frame => BabyMetroidVisualToolingDefinitions.Spritemap(frame.Index), ShitroidNativeOps);

    /// <summary>Compares the Zebetites' typed health segments and animations with their native lists.</summary>
    internal static void CompareZebetites(IImportCartridgeSource cartridge) =>
        Lists(cartridge, ZebetiteBank, ZebetiteAnimations.Segments, ZebetiteAnimations.StartingAt, ZebetiteEntries,
            frame => ZebetiteVisualToolingDefinitions.Spritemap(frame.Index), new Dictionary<ushort, NativeListOp<ZebetiteSegment, NoAnimationCue>>());

    /// <summary>Compares the falling tubes' typed segments and animations with their native lists; returns each piece's native list.</summary>
    internal static Dictionary<MotherBrainTubePiece, NativeEnemyList<MotherBrainTubePiece>> CompareTubes(IImportCartridgeSource cartridge) =>
        Lists(cartridge, MotherBrainBank, MotherBrainTubeAnimations.Segments, MotherBrainTubeAnimations.StartingAt, TubeEntries,
            frame => EnemyFrameCartridgeLookupTooling.Word(MotherBrainVisualDefinitions.TubeFrames.Of(frame)),
            new Dictionary<ushort, NativeListOp<MotherBrainTubePiece, NoAnimationCue>>());

    /// <summary>Each glass program's native list in bank $86.</summary>
    internal static readonly Dictionary<MotherBrainGlassSegment, ushort> GlassEntries = new()
    {
        [MotherBrainGlassSegment.ShardGroup0] = 0xcc93,
        [MotherBrainGlassSegment.ShardGroup1] = 0xccb7,
        [MotherBrainGlassSegment.ShardGroup2] = 0xccdb,
        [MotherBrainGlassSegment.ShardGroup3] = 0xccff,
        [MotherBrainGlassSegment.ShardGroup4] = 0xcd23,
        [MotherBrainGlassSegment.ShardGroup5] = 0xcd47,
        [MotherBrainGlassSegment.ShardGroup6] = 0xcd6b,
        [MotherBrainGlassSegment.ShardGroup7] = 0xcd8f,
        [MotherBrainGlassSegment.Sparkle] = 0xcdb3,
    };

    /// <summary>
    /// Each direction's turret pose list and bullet pose list in bank $86, and the long address of its entry in the
    /// bullet direction selector's table, which follows the selector's opcode at $86:C131.
    /// </summary>
    internal static readonly TurretEntry[] TurretEntries =
    [
        new TurretEntry(MotherBrainTurretDirection.Left, 0xc101, 0xc143, 0x86c133),
        new TurretEntry(MotherBrainTurretDirection.DownLeft, 0xc107, 0xc149, 0x86c135),
        new TurretEntry(MotherBrainTurretDirection.Down, 0xc10d, 0xc14f, 0x86c137),
        new TurretEntry(MotherBrainTurretDirection.DownRight, 0xc113, 0xc155, 0x86c139),
        new TurretEntry(MotherBrainTurretDirection.Right, 0xc119, 0xc15b, 0x86c13b),
        new TurretEntry(MotherBrainTurretDirection.UpRight, 0xc11f, 0xc161, 0x86c13d),
        new TurretEntry(MotherBrainTurretDirection.Up, 0xc125, 0xc167, 0x86c13f),
        new TurretEntry(MotherBrainTurretDirection.UpLeft, 0xc12b, 0xc16d, 0x86c141),
    ];
    /// <summary>The bullet impact smoke list at $86:C19A.</summary>
    internal const ushort BulletImpact = 0xc19a;
    /// <summary>The hand beam list at $86:C796.</summary>
    internal const ushort HandBeamProgram = 0xc796;

    /// <summary>The hand beam's external calls and the cue each must become.</summary>
    internal static readonly Dictionary<int, MotherBrainHandBeamCue> HandBeamCalls = new()
    {
        [0x86c7fb] = MotherBrainHandBeamCue.SpawnNextBeam,
    };

    /// <summary>The fake-death room flash's fourteen timed records at $A9:D046, as long addresses.</summary>
    internal static readonly RoomFlashRecord[] RoomFlashRecords =
    [
        new RoomFlashRecord(0, 0xa9d046), new RoomFlashRecord(1, 0xa9d04a), new RoomFlashRecord(2, 0xa9d04e), new RoomFlashRecord(3, 0xa9d052), new RoomFlashRecord(4, 0xa9d056), new RoomFlashRecord(5, 0xa9d05a), new RoomFlashRecord(6, 0xa9d05e),
        new RoomFlashRecord(7, 0xa9d062), new RoomFlashRecord(8, 0xa9d066), new RoomFlashRecord(9, 0xa9d06a), new RoomFlashRecord(10, 0xa9d06e), new RoomFlashRecord(11, 0xa9d072), new RoomFlashRecord(12, 0xa9d076), new RoomFlashRecord(13, 0xa9d07a),
    ];
    /// <summary>The room flash's closing goto at $A9:D07E, as a long address.</summary>
    internal const int RoomFlashLoop = 0xa9d07e;
    /// <summary>The room flash's goto opcode and its target, the flash's first record.</summary>
    internal static readonly RoomFlashGotoValue RoomFlashGoto = new RoomFlashGotoValue(0x9b0f, 0xd046);

    /// <summary>Each native <see cref="MotherBrainTubeCollapseFunction"/> routine pointer and the function it decodes to.</summary>
    internal static readonly Dictionary<ushort, MotherBrainTubeCollapseFunction> NativeTubeCollapseFunctions = new()
    {
        [0x8949] = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots,
        [0x896e] = MotherBrainTubeCollapseFunction.ClearBottomLeftTube,
        [0x8983] = MotherBrainTubeCollapseFunction.SpawnTopRightTube,
        [0x89a0] = MotherBrainTubeCollapseFunction.ClearCeilingColumn9,
        [0x89b5] = MotherBrainTubeCollapseFunction.SpawnTopLeftTube,
        [0x89d2] = MotherBrainTubeCollapseFunction.ClearCeilingColumn6,
        [0x89e7] = MotherBrainTubeCollapseFunction.SpawnBottomRightTube,
        [0x89fa] = MotherBrainTubeCollapseFunction.ClearBottomRightTube,
        [0x8a0f] = MotherBrainTubeCollapseFunction.SpawnBottomMiddleLeftTube,
        [0x8a22] = MotherBrainTubeCollapseFunction.ClearBottomMiddleLeftTube,
        [0x8a37] = MotherBrainTubeCollapseFunction.SpawnTopMiddleLeftTube,
        [0x8a54] = MotherBrainTubeCollapseFunction.ClearCeilingColumn7,
        [0x8a69] = MotherBrainTubeCollapseFunction.SpawnTopMiddleRightTube,
        [0x8a86] = MotherBrainTubeCollapseFunction.ClearCeilingColumn8,
        [0x8a9b] = MotherBrainTubeCollapseFunction.SpawnBottomMiddleRightTube,
        [0x8aae] = MotherBrainTubeCollapseFunction.ClearBottomMiddleRightTube,
        [0x8ac3] = MotherBrainTubeCollapseFunction.SpawnMainTube,
        [0x8ad6] = MotherBrainTubeCollapseFunction.ClearBottomMiddleTubes,
        [0x8ae4] = MotherBrainTubeCollapseFunction.Finished,
    };

    /// <summary>Each native <see cref="MotherBrainBrainFunction"/> routine pointer and the function it decodes to.</summary>
    internal static readonly Dictionary<ushort, MotherBrainBrainFunction> NativeBrainFunctions = new()
    {
        [0x87a2] = MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn,
        [0x87d0] = MotherBrainBrainFunction.SetupBrainToBeDrawn,
    };}
