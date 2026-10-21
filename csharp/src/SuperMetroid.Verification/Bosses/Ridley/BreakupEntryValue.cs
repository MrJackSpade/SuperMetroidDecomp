using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The frame and entry of one breakup entry value.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct BreakupEntryValue(RidleyBreakupFrame Frame, ushort Entry);
