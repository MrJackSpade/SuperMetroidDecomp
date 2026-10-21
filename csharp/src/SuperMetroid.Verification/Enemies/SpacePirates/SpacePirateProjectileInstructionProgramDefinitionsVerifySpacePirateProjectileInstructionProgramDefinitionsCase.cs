using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The direction, program, loop and movement of one space pirate projectile instruction program definitions verify space pirate projectile instruction program definitions case.</summary>
/// <param name="Direction">The direction.</param>
/// <param name="Program">The program.</param>
/// <param name="Loop">The loop.</param>
/// <param name="Movement">The movement.</param>
internal readonly record struct SpacePirateProjectileInstructionProgramDefinitionsVerifySpacePirateProjectileInstructionProgramDefinitionsCase(ushort Direction, ushort Program, ushort Loop, EnemyProjectilePreInstruction Movement);
