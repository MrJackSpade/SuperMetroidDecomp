using B = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainBodyCue;
using H = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadCue;
using K = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainHeadCueKind;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

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
    internal static readonly (MotherBrainBodyProgram Program, ushort Entry)[] BodyEntries =
    [
        (MotherBrainBodyProgram.WalkForwardReallyFast, 0x9730),
        (MotherBrainBodyProgram.WalkForwardFast, 0x976a),
        (MotherBrainBodyProgram.WalkForwardMedium, 0x97a4),
        (MotherBrainBodyProgram.WalkForwardSlow, 0x97de),
        (MotherBrainBodyProgram.WalkForwardReallySlow, 0x9818),
        (MotherBrainBodyProgram.WalkBackwardSlow, 0x9852),
        (MotherBrainBodyProgram.WalkBackwardReallyFast, 0x988c),
        (MotherBrainBodyProgram.WalkBackwardFast, 0x98c6),
        (MotherBrainBodyProgram.WalkBackwardMedium, 0x9900),
        (MotherBrainBodyProgram.WalkBackwardReallySlow, 0x993a),
        (MotherBrainBodyProgram.CrouchAndThenStandUp, 0x9974),
        (MotherBrainBodyProgram.StandAfterCrouchSlow, 0x99aa),
        (MotherBrainBodyProgram.StandAfterCrouchFast, 0x99c6),
        (MotherBrainBodyProgram.StandAfterLeaning, 0x99e2),
        (MotherBrainBodyProgram.LeanDown, 0x99f2),
        (MotherBrainBodyProgram.Crouched, 0x9a02),
        (MotherBrainBodyProgram.CrouchSlow, 0x9a0a),
        (MotherBrainBodyProgram.CrouchFast, 0x9a26),
        (MotherBrainBodyProgram.HandBeam, 0x9a42),
        (MotherBrainBodyProgram.InitialDummy, 0x9c13),
    ];

    /// <summary>Each brain program's native entry point in bank $A9.</summary>
    internal static readonly (MotherBrainHeadProgram Program, ushort Entry)[] HeadEntries =
    [
        (MotherBrainHeadProgram.StretchingPhase2, 0x9b7f),
        (MotherBrainHeadProgram.StretchingPhase2Loop, 0x9bab),
        (MotherBrainHeadProgram.StretchingPhase3, 0x9bb3),
        (MotherBrainHeadProgram.StretchingPhase3Loop, 0x9bdf),
        (MotherBrainHeadProgram.HyperBeamRecoil, 0x9be7),
        (MotherBrainHeadProgram.HyperBeamRecoilLoop, 0x9c0b),
        (MotherBrainHeadProgram.Initial, 0x9c21),
        (MotherBrainHeadProgram.Decapitated, 0x9c29),
        (MotherBrainHeadProgram.DecapitatedLoop, 0x9c31),
        (MotherBrainHeadProgram.DyingDrool, 0x9c39),
        (MotherBrainHeadProgram.DyingDroolBurst, 0x9c47),
        (MotherBrainHeadProgram.DyingDroolLoop, 0x9c5f),
        (MotherBrainHeadProgram.FiringRainbowBeam, 0x9c77),
        (MotherBrainHeadProgram.NeutralPhase2, 0x9c87),
        (MotherBrainHeadProgram.NeutralPhase2Hold, 0x9c9f),
        (MotherBrainHeadProgram.NeutralPhase3, 0x9cb9),
        (MotherBrainHeadProgram.NeutralPhase3Hold, 0x9cd1),
        (MotherBrainHeadProgram.Corpse, 0x9d25),
        (MotherBrainHeadProgram.CorpseLoop, 0x9d35),
        (MotherBrainHeadProgram.FourOnionRingsPhase2, 0x9d3d),
        (MotherBrainHeadProgram.TwoOnionRingsPhase2, 0x9d7f),
        (MotherBrainHeadProgram.AttackingBabyMetroid, 0x9db1),
        (MotherBrainHeadProgram.AttackingSamusFourOnionRingsPhase3, 0x9dbb),
        (MotherBrainHeadProgram.FourOnionRingsPhase3, 0x9dc1),
        (MotherBrainHeadProgram.BombPhase2, 0x9ecc),
        (MotherBrainHeadProgram.BombPhase3, 0x9f00),
        (MotherBrainHeadProgram.Laser, 0x9f34),
        (MotherBrainHeadProgram.ChargingRainbowBeam, 0x9f6c),
        (MotherBrainHeadProgram.ChargingRainbowBeamLoop, 0x9f7a),
    ];

    /// <summary>Each cutscene Baby program's native entry point in bank $A9.</summary>
    internal static readonly (MotherBrainBabyProgram Program, ushort Entry)[] BabyEntries =
    [
        (MotherBrainBabyProgram.Initial, 0xcfa2),
        (MotherBrainBabyProgram.DrainingMotherBrain, 0xcfb8),
        (MotherBrainBabyProgram.TakingFatalBlow, 0xcfce),
    ];

    /// <summary>Each Shitroid program's native entry point in bank $A9.</summary>
    internal static readonly (ShitroidProgram Program, ushort Entry)[] ShitroidEntries =
    [
        (ShitroidProgram.FinishDraining, 0xf906),
        (ShitroidProgram.Normal, 0xf90e),
        (ShitroidProgram.LatchedOn, 0xf924),
        (ShitroidProgram.Remorse, 0xf93a),
    ];

    /// <summary>The bank-$A9 opcodes of body programs and the body steps each becomes, with its operand words.</summary>
    private static readonly Dictionary<ushort, (int Operands, Func<ushort[], B> Cue)> BodyOps = new()
    {
        [0x95fc] = (0, _ => B.Move(new(1, -2))),
        [0x95b6] = (0, _ => B.MoveWithScrollBias(-10, 4)),
        [0x95c0] = (0, _ => B.MoveWithScrollBias(-16, 4)),
        [0x95ca] = (0, _ => B.MoveWithScrollBias(-12, -2)),
        [0x95de] = (0, _ => B.MoveWithScrollBias(12, 4)),
        [0x95e8] = (0, _ => B.MoveWithScrollBias(16, -2)),
        [0x95f2] = (0, _ => B.MoveWithScrollBias(10, -2)),
        [0x960c] = (0, _ => B.Move(new(2, 0))),
        [0x961c] = (0, _ => B.Move(new(0, 1))),
        [0x9622] = (0, _ => B.StepAndMove(new(3, 1))),
        [0x9638] = (0, _ => B.Move(new(15, -2))),
        [0x9648] = (0, _ => B.Move(new(6, -4))),
        [0x9658] = (0, _ => B.Move(new(-2, 4))),
        [0x9668] = (0, _ => B.StepAndMove(new(-1, 2))),
        [0x967e] = (0, _ => B.StepAndMove(new(-1, 2))),
        [0x9694] = (0, _ => B.Move(new(-2, 0))),
        [0x96a4] = (0, _ => B.Move(new(0, -1))),
        [0x96aa] = (0, _ => B.Move(new(-3, -1))),
        [0x96ba] = (0, _ => B.StepAndMove(new(-15, 2))),
        [0x96d0] = (0, _ => B.Move(new(-6, 4))),
        [0x96e0] = (0, _ => B.Move(new(2, -4))),
        [0x96f0] = (0, _ => B.Move(new(1, -2))),
        [0x9700] = (0, _ => B.SetPose(MotherBrainBodyPose.Standing)),
        [0x9708] = (0, _ => B.SetPose(MotherBrainBodyPose.Walking)),
        [0x9710] = (0, _ => B.SetPose(MotherBrainBodyPose.Crouched)),
        [0x9718] = (0, _ => B.SetPose(MotherBrainBodyPose.CrouchingTransition)),
        [0x9720] = (0, _ => B.SetPose(MotherBrainBodyPose.DeathBeam)),
        [0x9728] = (0, _ => B.SetPose(MotherBrainBodyPose.LeaningDown)),
        [0x9ac8] = (3, operands => B.SpawnHandDust(new(unchecked((short)operands[0]), unchecked((short)operands[1])), (MiscDustAnimation)operands[2])),
        [0x9aef] = (0, _ => B.FireHandBeam),
        [0x9b05] = (0, _ => B.AdvanceHandBeamPhase),
    };

    /// <summary>The bank-$A9 brain opcodes that become a single cue, with their operand words.</summary>
    private static readonly Dictionary<ushort, (int Operands, Func<ushort[], H> Cue)> HeadCueOps = new()
    {
        [0x9ea3] = (0, _ => H.Of(K.CountBabyMetroidAttack)),
        [0x9eb5] = (0, _ => H.Of(K.ResetBabyMetroidAttacks)),
        [0x9b20] = (0, _ => H.Of(K.DisableNeckMovement)),
        [0x9e37] = (0, _ => H.Of(K.AimOnionRingsAtBabyMetroid)),
        [0x9e5b] = (0, _ => H.Of(K.AimOnionRingsAtSamus)),
        [0x9df7] = (0, _ => H.Of(K.QueueBabyMetroidAttackSound)),
        [0x9e29] = (0, _ => H.Of(K.SpawnOnionRing)),
        [0x9b32] = (1, operands => H.Sound3(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, operands[0]))),
        [0x9b28] = (1, operands => H.Sound2(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, operands[0]))),
        [0x9ebd] = (1, operands => H.Bomb(operands[0])),
        [0x9b6d] = (0, _ => H.Of(K.SpawnBigPurpleBreath)),
        [0x9d0d] = (0, _ => H.Of(K.MaybeHoldNeutralPhase3)),
        [0x9b3c] = (0, _ => H.Of(K.SpawnDrool)),
        [0x9b77] = (0, _ => H.Of(K.ShakeBrain)),
        [0x9cad] = (0, _ => H.Of(K.MaybeHoldNeutralPhase2)),
        [0x9f46] = (0, _ => H.Of(K.SpawnLaser)),
        [0x9f84] = (0, _ => H.Of(K.SpawnRainbowBeamChargeParticle)),
        [0x9f8e] = (0, _ => H.Of(K.PrepareRainbowBeamCharge)),
    };

    /// <summary>Each Zebetite health program's native entry point in bank $A6, six bytes apart.</summary>
    internal static readonly (ZebetiteProgram Program, ushort Entry)[] ZebetiteEntries =
    [
        (ZebetiteProgram.LargeAtLeast800, 0xfdcc),
        (ZebetiteProgram.LargeAtLeast600, 0xfdd2),
        (ZebetiteProgram.LargeAtLeast400, 0xfdd8),
        (ZebetiteProgram.LargeAtLeast200, 0xfdde),
        (ZebetiteProgram.LargeBelow200, 0xfde4),
        (ZebetiteProgram.SplitAtLeast800, 0xfdea),
        (ZebetiteProgram.SplitAtLeast600, 0xfdf0),
        (ZebetiteProgram.SplitAtLeast400, 0xfdf6),
        (ZebetiteProgram.SplitAtLeast200, 0xfdfc),
        (ZebetiteProgram.SplitBelow200, 0xfe02),
    ];

    /// <summary>Each falling tube's pose list in bank $A9, six bytes apart.</summary>
    internal static readonly (MotherBrainTubePiece Program, ushort Entry)[] TubeEntries =
    [
        (MotherBrainTubePiece.BottomLeft, 0x8c69),
        (MotherBrainTubePiece.BottomRight, 0x8c6f),
        (MotherBrainTubePiece.BottomMiddleLeft, 0x8c75),
        (MotherBrainTubePiece.BottomMiddleRight, 0x8c7b),
        (MotherBrainTubePiece.Main, 0x8c81),
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
        Walk(cartridge, MotherBrainBank, MotherBrainAnimations.Body, BodyEntries, frame => MotherBrainBodyVisualDefinitions.Frame(frame.Index).Pointer, BodyNativeOps);

    /// <summary>Compares every typed brain program with its native list; returns each native record's typed cursor.</summary>
    internal static Dictionary<ushort, AnimationProgramCursor<MotherBrainHeadProgram>> CompareHead(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, MotherBrainAnimations.Head, HeadEntries, frame => MotherBrainVisualDefinitions.Frame(frame.Index).Pointer, HeadNativeOps);

    /// <summary>Compares the cutscene Baby's typed programs with their native lists.</summary>
    internal static Dictionary<ushort, AnimationProgramCursor<MotherBrainBabyProgram>> CompareBaby(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, MotherBrainBabyAnimations.Programs, BabyEntries, frame => BabyMetroidVisualDefinitions.Spritemap(frame.Index), BabyNativeOps);

    /// <summary>Compares the Shitroid's typed programs with their native lists.</summary>
    internal static void CompareShitroid(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, ShitroidAnimations.Programs, ShitroidEntries, frame => BabyMetroidVisualDefinitions.Spritemap(frame.Index), ShitroidNativeOps);

    /// <summary>Compares the Zebetites' typed health programs with their native lists.</summary>
    internal static void CompareZebetites(IImportCartridgeSource cartridge) =>
        Walk(cartridge, ZebetiteBank, ZebetiteAnimations.Programs, ZebetiteEntries, frame => ZebetiteVisualDefinitions.Spritemap(frame.Index),
            new Dictionary<ushort, NativeOp<ZebetiteProgram, ZebetiteFrame, NoAnimationCue>>());

    /// <summary>Compares the falling tubes' typed programs with their native lists; returns each native record's typed cursor.</summary>
    internal static Dictionary<ushort, AnimationProgramCursor<MotherBrainTubePiece>> CompareTubes(IImportCartridgeSource cartridge) =>
        Walk(cartridge, MotherBrainBank, MotherBrainTubeAnimations.Programs, TubeEntries, frame => MotherBrainVisualToolingDefinitions.TubeFrame(frame.Index).Pointer,
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
    internal static readonly (MotherBrainTurretDirection Direction, ushort TurretPose, ushort BulletPose, int SelectorEntry)[] TurretEntries =
    [
        (MotherBrainTurretDirection.Left, 0xc101, 0xc143, 0x86c133),
        (MotherBrainTurretDirection.DownLeft, 0xc107, 0xc149, 0x86c135),
        (MotherBrainTurretDirection.Down, 0xc10d, 0xc14f, 0x86c137),
        (MotherBrainTurretDirection.DownRight, 0xc113, 0xc155, 0x86c139),
        (MotherBrainTurretDirection.Right, 0xc119, 0xc15b, 0x86c13b),
        (MotherBrainTurretDirection.UpRight, 0xc11f, 0xc161, 0x86c13d),
        (MotherBrainTurretDirection.Up, 0xc125, 0xc167, 0x86c13f),
        (MotherBrainTurretDirection.UpLeft, 0xc12b, 0xc16d, 0x86c141),
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
    internal static readonly (int Frame, int Record)[] RoomFlashRecords =
    [
        (0, 0xa9d046), (1, 0xa9d04a), (2, 0xa9d04e), (3, 0xa9d052), (4, 0xa9d056), (5, 0xa9d05a), (6, 0xa9d05e),
        (7, 0xa9d062), (8, 0xa9d066), (9, 0xa9d06a), (10, 0xa9d06e), (11, 0xa9d072), (12, 0xa9d076), (13, 0xa9d07a),
    ];
    /// <summary>The room flash's closing goto at $A9:D07E, as a long address.</summary>
    internal const int RoomFlashLoop = 0xa9d07e;
    /// <summary>The room flash's goto opcode and its target, the flash's first record.</summary>
    internal static readonly (ushort Opcode, ushort Target) RoomFlashGoto = (0x9b0f, 0xd046);

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
        (TProgram Program, ushort Entry)[] entries, Func<TFrame, ushort> spritemap,
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
                    AssertEqual(AnimationProgramStepKind.FallInto, steps[index].Kind, $"{at}: runs on into the next authored program");
                    AssertEqual(next, steps[index].Target, $"{at}: fall-through target");
                    AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                    break;
                }
                map.TryAdd(cursor, new(program, index));
                ushort word = cartridge.ReadCartridgeWord(bank | cursor);
                switch (NativeEnemyListDefinitions.Classify(word))
                {
                    case NativeEnemyListDefinitions.WordKind.Frame:
                        AssertEqual(AnimationProgramStepKind.Show, steps[index].Kind, $"{at}: frame");
                        AssertEqual((int)word, steps[index].Duration, $"{at}: duration");
                        AssertEqual(cartridge.ReadCartridgeWord(bank | (cursor + 2)), spritemap(steps[index].Frame), $"{at}: spritemap");
                        cursor = unchecked((ushort)(cursor + 4));
                        index++;
                        continue;
                    case NativeEnemyListDefinitions.WordKind.Sleep:
                        AssertEqual(AnimationProgramStepKind.Hold, steps[index].Kind, $"{at}: sleep");
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
                            AssertEqual(step.Kind, steps[index].Kind, $"{at}: opcode ${word:X4} step kind");
                            if (step.Kind == AnimationProgramStepKind.Cue)
                                AssertEqual(step.Cue, steps[index].Cue, $"{at}: opcode ${word:X4} cue");
                            else
                                AssertEqual(step.Target, steps[index].Target, $"{at}: opcode ${word:X4} target");
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
