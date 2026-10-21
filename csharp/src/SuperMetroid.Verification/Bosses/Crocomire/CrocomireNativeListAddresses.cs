using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Enemies;

/// <summary>
/// The native bank-$A4 and bank-$86 lists the Crocomire ROM oracle (#1282) walks: list entry points, callbacks,
/// the common list opcodes and minimal readers that decode them. Only verification reads them; the port runs
/// typed programs.
/// </summary>
internal static class CrocomireNativeListAddresses
{
    /// <summary>Crocomire's instruction bank, $A4.</summary>
    internal const byte BodyBankNumber = 0xa4;
    /// <summary>Crocomire's instruction bank, $A4, as a long-address base.</summary>
    internal const int BodyBank = 0xa40000;
    /// <summary>The bank-$86 projectile instruction lists, as a long-address base.</summary>
    internal const int ProjectileBank = 0x860000;
    /// <summary>The bank-$86 opcodes a Crocomire projectile list uses besides frames.</summary>
    private static readonly Dictionary<ushort, ProjectileWordKind> ProjectileWords = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.GotoY] = ProjectileWordKind.GoTo,
        [(ushort)NativeEnemyProjectileInstruction.SpawnEnemyDropsWithCrocomireChances] = ProjectileWordKind.CrocomireDrop,
        [(ushort)NativeEnemyProjectileInstruction.Delete] = ProjectileWordKind.Delete,
    };

    /// <summary>Each typed body program's native entry point in bank $A4, in authored order.</summary>
    internal static readonly BodyEntryValue[] BodyEntries =
    [
        new BodyEntryValue(CrocomireBodyProgram.Initial, 0xbade),
        new BodyEntryValue(CrocomireBodyProgram.UnusedChargeForwardOneStep, 0xbaea),
        new BodyEntryValue(CrocomireBodyProgram.ProjectileAttack, 0xbb36),
        new BodyEntryValue(CrocomireBodyProgram.ProjectileAttackLoop, 0xbb94),
        new BodyEntryValue(CrocomireBodyProgram.StepForwardAfterDelay, 0xbbca),
        new BodyEntryValue(CrocomireBodyProgram.StepForward, 0xbbce),
        new BodyEntryValue(CrocomireBodyProgram.StepBack, 0xbc30),
        new BodyEntryValue(CrocomireBodyProgram.SteppingBack, 0xbc34),
        new BodyEntryValue(CrocomireBodyProgram.WaitForFirstSecondDamage, 0xbc56),
        new BodyEntryValue(CrocomireBodyProgram.MovingClaws, 0xbcd8),
        new BodyEntryValue(CrocomireBodyProgram.Roar, 0xbd2a),
        new BodyEntryValue(CrocomireBodyProgram.RoarCloseMouth, 0xbd8e),
        new BodyEntryValue(CrocomireBodyProgram.RoarCloseMouthLoop, 0xbda2),
        new BodyEntryValue(CrocomireBodyProgram.PowerBombReactionMouthFullyOpen, 0xbdae),
        new BodyEntryValue(CrocomireBodyProgram.PowerBombReactionMouthPartiallyOpen, 0xbdb2),
        new BodyEntryValue(CrocomireBodyProgram.PowerBombReactionMouthNotOpen, 0xbdb6),
        new BodyEntryValue(CrocomireBodyProgram.PowerBombReactionMouthNotOpenLoop, 0xbe06),
        new BodyEntryValue(CrocomireBodyProgram.NearSpikeWallCharge, 0xbe7e),
        new BodyEntryValue(CrocomireBodyProgram.NearSpikeWallChargeLoop, 0xbeec),
        new BodyEntryValue(CrocomireBodyProgram.BackOffFromSpikeWall, 0xbf3c),
        new BodyEntryValue(CrocomireBodyProgram.MeltingOneTopRow, 0xbf64),
        new BodyEntryValue(CrocomireBodyProgram.MeltingOneTopTwoRows, 0xbf6c),
        new BodyEntryValue(CrocomireBodyProgram.MeltingOneTopThreeRows, 0xbf72),
        new BodyEntryValue(CrocomireBodyProgram.MeltingOneTopFourRows, 0xbf78),
        new BodyEntryValue(CrocomireBodyProgram.MeltingTwoTopRow, 0xbf7e),
        new BodyEntryValue(CrocomireBodyProgram.MeltingTwoTopTwoRows, 0xbf86),
        new BodyEntryValue(CrocomireBodyProgram.MeltingTwoTopThreeRows, 0xbf8c),
        new BodyEntryValue(CrocomireBodyProgram.MeltingTwoTopFourRows, 0xbf92),
        new BodyEntryValue(CrocomireBodyProgram.BridgeCollapsed, 0xbfb0),
        new BodyEntryValue(CrocomireBodyProgram.SkeletonFalling, 0xe14a),
        new BodyEntryValue(CrocomireBodyProgram.SkeletonFallsApart, 0xe158),
        new BodyEntryValue(CrocomireBodyProgram.SkeletonStable, 0xe1c6),
        new BodyEntryValue(CrocomireBodyProgram.Dead, 0xe1cc),
        new BodyEntryValue(CrocomireBodyProgram.SkeletonFlowingDownRiver, 0xe1d2),
    ];

    /// <summary>The cue each native bank-$A4 Crocomire callback must become.</summary>
    internal static readonly Dictionary<ushort, CrocomireBodyCue> BodyCallbacks = new()
    {
        [0x86a6] = new(CrocomireBodyCueKind.FightAi, 0),
        [0x8752] = new(CrocomireBodyCueKind.MaybeStartProjectileAttack, 0),
        [0x8cfb] = new(CrocomireBodyCueKind.Cry, 0),
        [0x8d07] = new(CrocomireBodyCueKind.BigExplosionSound, 0),
        [0x8d13] = new(CrocomireBodyCueKind.SkeletonCollapseSound, 0),
        [0x8fc7] = new(CrocomireBodyCueKind.ShakeScreen, 0),
        [0x8fdf] = new(CrocomireBodyCueKind.MoveLeft, 0),
        [0x8ffa] = new(CrocomireBodyCueKind.MoveLeftWithDust, 0),
        [0x8fff] = new(CrocomireBodyCueKind.MoveLeftWithDust, 0),
        [0x901d] = new(CrocomireBodyCueKind.MoveLeftTowardSpikeWall, 0),
        [0x905b] = new(CrocomireBodyCueKind.MoveRightIfOnScreen, 0),
        [0x907f] = new(CrocomireBodyCueKind.MoveRight, 0),
        [0x908f] = new(CrocomireBodyCueKind.MoveRightIfOnScreenWithDust, 0),
        [0x9094] = new(CrocomireBodyCueKind.MoveRightWithDust, 0),
        [0x9a9b] = new(CrocomireBodyCueKind.Dust, -32),
        [0x9aa0] = new(CrocomireBodyCueKind.Dust, 0),
        [0x9aa5] = new(CrocomireBodyCueKind.Dust, -16),
        [0x9aaa] = new(CrocomireBodyCueKind.Dust, 16),
        [0x9aaf] = new(CrocomireBodyCueKind.Dust, 0),
        [0x9ab4] = new(CrocomireBodyCueKind.Dust, 8),
        [0x9ab9] = new(CrocomireBodyCueKind.Dust, 16),
        [0x9abe] = new(CrocomireBodyCueKind.Dust, 24),
        [0x9ac3] = new(CrocomireBodyCueKind.Dust, 32),
        [0x9ac8] = new(CrocomireBodyCueKind.Dust, 40),
        [0x9acd] = new(CrocomireBodyCueKind.Dust, 48),
        [0x9ad2] = new(CrocomireBodyCueKind.Dust, 56),
        [0x9ad7] = new(CrocomireBodyCueKind.Dust, 64),
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
        new TongueEntry(CrocomireTongueProgram.Fight, 0xbe56),
        new TongueEntry(CrocomireTongueProgram.Sleep, 0xbf62),
        new TongueEntry(CrocomireTongueProgram.Melting, 0xbf98),
    ];
    /// <summary>The native entry point of <paramref name="program"/>.</summary>
    internal static ushort BodyEntry(CrocomireBodyProgram program) => BodyEntries.Single(item => item.Program == program).Entry;

    /// <summary>The body program whose native list starts at <paramref name="address"/>, if one does.</summary>
    internal static CrocomireBodyProgram? BodyProgramAt(ushort address) =>
        BodyEntries.Where(item => item.Entry == address).Select(item => (CrocomireBodyProgram?)item.Program).SingleOrDefault();

    /// <summary>Whether the native entries ascend, so the typed declaration order is the authored order.</summary>
    internal static bool BodyEntriesAscend => BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends);

    /// <summary>Whether a native reader's cursor has reached <paramref name="program"/>'s list or a later one.</summary>
    internal static bool HasReached(NativeEnemyListDefinitions.Reader reader, CrocomireBodyProgram program) => reader.Cursor >= BodyEntry(program);

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
