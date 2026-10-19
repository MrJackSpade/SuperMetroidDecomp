using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>#641: the Tourian statue projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class TourianStatueProjectileAnimationsVerification
{
    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (TourianStatueProjectileProgram Program, ushort Entry)[] Entries =
    [
        (TourianStatueProjectileProgram.Delete, TourianStatueProjectileInstructionProgramToolingDefinitions.Delete),
        (TourianStatueProjectileProgram.Splash, TourianStatueProjectileInstructionProgramDefinitionsConstants.Splash),
        (TourianStatueProjectileProgram.EyeGlow, TourianStatueProjectileInstructionProgramDefinitionsConstants.EyeGlow),
        (TourianStatueProjectileProgram.Particle, TourianStatueProjectileInstructionProgramDefinitions.Particle),
        (TourianStatueProjectileProgram.Tail, TourianStatueProjectileInstructionProgramDefinitions.Tail),
        (TourianStatueProjectileProgram.Soul, TourianStatueProjectileInstructionProgramDefinitions.Soul),
        (TourianStatueProjectileProgram.BaseDecoration, TourianStatueProjectileInstructionProgramDefinitionsConstants.BaseDecoration),
        (TourianStatueProjectileProgram.BaseDecorationLoop, BaseDecorationLoop),
        (TourianStatueProjectileProgram.Ridley, TourianStatueProjectileInstructionProgramDefinitionsConstants.Ridley),
        (TourianStatueProjectileProgram.Phantoon, TourianStatueProjectileInstructionProgramDefinitionsConstants.Phantoon),
    ];

    /// <summary>$86:B862, where the base decoration's list rests after installing its movement.</summary>
    private const ushort BaseDecorationLoop = 0xb862;

    /// <summary>$86:B818, the spawn-tail routine the particle's list would run on into once its counter ran out.</summary>
    private const ushort ParticleListEnd = 0xb818;

    /// <summary>Each command the lists use, the cue it becomes and its operand bytes.</summary>
    private static readonly Dictionary<ushort, (TourianStatueProjectileCueKind Kind, int OperandBytes)> Commands = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.Delete] = (TourianStatueProjectileCueKind.Delete, 0),
        [(ushort)NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = (TourianStatueProjectileCueKind.PlayReleaseSound, 1),
        [(ushort)TourianStatueInstruction.Earthquake] = (TourianStatueProjectileCueKind.Earthquake, 0),
        [(ushort)TourianStatueInstruction.SpawnParticle] = (TourianStatueProjectileCueKind.SpawnParticle, 0),
        [(ushort)TourianStatueInstruction.SpawnTail] = (TourianStatueProjectileCueKind.SpawnTail, 0),
        [(ushort)TourianStatueInstruction.AddY] = (TourianStatueProjectileCueKind.MoveDown, 2),
        [(ushort)NativeEnemyProjectileInstruction.PreInstructionInY] = (TourianStatueProjectileCueKind.StartMoving, 2),
    };

    /// <summary>The oracle rules for the statue projectile programs.</summary>
    private static readonly NativeEnemyProjectileCueRules<TourianStatueProjectileCue> Rules = new(
        word => Commands.TryGetValue(word, out var command) ? command.OperandBytes : null,
        (word, operands, cue, at) =>
        {
            AssertEqual(Commands[word].Kind, cue.Kind, $"{at}: cue");
            switch (cue.Kind)
            {
                case TourianStatueProjectileCueKind.PlayReleaseSound:
                    AssertEqual(SoundEffectLibrary2Sounds.TourianStatueRelease.Value, (int)operands[0], $"{at}: sound");
                    break;
                case TourianStatueProjectileCueKind.MoveDown:
                    AssertEqual((int)(short)(operands[0] | operands[1] << 8), cue.Pixels, $"{at}: Y offset");
                    break;
                case TourianStatueProjectileCueKind.StartMoving:
                    AssertEqual((ushort)(operands[0] | operands[1] << 8), EnemyProjectilePreInstructionAccess.Word(cue.Movement), $"{at}: movement");
                    break;
            }
        },
        cue => cue.Kind == TourianStatueProjectileCueKind.Delete,
        EndsAt: address => address == ParticleListEnd);

    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, TourianStatueProjectileAnimations.Programs,
        frame => EnemyProjectileFrames.ProgramFrameKey(TourianStatueProjectileFrameDefinitions.ShownProjectileFrames[frame]), Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyTourianStatueProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Tourian statue projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
