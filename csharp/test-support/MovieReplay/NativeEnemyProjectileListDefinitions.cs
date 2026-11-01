using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.TestSupport.VerificationImporters;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// A minimal reader of native bank-$86 enemy projectile lists for the #641 ROM oracles: it steps a list with the
/// projectile interpreter's timer gate so a typed projectile animation can be compared with it update by update.
/// Only verification reads cartridge lists; the port runs typed animations.
/// </summary>
internal static class NativeEnemyProjectileListDefinitions
{
    /// <summary>The projectile bank, as a long-address base.</summary>
    internal const int ProjectileBank = 0x860000;
    /// <summary>Bit 15 marks an instruction word; a clear bit makes the word a frame duration.</summary>
    private const ushort InstructionBit = 0x8000;

    /// <summary>What a native projectile list word does.</summary>
    internal enum WordKind
    {
        /// <summary>A frame: a duration and a spritemap operand.</summary>
        Frame,
        /// <summary>The pre-instruction install and its routine operand.</summary>
        FrameBehavior,
        /// <summary>Clears the pre-instruction.</summary>
        ClearFrameBehavior,
        /// <summary>Clears the projectile's palette selection.</summary>
        UsePaletteZero,
        /// <summary>A long call to an external routine, recorded rather than run.</summary>
        CallExternal,
        /// <summary>The goto and its target operand.</summary>
        GoTo,
        /// <summary>The sleep.</summary>
        Sleep,
        /// <summary>The deletion.</summary>
        Delete,
        /// <summary>The general-timer load and its count operand.</summary>
        SetCounter,
        /// <summary>The general-timer countdown and its target operand.</summary>
        RepeatWhileCounting,
        /// <summary>An owner callback with no operands, recorded rather than run.</summary>
        Callback,
        /// <summary>The random scatter and its two packed operand words, recorded rather than run.</summary>
        Scatter,
        /// <summary>An owner callback with one operand word, recorded rather than run.</summary>
        CallbackWithWord,
    }

    /// <summary>The bank-$86 opcodes the reader decodes besides frames.</summary>
    private static readonly Dictionary<ushort, WordKind> Words = new()
    {
        [(ushort)NativeEnemyProjectileInstruction.FrameBehaviorInY] = WordKind.FrameBehavior,
        [(ushort)NativeEnemyProjectileInstruction.ClearFrameBehavior] = WordKind.ClearFrameBehavior,
        [(ushort)NativeEnemyProjectileInstruction.UsePalette0] = WordKind.UsePaletteZero,
        [(ushort)NativeEnemyProjectileInstruction.CallExternalFunctionInY] = WordKind.CallExternal,
        [(ushort)NativeEnemyProjectileInstruction.GotoY] = WordKind.GoTo,
        [(ushort)NativeEnemyProjectileInstruction.Sleep] = WordKind.Sleep,
        [(ushort)NativeEnemyProjectileInstruction.Delete] = WordKind.Delete,
        [(ushort)NativeEnemyProjectileInstruction.TimerInY] = WordKind.SetCounter,
        [(ushort)NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero] = WordKind.RepeatWhileCounting,
        [(ushort)NativeEnemyProjectileInstruction.SpawnPhantoonDrop] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.SporeSpawner_SpawnSpore] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.Spores_SetProperties3000] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.EnemyDeathExpl_QueueEnemyKilledSoundFX] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.Spores_SpawnEnemyDrops] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.MagdolliteFlame_SpawnDrops] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.KagoBugStartJumpInstruction] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.NoobTubeShardAssignFallingAngle] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.NoobTubeBubbleAssignFallingAngle] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.CalculateDirectionTowardsSamus] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.DownwardGateSetYVelocityInstruction] = WordKind.CallbackWithWord,
        [(ushort)NativeEnemyProjectileInstruction.KagoBugStartIdleInstruction] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.KagoBugSpawnDropInstruction] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.KagoBugUsePaletteZeroInstruction] = WordKind.Callback,
        [(ushort)NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = WordKind.Scatter,
        [(ushort)NativeEnemyProjectileInstruction.TourianStatueResetDustPosition] = WordKind.Callback,
    };

    /// <summary>Decodes a native projectile list word, rejecting words outside the compared set.</summary>
    internal static WordKind Classify(ushort word) =>
        (word & InstructionBit) == 0 ? WordKind.Frame
        : Words.TryGetValue(word, out WordKind kind) ? kind
        : throw new InvalidDataException($"Projectile list word ${word:X4} is outside the compared set.");
}
