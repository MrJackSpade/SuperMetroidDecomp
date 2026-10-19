using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Areas.Ceres.CeresFallingDebrisNativeListAddresses;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// #1290: the Ceres falling tiles' typed programs against the retail cartridge. Each program must match its native
/// bank-$86 list update by update, showing its single pose and then sleeping, without production reading the list.
/// </summary>
internal static class CeresFallingDebrisAnimationsVerification
{
    /// <summary>Both tile programs match their native lists.</summary>
    internal static void VerifyCeresFallingDebrisAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int updates = 0;
        foreach ((CeresFallingDebrisProgram program, RoomEnemyProjectileKind kind, ushort entry) in Entries)
        {
            AssertEqual(entry, program == CeresFallingDebrisProgram.Light
                ? CeresFallingDebrisVisualDefinitionsConstants.LightDefinitionList
                : CeresFallingDebrisVisualDefinitionsConstants.DarkDefinitionList, $"the {program} tile definition names its native list");
            var enemies = new RoomEnemySystem();
            var guard = new MutableCartridgeReadGuard(rom);
            guard.Forbid(address => address is >= ListsStart and < ListsEnd,
                address => $"Production read a native Ceres falling tile list at ${address:X6}.");
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyProjectileSlot tile = enemies.Projectiles.EnemyProjectiles[^1];
            tile.Kind = kind;
            tile.StartAnimation(new CeresFallingDebrisAnimation(program));
            (bool deleted, int compared) = NativeEnemyProjectileParity.Compare(cartridge, enemies, tile, entry, 16, $"Ceres falling tile {program}");
            AssertTrue(!deleted, $"the {program} tile sleeps rather than deleting");
            updates += compared;
        }
        AssertTrue(CeresFallingDebrisVisualDefinitions.Frames.SequenceEqual(Enum.GetValues<CeresFallingDebrisProgram>()),
            "each tile program installs its one frame");
        Console.WriteLine($"  Ceres falling tile programs: {Entries.Length} programs ({updates} updates) match bank $86.");
    }
}
