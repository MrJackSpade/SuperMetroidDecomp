using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Death;

/// <summary>
/// The order Samus's death sequence steps through (#1512): which death step each explosion frame shows her suit and
/// suitless body in, and the colour the room and every other sprite whiten to at each whiteout step. The import decodes
/// both from the installed death artwork; the colours of her death steps are pre-coloured on her sheets as shades, so only
/// the room's whiteout colour, which the room still shows through its own colours, remains a colour here.
/// </summary>
public sealed class SamusDeathSteps
{
    /// <summary>The death step each explosion frame shows, by explosion frame.</summary>
    private readonly ImmutableArray<int> explosionSteps;

    /// <summary>The whiteout colour of each whiteout step.</summary>
    private readonly ImmutableArray<RgbColor> whiteout;

    /// <summary>Keeps the explosion order and the whiteout colours.</summary>
    /// <param name="explosionSteps">The death step each explosion frame shows, one per explosion frame.</param>
    /// <param name="whiteout">The colour of each whiteout step, one per step.</param>
    /// <exception cref="InvalidDataException">A list has the wrong length or names a death step that does not exist.</exception>
    public SamusDeathSteps(ImmutableArray<int> explosionSteps, ImmutableArray<RgbColor> whiteout)
    {
        if (explosionSteps.IsDefault || explosionSteps.Length != SamusDeathExplosionTimingDefinitions.StepCount ||
            explosionSteps.Any(step => step is < 0 or >= SamusPaletteDefinitions.Death.PaletteCount))
            throw new InvalidDataException("Samus's death explosion names one existing death step per explosion frame.");
        if (whiteout.IsDefault || whiteout.Length != SamusPaletteDefinitions.Death.WhiteoutShadeCount)
            throw new InvalidDataException("Samus's death whiteout holds one colour per whiteout step.");
        this.explosionSteps = explosionSteps;
        this.whiteout = whiteout;
    }

    /// <summary>The death step explosion frame <paramref name="frame"/> shows her suit and suitless body in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The frame is not an explosion frame.</exception>
    public int ExplosionStep(int frame) =>
        explosionSteps[Ensure.BetweenInclusive(0, explosionSteps.Length - 1, frame)];

    /// <summary>The colour the room and every other sprite whiten to at whiteout step <paramref name="step"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is not a whiteout step.</exception>
    public RgbColor WhiteoutColour(int step) => whiteout[Ensure.BetweenInclusive(0, whiteout.Length - 1, step)];
}
