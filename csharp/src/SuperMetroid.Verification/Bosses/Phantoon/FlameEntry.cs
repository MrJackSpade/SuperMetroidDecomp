using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>The program and entry of one flame entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct FlameEntry(PhantoonFlameProgram Program, ushort Entry);
