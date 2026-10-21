using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>The facing and program of one verify yapping maw body projectile behavior case.</summary>
/// <param name="Facing">The facing.</param>
/// <param name="Program">The program.</param>
internal readonly record struct VerifyYappingMawBodyProjectileBehaviorCase(ushort Facing, YappingMawBodyProjectileProgram Program);
