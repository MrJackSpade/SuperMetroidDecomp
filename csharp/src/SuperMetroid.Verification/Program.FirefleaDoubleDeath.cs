using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: EnemyTouch_Fireflea ($A3:8E6B) calls CommonA3_NormalEnemyTouchAI and then
    // EnemyDeath unconditionally. When the touch already killed the Fireflea, the second
    // EnemyDeath runs on the cleared slot: a small explosion spawns at (0,0) with header zero
    // and a second kill is counted. The 100% movie's screw attack through Lower Norfair's
    // Fireflea room shows that explosion in the next lower projectile slot.
    private static void VerifyFirefleaDoubleDeath()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.LowerNorfairFireflea);
        var samus = runtime.Samus!;
        RoomEnemySlot fireflea = runtime.Enemies.Slots.First(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.FirefleaDefinition);
        ushort x = fireflea.XPosition, y = fireflea.YPosition;
        // Screw attack contact ($0A6E = 3) kills through CommonA3_NormalEnemyTouchAI.
        samus.HorizontalSpeed.ContactDamageIndex = 3;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.ScrewAttack;
        ushort killsBefore = runtime.Enemies.EnemiesKilled;

        typeof(RoomEnemySystem).GetMethod("ResolveFirefleaTouch", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [fireflea, samus, (ushort)0]);

        var explosions = runtime.Enemies.EnemyProjectiles
            .Where(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion).ToArray();
        AssertEqual(2, explosions.Length, "the touch death and the unconditional EnemyDeath each spawn an explosion");
        var real = explosions.Single(p => p.XPosition == x && p.YPosition == y);
        var cleared = explosions.Single(p => p != real);
        AssertEqual((ushort)0, cleared.XPosition, "the second explosion reads the cleared X position");
        AssertEqual((ushort)0, cleared.YPosition, "the second explosion reads the cleared Y position");
        AssertEqual((ushort)0, cleared.EnemyHeaderPointer, "the second explosion carries header zero");
        AssertEqual(EnemyDeathInstructionProgramDefinitions.SmallExplosion, cleared.InstructionPointer,
            "EnemyDeath's returned A selects the small explosion");
        AssertEqual((ushort)(killsBefore + 2), runtime.Enemies.EnemiesKilled, "both deaths count a kill");
        Console.WriteLine("Fireflea double death: a killing touch also spawns the cleared-slot small explosion.");
    }
}
