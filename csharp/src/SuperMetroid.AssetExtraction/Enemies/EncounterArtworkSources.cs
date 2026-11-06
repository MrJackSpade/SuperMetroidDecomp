using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Which source each step of an encounter's artwork sequence copies its characters from (#1513).</summary>
internal static class EncounterArtworkSources
{
    /// <summary>
    /// The source step <paramref name="step"/> of <paramref name="sequence"/> copies from: the sheet of Mother Brain's page it
    /// shows, whose sheets each hold the artwork of other frames; any other encounter's own pages.
    /// </summary>
    internal static EncounterArtworkSource Of(EncounterArtworkSequence sequence, int step) =>
        sequence == EncounterArtworkSequence.MotherBrainPages ? Of(MotherBrainSpritePageDefinitions.Sequence[step].Sheet)
            : EncounterArtworkSource.EncounterPages;

    /// <summary>
    /// The step at which <paramref name="source"/>'s first run of pages in <paramref name="sequence"/> has loaded, or
    /// none for the room's load, which is in place before the first step.
    /// </summary>
    /// <exception cref="InvalidDataException">No step of the sequence copies from the source.</exception>
    internal static int? CompletionStep(EncounterArtworkSequence sequence, EncounterArtworkSource source)
    {
        if (source == EncounterArtworkSource.RoomLoad)
            return null;
        int count = EncounterArtworkUploads.StepCount(sequence);
        int first = Enumerable.Range(0, count).FirstOrDefault(step => Of(sequence, step) == source, -1);
        if (first < 0)
            throw new InvalidDataException($"No step of {sequence} copies {source}'s pages.");
        int last = first;
        while (last + 1 < count && Of(sequence, last + 1) == source)
            last++;
        return last;
    }

    /// <summary>The source a page of Mother Brain's <paramref name="sheet"/> copies from.</summary>
    /// <exception cref="InvalidDataException">The sheet is not one of hers.</exception>
    private static EncounterArtworkSource Of(MotherBrainSpritePageSheet sheet) => sheet switch
    {
        MotherBrainSpritePageSheet.Legs => EncounterArtworkSource.MotherBrainLegs,
        MotherBrainSpritePageSheet.BabyMetroid => EncounterArtworkSource.MotherBrainBabyMetroid,
        MotherBrainSpritePageSheet.Attack => EncounterArtworkSource.MotherBrainAttack,
        MotherBrainSpritePageSheet.ExplodedDoor => EncounterArtworkSource.MotherBrainExplodedDoor,
        MotherBrainSpritePageSheet.Corpse => EncounterArtworkSource.MotherBrainCorpse,
        MotherBrainSpritePageSheet.EscapeTimer => EncounterArtworkSource.EscapeTimer,
        MotherBrainSpritePageSheet.EscapeText => EncounterArtworkSource.EscapeText,
        _ => throw new InvalidDataException($"Undefined {nameof(MotherBrainSpritePageSheet)} {sheet}."),
    };
}
