using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Guards a typed projectile family's behavior check against any read of the native lists it replaced (#641).</summary>
internal static class NativeEnemyProjectileListGuard
{
    /// <summary>A read guard over <paramref name="rom"/> that rejects every byte of the family's bank-$86 list extent.</summary>
    internal static CartridgeReadGuard Forbidding(ISnesAddressSpace rom, MotherBrainEffectNativeListAddressesExtent extent, string family) =>
        new CartridgeReadGuard(rom).AlsoForbidding(extent, family);

    /// <summary>Also rejects every byte of another family's bank-$86 list extent, for checks that run several families.</summary>
    internal static CartridgeReadGuard AlsoForbidding(this CartridgeReadGuard guard, MotherBrainEffectNativeListAddressesExtent extent, string family) =>
        guard.ForbidRange((int)new SnesAddress(SnesLittleEndian.BankByte(EnemyProjectileCodePointers.BankBase), extent.First),
            extent.End - extent.First, address => $"Production read {family} list byte ${address:X6}.");
}
