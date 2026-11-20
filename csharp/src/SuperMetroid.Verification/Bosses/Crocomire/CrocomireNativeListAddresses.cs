using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.MovieReplay.Verification;
namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// The native bank-$A4 and bank-$86 lists the Crocomire ROM oracle (#1282) walks: list entry points, callbacks,
/// the common list opcodes and minimal readers that decode them. Only verification reads them; the port runs
/// typed programs.
/// </summary>
internal static class CrocomireNativeListAddresses
{
    /// <summary>Crocomire's instruction bank, $A4.</summary>
    internal const byte BodyBankNumber = 0xa4;
    /// <summary>The bank-$86 projectile instruction lists, as a long-address base.</summary>
    internal const int ProjectileBank = 0x860000;
    /// <summary>The bank-$86 opcodes a Crocomire projectile list uses besides frames.</summary>
    private static readonly Dictionary<ushort, ProjectileWordKind> ProjectileWords = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.GotoY] = ProjectileWordKind.GoTo,
        [(ushort)NativeEnemyProjectileInstruction.SpawnEnemyDropsWithCrocomireChances] = ProjectileWordKind.CrocomireDrop,
        [(ushort)NativeEnemyProjectileInstruction.Delete] = ProjectileWordKind.Delete,
    };

    /// <summary>
    /// Each body segment's native list in bank $A4, in authored order (#2399). A segment the cartridge writes out again in
    /// another list lists each copy where the cartridge places it: the claw swing heads the projectile attack, the moving
    /// claws and the closed-mouth Power Bomb reaction, and the gaping mouth's run heads its copies in the roar and in the
    /// charge near the spike wall. Where a typed segment begins inside a cartridge list, its entry is the place in that list
    /// its run starts: the run before it runs on into it there.
    /// </summary>
    internal static readonly BodyEntryValue[] BodyEntries =
    [
        new BodyEntryValue(CrocomireBodySegment.IdleSway, 0xbade),
        new BodyEntryValue(CrocomireBodySegment.ClawSwing, 0xbb36),
        new BodyEntryValue(CrocomireBodySegment.ProjectileAttackOpenMouth, 0xbb86),
        // The part of the list its goto loops back to (#2296), as the next two are.
        new BodyEntryValue(CrocomireBodySegment.ProjectileAttackSpitting, 0xbb94),
        new BodyEntryValue(CrocomireBodySegment.StepForwardAfterDelay, 0xbbca),
        new BodyEntryValue(CrocomireBodySegment.StepForward, 0xbbce),
        new BodyEntryValue(CrocomireBodySegment.StepBack, 0xbc30),
        new BodyEntryValue(CrocomireBodySegment.SteppingBack, 0xbc34),
        new BodyEntryValue(CrocomireBodySegment.AwaitingHit, 0xbc56),
        new BodyEntryValue(CrocomireBodySegment.ClawSwing, 0xbcd8),
        new BodyEntryValue(CrocomireBodySegment.ClawSwingFightCue, 0xbd28),
        new BodyEntryValue(CrocomireBodySegment.RoarOpenMouth, 0xbd2a),
        new BodyEntryValue(CrocomireBodySegment.Gaping, 0xbd34),
        new BodyEntryValue(CrocomireBodySegment.RoarGapingHold, 0xbd82),
        new BodyEntryValue(CrocomireBodySegment.RoarCloseMouth, 0xbd8e),
        new BodyEntryValue(CrocomireBodySegment.RoarCloseMouthLoop, 0xbda2),
        new BodyEntryValue(CrocomireBodySegment.PowerBombReactionMouthFullyOpen, 0xbdae),
        new BodyEntryValue(CrocomireBodySegment.PowerBombReactionMouthPartiallyOpen, 0xbdb2),
        new BodyEntryValue(CrocomireBodySegment.ClawSwing, 0xbdb6),
        new BodyEntryValue(CrocomireBodySegment.PowerBombReactionCharge, 0xbe06),
        new BodyEntryValue(CrocomireBodySegment.NearSpikeWallChargeOpenMouth, 0xbe7e),
        new BodyEntryValue(CrocomireBodySegment.Gaping, 0xbe8c),
        new BodyEntryValue(CrocomireBodySegment.NearSpikeWallChargeCloseMouth, 0xbeda),
        new BodyEntryValue(CrocomireBodySegment.NearSpikeWallChargeSteps, 0xbeec),
        new BodyEntryValue(CrocomireBodySegment.BackOffFromSpikeWall, 0xbf3c),
        new BodyEntryValue(CrocomireBodySegment.MeltingFleshTopRow, 0xbf64),
        new BodyEntryValue(CrocomireBodySegment.MeltingFleshTopTwoRows, 0xbf6c),
        new BodyEntryValue(CrocomireBodySegment.MeltingFleshTopThreeRows, 0xbf72),
        new BodyEntryValue(CrocomireBodySegment.MeltingFleshTopFourRows, 0xbf78),
        new BodyEntryValue(CrocomireBodySegment.MeltingBareSkullTopRow, 0xbf7e),
        new BodyEntryValue(CrocomireBodySegment.MeltingBareSkullTopTwoRows, 0xbf86),
        new BodyEntryValue(CrocomireBodySegment.MeltingBareSkullTopThreeRows, 0xbf8c),
        new BodyEntryValue(CrocomireBodySegment.MeltingBareSkullTopFourRows, 0xbf92),
        new BodyEntryValue(CrocomireBodySegment.BridgeCollapsed, 0xbfb0),
        new BodyEntryValue(CrocomireBodySegment.SkeletonFalling, 0xe14a),
        new BodyEntryValue(CrocomireBodySegment.SkeletonFallsApart, 0xe158),
        new BodyEntryValue(CrocomireBodySegment.SkeletonStable, 0xe1c6),
        new BodyEntryValue(CrocomireBodySegment.Dead, 0xe1cc),
        new BodyEntryValue(CrocomireBodySegment.SkeletonFlowingDownRiver, 0xe1d2),
    ];

    /// <summary>The native list each body animation starts at in bank $A4, the cartridge's entry for it (#2399).</summary>
    internal static readonly IReadOnlyDictionary<CrocomireBodyAnimation, ushort> BodyStarts = new Dictionary<CrocomireBodyAnimation, ushort>
    {
        [CrocomireBodyAnimation.IdleSway] = 0xbade,
        [CrocomireBodyAnimation.ProjectileAttack] = 0xbb36,
        [CrocomireBodyAnimation.StepForwardAfterDelay] = 0xbbca,
        [CrocomireBodyAnimation.StepForward] = 0xbbce,
        [CrocomireBodyAnimation.StepBack] = 0xbc30,
        [CrocomireBodyAnimation.SteppingBack] = 0xbc34,
        [CrocomireBodyAnimation.AwaitingHit] = 0xbc56,
        [CrocomireBodyAnimation.MovingClaws] = 0xbcd8,
        [CrocomireBodyAnimation.Roar] = 0xbd2a,
        [CrocomireBodyAnimation.RoarCloseMouth] = 0xbd8e,
        [CrocomireBodyAnimation.PowerBombReactionMouthFullyOpen] = 0xbdae,
        [CrocomireBodyAnimation.PowerBombReactionMouthPartiallyOpen] = 0xbdb2,
        [CrocomireBodyAnimation.PowerBombReactionMouthNotOpen] = 0xbdb6,
        [CrocomireBodyAnimation.NearSpikeWallCharge] = 0xbe7e,
        [CrocomireBodyAnimation.BackOffFromSpikeWall] = 0xbf3c,
        [CrocomireBodyAnimation.MeltingFleshTopRow] = 0xbf64,
        [CrocomireBodyAnimation.MeltingFleshTopTwoRows] = 0xbf6c,
        [CrocomireBodyAnimation.MeltingFleshTopThreeRows] = 0xbf72,
        [CrocomireBodyAnimation.MeltingFleshTopFourRows] = 0xbf78,
        [CrocomireBodyAnimation.MeltingBareSkullTopRow] = 0xbf7e,
        [CrocomireBodyAnimation.MeltingBareSkullTopTwoRows] = 0xbf86,
        [CrocomireBodyAnimation.MeltingBareSkullTopThreeRows] = 0xbf8c,
        [CrocomireBodyAnimation.MeltingBareSkullTopFourRows] = 0xbf92,
        [CrocomireBodyAnimation.BridgeCollapsed] = 0xbfb0,
        [CrocomireBodyAnimation.SkeletonFalling] = 0xe14a,
        [CrocomireBodyAnimation.SkeletonFallsApart] = 0xe158,
        [CrocomireBodyAnimation.SkeletonStable] = 0xe1c6,
        [CrocomireBodyAnimation.Dead] = 0xe1cc,
        [CrocomireBodyAnimation.SkeletonFlowingDownRiver] = 0xe1d2,
    };

    /// <summary>The cue each native bank-$A4 Crocomire callback must become.</summary>
    internal static readonly Dictionary<ushort, CrocomireBodyCue> BodyCallbacks = new()
    {
        [0x86a6] = new CrocomireBodyCue.Signal(CrocomireBodyCueKind.FightAi),
        [0x8752] = new CrocomireBodyCue.Signal(CrocomireBodyCueKind.MaybeStartProjectileAttack),
        [0x8cfb] = new CrocomireBodyCue.Signal(CrocomireBodyCueKind.Cry),
        [0x8d07] = new CrocomireBodyCue.Signal(CrocomireBodyCueKind.BigExplosionSound),
        [0x8d13] = new CrocomireBodyCue.Signal(CrocomireBodyCueKind.SkeletonCollapseSound),
        [0x8fc7] = new CrocomireBodyCue.Signal(CrocomireBodyCueKind.ShakeScreen),
        [0x8fdf] = new CrocomireBodyCue.Step(CrocomireStepDefinitions.Advance),
        [0x8ffa] = new CrocomireBodyCue.Step(CrocomireStepDefinitions.AdvanceRaisingDust),
        [0x8fff] = new CrocomireBodyCue.Step(CrocomireStepDefinitions.AdvanceRaisingDust),
        [0x901d] = new CrocomireBodyCue.Signal(CrocomireBodyCueKind.StepTowardSpikeWall),
        [0x905b] = new CrocomireBodyCue.Step(CrocomireStepDefinitions.StepBack),
        [0x907f] = new CrocomireBodyCue.Step(CrocomireStepDefinitions.BackOff),
        [0x908f] = new CrocomireBodyCue.Step(CrocomireStepDefinitions.StepBackRaisingDust),
        [0x9094] = new CrocomireBodyCue.Step(CrocomireStepDefinitions.BackOffRaisingDust),
        [0x9a9b] = new CrocomireBodyCue.Dust(-32),
        [0x9aa0] = new CrocomireBodyCue.Dust(0),
        [0x9aa5] = new CrocomireBodyCue.Dust(-16),
        [0x9aaa] = new CrocomireBodyCue.Dust(16),
        [0x9aaf] = new CrocomireBodyCue.Dust(0),
        [0x9ab4] = new CrocomireBodyCue.Dust(8),
        [0x9ab9] = new CrocomireBodyCue.Dust(16),
        [0x9abe] = new CrocomireBodyCue.Dust(24),
        [0x9ac3] = new CrocomireBodyCue.Dust(32),
        [0x9ac8] = new CrocomireBodyCue.Dust(40),
        [0x9acd] = new CrocomireBodyCue.Dust(48),
        [0x9ad2] = new CrocomireBodyCue.Dust(56),
        [0x9ad7] = new CrocomireBodyCue.Dust(64),
    };

    /// <summary>The bank-$A4 sound callbacks and the sound each queues.</summary>
    internal static readonly Dictionary<ushort, SoundEffectId> BodySoundCallbacks = new()
    {
        [0x8cfb] = CrocomireSoundDefinitions.Cry,
        [0x8d07] = CrocomireSoundDefinitions.BigExplosion,
        [0x8d13] = CrocomireSoundDefinitions.SkeletonCollapse,
    };

    /// <summary>The tongue programs' native entry points in bank $A4.</summary>
    internal static readonly TongueEntry[] TongueEntries =
    [
        new TongueEntry(CrocomireTongueSegment.Fight, 0xbe56),
        new TongueEntry(CrocomireTongueSegment.Hidden, 0xbf62),
        new TongueEntry(CrocomireTongueSegment.Melting, 0xbf98),
    ];
    /// <summary>The native entry point of <paramref name="segment"/>, a segment the cartridge writes out once.</summary>
    internal static ushort BodyEntry(CrocomireBodySegment segment) => BodyEntries.Single(item => item.Program == segment).Entry;

    /// <summary>Whether the native entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool BodyEntriesAscend => BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends);

    /// <summary>Whether a native reader's cursor has reached <paramref name="segment"/>'s list or a later one.</summary>
    internal static bool HasReached(NativeEnemyListReader reader, CrocomireBodySegment segment) => reader.Cursor >= BodyEntry(segment);

    /// <summary>The sound a native body callback queues, if it is a sound callback.</summary>
    internal static SoundEffectId? BodySound(ushort callback) => BodySoundCallbacks.TryGetValue(callback, out SoundEffectId sound) ? sound : null;

    /// <summary>What a native bank-$86 Crocomire projectile list word does.</summary>
    internal enum ProjectileWordKind
    {
        /// <summary>A frame: a duration and a spritemap operand.</summary>
        Frame,
        /// <summary>The goto and its target operand.</summary>
        GoTo,
        /// <summary>The drop with Crocomire's chances.</summary>
        CrocomireDrop,
        /// <summary>The deletion.</summary>
        Delete,
    }

    /// <summary>Decodes a native Crocomire projectile list word, rejecting words outside the compared set.</summary>
    internal static ProjectileWordKind ClassifyProjectileWord(ushort word) =>
        (word & NativeEnemyListDefinitions.InstructionBit) == 0 ? ProjectileWordKind.Frame
        : ProjectileWords.TryGetValue(word, out ProjectileWordKind kind) ? kind
        : throw new InvalidDataException($"Crocomire projectile list word ${word:X4} is outside the compared set.");

    /// <summary>A native bank-$86 Crocomire projectile list with the projectile interpreter's timer gate.</summary>
    internal sealed class NativeProjectileList(ushort cursor, ushort timer)
    {
        /// <summary>The next native instruction.</summary>
        private ushort cursor = cursor;
        /// <summary>Updates remaining before the next native instruction is consumed.</summary>
        internal ushort Timer { get; private set; } = timer;
        /// <summary>The spritemap operand of the latest native frame.</summary>
        internal ushort Operand { get; private set; }
        /// <summary>Drops the list has allocated.</summary>
        internal int Drops { get; private set; }
        /// <summary>Whether the list has deleted its projectile.</summary>
        internal bool Deleted { get; private set; }

        /// <summary>Runs one update.</summary>
        internal void Step(IImportCartridgeSource cartridge)
        {
            ushort old = Timer;
            Timer = WrappingCounter.Decrement(Timer).Value;
            if (old != 1)
                return;
            for (int command = 0; command < 8; command++)
            {
                ushort word = cartridge.ReadCartridgeWord(ProjectileBank | cursor);
                switch (ClassifyProjectileWord(word))
                {
                    case ProjectileWordKind.Frame:
                        Timer = word;
                        Operand = unchecked((ushort)(cursor + 2));
                        cursor = unchecked((ushort)(cursor + 4));
                        return;
                    case ProjectileWordKind.GoTo:
                        cursor = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
                        continue;
                    case ProjectileWordKind.CrocomireDrop:
                        Drops++;
                        cursor = unchecked((ushort)(cursor + 2));
                        continue;
                    case ProjectileWordKind.Delete:
                        Deleted = true;
                        return;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(ProjectileWordKind)}.");
                }
            }
            throw new InvalidDataException("A native Crocomire projectile list ran eight commands without a frame.");
        }
    }
}
