namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="SpriteArtworkPlacement"/> queries only verification asks (#1466).</summary>
internal static class SpriteArtworkPlacementAccess
{
    /// <summary>A placement's character overlap.</summary>
    extension(SpriteArtworkPlacement self)
    {
        /// <summary>Whether the placement shares a character with <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>.</summary>
        internal bool Overlaps(int firstCharacter, int characterCount) =>
            self.FirstCharacter < firstCharacter + characterCount && firstCharacter < self.FirstCharacter + self.CharacterCount;
    }
}
