using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>The program, kind and entry of one ceres falling debris native list addresses entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Kind">The kind.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct CeresFallingDebrisNativeListAddressesEntry(CeresFallingDebrisProgram Program, RoomEnemyProjectileKind Kind, ushort Entry);
