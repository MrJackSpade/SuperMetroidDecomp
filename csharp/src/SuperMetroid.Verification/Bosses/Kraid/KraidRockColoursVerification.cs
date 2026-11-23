using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// The rocks Kraid's rise throws up show in his rock colours (#2640), the colours his initialization loads into his rock row,
/// not white: every colour their pictures show on his room's shared sheet is one his rock colours substitute.
/// </summary>
internal static class KraidRockColoursVerification
{
    /// <summary>The pictures of the rocks his rise throws up.</summary>
    private static readonly EnemyProjectileFrameId[] Rocks = [EnemyProjectileFrameId.KraidRockLarge, EnemyProjectileFrameId.KraidRockSmall];

    /// <summary>Checks every colour of his rocks' pictures for a substitution of his rock colours.</summary>
    internal static void VerifyKraidRocksShowHisRockColours()
    {
        SpawnedObjectSheet shared = InstalledFixtures.EnemySheets.Spawned(RoomEnemyGraphicsSetDefinitions.Kraid, SpawnedObjectOwner.Shared);
        foreach (EnemyProjectileFrameId rock in Rocks)
        {
            SpriteSheetFrame frame = shared.Frame(EnemySheetDefinitions.ProjectileFrameName(rock));
            PictureRecolouring rocks = frame.Sheet.Shade(KraidShades.Rocks);
            HashSet<ColourKey> substituted = [.. rocks.Substitutions.Select(substitution => substitution.Replaced)];
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
                $"his rock colours substitute every colour of {rock}'s picture; {unshaded.Length} of {shown.Count} are not ({string.Join(", ", unshaded)})");
        }
    }

    /// <summary>Runs the Kraid rock colours check.</summary>
    [VerificationCommand("--kraid-rock-colours")]
    internal static int KraidRockColoursCommand()
    {
        Suite(nameof(VerifyKraidRocksShowHisRockColours), VerifyKraidRocksShowHisRockColours);
        return 0;
    }
}
