using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Verification.Enemies.Fireflea;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the second enemy death triggered by a killing Fireflea touch.</summary>
internal static class FirefleaDoubleDeathVerification
{
    /// <summary>Confirms $A3:8E6B's unconditional second death uses the cleared slot, spawning a small explosion at (0,0) and counting another kill.</summary>
    internal static void VerifyFirefleaDoubleDeath()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.LowerNorfairFireflea);
        var samus = runtime.Player.IntroducedSamus;
        RoomEnemySlot fireflea = runtime.Enemies.SlotTable.Slots.First(slot =>
            slot.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea);
        ushort x = fireflea.Position.X.Whole, y = fireflea.Position.Y.Whole;
        // Screw attack contact ($0A6E = 3) kills through CommonA3_NormalEnemyTouchAI.
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.ScrewAttack;
        ushort killsBefore = runtime.Enemies.RoomStatus.EnemiesKilled;

        runtime.Enemies.FirefleaBehavior.ResolveFirefleaTouch(fireflea, samus);

        var explosions = runtime.Enemies.Projectiles.EnemyProjectiles
            .Where(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion).ToArray();
        AssertEqual(2, explosions.Length, "the touch death and the unconditional EnemyDeath each spawn an explosion");
        var real = explosions.Single(p => p.Position.X.Whole == x && p.Position.Y.Whole == y);
        var cleared = explosions.Single(p => p != real);
        AssertEqual((ushort)0, cleared.Position.X.Whole, "the second explosion reads the cleared X position");
        AssertEqual((ushort)0, cleared.Position.Y.Whole, "the second explosion reads the cleared Y position");
        AssertEqual(EnemyDefinitionId.None, cleared.EnemyHeaderPointer, "the second explosion carries header zero");
        AssertEqual(EnemyDeathInstructionProgramDefinitions.SmallExplosion, cleared.InstructionPointer,
            "EnemyDeath's returned A selects the small explosion");
        AssertEqual((ushort)(killsBefore + 2), runtime.Enemies.RoomStatus.EnemiesKilled, "both deaths count a kill");
        Console.WriteLine("Fireflea double death: a killing touch also spawns the cleared-slot small explosion.");
    }

    /// <summary>Runs the focused Lower Norfair screw-attack Fireflea death check.</summary>
    [VerificationCommand("--fireflea-double-death")]
    internal static int FirefleaDoubleDeathCommand()
    {
        VerifyFirefleaDoubleDeath();
        return 0;
    }
}
