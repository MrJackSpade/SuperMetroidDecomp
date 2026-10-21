using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skree;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>The spawn and program of one verify skree metaree particle behavior case.</summary>
/// <param name="Spawn">The spawn.</param>
/// <param name="Program">The program.</param>
internal readonly record struct VerifySkreeMetareeParticleBehaviorCase(Action<RoomEnemySlot> Spawn, SkreeMetareeParticleProgram Program);
