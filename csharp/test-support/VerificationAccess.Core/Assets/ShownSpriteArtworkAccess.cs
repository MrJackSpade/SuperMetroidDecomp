namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="ShownSpriteArtwork"/> members production does not use.</summary>
internal static class ShownSpriteArtworkAccess
{
    /// <summary>Exposes <see cref="ShownSpriteArtwork"/> state that only verification reads.</summary>
    extension(ShownSpriteArtwork self)
    {
        /// <summary>The looks in the order they were last shown, oldest first.</summary>
        public IReadOnlyList<SpriteArtworkSource> InOrder => self.shown;
    }
}
