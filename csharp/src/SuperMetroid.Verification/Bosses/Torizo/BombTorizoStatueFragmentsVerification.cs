using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// Bomb Torizo's statue fragments show the statue's own pieces as the Chozo hand crumbles (#2623): each is drawn over the
/// debris the hand's room object copies over the sprite characters, so the eight fragments are eight different pieces. They
/// were composed over characters no copy filled, where all eight showed one picture of small crosses.
/// </summary>
internal static class BombTorizoStatueFragmentsVerification
{
    /// <summary>The fragments the crumbling statue drops.</summary>
    private static readonly EnemyProjectileFrameId[] Fragments =
    [
        EnemyProjectileFrameId.BombTorizoStatueMossClump,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitRight,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitCenter,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitSpot,
        EnemyProjectileFrameId.BombTorizoStatueRubbleMossyTop,
        EnemyProjectileFrameId.BombTorizoStatueRubbleMossSpeck,
        EnemyProjectileFrameId.BombTorizoStatueRubbleLitBase,
        EnemyProjectileFrameId.BombTorizoStatueSlab,
    ];

    /// <summary>Checks that the fragments' pictures are all different pieces.</summary>
    internal static void VerifyBombTorizoStatueFragmentsShowTheStatue()
    {
        SpawnedObjectSheet shared = InstalledFixtures.EnemySheets.Spawned(RoomEnemyGraphicsSetDefinitions.BombTorizoBeforeTimebomb, SpawnedObjectOwner.Shared);
        string[] pixels = [.. Fragments.Select(fragment =>
        {
            SpriteSheetFrame frame = shared.Frame(EnemySheetDefinitions.ProjectileFrameName(fragment));
            var shown = new System.Text.StringBuilder();
            for (int y = frame.Corner.Y; y < frame.Corner.Y + frame.Size.Y; y++)
            for (int x = frame.Corner.X; x < frame.Corner.X + frame.Size.X; x++)
                shown.Append(frame.Sheet.Pixels[y * frame.Sheet.Size.X + x].ToString()).Append(';');
            return shown.ToString();
        })];
        AssertEqual(Fragments.Length, pixels.Distinct().Count(), "the statue's eight fragments show eight different pieces");
    }

    /// <summary>Runs the Bomb Torizo statue fragment check.</summary>
    [VerificationCommand("--bomb-torizo-statue-fragments")]
    internal static int BombTorizoStatueFragmentsCommand()
    {
        Suite(nameof(VerifyBombTorizoStatueFragmentsShowTheStatue), VerifyBombTorizoStatueFragmentsShowTheStatue);
        return 0;
    }
}
