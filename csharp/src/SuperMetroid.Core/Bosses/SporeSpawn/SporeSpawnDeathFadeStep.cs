namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// One step of Spore Spawn's death fade over his body, his room's level and its background (#2316): his hardening shows
/// each in turn, and a defeated room loads him at the last, the husk's colours.
/// </summary>
public readonly record struct SporeSpawnDeathFadeStep
{
    /// <summary>The steps of the fade.</summary>
    private const int StepCount = 7;

    /// <summary>The step at <paramref name="step"/> of the fade.</summary>
    private SporeSpawnDeathFadeStep(int step) => Step = step;

    /// <summary>The step's place in the fade, from zero: the step of its installed shades.</summary>
    public int Step { get; }

    /// <summary>Every step of the fade, in the order his hardening shows them.</summary>
    public static IReadOnlyList<SporeSpawnDeathFadeStep> All { get; } = [.. Enumerable.Range(0, StepCount).Select(step => new SporeSpawnDeathFadeStep(step))];

    /// <summary>The fade's last step: the husk's colours.</summary>
    public static SporeSpawnDeathFadeStep Husk => All[^1];
}
