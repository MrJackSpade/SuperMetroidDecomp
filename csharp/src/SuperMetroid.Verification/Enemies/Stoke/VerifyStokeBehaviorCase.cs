using SuperMetroid.Core.Enemies.Stoke;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>The direction, walk, loop, attack, moving and fireball of one verify stoke behavior case.</summary>
/// <param name="Direction">The direction.</param>
/// <param name="Walk">The walk.</param>
/// <param name="Loop">The loop.</param>
/// <param name="Attack">The attack.</param>
/// <param name="Moving">The moving.</param>
/// <param name="Fireball">The fireball.</param>
internal readonly record struct VerifyStokeBehaviorCase(StokeDirection Direction, StokeProgram Walk, StokeProgram Loop, StokeProgram Attack, StokeAiFunction Moving, ushort Fireball);
