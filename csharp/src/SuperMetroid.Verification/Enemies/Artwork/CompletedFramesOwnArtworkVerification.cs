using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds every frame whose artwork is its one look's own to one finished picture (#1513, #1538): on every installed enemy
/// and spawned-object sheet of a graphics set that plays the encounter, such a frame is pictured once, under its own name,
/// as its look leaves it, or shows nothing where its look's copies never write the characters it reads, as Mother Brain's
/// initial body frame reads the head artwork her room loads where her legs never land. No such frame has a picture of
/// another look.
/// </summary>
internal static class CompletedFramesOwnArtworkVerification
{
    /// <summary>Fails, naming every frame whose artwork is its look's own that a sheet pictures in more than one look.</summary>
    internal static void VerifyCompletedFramesOwnArtwork()
    {
        InstalledEnemySheets sheets = InstalledFixtures.EnemySheets;
        var findings = new List<string>();
        int checkedFrames = 0;
        foreach (EncounterArtworkDrawing drawing in EncounterArtworkDrawingDefinitions.All.Where(drawing => drawing.ArtworkOfLook))
        {
            AssertEqual(1, drawing.Looks.Count, $"a frame whose artwork is its look's own is drawn in one look of {drawing.Encounter}");
            string[] frames = [.. drawing.Frames.SelectMany(frame => new[] { EnemySheetDefinitions.FrameName(frame), EnemySheetDefinitions.EncounterFrameName(frame),
                    EnemySheetDefinitions.SpriteObjectFrameName(frame) })
                .Concat(drawing.Projectiles.Select(EnemySheetDefinitions.ProjectileFrameName))];
            foreach ((RoomEnemyGraphicsSetDefinition set, PopulationArtwork artwork) in sheets.BySet.Where(entry => Plays(entry.Key.Name, drawing.Encounter)))
            {
                foreach ((EnemyDefinitionId enemy, EnemyArtworkVariant<SpriteSheet> variant) in artwork.enemies)
                {
                    checkedFrames += Check($"{set.Name} {enemy} ({variant.Name})", [.. variant.Sheet.FrameNames, .. variant.Sheet.BlankFrameNames], frames,
                        drawing.Looks[0], findings);
                }
                // A variant's spawned-object pictures are on its own sheet and the common sheets (#1513).
                foreach ((SpawnedObjectOwner owner, EnemyArtworkVariant<SpawnedObjectSheet> variant) in artwork.spawned)
                {
                    checkedFrames += Check($"{set.Name} spawned {owner} ({variant.Name})", [.. variant.Sheet.frames.Keys, .. variant.Sheet.empty], frames,
                        drawing.Looks[0], findings);
                }
            }
        }
        AssertTrue(checkedFrames > 0, "some sheet holds a frame whose artwork is its look's own");
        AssertTrue(findings.Count == 0,
            $"{findings.Count} frames whose artwork is their look's own are pictured in more than one look:{Environment.NewLine}"
            + string.Join(Environment.NewLine, findings.Distinct(StringComparer.Ordinal)));
        Console.WriteLine($"  Completed frames: {checkedFrames} frames whose artwork is their look's own show one finished picture or nothing.");
    }

    /// <summary>
    /// Counts the frames of <paramref name="frames"/> that <paramref name="pictures"/>, a sheet's or a set's spawned-object
    /// picture names, hold, and records each they picture in a look of an encounter's, under a name that carries one.
    /// </summary>
    private static int Check(string label, IReadOnlyList<string> pictures, string[] frames, string look, List<string> findings)
    {
        int held = 0;
        foreach (string frame in frames)
        {
            if (pictures.Contains(frame, StringComparer.Ordinal))
                held++;
            foreach (string picture in pictures.Where(picture => picture.StartsWith(frame + "|", StringComparison.Ordinal)))
                findings.Add($"{label} pictures {frame}, whose artwork is its {look} look's own, as {picture}");
        }
        return held;
    }

    /// <summary>Whether the graphics set named <paramref name="set"/> plays <paramref name="encounter"/>.</summary>
    private static bool Plays(string set, ComposedEncounter encounter) =>
        EncounterLookUploads.Of(RoomEnemyGraphicsSetLists.Members[set]).Contains(encounter);

    /// <summary>Runs the completed frames' own-artwork rule.</summary>
    [VerificationCommand("--completed-frames-own-artwork")]
    internal static int CompletedFramesOwnArtworkCommand()
    {
        Suite(nameof(VerifyCompletedFramesOwnArtwork), VerifyCompletedFramesOwnArtwork);
        return 0;
    }
}
