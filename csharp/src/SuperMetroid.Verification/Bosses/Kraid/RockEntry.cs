using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The program and entry of one rock entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct RockEntry(KraidRockProgram Program, ushort Entry);
