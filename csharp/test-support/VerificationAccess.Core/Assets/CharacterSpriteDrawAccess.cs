using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification's view of a draw composed from characters: the frame, artwork, palette and placed parts of a
/// <see cref="CharacterSpriteDraw"/>. Reading them from any other kind of draw fails, because that draw has none.
/// </summary>
internal static class CharacterSpriteDrawAccess
{
    /// <summary>The character members of a draw known to be composed from characters.</summary>
    extension(SpriteDraw draw)
    {
        /// <summary>The whole frame shown.</summary>
        internal SpriteFrame Frame => Character(draw).Frame;

        /// <summary>The artwork the frame's characters index.</summary>
        internal SpriteArtwork Artwork => Character(draw).Artwork;

        /// <summary>The OBJ palette row for parts that do not choose their own.</summary>
        internal int Palette => Character(draw).Palette;

        /// <summary>The character draw's priority for parts that do not choose their own.</summary>
        internal int Priority => Character(draw).Priority;

        /// <summary>Mirrors the whole frame about its origin.</summary>
        internal SnesTileFlipFlags Flip => Character(draw).Flip;

        /// <summary>Places one part on the screen.</summary>
        internal SpriteScreenPart Place(int index) => Character(draw).Place(index);
    }

    /// <summary>The draw as a character draw; any other kind is an error in the check that reads it.</summary>
    private static CharacterSpriteDraw Character(SpriteDraw draw) => draw as CharacterSpriteDraw ??
        throw new InvalidOperationException($"{draw} is a {draw.GetType().Name}, not composed from characters.");
}
