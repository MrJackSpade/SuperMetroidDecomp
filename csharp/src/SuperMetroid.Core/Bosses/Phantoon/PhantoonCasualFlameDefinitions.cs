namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's casual-flame bursts.</summary>
public static class PhantoonCasualFlameDefinitions
{
    /// <summary>Mouth-scheduler updates every burst waits after its last flame.</summary>
    private const int BurstCooldown = 180;

    /// <summary>
    /// The casual-flame bursts, selected by the casual-flame update: uniform bursts of five, three and seven flames,
    /// then an irregular seven-flame burst. Uniform waits are 16 updates per burst rank, with rank 1..3 containing
    /// 2*rank+1 flames. Each burst's intervals are listed in the order the mouth waits them.
    /// </summary>
    internal static readonly PhantoonCasualFlameSchedule[] Schedules =
    [
        new() { Cooldown = BurstCooldown, Intervals = [32, 32, 32, 32, 32] },
        new() { Cooldown = BurstCooldown, Intervals = [16, 16, 16] },
        new() { Cooldown = BurstCooldown, Intervals = [48, 48, 48, 48, 48, 48, 48] },
        new() { Cooldown = BurstCooldown, Intervals = [32, 16, 32, 64, 32, 64, 16] },
    ];
}
