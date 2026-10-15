using static SuperMetroid.Verification.Rooms.Doors.EyeDoorProjectileNativeListAddresses;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Doors.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>#641: the eye door's projectile typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class EyeDoorProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyEyeDoorProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        int steps = NativeEnemyProjectileProgramOracle.CompareAll(CartridgeImportSource.Require(rom), Entries, EyeDoorProjectileAnimations.Programs,
            EyeDoorProjectileFrameDefinitions.PresentationKey, EyeDoorProjectileCueRules.Rules);
        Console.WriteLine($"  eye door projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
