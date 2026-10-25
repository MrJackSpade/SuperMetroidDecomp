using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One Tourian statue's installed picture characters.</summary>
/// <param name="Statue">The statue.</param>
/// <param name="Picture">The picture shown.</param>
internal sealed record TourianStatuePictureCharacters(AnimatedTileObject Statue, TourianStatueTilePicture Picture) : ObjCharacterSource
{
    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The statue's pictures are not the size its program copies.</exception>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver)
    {
        TourianStatueAnimatedTileProgramDefinition definition =
            TourianStatueAnimatedTileMechanicsDefinitions.All.Single(statue => statue.AnimatedObject == Statue);
        return InstalledSpriteCharacterArtwork.Of(resolver).AnimatedTiles.TourianStatuePicture(
            definition.AnimatedObject, Picture, definition.FrameByteCount);
    }

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"{Statue} {Picture}";
}
