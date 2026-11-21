using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// One of the shades Samus's suit and her body without it show as she dies (#2387): the flash alternates the first two,
/// and each frame of the suit's explosion shows the one the installation names for it. Her colours under each are
/// <see cref="SamusShades.DeathSuit"/> and <see cref="SamusShades.DeathSuitless"/>.
/// </summary>
public readonly partial record struct SamusDeathShade
{
    /// <summary>The shade <paramref name="step"/> of the death shades, from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no such death shade.</exception>
    public SamusDeathShade(int step) =>
        Step = Ensure.BetweenInclusive(0, SamusShadeDefinitions.Death.ExplosionShadeCount - 1, step);

    /// <summary>The shade the flash starts on and the explosion starts from: the first.</summary>
    public static SamusDeathShade First { get; } = new(0);

    /// <summary>The shade the flash alternates with the first: the second.</summary>
    public static SamusDeathShade Second { get; } = new(1);

    /// <summary>Every death shade, in order.</summary>
    public static IReadOnlyList<SamusDeathShade> All { get; } =
        [.. Enumerable.Range(0, SamusShadeDefinitions.Death.ExplosionShadeCount).Select(step => new SamusDeathShade(step))];

    /// <summary>The shade's step, from zero, which its installed key carries.</summary>
    public int Step { get; }
}
