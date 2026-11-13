using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.TestSupport.MovieReplay.PolypRockNativeListAddresses;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>#641: the Polyp's lava rock typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class PolypRockAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPolypRockAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        foreach (EnemyProjectileFrameId frame in PolypRockFrameToolingDefinitions.Frames)
        {
            ushort key = EnemyProjectileProgramFrameSources.Operand(frame);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | key), PolypRockFrameToolingDefinitions.Spritemap(frame),
                $"Polyp lava rock frame {frame} spritemap");
            AssertTrue(PolypRockFrameToolingDefinitions.TryFrame(key, out EnemyProjectileFrameId found) && found == frame, $"Polyp lava rock frame {frame} is found by its key");
        }
        Console.WriteLine($"  Polyp lava rock programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
