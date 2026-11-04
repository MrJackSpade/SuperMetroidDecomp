using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Verification's replay of the steps a room's encounters have reached (#1513): every step up to each sequence's reached
/// one, copied in order where the import says the cartridge copies it, for checks that compare the characters those steps
/// leave with the cartridge's own transfers. The corpse rot is its simulation's and is not replayed.
/// </summary>
internal static class EncounterArtworkPlacements
{
    /// <summary>The placements the steps <paramref name="progress"/> records leave, with their artwork as <paramref name="resolver"/> holds it.</summary>
    internal static SpriteArtworkPlacements Of(EncounterArtworkProgress progress, IInstalledArtworkContent resolver)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(resolver);
        var placements = new SpriteArtworkPlacements();
        foreach (EncounterArtworkSequence sequence in Enum.GetValues<EncounterArtworkSequence>())
        {
            if (sequence == EncounterArtworkSequence.MotherBrainCorpseRot || progress.StepOf(sequence) is not int reached)
                continue;
            for (int step = 0; step <= reached; step++)
            {
                foreach (EncounterArtworkUpload upload in EncounterArtworkUploads.Step(sequence, step, resolver))
                    placements.Record(upload.FirstByte, upload.Characters.Length, new StagedObjCharacters(upload.Characters.Span));
            }
        }
        return placements;
    }

    /// <summary>
    /// The placements <paramref name="steps"/> of <paramref name="sequence"/> leave, copied in the order given, with their
    /// artwork as <paramref name="resolver"/> holds it: the pages a check shows, apart from the sequence's other steps.
    /// </summary>
    internal static SpriteArtworkPlacements Of(EncounterArtworkSequence sequence, IEnumerable<int> steps, IInstalledArtworkContent resolver)
    {
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(resolver);
        var placements = new SpriteArtworkPlacements();
        foreach (int step in steps)
        {
            foreach (EncounterArtworkUpload upload in EncounterArtworkUploads.Step(sequence, step, resolver))
                placements.Record(upload.FirstByte, upload.Characters.Length, new StagedObjCharacters(upload.Characters.Span));
        }
        return placements;
    }

    /// <summary>The step of Mother Brain's artwork sequence each of <paramref name="pages"/> takes when shown in order from none (#1513).</summary>
    internal static IReadOnlyList<int> MotherBrainSteps(IEnumerable<MotherBrainSpritePage> pages)
    {
        var progress = new EncounterArtworkProgress();
        var steps = new List<int>();
        foreach (MotherBrainSpritePage page in pages)
        {
            progress.Show(EncounterArtworkSequence.MotherBrainPages, MotherBrainSpritePageDefinitions.Sequence, page);
            steps.Add(progress.StepOf(EncounterArtworkSequence.MotherBrainPages) ?? throw new InvalidOperationException($"{page} reaches no step."));
        }
        return steps;
    }
}
