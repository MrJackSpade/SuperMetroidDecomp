using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds every frame whose artwork a run of an encounter's pages completes to its own artwork (#1513): on every installed
/// enemy and spawned-object sheet of a graphics set that plays the sequence, such a frame is pictured as those pages leave
/// it, or not at all. A frame pictured as the room's load leaves it, under its plain name, reads characters its completing
/// pages never write: what lies there is another family's artwork, as Mother Brain's initial body frame read the head
/// artwork her room loads where her legs never land, and her leg sheet showed a white head.
/// </summary>
internal static class CompletedFramesOwnArtworkVerification
{
    /// <summary>Fails, naming every frame a completing run of pages never writes that a sheet pictures over the room's load.</summary>
    internal static void VerifyCompletedFramesOwnArtwork()
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        var findings = new List<string>();
        int checkedFrames = 0;
        foreach (EncounterArtworkDrawing drawing in EncounterArtworkDrawingDefinitions.All)
        {
            if (drawing.CompletedBy is not EncounterArtworkSource completing || completing == EncounterArtworkSource.RoomLoad)
                continue;
            string[] frames = [.. drawing.Frames.SelectMany(frame => new[] { EnemySheetDefinitions.FrameName(frame), EnemySheetDefinitions.EncounterFrameName(frame),
                    EnemySheetDefinitions.SpriteObjectFrameName(frame) })
                .Concat(drawing.Projectiles.Select(EnemySheetDefinitions.ProjectileFrameName))];
            foreach ((EnemySheetKey key, SpriteSheet sheet) in sheets.sheets)
            {
                if (Plays(key.Set, drawing.Sequence))
                    checkedFrames += Check($"{key.Set} {key.Enemy}", sheet.frames, frames, completing, findings);
            }
            // A set's spawned-object pictures are on its own sheet and the common sheets (#1513).
            foreach ((SpawnedObjectSheetKey key, SpawnedObjectSheet sheet) in sheets.spawned)
            {
                if (Plays(key.Set, drawing.Sequence))
                    checkedFrames += Check($"{key.Set} spawned {key.Owner}", sheet.frames, frames, completing, findings);
            }
        }
        AssertTrue(checkedFrames > 0, "some sheet holds a frame whose artwork an encounter's pages complete");
        AssertTrue(findings.Count == 0,
            $"{findings.Count} frames are pictured over their room's load, which their completing pages never write:{Environment.NewLine}"
            + string.Join(Environment.NewLine, findings.Distinct(StringComparer.Ordinal)));
        Console.WriteLine($"  Completed frames: {checkedFrames} frames whose pages complete them show those pages' artwork or nothing.");
    }

    /// <summary>
    /// Counts the frames of <paramref name="frames"/> that <paramref name="pictures"/>, a sheet's or a set's spawned-object
    /// pictures by name, hold in any picture, and records each they picture under its plain name, as the room's load leaves it.
    /// </summary>
    private static int Check(string label, Dictionary<string, SpriteSheetFrame> pictures, string[] frames, EncounterArtworkSource completing,
        List<string> findings)
    {
        int held = 0;
        foreach (string frame in frames)
        {
            if (pictures.Keys.Any(picture => picture == frame || picture.StartsWith(frame + "|", StringComparison.Ordinal)))
                held++;
            if (pictures.TryGetValue(frame, out SpriteSheetFrame? plain))
                findings.Add($"{label} ({plain.Sheet.Name}) pictures {frame} over its room's load, which {completing}'s pages never write");
        }
        return held;
    }

    /// <summary>Whether the graphics set named <paramref name="set"/> plays <paramref name="sequence"/>.</summary>
    private static bool Plays(string set, EncounterArtworkSequence sequence) =>
        EncounterArtworkUploads.Of(RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate => candidate.Name == set)).Contains(sequence);

    /// <summary>Runs the completed frames' own-artwork rule.</summary>
    [VerificationCommand("--completed-frames-own-artwork")]
    internal static int CompletedFramesOwnArtworkCommand()
    {
        Suite(nameof(VerifyCompletedFramesOwnArtwork), VerifyCompletedFramesOwnArtwork);
        return 0;
    }
}
