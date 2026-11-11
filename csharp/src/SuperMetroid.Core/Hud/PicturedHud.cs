using System.Collections.Immutable;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The HUD as the backend draws it (#1466): each placed image's installed picture, in drawing order, a later picture
/// covering an earlier one where they meet. The backend draws it as its own display layer at the HUD's fixed place, the
/// top of the screen; no background plane holds it.
/// </summary>
public sealed class PicturedHud
{
    /// <summary>Keeps the placed pictures in drawing order.</summary>
    /// <param name="pictures">The placed pictures, earliest first.</param>
    /// <exception cref="ArgumentException">A picture runs past the HUD's edge.</exception>
    public PicturedHud(ImmutableArray<PlacedHudPicture> pictures)
    {
        if (pictures.IsDefault)
            throw new ArgumentException("A pictured HUD needs its placements.", nameof(pictures));
        foreach (PlacedHudPicture placed in pictures)
        {
            Extent16<ImagePixels> size = Ensure.NotNull(placed.Picture).Size;
            if (placed.Corner.X + size.X > GameplayHudDefinitions.WidthPixels || placed.Corner.Y + size.Y > GameplayHudDefinitions.HeightPixels)
                throw new ArgumentException($"The HUD picture placed at {placed.Corner} runs past the HUD's edge.", nameof(pictures));
        }
        Pictures = pictures;
    }

    /// <summary>The placed pictures, earliest first.</summary>
    public ImmutableArray<PlacedHudPicture> Pictures { get; }
}
