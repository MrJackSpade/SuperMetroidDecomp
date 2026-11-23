using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// Ridley's and Phantoon's statues in the Tourian entrance show their eye gems (#2625): Ridley's statue's eyes are its
/// row's colours nine to eleven and Phantoon's the statue body row's twelve to fourteen, which their blacked-out looks
/// clear. Each statue's eye animation copies its eye pictures over the statue's sprite characters, so its picture as the
/// animation shows its bright eyes inks them.
/// </summary>
internal static class TourianStatueEyeGemsVerification
{
    /// <summary>Checks that each sprite statue's picture shows its eyes in the colours its blacked-out look clears.</summary>
    internal static void VerifyTourianStatueEyeGems()
    {
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate =>
            RoomEnemyGraphicsSetLists.MembersOf(candidate).Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.TourianStatue));
        SpriteSheet composed = EnemySheetFixtures.ComposedSpawnedSheet(set, SpawnedObjectOwner.Shared);
        Check(TourianSpriteStatue.Ridley, EnemyProjectileFrameId.TourianStatueRidley, EnemySheetRowDefinitions.TourianStatueRidley, 9);
        Check(TourianSpriteStatue.Phantoon, EnemyProjectileFrameId.TourianStatuePhantoon, EnemySheetRowDefinitions.TourianStatueBody, 12);

        void Check(TourianSpriteStatue sprite, EnemyProjectileFrameId statue, int row, int firstEyeColour)
        {
            // The statue as its eye animation shows its bright eyes (#2625).
            SpriteSheetFrame frame = composed.Frame(EncounterLook.PictureName(EnemySheetDefinitions.ProjectileFrameName(statue),
                [TourianSpriteStatueEyeLooks.Of(sprite, TourianStatueBlockPicture.EyesBright)]));
            SortedSet<int> indices = [];
            for (int y = frame.Corner.Y; y < frame.Corner.Y + frame.Size.Y; y++)
            for (int x = frame.Corner.X; x < frame.Corner.X + frame.Size.X; x++)
            {
                int pixel = y * frame.Sheet.Size.X + x;
                if (frame.Sheet.Pixels[pixel].A != 0)
                    indices.UnionWith(InstalledPictureColoursConstants.ComposedIndicesOf(frame.Sheet, frame.Sheet.KeyAt(pixel)));
            }
            int[] eyes = [.. Enumerable.Range(row * LoadedSetColours.RowColours + firstEyeColour, 3)];
            AssertTrue(eyes.Any(indices.Contains),
                $"{statue}'s picture shows its eyes in colours {string.Join(", ", eyes.Select(index => $"{index:X2}"))}; it inks {string.Join(" ", indices.Select(index => $"{index:X2}"))}");
        }
    }

    /// <summary>Runs the Tourian statue eye gem check.</summary>
    [VerificationCommand("--tourian-statue-eye-gems")]
    internal static int TourianStatueEyeGemsCommand()
    {
        Suite(nameof(VerifyTourianStatueEyeGems), VerifyTourianStatueEyeGems);
        return 0;
    }
}
