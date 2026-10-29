using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Verification access to <see cref="ShownSpriteArtwork"/> (#1463): the placements gameplay's shown looks leave among the
/// cartridge's OBJ characters, as the import pipeline places each, for checks that compare what the characters hold.
/// </summary>
internal static class ShownSpriteArtworkAccess
{
    /// <summary>Exposes the placements the looks leave and records a look by its placement.</summary>
    extension(ShownSpriteArtwork self)
    {
        /// <summary>The placements the shown looks leave, each where the cartridge shows it, replayed now.</summary>
        public SpriteArtworkPlacements Placed => SpriteArtworkPlacements.Of(self);

        /// <summary>The placements the shown looks leave, in order.</summary>
        public IReadOnlyList<SpriteArtworkPlacement> Uploads => SpriteArtworkPlacements.Of(self).placements;

        /// <summary>The placement shown last over any of <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>, if any has been.</summary>
        public SpriteArtworkPlacement? LastOver(int firstCharacter, int characterCount) =>
            SpriteArtworkPlacements.Of(self).LastOver(firstCharacter, characterCount);

        /// <summary>Shows <paramref name="placement"/>'s look, as production's setup and encounters do.</summary>
        /// <exception cref="InvalidDataException">The cartridge shows the look somewhere else.</exception>
        public void Record(SpriteArtworkPlacement placement)
        {
            Ensure.NotNull(placement);
            SpriteArtworkPlacement canonical = EncounterCharacterPlacements.Placed(placement.Source);
            if (canonical != placement)
                throw new InvalidDataException($"{placement.Source} shows from character {canonical.FirstCharacter} over {canonical.CharacterCount}, not as recorded.");
            self.Show(placement.Source);
        }
    }
}
