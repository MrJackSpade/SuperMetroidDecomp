using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyOverlappingEnemyShots()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_setRandomNumber", flags)!.SetValue(enemies, (Action<ushort>)(_ => { }));
        var interactive = (List<ushort>)typeof(RoomEnemySystem).GetField("_interactiveEnemyIndexes", flags)!.GetValue(enemies)!;
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeBeetom", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState, ushort>>(enemies);
        var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, XPosition = 64, YPosition = 128 };
        for (int index = 0; index < 3; index++)
        {
            var enemy = enemies.Slots[index];
            enemy.EnemyDefinitionPointer = EnemyDefinitionId.Beetom;
            enemy.Definition = RoomEnemyDefinitionCatalog.Get(enemy.EnemyDefinitionPointer);
            enemy.XPosition = enemy.YPosition = 128;
            enemy.XRadius = enemy.Definition.XRadius;
            enemy.YRadius = enemy.Definition.YRadius;
            // Keep the real Beetom callback and vulnerability, but enough health to
            // observe damage independently from the death actor and drop machinery.
            enemy.Health = 180;
            initialize(enemy, samus, 0);
            enemy.SpritemapPointer = 1;
            interactive.Add(enemy.NativeIndex);
        }
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = 0x8100;
        shot.Damage = 100;
        shot.Direction = 2;
        shot.XPosition = shot.YPosition = 128;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
        shot.PreInstruction = SamusProjectilePreInstruction.Missile;

        AssertEqual(3, enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus),
            "one missile reaches all three overlapping Beetoms in the same pass");
        foreach (var enemy in enemies.Slots.Take(3))
            AssertEqual(80, enemy.Health, "each overlapping Beetom takes native missile damage");
        AssertTrue(shot.PackedDirection.HasLowByteLifecycleState, "non-piercing shot is marked for deferred removal");
        AssertEqual((ushort)0x8100, shot.Type, "collision preserves payload until projectile processing");
        var level = CreateRoom(16, 16, new ushort[256], new byte[256]);
        shots.StepFrame(bus, level, samus, 0, 0, 0, 0, bombs, projectileProducerEnabled: false);
        AssertEqual((ushort)0, shot.Type, "next projectile pass consumes the removal marker");
        foreach (var enemy in enemies.Slots.Take(3))
            enemy.InvincibilityTimer = enemy.FlashTimer = enemy.AiHandlerBits = 0;
        AssertEqual(0, enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus),
            "removed projectile cannot damage the stack on the next pass");
        Console.WriteLine("Overlapping Beetoms: all three receive one shot's damage; deferred removal prevents a later hit.");
    }
}
