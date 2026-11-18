namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's casual-flame bursts (#2258).</summary>
public static class PhantoonCasualFlameDefinitions
{
    /// <summary>Mouth-scheduler updates every burst waits after its last flame.</summary>
    private const int BurstCooldown = 180;

    /// <summary>Five flames, 32 updates apart.</summary>
    public static PhantoonCasualFlameSchedule SteadyFive { get; } = new()
    {
        Cooldown = BurstCooldown,
        Flames = [new(32), new(32), new(32), new(32), new(32)],
    };

    /// <summary>Three flames in quick succession, 16 updates apart.</summary>
    public static PhantoonCasualFlameSchedule QuickThree { get; } = new()
    {
        Cooldown = BurstCooldown,
        Flames = [new(16), new(16), new(16)],
    };

    /// <summary>Seven flames, slowly, 48 updates apart.</summary>
    public static PhantoonCasualFlameSchedule SlowSeven { get; } = new()
    {
        Cooldown = BurstCooldown,
        Flames = [new(48), new(48), new(48), new(48), new(48), new(48), new(48)],
    };

    /// <summary>Seven flames at uneven waits, first flame first.</summary>
    public static PhantoonCasualFlameSchedule UnevenSeven { get; } = new()
    {
        Cooldown = BurstCooldown,
        Flames = [new(32), new(16), new(32), new(64), new(32), new(64), new(16)],
    };

    /// <summary>The bursts the casual-flame update chooses from, by its random draw, each as likely as the others.</summary>
    public static IReadOnlyList<PhantoonCasualFlameSchedule> Choices { get; } = [SteadyFive, QuickThree, SlowSeven, UnevenSeven];
}
