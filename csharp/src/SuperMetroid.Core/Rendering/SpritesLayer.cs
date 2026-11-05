using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A layer of sprites (#1514): the sprite draws a screen shows at this place in its layer order, over the layers behind it,
/// the frontmost draw's pixel showing where its draws overlap. Which sprites belong to which layer is the screen's
/// decision: a screen showing its stars behind a planet and its debris in front of it publishes two sprite layers, one on
/// each side of the planet. Every draw is drawn whole at the layer's place; no part of a draw is placed among the layers
/// on its own.
/// </summary>
public sealed record SpritesLayer : DisplayLayer
{
    /// <summary>Keeps the layer's draws, how they show and their tint.</summary>
    /// <param name="draws">The draws, frontmost first, each naming the shading it shows in.</param>
    /// <param name="blend">How the sprites show over the layers behind them.</param>
    /// <param name="tint">
    /// A colour added to each of the layer's sprite pixels that take part in blends, as the flyaway's white sky tints the
    /// gunship's sprites; null for none.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The draw list is missing, holds a draw that names no shading, or a tint is given for sprites that glow.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="blend"/> is not a defined blend.</exception>
    public SpritesLayer(ImmutableArray<SpriteDraw> draws, SpriteBlend blend = SpriteBlend.Drawn, RgbColor? tint = null)
    {
        if (draws.IsDefault)
            throw new ArgumentException("A sprite layer needs its draws.", nameof(draws));
        foreach (SpriteDraw draw in draws)
        {
            if (draw is not ImageSpriteDraw { Shading: not null })
                throw new ArgumentException("A sprite layer's draw is a picture naming the shading it shows in.", nameof(draws));
        }
        Blend = blend switch
        {
            SpriteBlend.Drawn or SpriteBlend.Glows => blend,
            _ => throw new ArgumentOutOfRangeException(nameof(blend), blend, "Undefined sprite blend."),
        };
        if (tint is not null && blend != SpriteBlend.Drawn)
            throw new ArgumentException("A tint brightens drawn sprites, not sprites that glow.", nameof(tint));
        Draws = draws;
        Tint = tint;
    }

    /// <summary>The draws, frontmost first.</summary>
    public ImmutableArray<SpriteDraw> Draws { get; }

    /// <summary>How the sprites show over the layers behind them.</summary>
    public SpriteBlend Blend { get; }

    /// <summary>The colour added to each of the layer's sprite pixels that take part in blends, or null for none.</summary>
    public RgbColor? Tint { get; }

    /// <summary>Whether <paramref name="other"/> shows the same draws, in the same order, blended and tinted alike.</summary>
    /// <param name="other">The layer compared, or null.</param>
    /// <returns>True when both layers show equal draws the same way.</returns>
    public bool Equals(SpritesLayer? other) => other is not null && Blend == other.Blend && Tint == other.Tint
        && Draws.AsSpan().SequenceEqual(other.Draws.AsSpan());

    /// <summary>A hash of the blend, the tint and the number of draws, consistent with <see cref="Equals(SpritesLayer)"/>.</summary>
    /// <returns>The hash.</returns>
    public override int GetHashCode() => HashCode.Combine(Blend, Tint, Draws.Length);
}
