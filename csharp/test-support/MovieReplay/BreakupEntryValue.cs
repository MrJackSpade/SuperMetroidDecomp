using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>The frame and entry of one breakup entry value.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct BreakupEntryValue(EnemyFrameId Frame, ushort Entry);
