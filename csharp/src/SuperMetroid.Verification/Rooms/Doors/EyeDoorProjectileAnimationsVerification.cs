using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Rooms.Doors.EyeDoorProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor.Artwork;
using SuperMetroid.Core.Enemies.EyeDoor;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#641: the eye door's projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class EyeDoorProjectileAnimationsVerification
{
    /// <summary>The family's typed programs beside their native lists.</summary>
    internal static NativeEnemyProjectileFamily Family { get; } = NativeEnemyProjectileFamily.Of(Entries, EyeDoorProjectileAnimations.Programs, (frame => EnemyProjectileProgramFrameSources.Operand(EyeDoorProjectileFrameDefinitions.ShownProjectileFrames[frame])), EyeDoorProjectileCueRules.Rules);

    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEyeDoorProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  eye door projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
