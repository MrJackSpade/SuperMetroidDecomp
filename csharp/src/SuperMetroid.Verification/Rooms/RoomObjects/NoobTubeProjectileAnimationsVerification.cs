using static SuperMetroid.TestSupport.MovieReplay.NoobTubeProjectileNativeListAddresses;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#641: the n00b tube's crack, shards and bubbles typed programs, each matched step by step to its native bank-$86 list.</summary>
internal static class NoobTubeProjectileAnimationsVerification
{
    /// <summary>Every program matches its native list.</summary>
    internal static void VerifyNoobTubeProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        int steps = Family.CompareAll(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  n00b tube projectile programs: {Entries.Length} programs ({steps} steps) match bank $86.");
    }
}
