using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>Checks enemy projectile frames take their installed sheet keys from their document names (#2370).</summary>
internal static class EnemyProjectileFrameSheetKeysVerification
{
    /// <summary>
    /// Every projectile frame has a document name, each name gives a distinct sheet key, and the spawned-object sheet frame
    /// a projectile shows is named by that key, never by its identity's name.
    /// </summary>
    internal static void VerifyEnemyProjectileFrameSheetKeys()
    {
        EnemyProjectileFrameId[] frames = Enum.GetValues<EnemyProjectileFrameId>();
        AssertEqual(frames.Length, EnemyProjectileFrameNameDefinitions.Names.Count, "every projectile frame has a document name");
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (EnemyProjectileFrameId frame in frames)
        {
            string key = EnemyProjectileFrameSheetKeys.Of(frame);
            AssertTrue(keys.Add(key), $"projectile frame {frame}'s sheet key {key} is its own");
            AssertEqual(EnemyFrameSheetKeys.PascalCase(EnemyProjectileFrameNameDefinitions.Names[frame]), key,
                $"projectile frame {frame}'s sheet key comes from its document name");
            AssertEqual("Projectile." + key, EnemySheetDefinitions.ProjectileFrameName(frame),
                $"projectile frame {frame} is shown from the sheet frame its key names");
        }
        Console.WriteLine($"Projectile sheet keys: {frames.Length} frames each keyed by its own document name.");
    }

    /// <summary>Runs the projectile sheet-key check.</summary>
    [VerificationCommand("--enemy-projectile-frame-sheet-keys")]
    internal static int EnemyProjectileFrameSheetKeysCommand()
    {
        Suite(nameof(VerifyEnemyProjectileFrameSheetKeys), VerifyEnemyProjectileFrameSheetKeys);
        return 0;
    }
}
