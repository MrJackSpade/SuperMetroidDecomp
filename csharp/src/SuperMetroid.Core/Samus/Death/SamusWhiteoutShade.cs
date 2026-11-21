using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// One shade of the whiteout Samus's death explosion washes everything but her body in (#2387): the room and every other
/// sprite whiten to the shade's installed colour (<see cref="SamusDeathSteps.WhiteoutColour"/>), and her beams and grapple
/// flare show <see cref="SamusShades.DeathWhiteout"/>. The explosion brightens through them to
/// <see cref="Brightest"/>, and its last update shows <see cref="Final"/>.
/// </summary>
public readonly record struct SamusWhiteoutShade
{
    /// <summary>The shade <paramref name="step"/> of the whiteout, from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">There is no such whiteout shade.</exception>
    internal SamusWhiteoutShade(int step) =>
        Step = Ensure.BetweenInclusive(0, SamusShadeDefinitions.Death.WhiteoutShadeCount - 1, step);

    /// <summary>The brightest shade the explosion brightens to and holds while it lasts.</summary>
    public static SamusWhiteoutShade Brightest { get; } = new(SamusDeathSequenceDefinitions.BrightestExplosionWhiteoutShade);

    /// <summary>The shade the explosion's last update shows, as everything but Samus's body turns white.</summary>
    public static SamusWhiteoutShade Final { get; } = new(SamusDeathSequenceDefinitions.FinalWhiteoutShade);

    /// <summary>Every whiteout shade, in order.</summary>
    public static IReadOnlyList<SamusWhiteoutShade> All { get; } =
        [.. Enumerable.Range(0, SamusShadeDefinitions.Death.WhiteoutShadeCount).Select(step => new SamusWhiteoutShade(step))];

    /// <summary>The shades the explosion brightens through before <see cref="Brightest"/>, in order (#2447).</summary>
    public static IReadOnlyList<SamusWhiteoutShade> Brightening { get; } = [.. All.Take(Brightest.Step)];

    /// <summary>The shade's step, from zero, which its installed key and colour carry.</summary>
    public int Step { get; }
}
