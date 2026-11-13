using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>Audits named bank-$86 enemy-projectile callback addresses.</summary>
internal static class EnemyProjectileCodePointersVerification
{
    /// <summary>
    /// Treats the bank-$86 callback and list catalogs as the executable coverage manifest:
    /// every entry must be unique, mapped, and readable from the target cartridge.
    /// </summary>
    internal static void VerifyEnemyProjectileCodePointerCatalog()
    {
        VerifyEnemyProjectileCodePointerCatalogCase[] callbacks =
        [
            .. EnemyProjectileFrameBehaviorAccess.RoutineWords
                .Where(word => word != 0)
                .Select(word => new VerifyEnemyProjectileCodePointerCatalogCase($"pre-instruction {EnemyProjectileFrameBehaviorAccess.FromWord(word)}", word)),
            .. Enum.GetValues<NativeEnemyProjectileInstruction>().Select(code => new VerifyEnemyProjectileCodePointerCatalogCase($"instruction {code}", (ushort)code)),
        ];
        AssertTrue(callbacks.Length != 0, "enemy projectile callback catalog is populated");
        foreach (EnemyProjectileFrameBehavior frameBehavior in Enum.GetValues<EnemyProjectileFrameBehavior>())
            AssertTrue(EnemyProjectileFrameBehaviorAccess.RoutineWords.Any(word => EnemyProjectileFrameBehaviorAccess.FromWord(word) == frameBehavior),
                $"pre-instruction {frameBehavior} stands for a cartridge routine");

        AssertEqual(callbacks.Length, callbacks
            .Select(callback => callback.Pointer)
            .Distinct()
            .Count(), "enemy projectile callback addresses are unique");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        CartridgeImportAddressSpace? bus = File.Exists(romPath)
            ? SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath)
            : null;
        foreach ((string name, ushort pointer) in callbacks)
        {
            AssertTrue(pointer >= 0x8000,
                $"enemy projectile pointer {name} is in mapped bank-$86 ROM");
            if (bus is not null)
                _ = bus.ReadCartridgeByte(0x860000 | pointer);
        }

        Console.WriteLine(
            $"  Enemy projectiles: {callbacks.Length} callbacks are named{(bus is null ? "" : " and ROM-readable")}.");
    }
}
