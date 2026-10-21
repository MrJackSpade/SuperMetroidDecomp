using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The moving right, program, loop, movement and expected x of one verify space pirate projectile instruction program definitions case.</summary>
/// <param name="MovingRight">The moving right.</param>
/// <param name="Program">The program.</param>
/// <param name="Loop">The loop.</param>
/// <param name="Movement">The movement.</param>
/// <param name="ExpectedX">The expected x.</param>
internal readonly record struct VerifySpacePirateProjectileInstructionProgramDefinitionsCase(bool MovingRight, ushort Program, ushort Loop, EnemyProjectilePreInstruction Movement, ushort ExpectedX);
