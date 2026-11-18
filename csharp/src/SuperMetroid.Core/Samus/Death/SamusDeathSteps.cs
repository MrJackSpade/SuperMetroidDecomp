using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// The order Samus's death sequence steps through (#1512): which death shade each explosion frame shows her suit and
/// suitless body in, and the colour the room and every other sprite whiten to under each whiteout shade. The import decodes
/// both from the installed death artwork; the colours of her death shades are pre-coloured on her sheets, so only the
/// room's whiteout colour, which the room still shows through its own colours, remains a colour here.
/// </summary>
public sealed class SamusDeathSteps
{
    /// <summary>The death shade each explosion frame shows, by explosion frame.</summary>
    private readonly ImmutableArray<SamusDeathShade> explosionShades;

    /// <summary>The whiteout colour of each whiteout shade.</summary>
    private readonly ImmutableArray<RgbColor> whiteout;

    /// <summary>Keeps the explosion order and the whiteout colours.</summary>
    /// <param name="explosionShades">The death shade each explosion frame shows, one per explosion frame.</param>
    /// <param name="whiteout">The colour of each whiteout shade, one per shade.</param>
    /// <exception cref="InvalidDataException">A list has the wrong length.</exception>
    public SamusDeathSteps(ImmutableArray<SamusDeathShade> explosionShades, ImmutableArray<RgbColor> whiteout)
    {
        if (explosionShades.IsDefault || explosionShades.Length != SamusDeathExplosionTimingDefinitions.StepCount)
            throw new InvalidDataException("Samus's death explosion names one death shade per explosion frame.");
        if (whiteout.IsDefault || whiteout.Length != SamusShadeDefinitions.Death.WhiteoutShadeCount)
            throw new InvalidDataException("Samus's death whiteout holds one colour per whiteout shade.");
        this.explosionShades = explosionShades;
        this.whiteout = whiteout;
    }

    /// <summary>The death shade explosion frame <paramref name="frame"/> shows her suit and suitless body in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not an explosion frame.</exception>
    public SamusDeathShade ExplosionStep(int frame) =>
        explosionShades[Ensure.BetweenInclusive(0, explosionShades.Length - 1, frame)];

    /// <summary>The colour the room and every other sprite whiten to under <paramref name="shade"/>.</summary>
    public RgbColor WhiteoutColour(SamusWhiteoutShade shade) => whiteout[shade.Step];
}
