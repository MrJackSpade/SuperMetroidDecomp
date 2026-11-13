namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What Kraid's colours show (#1627): the hurt flash's colours, or one of his eight health bands', in place of a band
/// number with minus one standing for the flash.
/// </summary>
public readonly record struct KraidColourBand
{
    /// <summary>The health band shown, zero through seven, or null for the hurt flash.</summary>
    private readonly int? health;

    /// <summary>Takes the health band shown, or null for the hurt flash.</summary>
    private KraidColourBand(int? health) => this.health = health;

    /// <summary>The hurt flash's colours.</summary>
    public static KraidColourBand HurtFlash { get; } = new(null);

    /// <summary>Health band <paramref name="band"/>'s colours, zero through seven, lowest health first.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The band is not one of the eight.</exception>
    public static KraidColourBand Health(int band) =>
        new(Ensure.BetweenInclusive(0, KraidHealthThresholdDefinitions.EighthCount - 1, band));

    /// <summary>Every colour band, the hurt flash first and then each health band, lowest first: the order his colours hold them in.</summary>
    public static IReadOnlyList<KraidColourBand> All { get; } =
        [HurtFlash, .. Enumerable.Range(0, KraidHealthThresholdDefinitions.EighthCount).Select(Health)];

    /// <summary>The health band shown, zero through seven, or null for the hurt flash.</summary>
    public int? HealthBand => health;

    /// <summary>
    /// The band's place among his health colours' groups, which hold the hurt flash's group first and then each health
    /// band's: zero for the flash, one through eight for the bands.
    /// </summary>
    public int ColourGroup => health is { } band ? band + 1 : 0;

    /// <inheritdoc />
    public override string ToString() => health is { } band ? $"health band {band}" : "hurt flash";
}
