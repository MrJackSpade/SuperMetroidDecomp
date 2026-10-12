using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class FirefleaDoubleDeathVerification
{
    // #1269: EnemyTouch_Fireflea ($A3:8E6B) calls CommonA3_NormalEnemyTouchAI and then
    // EnemyDeath unconditionally. When the touch already killed the Fireflea, the second
    // EnemyDeath runs on the cleared slot: a small explosion spawns at (0,0) with header zero
    // and a second kill is counted. The 100% movie's screw attack through Lower Norfair's
    // Fireflea room shows that explosion in the next lower projectile slot.
    internal static void VerifyFirefleaDoubleDeath()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.LowerNorfairFireflea);
        var samus = runtime.Player.Samus!;
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

    [VerificationCommand("--fireflea-double-death")]
    internal static int FirefleaDoubleDeathCommand()
    {
        VerifyFirefleaDoubleDeath();
        return 0;
    }
}
