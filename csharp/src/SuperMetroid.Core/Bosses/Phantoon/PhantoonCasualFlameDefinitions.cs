namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon mouth schedules retain the native header and reverse-read timer layout.</summary>
public static class PhantoonCasualFlameDefinitions
{
    /// <summary>
    /// The mouth's remaining-flame count when no schedule is active: the next expiry picks a new pattern.
    /// </summary>
    public const int NoActiveSchedule = -1;

    /// <summary>
    /// The native casual-flame tables at $A7:CD05, $A7:CD13, $A7:CD1D and $A7:CD2F, selected by $A7:CCFD: flame count,
    /// 180-update cooldown, then one interval per flame. $A7:CCFD selects five-, three-, seven-flame uniform bursts,
    /// then an irregular seven-flame burst whose seven reverse-read intervals are independent timing. Uniform holds are
    /// 16 ticks per burst rank, with rank 1..3 containing 2*rank+1 flames. Timing words count mouth-scheduler updates;
    /// the scheduler reads intervals from the end toward the header before starting each flame's mouth animation.
    /// </summary>
    internal static readonly ushort[][] Schedules =
    [
        [5, 180, 32, 32, 32, 32, 32],
        [3, 180, 16, 16, 16],
        [7, 180, 48, 48, 48, 48, 48, 48, 48],
        [7, 180, 16, 64, 32, 64, 32, 16, 32],
    ];
}
