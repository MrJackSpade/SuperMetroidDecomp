using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The characters a graphics set leaves in a given look of its encounters (#1538), which only verification asks for: the
/// import composes every look's pictures itself.
/// </summary>
internal static class SetCharactersAccess
{
    /// <summary>A set's characters in one look of its encounters.</summary>
    extension(SetCharacters self)
    {
        /// <summary>
        /// The characters as the set's encounters leave them once each shows its look of <paramref name="shown"/>: every
        /// look of each such encounter up to the one shown copied in order over the room's load, which a frame's picture of
        /// those looks paints like.
        /// </summary>
        /// <exception cref="InvalidDataException">A look is of an encounter the set's members do not play, or the encounter has no such look.</exception>
        internal SpriteArtwork Page(IReadOnlyList<EncounterLook> shown)
        {
            Ensure.NotNull(shown);
            return self.Composed([.. shown.Select(look => self.Encounters.FirstOrDefault(encounter => EncounterLookUploads.ShownAs(encounter.Encounter) == look.Encounter)
                is { } played ? (played, ComposedLook.Of(played.Encounter, look))
                : throw new InvalidDataException($"The set's members play no encounter that shows {look}."))]);
        }

        /// <summary>
        /// The characters as the set's members leave them once <paramref name="encounter"/> shows its look at
        /// <paramref name="look"/>: every look of it up to that one copied in order over the room's load, which the finished
        /// pictures of a frame drawn in that look paint like, whether or not gameplay shows the encounter's looks.
        /// </summary>
        /// <exception cref="InvalidDataException">The set's members do not play the encounter.</exception>
        internal SpriteArtwork Page(ComposedEncounter encounter, int look)
        {
            EncounterLookSteps played = self.Encounters.FirstOrDefault(candidate => candidate.Encounter == encounter)
                ?? throw new InvalidDataException($"The set's members do not play {encounter}.");
            return self.Composed([(played, EncounterLookUploads.Looks(encounter)[look])]);
        }

        /// <summary>
        /// The room load's characters with every look of each encounter in <paramref name="shown"/> up to the one it shows
        /// copied over them in order, as the import composes a frame's picture of those looks.
        /// </summary>
        private SpriteArtwork Composed(IReadOnlyList<(EncounterLookSteps Played, ComposedLook Look)> shown)
        {
            // The loaded page's characters are the room load's table, followed by the page's blank overhang.
            byte[] characters = self.Loaded.Characters[..SpriteCharacterTableLayout.TableByteCount].ToArray();
            foreach ((EncounterLookSteps played, ComposedLook look) in shown)
            {
                foreach (EncounterArtworkUpload upload in played.CopiesThrough(look))
                    upload.ApplyTo(characters);
            }
            return ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), characters);
        }
    }
}
