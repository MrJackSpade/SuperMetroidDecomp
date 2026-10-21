using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>The spit and program of one verify ki hunter acid spit behavior case.</summary>
/// <param name="Spit">The spit.</param>
/// <param name="Program">The program.</param>
internal readonly record struct VerifyKiHunterAcidSpitBehaviorCase(RoomEnemyProjectileSlot Spit, KiHunterAcidSpitProgram Program);
