using SuperMetroid.Core.Assets;
using K = SuperMetroid.Core.Game.CrocomireBodyCueKind;
using P = SuperMetroid.Core.Game.CrocomireBodyProgram;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Crocomire's body, melting and skeleton animation programs, declared in the order the cartridge authors them
/// in bank $A4. The fight AI depends on that order: it asks whether the program it is running lies at or after
/// a given program, as the native routines compare instruction pointers.
/// </summary>
public enum CrocomireBodyProgram
{
    /// <summary><c>InstList_Crocomire_Initial</c> (<c>$A4:BADE</c>).</summary>
    Initial,
    /// <summary><c>UNUSED_InstList_Crocomire_ChargeForwardOneStep_A4BAEA</c> (<c>$A4:BAEA</c>).</summary>
    UnusedChargeForwardOneStep,
    /// <summary><c>InstList_Crocomire_ProjectileAttack_0</c> (<c>$A4:BB36</c>).</summary>
    ProjectileAttack,
    /// <summary><c>InstList_Crocomire_ProjectileAttack_1</c> (<c>$A4:BB94</c>).</summary>
    ProjectileAttackLoop,
    /// <summary><c>InstList_Crocomire_StepForwardAfterDelay</c> (<c>$A4:BBCA</c>).</summary>
    StepForwardAfterDelay,
    /// <summary><c>InstList_Crocomire_StepForward</c> (<c>$A4:BBCE</c>).</summary>
    StepForward,
    /// <summary><c>InstList_Crocomire_StepBack</c> (<c>$A4:BC30</c>).</summary>
    StepBack,
    /// <summary><c>InstList_Crocomire_SteppingBack</c> (<c>$A4:BC34</c>).</summary>
    SteppingBack,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage</c> (<c>$A4:BC56</c>).</summary>
    WaitForFirstSecondDamage,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_MovingClaws</c> (<c>$A4:BCD8</c>).</summary>
    MovingClaws,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_Roar</c> (<c>$A4:BD2A</c>).</summary>
    Roar,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_0</c> (<c>$A4:BD8E</c>).</summary>
    RoarCloseMouth,
    /// <summary><c>InstList_Crocomire_WaitForFirstSecondDamage_RoarCloseMouth_1</c> (<c>$A4:BDA2</c>).</summary>
    RoarCloseMouthLoop,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthFullyOpen</c> (<c>$A4:BDAE</c>).</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthPartiallyOpen</c> (<c>$A4:BDB2</c>).</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_0</c> (<c>$A4:BDB6</c>).</summary>
    PowerBombReactionMouthNotOpen,
    /// <summary><c>InstList_Crocomire_PowerBombReaction_MouthNotOpen_1</c> (<c>$A4:BE06</c>).</summary>
    PowerBombReactionMouthNotOpenLoop,
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_0</c> (<c>$A4:BE7E</c>).</summary>
    NearSpikeWallCharge,
    /// <summary><c>InstList_CrocomireTongue_NearSpikeWallCharge_1</c> (<c>$A4:BEEC</c>).</summary>
    NearSpikeWallChargeLoop,
    /// <summary><c>InstList_Crocomire_BackOffFromSpikeWall</c> (<c>$A4:BF3C</c>).</summary>
    BackOffFromSpikeWall,
    /// <summary><c>InstList_Crocomire_Melting1_TopRow</c> (<c>$A4:BF64</c>).</summary>
    MeltingOneTopRow,
    /// <summary><c>InstList_Crocomire_Melting1_Top2Rows</c> (<c>$A4:BF6C</c>).</summary>
    MeltingOneTopTwoRows,
    /// <summary><c>InstList_Crocomire_Melting1_Top3Rows</c> (<c>$A4:BF72</c>).</summary>
    MeltingOneTopThreeRows,
    /// <summary><c>InstList_Crocomire_Melting1_Top4Rows</c> (<c>$A4:BF78</c>).</summary>
    MeltingOneTopFourRows,
    /// <summary><c>InstList_Crocomire_Melting2_TopRow</c> (<c>$A4:BF7E</c>).</summary>
    MeltingTwoTopRow,
    /// <summary><c>InstList_Crocomire_Melting2_Top2Rows</c> (<c>$A4:BF86</c>).</summary>
    MeltingTwoTopTwoRows,
    /// <summary><c>InstList_Crocomire_Melting2_Top3Rows</c> (<c>$A4:BF8C</c>).</summary>
    MeltingTwoTopThreeRows,
    /// <summary><c>InstList_Crocomire_Melting2_Top4Rows</c> (<c>$A4:BF92</c>).</summary>
    MeltingTwoTopFourRows,
    /// <summary><c>InstList_CrocomireTongue_BridgeCollapsed</c> (<c>$A4:BFB0</c>).</summary>
    BridgeCollapsed,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Falling</c> (<c>$A4:E14A</c>).</summary>
    SkeletonFalling,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FallsApart_0</c> (<c>$A4:E158</c>).</summary>
    SkeletonFallsApart,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_1</c> (<c>$A4:E1C6</c>).</summary>
    SkeletonStable,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_Dead</c> (<c>$A4:E1CC</c>).</summary>
    Dead,
    /// <summary><c>InstList_CrocomireCorpse_Skeleton_FlowingDownTheRiver</c> (<c>$A4:E1D2</c>).</summary>
    SkeletonFlowingDownRiver,

}

/// <summary>Which visual catalog a <see cref="CrocomireBodyFrame"/> indexes.</summary>
public enum CrocomireBodyFrameSet
{
    /// <summary>The fifty fight-body and melting roots of <see cref="CrocomireBodyVisualDefinitions"/>.</summary>
    Body,
    /// <summary>The thirty-three skeleton roots of <see cref="CrocomireSkeletonVisualDefinitions"/>.</summary>
    Skeleton,
}

/// <summary>One frame of Crocomire's body programs: an index into its body or skeleton visual catalog.</summary>
/// <param name="Set">The catalog the frame belongs to.</param>
/// <param name="Index">The frame's position in that catalog.</param>
public readonly record struct CrocomireBodyFrame(CrocomireBodyFrameSet Set, int Index)
{
    /// <summary>The installed extended spritemap the frame shows.</summary>
    internal ushort Spritemap => Set switch
    {
        CrocomireBodyFrameSet.Body => CrocomireBodyVisualDefinitions.FramePointer(Index),
        CrocomireBodyFrameSet.Skeleton => CrocomireSkeletonVisualDefinitions.FramePointer(Index),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyFrameSet)} {Set}."),
    };
}

/// <summary>What a cue of Crocomire's body programs does.</summary>
internal enum CrocomireBodyCueKind
{
    /// <summary><c>Instruction_Crocomire_FightAI</c>: runs the fight-AI function, which may start another program.</summary>
    FightAi,
    /// <summary>Starts the projectile attack on a one-in-four random draw.</summary>
    MaybeStartProjectileAttack,
    /// <summary>Queues Crocomire's cry.</summary>
    Cry,
    /// <summary>Queues the big explosion sound.</summary>
    BigExplosionSound,
    /// <summary>Queues the skeleton-collapse sound.</summary>
    SkeletonCollapseSound,
    /// <summary>Shakes the screen and queues its sound.</summary>
    ShakeScreen,
    /// <summary>Moves four pixels left unless a mouth hit is pending.</summary>
    MoveLeft,
    /// <summary>Spawns random foot dust, then moves four pixels left unless a mouth hit is pending.</summary>
    MoveLeftWithDust,
    /// <summary>Moves four pixels left, backing off when the spike wall stops it, and otherwise spawns foot dust.</summary>
    MoveLeftTowardSpikeWall,
    /// <summary>Moves four pixels right while the body is on screen.</summary>
    MoveRightIfOnScreen,
    /// <summary>Moves four pixels right.</summary>
    MoveRight,
    /// <summary>Spawns random foot dust, then moves four pixels right while the body is on screen.</summary>
    MoveRightIfOnScreenWithDust,
    /// <summary>Spawns random foot dust, then moves four pixels right.</summary>
    MoveRightWithDust,
    /// <summary>Spawns a big dust cloud at <see cref="CrocomireBodyCue.DustOffset"/> pixels from the body.</summary>
    Dust,
}

/// <summary>One cue of Crocomire's body programs.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="DustOffset">The horizontal offset of a <see cref="CrocomireBodyCueKind.Dust"/> cue's cloud.</param>
internal readonly record struct CrocomireBodyCue(CrocomireBodyCueKind Kind, int DustOffset);

/// <summary>Crocomire's typed body programs (#641), in place of its bank-$A4 instruction lists.</summary>
internal static class CrocomireBodyAnimations
{
    /// <summary>Every body, melting and skeleton program.</summary>
    internal static AnimationProgramSet<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> Programs { get; } = new(
        new Dictionary<CrocomireBodyProgram, AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue>[]>
    {
        [P.Initial] =
        [
            Show(Body(12), 1),
            Cue(K.FightAi),
            GoTo(P.Initial),
        ],
        [P.UnusedChargeForwardOneStep] =
        [
            Show(Body(0), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(1), 8),
            Cue(K.MoveLeft),
            Show(Body(2), 8),
            Cue(K.MoveLeft),
            Show(Body(3), 8),
            Cue(K.MoveLeftWithDust),
            Show(Body(4), 8),
            Cue(K.MoveLeft),
            Show(Body(5), 8),
            Cue(K.MoveLeft),
            Show(Body(6), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(7), 8),
            Cue(K.MoveLeft),
            Show(Body(8), 8),
            Cue(K.MoveLeft),
            Show(Body(9), 8),
            Cue(K.MoveLeftWithDust),
            Show(Body(10), 8),
            Cue(K.MoveLeft),
            Show(Body(11), 8),
            Cue(K.FightAi),
            FallInto(P.ProjectileAttack),
        ],
        [P.ProjectileAttack] =
        [
            Dust(16),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(0),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(-16),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(40),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Show(Body(24), 5),
            Cue(K.Cry),
            Show(Body(25), 5),
            Show(Body(26), 5),
            FallInto(P.ProjectileAttackLoop),
        ],
        [P.ProjectileAttackLoop] =
        [
            Cue(K.FightAi),
            Show(Body(37), 8),
            Show(Body(38), 7),
            Show(Body(39), 7),
            Show(Body(40), 7),
            Show(Body(41), 7),
            GoTo(P.ProjectileAttackLoop),
        ],
        [P.StepForwardAfterDelay] =
        [
            Show(Body(27), 180),
            FallInto(P.StepForward),
        ],
        [P.StepForward] =
        [
            Cue(K.MaybeStartProjectileAttack),
            Show(Body(24), 5),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(Body(26), 5),
            Cue(K.FightAi),
            Show(Body(28), 16),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(Body(29), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(30), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(31), 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(Body(32), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(33), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(34), 4),
            Cue(K.MoveLeftWithDust),
            Cue(K.FightAi),
            Show(Body(35), 4),
            Cue(K.MoveLeft),
            Cue(K.FightAi),
            Show(Body(36), 4),
            Cue(K.FightAi),
            GoTo(P.StepForward),
        ],
        [P.StepBack] =
        [
            Show(Body(25), 2),
            FallInto(P.SteppingBack),
        ],
        [P.SteppingBack] =
        [
            Show(Body(11), 8),
            Cue(K.MoveRightIfOnScreenWithDust),
            Show(Body(10), 8),
            Cue(K.MoveRightIfOnScreen),
            Show(Body(9), 8),
            Cue(K.MoveRightIfOnScreen),
            Show(Body(8), 8),
            Cue(K.MoveRightIfOnScreenWithDust),
            Show(Body(7), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveRightIfOnScreen),
            Cue(K.FightAi),
            FallInto(P.WaitForFirstSecondDamage),
        ],
        [P.WaitForFirstSecondDamage] =
        [
            Show(Body(18), 34),
            Show(Body(17), 2),
            Show(Body(16), 2),
            Show(Body(15), 2),
            Show(Body(14), 2),
            Show(Body(13), 2),
            Show(Body(12), 2),
            Show(Body(12), 16),
            Show(Body(13), 1),
            Show(Body(14), 1),
            Show(Body(15), 1),
            Show(Body(16), 1),
            Show(Body(17), 1),
            Show(Body(18), 16),
            Show(Body(17), 1),
            Show(Body(16), 1),
            Show(Body(15), 1),
            Show(Body(14), 1),
            Show(Body(13), 1),
            Show(Body(12), 1),
            Show(Body(13), 1),
            Show(Body(14), 1),
            Show(Body(15), 1),
            Show(Body(16), 1),
            Show(Body(17), 1),
            Show(Body(18), 1),
            Show(Body(17), 1),
            Show(Body(16), 1),
            Show(Body(15), 1),
            Show(Body(14), 1),
            Show(Body(13), 1),
            Show(Body(12), 1),
            Cue(K.FightAi),
            FallInto(P.MovingClaws),
        ],
        [P.MovingClaws] =
        [
            Dust(16),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(0),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(-16),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(40),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            Cue(K.FightAi),
            FallInto(P.Roar),
        ],
        [P.Roar] =
        [
            Show(Body(24), 48),
            Cue(K.Cry),
            Show(Body(25), 5),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 32),
            Cue(K.FightAi),
            Show(Body(26), 1),
            Cue(K.FightAi),
            FallInto(P.RoarCloseMouth),
        ],
        [P.RoarCloseMouth] =
        [
            Show(Body(26), 32),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.FightAi),
            Show(Body(25), 8),
            Show(Body(24), 2),
            FallInto(P.RoarCloseMouthLoop),
        ],
        [P.RoarCloseMouthLoop] =
        [
            Show(Body(24), 1),
            Cue(K.FightAi),
            Show(Body(24), 1),
            Cue(K.FightAi),
            FallInto(P.PowerBombReactionMouthFullyOpen),
        ],
        [P.PowerBombReactionMouthFullyOpen] =
        [
            Show(Body(25), 2),
            FallInto(P.PowerBombReactionMouthPartiallyOpen),
        ],
        [P.PowerBombReactionMouthPartiallyOpen] =
        [
            Show(Body(24), 2),
            FallInto(P.PowerBombReactionMouthNotOpen),
        ],
        [P.PowerBombReactionMouthNotOpen] =
        [
            Dust(16),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(0),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-32),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(-16),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(0),
            Show(Body(19), 5),
            Show(Body(20), 5),
            Dust(40),
            Show(Body(21), 5),
            Show(Body(22), 5),
            Show(Body(23), 5),
            Cue(K.BigExplosionSound),
            Dust(-16),
            FallInto(P.PowerBombReactionMouthNotOpenLoop),
        ],
        [P.PowerBombReactionMouthNotOpenLoop] =
        [
            Show(Body(0), 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(1), 4),
            Cue(K.MoveLeft),
            Show(Body(2), 4),
            Cue(K.MoveLeft),
            Show(Body(3), 4),
            Cue(K.MoveLeftWithDust),
            Show(Body(4), 4),
            Cue(K.MoveLeft),
            Show(Body(5), 4),
            Cue(K.MoveLeft),
            Show(Body(6), 4),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(7), 4),
            Cue(K.MoveLeft),
            Show(Body(8), 4),
            Cue(K.MoveLeft),
            Show(Body(9), 4),
            Cue(K.MoveLeftWithDust),
            Show(Body(10), 4),
            Cue(K.MoveLeft),
            Show(Body(11), 4),
            Cue(K.FightAi),
            GoTo(P.PowerBombReactionMouthNotOpenLoop),
        ],
        [P.NearSpikeWallCharge] =
        [
            Show(Body(24), 5),
            Cue(K.Cry),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(26), 2),
            Cue(K.FightAi),
            Show(Body(25), 5),
            Cue(K.FightAi),
            Show(Body(25), 8),
            Cue(K.FightAi),
            Show(Body(24), 2),
            Cue(K.FightAi),
            FallInto(P.NearSpikeWallChargeLoop),
        ],
        [P.NearSpikeWallChargeLoop] =
        [
            Show(Body(0), 3),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(1), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(2), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(3), 3),
            Cue(K.MoveLeftWithDust),
            Show(Body(4), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(5), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(6), 3),
            Cue(K.ShakeScreen),
            Cue(K.MoveLeftWithDust),
            Show(Body(7), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(8), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(9), 3),
            Cue(K.MoveLeftWithDust),
            Show(Body(10), 3),
            Cue(K.MoveLeftTowardSpikeWall),
            Show(Body(11), 3),
            Cue(K.FightAi),
            GoTo(P.NearSpikeWallChargeLoop),
        ],
        [P.BackOffFromSpikeWall] =
        [
            Show(Body(11), 8),
            Cue(K.MoveRightWithDust),
            Show(Body(10), 8),
            Cue(K.MoveRight),
            Show(Body(9), 8),
            Cue(K.MoveRight),
            Show(Body(8), 8),
            Cue(K.MoveRightWithDust),
            Show(Body(7), 8),
            Cue(K.ShakeScreen),
            Cue(K.MoveRight),
            Cue(K.FightAi),
            GoTo(P.BackOffFromSpikeWall),
        ],
        [P.MeltingOneTopRow] =
        [
            Show(Body(42), 32767),
            GoTo(P.MeltingOneTopRow),
        ],
        [P.MeltingOneTopTwoRows] =
        [
            Show(Body(43), 32767),
            Hold,
        ],
        [P.MeltingOneTopThreeRows] =
        [
            Show(Body(44), 32767),
            Hold,
        ],
        [P.MeltingOneTopFourRows] =
        [
            Show(Body(45), 32767),
            Hold,
        ],
        [P.MeltingTwoTopRow] =
        [
            Show(Body(46), 32767),
            GoTo(P.MeltingTwoTopRow),
        ],
        [P.MeltingTwoTopTwoRows] =
        [
            Show(Body(47), 32767),
            Hold,
        ],
        [P.MeltingTwoTopThreeRows] =
        [
            Show(Body(48), 32767),
            Hold,
        ],
        [P.MeltingTwoTopFourRows] =
        [
            Show(Body(49), 32767),
            Hold,
        ],
        [P.BridgeCollapsed] =
        [
            Show(Body(25), 5),
            Show(Body(24), 5),
            Cue(K.Cry),
            Show(Body(25), 5),
            Show(Body(26), 5),
            Hold,
        ],
        [P.SkeletonFalling] =
        [
            Show(Skeleton(0), 10),
            Show(Skeleton(1), 10),
            Show(Skeleton(2), 10),
            Hold,
        ],
        [P.SkeletonFallsApart] =
        [
            Show(Skeleton(3), 10),
            Dust(-32),
            Show(Skeleton(4), 5),
            Dust(0),
            Show(Skeleton(4), 5),
            Dust(-16),
            Show(Skeleton(5), 5),
            Dust(16),
            Show(Skeleton(5), 5),
            Show(Skeleton(6), 10),
            Show(Skeleton(7), 32),
            Show(Skeleton(8), 16),
            Cue(K.SkeletonCollapseSound),
            Show(Skeleton(9), 10),
            Show(Skeleton(10), 9),
            Show(Skeleton(11), 9),
            Dust(0),
            Show(Skeleton(12), 8),
            Dust(8),
            Show(Skeleton(13), 8),
            Dust(16),
            Show(Skeleton(14), 7),
            Dust(24),
            Show(Skeleton(15), 7),
            Dust(32),
            Show(Skeleton(16), 6),
            Dust(40),
            Show(Skeleton(17), 6),
            Dust(48),
            Show(Skeleton(18), 6),
            Dust(56),
            Show(Skeleton(19), 5),
            Dust(64),
            Cue(K.BigExplosionSound),
            Show(Skeleton(20), 5),
            FallInto(P.SkeletonStable),
        ],
        [P.SkeletonStable] =
        [
            Show(Skeleton(21), 32767),
            Hold,
        ],
        [P.Dead] =
        [
            Show(Skeleton(22), 32767),
            Hold,
        ],
        [P.SkeletonFlowingDownRiver] =
        [
            Show(Skeleton(23), 4),
            Show(Skeleton(24), 4),
            Show(Skeleton(25), 4),
            Show(Skeleton(26), 4),
            Show(Skeleton(27), 4),
            Show(Skeleton(28), 4),
            Show(Skeleton(29), 4),
            Show(Skeleton(30), 4),
            Show(Skeleton(31), 4),
            Show(Skeleton(32), 20),
            GoTo(P.SkeletonFlowingDownRiver),
        ],
    });

    /// <summary>A fight-body or melting frame.</summary>
    private static CrocomireBodyFrame Body(int index) => new(CrocomireBodyFrameSet.Body, index);

    /// <summary>A skeleton frame.</summary>
    private static CrocomireBodyFrame Skeleton(int index) => new(CrocomireBodyFrameSet.Skeleton, index);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> Show(CrocomireBodyFrame frame, int duration) =>
        AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue>.Show(frame, duration);

    /// <summary>Publishes a cue of <paramref name="kind"/>.</summary>
    private static AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> Cue(CrocomireBodyCueKind kind) =>
        AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue>.Publish(new(kind, 0));

    /// <summary>Publishes a big dust cloud <paramref name="offset"/> pixels from the body.</summary>
    private static AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> Dust(int offset) =>
        AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue>.Publish(new(CrocomireBodyCueKind.Dust, offset));

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> GoTo(CrocomireBodyProgram program) =>
        AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> FallInto(CrocomireBodyProgram program) =>
        AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue>.FallInto(program);

    /// <summary>Holds the last frame: the native sleep.</summary>
    private static AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue> Hold =>
        AnimationProgramStep<CrocomireBodyProgram, CrocomireBodyFrame, CrocomireBodyCue>.Hold;
}
