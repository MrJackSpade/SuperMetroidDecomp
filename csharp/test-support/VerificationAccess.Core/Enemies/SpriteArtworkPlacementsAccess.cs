using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="SpriteArtworkPlacements"/> members production does not use.</summary>
internal static class SpriteArtworkPlacementsAccess
{
    /// <summary>Exposes <see cref="SpriteArtworkPlacements"/> state that only verification reads.</summary>
    extension(SpriteArtworkPlacements self)
    {
        /// <summary>Shows <paramref name="placement"/>'s artwork over its characters, over whatever showed there before.</summary>
        public void Show(SpriteArtworkPlacement placement)
        {
            Ensure.NotNull(placement);
            // A placement wholly covered by a later one no longer shows anywhere; dropping it keeps the record bounded.
            for (int index = self.placements.Count - 1; index >= 0; index--)
            {
                if (placement.Covers(self.placements[index].FirstCharacter, self.placements[index].CharacterCount))
                    self.placements.RemoveAt(index);
            }
            self.placements.Add(placement);
        }
        /// <summary>The placement shown last over any of <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>, if any has been.</summary>
        public SpriteArtworkPlacement? LastOver(int firstCharacter, int characterCount)
        {
            for (int index = self.placements.Count - 1; index >= 0; index--)
            {
                if (self.placements[index].Overlaps(firstCharacter, characterCount))
                    return self.placements[index];
            }
            return null;
        }
    }
}
