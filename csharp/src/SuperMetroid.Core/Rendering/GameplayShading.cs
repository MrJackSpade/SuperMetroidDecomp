using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How a published gameplay frame's background pictures are coloured (#1512): the solid colour shown where nothing is
/// drawn, and the shading each group of pictures shows, named by its owners' typed effect state. A backend colours each
/// picture from its group's shading alone; no picture shows another group's colours.
/// </summary>
public sealed record GameplayShading
{
    /// <summary>Each group's shading.</summary>
    private readonly ImmutableDictionary<GameplayPictures, PictureShading> shadings;

    /// <summary>Keeps the backdrop and each group's shading.</summary>
    /// <param name="backdrop">The colour shown where nothing is drawn.</param>
    /// <param name="shadings">The shading of every group of pictures.</param>
    /// <param name="sprites">The colour effects every sprite draw that names its shading shows after its own.</param>
    /// <exception cref="ArgumentException">A group has no shading.</exception>
    public GameplayShading(RgbColor backdrop, ImmutableDictionary<GameplayPictures, PictureShading> shadings,
        ImmutableArray<ColourEffect> sprites)
    {
        Ensure.NotNull(shadings);
        foreach (GameplayPictures pictures in Enum.GetValues<GameplayPictures>())
        {
            if (!shadings.ContainsKey(pictures))
                throw new ArgumentException($"A gameplay frame's shading names none for its {pictures} pictures.", nameof(shadings));
        }
        Backdrop = backdrop;
        this.shadings = shadings;
        Sprites = sprites.IsDefault ? [] : sprites;
    }

    /// <summary>The colour shown where nothing is drawn.</summary>
    public RgbColor Backdrop { get; }

    /// <summary>
    /// The colour effects every sprite draw that names its shading shows after its own: a door's fade, which darkens the
    /// room's actors with its pictures.
    /// </summary>
    public ImmutableArray<ColourEffect> Sprites { get; }

    /// <summary>The shading <paramref name="pictures"/> show.</summary>
    public PictureShading Of(GameplayPictures pictures) => shadings.TryGetValue(pictures, out PictureShading? shading)
        ? shading
        : throw new ArgumentOutOfRangeException(nameof(pictures), pictures, "Undefined group of gameplay pictures.");

    /// <summary>This shading with the backdrop shown in <paramref name="backdrop"/>, as X-ray shows its own.</summary>
    public GameplayShading WithBackdrop(RgbColor backdrop) => new(backdrop, shadings, Sprites);
}
