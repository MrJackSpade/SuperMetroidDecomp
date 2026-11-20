namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Where Kraid's head animation stands (#1600, #2399): the animation, named by the segment it starts at, and the step its
/// play reaches next, counted through its segments from the first, the animation's end standing for a play that is done or
/// holds there.
/// </summary>
/// <param name="Animation">The head animation, named by the segment it starts at.</param>
/// <param name="Step">
/// The animation's step the play reaches next, a count of the steps it has passed: a play that has passed every step,
/// done or holding there, has passed the animation's step count of them.
/// </param>
internal readonly record struct KraidHeadPlace(KraidHeadSegment Animation, int Step);
