using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
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
    /// <summary>The death shade each explosion picture shows.</summary>
    private readonly Dictionary<SamusDeathExplosionFrame, SamusDeathShade> explosionShades;

    /// <summary>The whiteout colour of each whiteout shade.</summary>
    private readonly ImmutableArray<RgbColor> whiteout;

    /// <summary>Keeps the explosion order and the whiteout colours.</summary>
    /// <param name="explosionShades">
    /// The death shade each explosion picture shows, one per picture in the order the explosion shows them
    /// (<see cref="SamusDeathSheetDefinitions.Explosion"/>).
    /// </param>
    /// <param name="whiteout">The colour of each whiteout shade, one per shade.</param>
    /// <exception cref="InvalidDataException">A list has the wrong length.</exception>
    public SamusDeathSteps(ImmutableArray<SamusDeathShade> explosionShades, ImmutableArray<RgbColor> whiteout)
    {
        if (explosionShades.IsDefault || explosionShades.Length != SamusDeathSheetDefinitions.Explosion.Count)
            throw new InvalidDataException("Samus's death explosion names one death shade per explosion picture.");
        if (whiteout.IsDefault || whiteout.Length != SamusShadeDefinitions.Death.WhiteoutShadeCount)
            throw new InvalidDataException("Samus's death whiteout holds one colour per whiteout shade.");
        this.explosionShades = [];
        for (int picture = 0; picture < explosionShades.Length; picture++)
            this.explosionShades[SamusDeathSheetDefinitions.Explosion[picture]] = explosionShades[picture];
        this.whiteout = whiteout;
    }

    /// <summary>The death shade explosion picture <paramref name="frame"/> shows her suit and suitless body in.</summary>
    public SamusDeathShade ExplosionShade(SamusDeathExplosionFrame frame) => explosionShades[frame];

    /// <summary>The colour the room and every other sprite whiten to under <paramref name="shade"/>.</summary>
    public RgbColor WhiteoutColour(SamusWhiteoutShade shade) => whiteout[shade.Step];
}
