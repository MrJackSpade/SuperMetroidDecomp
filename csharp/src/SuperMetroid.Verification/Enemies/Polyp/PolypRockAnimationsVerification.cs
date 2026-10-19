using static SuperMetroid.Verification.Enemies.Polyp.PolypRockNativeListAddresses;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Polyp;

/// <summary>#641: the Polyp's lava rock typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class PolypRockAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, PolypRockAnimations.Programs, (frame => EnemyProjectileFrames.ProgramFrameKey(PolypRockFrameDefinitions.ShownProjectileFrames[frame])), NativeEnemyProjectileCommonCues.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyPolypRockAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        for (int pose = 0; pose < PolypRockFrameToolingDefinitions.PoseCount; pose++)
        {
            PolypRockFrame frame = new(pose);
            ushort key = EnemyProjectileFrames.ProgramFrameKey(PolypRockFrameDefinitions.ShownProjectileFrames[frame]);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | key), PolypRockFrameToolingDefinitions.Spritemap(frame),
                $"Polyp lava rock pose {pose} spritemap");
            AssertTrue(PolypRockFrameToolingDefinitions.TryFrame(key, out PolypRockFrame found) && found == frame, $"Polyp lava rock pose {pose} is found by its key");
        }
        Console.WriteLine($"  Polyp lava rock programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
