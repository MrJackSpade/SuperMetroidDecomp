namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One flame of a casual-flame burst (<see cref="PhantoonCasualFlameSchedule"/>): the mouth's wait before it spits it.</summary>
/// <param name="WaitBeforeUpdates">
/// The mouth-scheduler updates waited before the flame, the last being the update the mouth spits it on.
/// </param>
public sealed record PhantoonCasualFlame(int WaitBeforeUpdates);
