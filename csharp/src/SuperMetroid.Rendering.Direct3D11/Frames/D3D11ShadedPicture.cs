using SuperMetroid.Core.Assets;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// A picture under one shading (#1512), the key a picture's header and colour slot are packed under: the same picture
/// shown under two shadings in one frame takes a header and a slot for each. The picture compares by identity, the
/// shading by value.
/// </summary>
/// <param name="Picture">The picture.</param>
/// <param name="Shading">The shading it shows, or null for the frame's colours.</param>
internal readonly record struct D3D11ShadedPicture(SpriteSheetFrame Picture, PictureShading? Shading = null)
{
    /// <inheritdoc />
    public bool Equals(D3D11ShadedPicture other) => ReferenceEquals(Picture, other.Picture) && Equals(Shading, other.Shading);

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Picture), Shading);
}
