namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Verification access to <see cref="BgArtworkBindings"/> members production does not use.</summary>
internal static class BgArtworkBindingsAccess
{
    /// <summary>Exposes <see cref="BgArtworkBindings"/> state that only verification reads.</summary>
    extension(BgArtworkBindings self)
    {
        /// <summary>The live bindings, oldest first, for verification's composition of what they show.</summary>
        internal IReadOnlyList<BgArtworkBinding> Bound => self.bindings;

        /// <summary>
        /// What shows over <paramref name="characterCount"/> of <paramref name="set"/>'s characters from
        /// <paramref name="firstCharacter"/>: the newest binding over them, which must cover them all.
        /// </summary>
        /// <exception cref="InvalidDataException">No binding covers the characters, or the newest over them covers only some.</exception>
        internal BgArtworkShown Showing(BgCharacterSet set, int firstCharacter, int characterCount) =>
            self.TryShowing(set, firstCharacter, characterCount, out BgArtworkShown shown) ? shown
                : throw new InvalidDataException($"No background artwork is bound over {set} characters {firstCharacter}..{firstCharacter + characterCount - 1}.");
    }
}
