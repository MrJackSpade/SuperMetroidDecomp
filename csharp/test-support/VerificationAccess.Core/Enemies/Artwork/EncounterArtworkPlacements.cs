using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Verification's replay of the looks a room's encounters show (#1538): every copy of each encounter's looks up to the one
/// it shows, in order, where the import says the cartridge copies it, for checks that compare the characters those looks
/// leave with the cartridge's own transfers.
/// </summary>
internal static class EncounterArtworkPlacements
{
    /// <summary>The placements the looks <paramref name="looks"/> shows leave, with their artwork as <paramref name="resolver"/> holds it.</summary>
    internal static SpriteArtworkPlacements Of(EncounterLooks looks, IInstalledArtworkContent resolver)
    {
        ArgumentNullException.ThrowIfNull(looks);
        ArgumentNullException.ThrowIfNull(resolver);
        var placements = new SpriteArtworkPlacements();
        foreach (ComposedEncounter encounter in Enum.GetValues<ComposedEncounter>())
        {
            if (EncounterLookUploads.ShownAs(encounter) is not EncounterArtwork shown)
                continue;
            int current = EncounterLookUploads.IndexOf(encounter, looks.Current(shown).Name);
            Record(placements, encounter, Enumerable.Range(0, current + 1), resolver);
        }
        return placements;
    }

    /// <summary>
    /// The placements the copies of <paramref name="lookPlaces"/>, places among <paramref name="encounter"/>'s looks, leave,
    /// copied in the order given, with their artwork as <paramref name="resolver"/> holds it: the looks a check shows, apart
    /// from the encounter's other looks.
    /// </summary>
    internal static SpriteArtworkPlacements Of(ComposedEncounter encounter, IEnumerable<int> lookPlaces, IInstalledArtworkContent resolver)
    {
        ArgumentNullException.ThrowIfNull(lookPlaces);
        ArgumentNullException.ThrowIfNull(resolver);
        var placements = new SpriteArtworkPlacements();
        Record(placements, encounter, lookPlaces, resolver);
        return placements;
    }

    /// <summary>The placements <paramref name="copies"/> leave, copied in the order given: a part of a look a check shows.</summary>
    internal static SpriteArtworkPlacements Of(IEnumerable<EncounterArtworkUpload> copies)
    {
        ArgumentNullException.ThrowIfNull(copies);
        var placements = new SpriteArtworkPlacements();
        foreach (EncounterArtworkUpload upload in copies)
            placements.Record(upload.FirstByte, upload.Characters.Length, new StagedObjCharacters(upload.Characters.Span));
        return placements;
    }

    /// <summary>Records every copy of <paramref name="lookPlaces"/>, places among <paramref name="encounter"/>'s looks, in order.</summary>
    private static void Record(SpriteArtworkPlacements placements, ComposedEncounter encounter, IEnumerable<int> lookPlaces, IInstalledArtworkContent resolver)
    {
        foreach (int look in lookPlaces)
        {
            foreach (EncounterArtworkUpload upload in EncounterLookUploads.Uploads(encounter, look, resolver))
                placements.Record(upload.FirstByte, upload.Characters.Length, new StagedObjCharacters(upload.Characters.Span));
        }
    }
}
