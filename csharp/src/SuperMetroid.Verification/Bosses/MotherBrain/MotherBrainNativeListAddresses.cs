using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodyCue;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadCue;
using K = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadCueKind;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;

/// <summary>
/// The native lists Mother Brain's ROM oracle (#1285) walks: every typed program's entry point, the bank-$A9 opcodes
/// with the steps each must become, and the walker that compares a typed program with its native list. Only
/// verification reads them; the port runs typed programs. The walker's address map also decodes a native snapshot's
/// list pointers into typed cursors.
/// </summary>
internal static class MotherBrainNativeListAddresses
{
    /// <summary>Mother Brain's bank, $A9.</summary>
    internal const int MotherBrainBank = 0xa90000;
    /// <summary>The Zebetites' bank, $A6.</summary>
    internal const int ZebetiteBank = 0xa60000;

    /// <summary>Each body program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<MotherBrainBodyProgram>[] BodyEntries =
    [
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkForwardReallyFast, 0x9730),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkForwardFast, 0x976a),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkForwardMedium, 0x97a4),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkForwardSlow, 0x97de),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkForwardReallySlow, 0x9818),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkBackwardSlow, 0x9852),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkBackwardReallyFast, 0x988c),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkBackwardFast, 0x98c6),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkBackwardMedium, 0x9900),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.WalkBackwardReallySlow, 0x993a),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.CrouchAndThenStandUp, 0x9974),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.StandAfterCrouchSlow, 0x99aa),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.StandAfterCrouchFast, 0x99c6),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.StandAfterLeaning, 0x99e2),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.LeanDown, 0x99f2),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.Crouched, 0x9a02),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.CrouchSlow, 0x9a0a),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.CrouchFast, 0x9a26),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.HandBeam, 0x9a42),
        new NativeProgramEntry<MotherBrainBodyProgram>(MotherBrainBodyProgram.InitialDummy, 0x9c13),
    ];

    /// <summary>Each brain program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<MotherBrainHeadProgram>[] HeadEntries =
    [
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.StretchingPhase2, 0x9b7f),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.StretchingPhase2Loop, 0x9bab),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.StretchingPhase3, 0x9bb3),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.StretchingPhase3Loop, 0x9bdf),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.HyperBeamRecoil, 0x9be7),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.HyperBeamRecoilLoop, 0x9c0b),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.Initial, 0x9c21),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.Decapitated, 0x9c29),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.DecapitatedLoop, 0x9c31),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.DyingDrool, 0x9c39),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.DyingDroolBurst, 0x9c47),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.DyingDroolLoop, 0x9c5f),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.FiringRainbowBeam, 0x9c77),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.NeutralPhase2, 0x9c87),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.NeutralPhase2Hold, 0x9c9f),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.NeutralPhase3, 0x9cb9),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.NeutralPhase3Hold, 0x9cd1),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.Corpse, 0x9d25),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.CorpseLoop, 0x9d35),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.FourOnionRingsPhase2, 0x9d3d),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.TwoOnionRingsPhase2, 0x9d7f),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.AttackingBabyMetroid, 0x9db1),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3, 0x9dbb),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.FourOnionRingsPhase3, 0x9dc1),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.BombPhase2, 0x9ecc),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.BombPhase3, 0x9f00),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.Laser, 0x9f34),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.ChargingRainbowBeam, 0x9f6c),
        new NativeProgramEntry<MotherBrainHeadProgram>(MotherBrainHeadProgram.ChargingRainbowBeamLoop, 0x9f7a),
    ];

    /// <summary>Each cutscene Baby program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<MotherBrainBabyProgram>[] BabyEntries =
    [
        new NativeProgramEntry<MotherBrainBabyProgram>(MotherBrainBabyProgram.Initial, 0xcfa2),
        new NativeProgramEntry<MotherBrainBabyProgram>(MotherBrainBabyProgram.DrainingMotherBrain, 0xcfb8),
        new NativeProgramEntry<MotherBrainBabyProgram>(MotherBrainBabyProgram.TakingFatalBlow, 0xcfce),
    ];

    /// <summary>Each Shitroid program's native entry point in bank $A9.</summary>
    internal static readonly NativeProgramEntry<ShitroidProgram>[] ShitroidEntries =
    [
        new NativeProgramEntry<ShitroidProgram>(ShitroidProgram.FinishDraining, 0xf906),
        new NativeProgramEntry<ShitroidProgram>(ShitroidProgram.Normal, 0xf90e),
        new NativeProgramEntry<ShitroidProgram>(ShitroidProgram.LatchedOn, 0xf924),
        new NativeProgramEntry<ShitroidProgram>(ShitroidProgram.Remorse, 0xf93a),
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
        [0x9b32] = new HeadCueOp(1, operands => H.Sound3(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, operands[0]))),
        [0x9b28] = new HeadCueOp(1, operands => H.Sound2(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, operands[0]))),
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
    internal static readonly NativeProgramEntry<ZebetiteProgram>[] ZebetiteEntries =
    [
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.LargeAtLeast800, 0xfdcc),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.LargeAtLeast600, 0xfdd2),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.LargeAtLeast400, 0xfdd8),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.LargeAtLeast200, 0xfdde),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.LargeBelow200, 0xfde4),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.SplitAtLeast800, 0xfdea),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.SplitAtLeast600, 0xfdf0),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.SplitAtLeast400, 0xfdf6),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.SplitAtLeast200, 0xfdfc),
        new NativeProgramEntry<ZebetiteProgram>(ZebetiteProgram.SplitBelow200, 0xfe02),
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

    /// <summary>What a native flow opcode becomes: its operand words, the typed steps, and whether it ends the list.</summary>
    /// <param name="Operands">The opcode's operand words.</param>
    /// <param name="Steps">The typed steps the opcode becomes, given its operands and the entry-to-program lookup.</param>
    /// <param name="Ends">Whether the opcode ends the native list.</param>
    internal readonly record struct NativeOp<TProgram, TFrame, TCue>(
        int Operands, Func<ushort[], Func<ushort, TProgram>, AnimationProgramStep<TProgram, TFrame, TCue>[]> Steps, bool Ends)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull;

    /// <summary>The body opcodes, each a single cue.</summary>
    private static Dictionary<ushort, NativeOp<MotherBrainBodyProgram, MotherBrainBodyFrame, B>> BodyNativeOps { get; } =
        BodyOps.ToDictionary(pair => pair.Key, pair => new NativeOp<MotherBrainBodyProgram, MotherBrainBodyFrame, B>(
            pair.Value.Operands, (operands, _) => [AnimationProgramStep<MotherBrainBodyProgram, MotherBrainBodyFrame, B>.Publish(pair.Value.Cue(operands))], false));

    /// <summary>The brain opcodes: single cues, and the gotos that end a list.</summary>
    private static Dictionary<ushort, NativeOp<MotherBrainHeadProgram, MotherBrainHeadFrame, H>> HeadNativeOps { get; } = BuildHeadNativeOps();

    /// <summary>Adds <c>MotherBrain_GotoX</c>, the neck-enabling goto and the dying-drool pick to the brain's cue opcodes.</summary>
    private static Dictionary<ushort, NativeOp<MotherBrainHeadProgram, MotherBrainHeadFrame, H>> BuildHeadNativeOps()
    {
        var ops = HeadCueOps.ToDictionary(pair => pair.Key, pair => new NativeOp<MotherBrainHeadProgram, MotherBrainHeadFrame, H>(
            pair.Value.Operands, (operands, _) => [AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, H>.Publish(pair.Value.Cue(operands))], false));
        // $A9:9B0F, MotherBrain_GotoX.
        ops[0x9b0f] = new(1, (operands, programAt) => [AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, H>.GoTo(programAt(operands[0]))], true);
        // $A9:9B14, MotherBrainHead_EnableNeckMovement_GotoX.
        ops[0x9b14] = new(1, (operands, programAt) =>
        [
            AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, H>.Publish(H.Of(K.EnableNeckMovement)),
            AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, H>.GoTo(programAt(operands[0])),
        ], true);
        // $A9:9C65, MotherBrainHead_GotoDyingDroolInstList: it picks the burst or the loop itself.
        ops[0x9c65] = new(0, (_, _) =>
        [
            AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, H>.Publish(H.Of(K.MaybeDroolAgain)),
            AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, H>.GoTo(MotherBrainHeadProgram.DyingDroolLoop),
        ], true);
        return ops;
    }

    /// <summary>The cutscene Baby's opcodes: its two same-bank gotos.</summary>
    private static Dictionary<ushort, NativeOp<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>> BabyNativeOps { get; } = new()
    {
        [0xcfb4] = new(0, (_, _) => [AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.GoTo(MotherBrainBabyProgram.Initial)], true),
        [0xcfca] = new(0, (_, _) => [AnimationProgramStep<MotherBrainBabyProgram, BabyMetroidFrame, NoAnimationCue>.GoTo(MotherBrainBabyProgram.DrainingMotherBrain)], true),
    };

    /// <summary>The Shitroid's opcodes: three gotos and the random remorse cry, whose goto operand is the remorse start; without the cry it goes to that operand.</summary>
    private static Dictionary<ushort, NativeOp<ShitroidProgram, BabyMetroidFrame, ShitroidCue>> ShitroidNativeOps { get; } = new()
    {
        [0xf920] = new(0, (_, _) => [AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.GoTo(ShitroidProgram.Normal)], true),
        [0xf936] = new(0, (_, _) => [AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.GoTo(ShitroidProgram.LatchedOn)], true),
        [0xf990] = new(0, (_, _) => [AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.GoTo(ShitroidProgram.Remorse)], true),
        [0xf994] = new(1, (operands, programAt) => programAt(operands[0]) == ShitroidProgram.Remorse
            ? [AnimationProgramStep<ShitroidProgram, BabyMetroidFrame, ShitroidCue>.Publish(ShitroidCue.MaybeRemorseCry)]
            : throw new InvalidDataException("The remorse cry's goto operand is not the remorse start."), false),
    };

    /// <summary>Compares every typed body program with its native list; returns each native record's typed cursor.</summary>
    internal static Dictionary<ushort, AnimationProgramCursor<MotherBrainBodyProgram>> CompareBody(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, MotherBrainAnimations.Body, BodyEntries, frame => MotherBrainBodyVisualDefinitionsConstants.Frame(frame.Index).Pointer, BodyNativeOps);

    /// <summary>Compares every typed brain program with its native list; returns each native record's typed cursor.</summary>
    internal static Dictionary<ushort, AnimationProgramCursor<MotherBrainHeadProgram>> CompareHead(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, MotherBrainAnimations.Head, HeadEntries, frame => MotherBrainVisualDefinitionsConstants.Frame(frame.Index).Pointer, HeadNativeOps);

    /// <summary>Compares the cutscene Baby's typed programs with their native lists.</summary>
    internal static Dictionary<ushort, AnimationProgramCursor<MotherBrainBabyProgram>> CompareBaby(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, MotherBrainBabyAnimations.Programs, BabyEntries, frame => BabyMetroidVisualToolingDefinitions.Spritemap(frame.Index), BabyNativeOps);

    /// <summary>Compares the Shitroid's typed programs with their native lists.</summary>
    internal static void CompareShitroid(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, ShitroidAnimations.Programs, ShitroidEntries, frame => BabyMetroidVisualToolingDefinitions.Spritemap(frame.Index), ShitroidNativeOps);

    /// <summary>Compares the Zebetites' typed health programs with their native lists.</summary>
    internal static void CompareZebetites(IImportCartridgeSource cartridge) =>
        Walk(cartridge, ZebetiteBank, ZebetiteAnimations.Programs, ZebetiteEntries, frame => ZebetiteVisualToolingDefinitions.Spritemap(frame.Index),
            new Dictionary<ushort, NativeOp<ZebetiteProgram, ZebetiteFrame, NoAnimationCue>>());

    /// <summary>Compares the falling tubes' typed programs with their native lists; returns each native record's typed cursor.</summary>
    internal static Dictionary<ushort, AnimationProgramCursor<MotherBrainTubePiece>> CompareTubes(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, MotherBrainTubeAnimations.Programs, TubeEntries, frame => EnemyFrameCartridgeLookupTooling.Word(MotherBrainVisualDefinitions.TubeFrames.Of(frame)),
            new Dictionary<ushort, NativeOp<MotherBrainTubePiece, MotherBrainTubeFrame, NoAnimationCue>>());

    /// <summary>Each glass program's native list in bank $86.</summary>
    internal static readonly Dictionary<MotherBrainGlassProgram, ushort> GlassEntries = new()
    {
        [MotherBrainGlassProgram.ShardGroup0] = 0xcc93,
        [MotherBrainGlassProgram.ShardGroup1] = 0xccb7,
        [MotherBrainGlassProgram.ShardGroup2] = 0xccdb,
        [MotherBrainGlassProgram.ShardGroup3] = 0xccff,
        [MotherBrainGlassProgram.ShardGroup4] = 0xcd23,
        [MotherBrainGlassProgram.ShardGroup5] = 0xcd47,
        [MotherBrainGlassProgram.ShardGroup6] = 0xcd6b,
        [MotherBrainGlassProgram.ShardGroup7] = 0xcd8f,
        [MotherBrainGlassProgram.Sparkle] = 0xcdb3,
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
    };
    /// <summary>
    /// Walks each program's native list beside its typed steps: frames must show the native spritemap for the native
    /// duration, opcodes must become the steps <paramref name="ops"/> names, a sleep must hold, and running into the
    /// next authored entry must fall into it. Returns every native record's typed cursor.
    /// </summary>
    internal static Dictionary<ushort, AnimationProgramCursor<TProgram>> Walk<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge, int bank, AnimationProgramSet<TProgram, TFrame, TCue> programs,
        NativeProgramEntry<TProgram>[] entries, Func<TFrame, ushort> spritemap,
        IReadOnlyDictionary<ushort, NativeOp<TProgram, TFrame, TCue>> ops)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        AssertEqual(Enum.GetValues<TProgram>().Length, entries.Length, $"every typed {typeof(TProgram).Name} has a native entry");
        Dictionary<ushort, TProgram> programAt = entries.ToDictionary(entry => entry.Entry, entry => entry.Program);
        var map = new Dictionary<ushort, AnimationProgramCursor<TProgram>>();
        foreach ((TProgram program, ushort entry) in entries)
        {
            var steps = programs.Steps(program);
            ushort cursor = entry;
            int index = 0;
            for (int record = 0; ; record++)
            {
                if (record > 256)
                    throw new InvalidDataException($"{program} ran past its native list.");
                string at = $"{program} step {index} (native record {record})";
                if (record > 0 && programAt.TryGetValue(cursor, out TProgram next))
                {
                    var fallInto = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.FallIntoStep>(steps[index], $"{at}: runs on into the next authored program");
                    AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                    AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                    break;
                }
                map.TryAdd(cursor, new(program, index));
                ushort word = cartridge.ReadCartridgeWord(bank | cursor);
                switch (NativeEnemyListDefinitions.Classify(word))
                {
                    case NativeEnemyListDefinitions.WordKind.Frame:
                        var show = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep>(steps[index], $"{at}: frame");
                        AssertEqual(word, show.Duration, $"{at}: duration");
                        AssertEqual(cartridge.ReadCartridgeWord(bank | (cursor + 2)), spritemap(show.Frame), $"{at}: spritemap");
                        cursor = unchecked((ushort)(cursor + 4));
                        index++;
                        continue;
                    case NativeEnemyListDefinitions.WordKind.Sleep:
                        AssertTrue(steps[index] is AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep, $"{at}: sleep");
                        AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                        break;
                    case NativeEnemyListDefinitions.WordKind.Callback:
                    {
                        if (!ops.TryGetValue(word, out NativeOp<TProgram, TFrame, TCue> op))
                            throw new InvalidDataException($"{at}: opcode ${word:X4} is outside the compared set.");
                        ushort[] operands = [.. Enumerable.Range(0, op.Operands)
                            .Select(operand => cartridge.ReadCartridgeWord(bank | (cursor + 2 + 2 * operand)))];
                        foreach (var step in op.Steps(operands, address => programAt[address]))
                        {
                            AssertEqual(step, steps[index], $"{at}: opcode ${word:X4} step");
                            index++;
                        }
                        cursor = unchecked((ushort)(cursor + 2 + 2 * op.Operands));
                        if (!op.Ends)
                            continue;
                        AssertEqual(steps.Count, index, $"{at}: opcode ${word:X4} ends the program");
                        break;
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
                break;
            }
        }
        return map;
    }
}
