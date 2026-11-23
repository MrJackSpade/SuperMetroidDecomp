using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// The order Samus's death sequence steps through (#1512): which death shade each explosion frame shows her suit and
/// suitless body in, and the colour the room and every other sprite whiten to under each whiteout shade. The import decodes
/// both from the installed death artwork; the colours of her death shades are pre-coloured on her sheets, so only the
/// room's whiteout colour, which the room still shows through its own colours, remains a colour here.
/// </summary>
public sealed partial class SamusDeathSteps
{
    /// <summary>The death shade each explosion picture shows.</summary>
    private readonly Dictionary<SamusDeathExplosionFrame, SamusDeathShade> explosionShades;

    /// <summary>The whiteout colour of each whiteout shade.</summary>
    private readonly Dictionary<SamusWhiteoutShade, RgbColor> whiteout;

    /// <summary>Keeps the death shade of each explosion picture and the colour of each whiteout shade.</summary>
    /// <param name="explosionShades">The death shade each explosion picture shows (<see cref="SamusDeathSheetDefinitions.Explosion"/>).</param>
    /// <param name="whiteout">The colour of each whiteout shade.</param>
    /// <exception cref="InvalidDataException">An explosion picture or a whiteout shade has none, or one is named that the death has not.</exception>
    public SamusDeathSteps(IReadOnlyDictionary<SamusDeathExplosionFrame, SamusDeathShade> explosionShades, IReadOnlyDictionary<SamusWhiteoutShade, RgbColor> whiteout)
    {
        Ensure.NotNull(explosionShades);
        Ensure.NotNull(whiteout);
        if (explosionShades.Count != SamusDeathSheetDefinitions.Explosion.Distinct().Count() ||
            SamusDeathSheetDefinitions.Explosion.Any(frame => !explosionShades.ContainsKey(frame)))
            throw new InvalidDataException("Samus's death explosion names one death shade per explosion picture.");
        if (whiteout.Count != SamusWhiteoutShade.All.Count || SamusWhiteoutShade.All.Any(shade => !whiteout.ContainsKey(shade)))
            throw new InvalidDataException("Samus's death whiteout holds one colour per whiteout shade.");
        this.explosionShades = new Dictionary<SamusDeathExplosionFrame, SamusDeathShade>(explosionShades);
        this.whiteout = new Dictionary<SamusWhiteoutShade, RgbColor>(whiteout);
    }

    /// <summary>The death shade explosion picture <paramref name="frame"/> shows her suit and suitless body in.</summary>
    public SamusDeathShade ExplosionShade(SamusDeathExplosionFrame frame) => explosionShades[frame];

    /// <summary>The colour the room and every other sprite whiten to under <paramref name="shade"/>.</summary>
    public RgbColor WhiteoutColour(SamusWhiteoutShade shade) => whiteout[shade];
}
