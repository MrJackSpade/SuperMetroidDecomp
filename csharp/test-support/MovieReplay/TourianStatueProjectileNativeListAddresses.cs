using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$86 lists the Tourian statue projectile ROM oracle (#641) walks: each program's entry point, the
/// commands the lists use, and the family that binds the typed programs to them. Only verification reads them; the
/// port runs typed programs.
/// </summary>
internal static class TourianStatueProjectileNativeListAddresses
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<TourianStatueProjectileProgram>[] Entries =
    [
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.Delete, TourianStatueProjectileInstructionProgramToolingDefinitions.Delete),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.Splash, TourianStatueProjectileInstructionProgramDefinitionsConstants.Splash),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.EyeGlow, TourianStatueProjectileInstructionProgramDefinitionsConstants.EyeGlow),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.Particle, TourianStatueProjectileInstructionProgramDefinitions.Particle),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.Tail, TourianStatueProjectileInstructionProgramDefinitions.Tail),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.Soul, TourianStatueProjectileInstructionProgramDefinitions.Soul),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.BaseDecoration, TourianStatueProjectileInstructionProgramDefinitionsConstants.BaseDecoration),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.BaseDecorationLoop, BaseDecorationLoop),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.Ridley, TourianStatueProjectileInstructionProgramDefinitionsConstants.Ridley),
        new NativeProgramEntry<TourianStatueProjectileProgram>(TourianStatueProjectileProgram.Phantoon, TourianStatueProjectileInstructionProgramDefinitionsConstants.Phantoon),
    ];

    /// <summary>$86:B862, where the base decoration's list rests after installing its movement.</summary>
    private const ushort BaseDecorationLoop = 0xb862;

    /// <summary>$86:B818, the spawn-tail routine the particle's list would run on into once its counter ran out.</summary>
    private const ushort ParticleListEnd = 0xb818;

    /// <summary>Each command the lists use, the cue it becomes and its operand bytes.</summary>
    private static readonly Dictionary<ushort, TourianStatueProjectileAnimationsCommand> Commands = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.Delete] = new TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue>.Exactly(new TourianStatueProjectileCue.Command(TourianStatueProjectileCueKind.Delete)), 0),
        [(ushort)NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = new TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue>.Exactly(new TourianStatueProjectileCue.Command(TourianStatueProjectileCueKind.PlayReleaseSound)), 1),
        [(ushort)TourianStatueInstruction.Earthquake] = new TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue>.Exactly(new TourianStatueProjectileCue.Command(TourianStatueProjectileCueKind.Earthquake)), 0),
        [(ushort)TourianStatueInstruction.SpawnParticle] = new TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue>.Exactly(new TourianStatueProjectileCue.Command(TourianStatueProjectileCueKind.SpawnParticle)), 0),
        [(ushort)TourianStatueInstruction.SpawnTail] = new TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue>.Exactly(new TourianStatueProjectileCue.Command(TourianStatueProjectileCueKind.SpawnTail)), 0),
        [(ushort)TourianStatueInstruction.AddY] = new TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue>.Of<TourianStatueProjectileCue.MoveDown>(""), 2),
        [(ushort)NativeEnemyProjectileInstruction.FrameBehaviorInY] = new TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue>.Of<TourianStatueProjectileCue.StartMoving>(""), 2),
    };

    /// <summary>The oracle rules for the statue projectile programs.</summary>
    private static readonly NativeEnemyProjectileCueRules<TourianStatueProjectileCue> Rules = new(
        word => Commands.TryGetValue(word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            Commands[word].Cue.Check(cue, at);
            switch (cue)
            {
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.PlayReleaseSound }:
                    VerificationAssertions.AssertEqual<SoundEffectId>(EnemySound.TourianStatueParticle, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[0]),
                        $"{at}: the import decodes the native sound byte to the release sound");
                    break;
                case TourianStatueProjectileCue.MoveDown moveDown:
                    VerificationAssertions.AssertEqual((short)(operands[0] | operands[1] << 8), moveDown.Pixels, $"{at}: Y offset");
                    break;
                case TourianStatueProjectileCue.StartMoving startMoving:
                    VerificationAssertions.AssertEqual(startMoving.Movement, EnemyProjectileFrameBehaviorAccess.FromWord((ushort)(operands[0] | operands[1] << 8)), $"{at}: movement");
                    break;
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.Delete } or TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.Earthquake } or TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.SpawnParticle } or TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.SpawnTail }:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined Tourian statue projectile cue {cue}.");
            }
        },
        cue => cue is TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.Delete },
        EndsAt: address => address == ParticleListEnd);

    /// <summary>
    /// The family's typed animations beside their native lists. A particle starts its counted list directly, with the
    /// counter its spawn cleared.
    /// </summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, TourianStatueProjectileAnimations.Segments,
        TourianStatueProjectileAnimations.StartingAt,
        frame => EnemyProjectileProgramFrameSources.Operand(frame), Rules,
        startsWithClearedCounter: new HashSet<TourianStatueProjectileProgram> { TourianStatueProjectileProgram.Particle });

}
