namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Where Kraid's head animation stands in its list (#1600): the segment, and the step its play reaches next, counted
/// from the list's first, the list's end standing for a play that is done or holds there.
/// </summary>
/// <param name="Animation">The head segment the animation plays.</param>
/// <param name="Step">The list step the play reaches next.</param>
internal readonly record struct KraidHeadListPosition(KraidHeadSegment Animation, int Step);
