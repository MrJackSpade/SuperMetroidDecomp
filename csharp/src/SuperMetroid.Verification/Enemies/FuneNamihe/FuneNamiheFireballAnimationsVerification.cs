using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.TestSupport.MovieReplay.FuneNamiheFireballNativeListAddresses;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>#641: Fune and Namihe's fireball typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class FuneNamiheFireballAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyFuneNamiheFireballAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        foreach (EnemyProjectileFrameId frame in FuneNamiheFireballFrameToolingDefinitions.Frames)
        {
            ushort key = EnemyProjectileProgramFrameSources.Operand(frame);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | key), FuneNamiheFireballFrameToolingDefinitions.Spritemap(frame),
                $"Fune/Namihe fireball frame {frame} spritemap");
            AssertTrue(FuneNamiheFireballFrameToolingDefinitions.TryFrame(key, out EnemyProjectileFrameId found) && found == frame, $"Fune/Namihe fireball frame {frame} is found by its key");
        }
        Console.WriteLine($"  Fune/Namihe fireball programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
