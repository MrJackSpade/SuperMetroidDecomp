using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>An atmosphere's artwork (#1706): one picture, repeating across and down at its size.</summary>
public sealed record AtmosphereArtwork : RoomEffectArtwork
{
    /// <summary>Checks that the picture installs a part.</summary>
    /// <param name="picture">The repeating picture.</param>
    /// <exception cref="InvalidDataException">The picture installs neither part.</exception>
    public AtmosphereArtwork(LayeredPicture picture)
    {
        Picture = Ensure.NotNull(picture);
        Size = picture.Size ?? throw new InvalidDataException("An atmosphere's picture installs neither of its parts.");
    }

    /// <summary>The repeating picture.</summary>
    public LayeredPicture Picture { get; }

    /// <summary>The picture's width and height, at which it repeats across and down.</summary>
    public Extent16<ImagePixels> Size { get; }
}
