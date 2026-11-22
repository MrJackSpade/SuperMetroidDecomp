using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads what one look of an encounter copies over the room's load, as the encounter's steps hold it.</summary>
internal static class EncounterLookUploadsAccess
{
    /// <summary>Reads an encounter's copies.</summary>
    extension(EncounterLookUploads)
    {
        /// <summary>What the look at <paramref name="look"/> of <paramref name="encounter"/> copies.</summary>
        internal static IReadOnlyList<EncounterArtworkUpload> Uploads(ComposedEncounter encounter, int look, IInstalledEncounterArtwork installed) =>
            EncounterLookUploads.Uploads(encounter, EncounterLookUploads.Looks(encounter)[look], installed);

        /// <summary>What <paramref name="look"/> of <paramref name="encounter"/> copies, as the encounter's steps hold it.</summary>
        internal static IReadOnlyList<EncounterArtworkUpload> Uploads(ComposedEncounter encounter, ComposedLook look, IInstalledEncounterArtwork installed) =>
            EncounterLookUploads.Steps(encounter, installed).CopiesOf(look);
    }
}
