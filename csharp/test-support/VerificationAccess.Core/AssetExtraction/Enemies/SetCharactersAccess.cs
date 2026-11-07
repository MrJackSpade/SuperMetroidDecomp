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
            SetCharacters.FrameLookPart[] parts = [.. shown.Select(look => self.Encounters.FirstOrDefault(encounter => EncounterLookUploads.ShownAs(encounter.Encounter) == look.Encounter)
                is { } played ? new SetCharacters.FrameLookPart(played, [EncounterLookUploads.IndexOf(played.Encounter, look.Name)], look.Encounter)
                : throw new InvalidDataException($"The set's members play no encounter that shows {look}."))];
            return self.Page(parts, [.. parts.Select(part => part.Looks[0])]);
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
            return self.Page([new SetCharacters.FrameLookPart(played, [look], EncounterLookUploads.ShownAs(encounter))], [look]);
        }
    }
}
