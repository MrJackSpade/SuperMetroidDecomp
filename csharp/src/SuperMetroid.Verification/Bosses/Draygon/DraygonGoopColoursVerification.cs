using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Draygon's goop shows his goop colours (#2655): its initialization draws it in the row his opening's colours run on into,
/// so every colour its flying pictures show on his room's shared sheet is one his opening's colours substitute. The import
/// drew it in no row, so it showed the pale colours of the room's first row.
/// </summary>
internal static class DraygonGoopColoursVerification
{
    /// <summary>The pictures of his goop in flight.</summary>
    private static readonly EnemyProjectileFrameId[] Goop =
    [
        EnemyProjectileFrameId.DraygonGoopSpiky, EnemyProjectileFrameId.DraygonGoopPuffed,
        EnemyProjectileFrameId.DraygonGoopRounded, EnemyProjectileFrameId.DraygonGoopLopsided,
    ];

    /// <summary>Checks every colour of his flying goop's pictures for a substitution of his opening's colours.</summary>
    internal static void VerifyDraygonGoopShowsHisGoopColours()
    {
        SpawnedObjectSheet shared = InstalledFixtures.EnemySheets.Spawned(RoomEnemyGraphicsSetDefinitions.DraygonDefault, SpawnedObjectOwner.Shared);
        foreach (EnemyProjectileFrameId goop in Goop)
        {
            SpriteSheetFrame frame = shared.Frame(EnemySheetDefinitions.ProjectileFrameName(goop));
            PictureRecolouring opening = frame.Sheet.Shade(DraygonShades.Intro);
            HashSet<ColourKey> substituted = [.. opening.Substitutions.Select(substitution => substitution.Replaced)];
            HashSet<ColourKey> shown = [];
            for (int y = frame.Corner.Y; y < frame.Corner.Y + frame.Size.Y; y++)
            for (int x = frame.Corner.X; x < frame.Corner.X + frame.Size.X; x++)
            {
                int pixel = y * frame.Sheet.Size.X + x;
                if (frame.Sheet.Pixels[pixel].A != 0)
                    shown.Add(frame.Sheet.KeyAt(pixel));
            }
            ColourKey[] unshaded = [.. shown.Where(key => !substituted.Contains(key))];
            AssertTrue(shown.Count > 0 && unshaded.Length == 0,
                $"his opening's colours substitute every colour of {goop}'s picture; {unshaded.Length} of {shown.Count} are not ({string.Join(", ", unshaded)})");
        }
        Console.WriteLine("  Draygon goop: his flying goop's pictures are in his goop colours.");
    }

    /// <summary>Runs the Draygon goop colours check.</summary>
    [VerificationCommand("--draygon-goop-colours")]
    internal static int DraygonGoopColoursCommand()
    {
        Suite(nameof(VerifyDraygonGoopShowsHisGoopColours), VerifyDraygonGoopShowsHisGoopColours);
        return 0;
    }
}
