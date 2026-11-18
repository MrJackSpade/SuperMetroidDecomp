namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// One step of Spore Spawn's death fade over his body, his room's level and its background (#2316, #2409): his hardening
/// shows each in turn, each a sixth more of the way from the colours his body, level and background start it in to the
/// husk's, and a defeated room loads him at the last, the husk's colours.
/// </summary>
public readonly record struct SporeSpawnDeathFadeStep
{
    /// <summary>The step whose installed shades are <paramref name="installedShade"/>.</summary>
    private SporeSpawnDeathFadeStep(int installedShade) => Step = installedShade;

    /// <summary>The number its installed shades take in their shades' names.</summary>
    public int Step { get; }

    /// <summary>The fade's first step, none of the way to the husk's colours: his body in his most hurt colours.</summary>
    public static SporeSpawnDeathFadeStep Unhardened { get; } = new(0);

    /// <summary>A sixth of the way to the husk's colours.</summary>
    public static SporeSpawnDeathFadeStep SixthHardened { get; } = new(1);

    /// <summary>A third of the way to the husk's colours.</summary>
    public static SporeSpawnDeathFadeStep ThirdHardened { get; } = new(2);

    /// <summary>Half of the way to the husk's colours.</summary>
    public static SporeSpawnDeathFadeStep HalfHardened { get; } = new(3);

    /// <summary>Two thirds of the way to the husk's colours.</summary>
    public static SporeSpawnDeathFadeStep TwoThirdsHardened { get; } = new(4);

    /// <summary>Five sixths of the way to the husk's colours.</summary>
    public static SporeSpawnDeathFadeStep FiveSixthsHardened { get; } = new(5);

    /// <summary>The fade's last step: the husk's colours, which a defeated room loads him in.</summary>
    public static SporeSpawnDeathFadeStep Husk { get; } = new(6);

    /// <summary>Every step of the fade, in the order his hardening shows them.</summary>
    public static IReadOnlyList<SporeSpawnDeathFadeStep> All { get; } =
        [Unhardened, SixthHardened, ThirdHardened, HalfHardened, TwoThirdsHardened, FiveSixthsHardened, Husk];
}
