using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>An atmosphere's artwork (#1706): one picture, repeating across and down at its size.</summary>
public sealed record AtmosphereArtwork : RoomEffectArtwork
{
    /// <summary>An atmosphere repeating <paramref name="picture"/>.</summary>
    /// <param name="picture">The repeating picture.</param>
    public AtmosphereArtwork(ShownLayeredPicture picture)
    {
        Picture = Ensure.NotNull(picture);
        Size = picture.Size;
    }

    /// <summary>The repeating picture.</summary>
    public ShownLayeredPicture Picture { get; }

    /// <summary>The picture's width and height, at which it repeats across and down.</summary>
    public Extent16<ImagePixels> Size { get; }
}
