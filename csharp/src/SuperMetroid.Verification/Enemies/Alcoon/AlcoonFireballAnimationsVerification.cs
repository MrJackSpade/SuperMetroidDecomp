using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.TestSupport.MovieReplay.AlcoonFireballNativeListAddresses;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>#641: Alcoon's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class AlcoonFireballAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyAlcoonFireballAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        foreach (EnemyProjectileFrameId frame in AlcoonFireballFrameToolingDefinitions.Frames)
        {
            ushort key = EnemyProjectileProgramFrameSources.Operand(frame);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | key), AlcoonFireballFrameToolingDefinitions.Spritemap(frame),
                $"Alcoon fireball frame {frame} spritemap");
            AssertTrue(AlcoonFireballFrameToolingDefinitions.TryFrame(key, out EnemyProjectileFrameId found) && found == frame, $"Alcoon fireball frame {frame} is found by its key");
        }
        Console.WriteLine($"  Alcoon fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
