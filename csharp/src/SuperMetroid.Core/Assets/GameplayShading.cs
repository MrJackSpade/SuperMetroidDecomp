using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a published gameplay frame's background pictures are coloured (#1512): the solid colour shown where nothing is
/// drawn, the shading each group of pictures shows, named by its owners' typed effect state, and the fade of the whole
/// display, if one runs. A backend colours each picture from <see cref="Of"/> alone; no picture shows another group's
/// colours.
/// </summary>
public sealed record GameplayShading
{
    /// <summary>Each group's own shading, before the display's fade.</summary>
    private readonly ImmutableDictionary<GameplayPictures, PictureShading> shadings;

    /// <summary>Keeps the backdrop, each group's shading and the display's fade.</summary>
    /// <param name="backdrop">The colour shown where nothing is drawn, the fade included.</param>
    /// <param name="shadings">The shading of every group of pictures, before the display's fade.</param>
    /// <param name="fade">The fade of the whole display, or null while none runs.</param>
    /// <exception cref="ArgumentException">A group has no shading.</exception>
    public GameplayShading(ShownColour backdrop, ImmutableDictionary<GameplayPictures, PictureShading> shadings, GameplayDisplayFade? fade)
    {
        Ensure.NotNull(shadings);
        foreach (GameplayPictures pictures in Enum.GetValues<GameplayPictures>())
        {
            if (!shadings.ContainsKey(pictures))
                throw new ArgumentException($"A gameplay frame's shading names none for its {pictures} pictures.", nameof(shadings));
        }
        Backdrop = Ensure.NotNull(backdrop);
        this.shadings = shadings;
        Fade = fade;
    }

    /// <summary>What is shown where nothing is drawn, which the backend shows (#2563).</summary>
    public ShownColour Backdrop { get; }

    /// <summary>The fade of the whole display, which every picture and every shaded sprite draw shows after its own shading.</summary>
    internal GameplayDisplayFade? Fade { get; }

    /// <summary>The shading <paramref name="pictures"/> show: their own, then the display's fade unless it spares them.</summary>
    public PictureShading Of(GameplayPictures pictures)
    {
        PictureShading own = shadings.TryGetValue(pictures, out PictureShading? shading)
            ? shading
            : throw new ArgumentOutOfRangeException(nameof(pictures), pictures, "Undefined group of gameplay pictures.");
        return Fade is not { } fade || fade.SparesHud && GameplayPictureDefinitions.SparedByHudSparingFade.Contains(pictures)
            ? own : own.With(fade.Effect);
    }

    /// <summary>
    /// <paramref name="shading"/>, a sprite draw's own, with the display's fade after it unless the fade spares the
    /// draw's owner, and on Samus's draws the shades the fade keeps lit after the fade.
    /// </summary>
    /// <param name="shading">The draw's own shading.</param>
    /// <param name="isSamus">Whether the draw is one of Samus's.</param>
    public PictureShading OfSprite(PictureShading shading, bool isSamus)
    {
        Ensure.NotNull(shading);
        if (Fade is not { } fade || fade.SparesSamus && isSamus)
            return shading;
        PictureShading faded = shading.With(fade.Effect);
        if (!isSamus)
            return faded;
        foreach (PictureShade lit in fade.LitOnSamus)
            faded = faded.OverEffect(lit);
        return faded;
    }

    /// <summary>
    /// The shading a draw of <paramref name="pictures"/>, whose draws show their group's shading over their own
    /// (<see cref="SpriteSourceKindDefinitions.GroupShaded"/>), shows when it names <paramref name="shading"/>: its own,
    /// then the group's shades and effects, the display's fade among them unless it spares the group, as it spares the HUD's
    /// escape timer (#1512).
    /// </summary>
    public PictureShading OfGroupShaded(PictureShading shading, GameplayPictures pictures)
    {
        Ensure.NotNull(shading);
        PictureShading group = Of(pictures);
        PictureShading shown = shading;
        foreach (PictureShade shade in group.Shades)
            shown = shown.With(shade);
        foreach (ColourEffect effect in group.Effects)
            shown = shown.With(effect);
        return shown;
    }

    /// <summary>This shading with the backdrop shown in <paramref name="backdrop"/>, as X-ray shows its own.</summary>
    public GameplayShading WithBackdrop(ShownColour backdrop) => new(backdrop, shadings, Fade);
}
