using SuperMetroid.Core.Bosses.Crocomire;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>One body segment's native list (#2399): the segment, and where in bank $A4 its run, or a copy of it, starts.</summary>
/// <param name="Program">The segment.</param>
/// <param name="Entry">The native address its run starts at.</param>
internal readonly record struct BodyEntryValue(CrocomireBodySegment Program, ushort Entry);
