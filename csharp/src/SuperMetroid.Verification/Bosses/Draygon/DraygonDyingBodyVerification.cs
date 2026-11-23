using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Draygon's body stays in view as he dies (#2654): each frame of his dying arm redraws only his arm's cells over the body
/// his layer already shows, so each is pictured as his whole body with his arm drawn over it. The import pictured all but
/// one with nothing beneath, so his body vanished as he died and only his arm was left.
/// </summary>
internal static class DraygonDyingBodyVerification
{
    /// <summary>The body each facing's dying arm is drawn over, with his arm's frames in the order he shows them.</summary>
    private static readonly (EnemyFrameId Body, EnemyFrameId[] Arm)[] Facings =
    [
        (EnemyFrameId.DraygonBodyFacingLeftJawShut,
        [
            EnemyFrameId.DraygonLimpArmFacingLeftClawRaised, EnemyFrameId.DraygonLimpArmFacingLeftClawDropping,
            EnemyFrameId.DraygonLimpArmFacingLeftClawDropped, EnemyFrameId.DraygonLimpArmFacingLeftClawCurled,
        ]),
        (EnemyFrameId.DraygonBodyFacingRightJawShut,
        [
            EnemyFrameId.DraygonLimpArmFacingRightClawRaised, EnemyFrameId.DraygonLimpArmFacingRightClawDropping,
            EnemyFrameId.DraygonLimpArmFacingRightClawDropped, EnemyFrameId.DraygonLimpArmFacingRightClawCurled,
        ]),
    ];

    /// <summary>
    /// Checks that each dying-arm frame holds every cell of his body, changed only at his arm's cells, and that its installed
    /// picture spans his whole body.
    /// </summary>
    internal static void VerifyDraygonDyingBodyStaysWhole()
    {
        BossSourceArtwork bosses = InstalledFixtures.BossArtwork;
        Dictionary<string, BossBodyFrameSource> sources = BossBodySheetComposer.Frames(bosses, BossBodyOwner.Draygon)
            .ToDictionary(frame => frame.Name, StringComparer.Ordinal);
        Dictionary<EnemyFrameId, EnemyBg2TilemapWrite[]> written = bosses.DraygonBg2Frames.Writes.Frames
            .ToDictionary(frame => frame.Key, frame => frame.Value);
        BossBodySheet sheet = InstalledFixtures.BossBodySheets.Of(BossBodyOwner.Draygon);
        foreach ((EnemyFrameId body, EnemyFrameId[] arm) in Facings)
        {
            SortedDictionary<int, BackgroundTile> whole = sources[EnemyFrameSheetKeys.Of(body)].Cells;
            SpriteSheetFrame bodyPicture = (sheet.Frame(body).Shown ?? throw new InvalidOperationException($"{body} shows a picture.")).Picture;
            var armCells = new HashSet<int>();
            foreach (EnemyFrameId frame in arm)
            {
                foreach (EnemyBg2TilemapWrite write in written[frame])
                    armCells.UnionWith(Enumerable.Range(write.FirstCell, write.Tiles.Length));
                SortedDictionary<int, BackgroundTile> cells = sources[EnemyFrameSheetKeys.Of(frame)].Cells;
                int[] missing = [.. whole.Keys.Where(cell => !cells.ContainsKey(cell))];
                AssertTrue(missing.Length == 0, $"{frame} holds every cell of his body; {missing.Length} of {whole.Count} are missing");
                int[] changed = [.. whole.Keys.Where(cell => !armCells.Contains(cell) && cells.TryGetValue(cell, out BackgroundTile tile) && tile != whole[cell])];
                AssertTrue(changed.Length == 0, $"{frame} changes his body only at his arm's cells; {changed.Length} other cells differ");
                SpriteSheetFrame picture = (sheet.Frame(frame).Shown ?? throw new InvalidOperationException($"{frame} shows a picture.")).Picture;
                AssertEqual(bodyPicture.Size, picture.Size, $"{frame}'s installed picture spans his whole body");
            }
        }
        Console.WriteLine("  Draygon dying body: each dying-arm frame shows his whole body with his arm drawn over it.");
    }

    /// <summary>Runs the Draygon dying body check.</summary>
    [VerificationCommand("--draygon-dying-body")]
    internal static int DraygonDyingBodyCommand()
    {
        Suite(nameof(VerifyDraygonDyingBodyStaysWhole), VerifyDraygonDyingBodyStaysWhole);
        return 0;
    }
}
