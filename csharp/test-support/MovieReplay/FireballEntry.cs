using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>The program and entry of one fireball entry.</summary>
/// <param name="Program">The program.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct FireballEntry(RidleyFireballSegment Program, ushort Entry);
