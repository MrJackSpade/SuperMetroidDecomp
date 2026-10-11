using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEnemyProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        ushort Word(int address) => unchecked((ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

        RoomEnemyProjectileKind[] kinds = Enum.GetValues<RoomEnemyProjectileKind>()
            .Where(kind => kind != RoomEnemyProjectileKind.None)
            .Distinct()
            .ToArray();
        // #1269 added Mother Brain's small purple breath and the Climb's fake-wall explosion.
        AssertEqual(123, kinds.Length, "compiled enemy-projectile definition count");

        var forbidden = new HashSet<int>();
        foreach (RoomEnemyProjectileKind kind in kinds)
        {
            int address = 0x860000 | (ushort)kind;
            for (int index = 0; index < 14; index++)
                forbidden.Add(address + index);

            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            AssertEqual(Word(address + 2), definition.PreInstruction,
                $"{kind} pre-instruction");
            AssertEqual(Word(address + 4), definition.InitialInstructionList,
                $"{kind} initial list");
            AssertEqual(Word(address + 6), definition.RadiusWord,
                $"{kind} radius word");
            AssertEqual(Word(address + 8), definition.Properties,
                $"{kind} properties");
            AssertEqual(Word(address + 10), definition.TouchInstructionList,
                $"{kind} touch list");
            AssertEqual(Word(address + 12), definition.ShotInstructionList,
                $"{kind} shot list");
        }

        var initialize = typeof(RoomEnemySystem)
            .GetMethod("InitializeEnemyProjectileFromDefinition", flags)!
            .CreateDelegate<Action<RoomEnemyProjectileSlot, RoomEnemyProjectileKind, ushort>>();
        var collideWithSamus = typeof(RoomEnemySystem)
            .GetMethod("ResolveEnemyProjectileSamusCollision", flags)!
            .CreateDelegate<Func<RoomEnemyProjectileSlot, SamusState, ushort?>>();

        const ushort populationPointer = 0x9400;
        var setupBus = new TestAddressSpace();
        setupBus.WriteByte(0xa10000 | populationPointer, 0xff);
        setupBus.WriteByte((0xa10000 | populationPointer) + 1, 0xff);
        setupBus.WriteByte((0xa10000 | populationPointer) + 2, 0);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            setupBus,
            populationPointer,
            0xffff,
            new SnesVram(),
            new SnesCgram(),
            () => 0);
        var guard = NewEnemyProjectileDefinitionReadGuard(setupBus, forbidden);
        RoomEnemyProjectileSlot projectile = enemies.EnemyProjectiles[^1];

        foreach (RoomEnemyProjectileKind kind in kinds)
        {
            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            projectile.Clear();
            initialize(projectile, kind, 0x0c00);
            AssertEqual(kind, projectile.Kind, $"{kind} production kind");
            AssertEqual(definition.PreInstruction, projectile.PreInstruction,
                $"{kind} production pre-instruction");
            AssertEqual(definition.InitialInstructionList, projectile.InstructionPointer,
                $"{kind} production initial list");
            AssertEqual((ushort)1, projectile.InstructionTimer,
                $"{kind} production timer");
            AssertEqual((ushort)0x8000, projectile.SpritemapPointer,
                $"{kind} production initial spritemap");
            AssertEqual(definition.Radius, projectile.Radius,
                $"{kind} production radius");
            AssertEqual(definition.Damage, projectile.Damage,
                $"{kind} production damage");
            AssertEqual(definition.DrawPriority, projectile.DrawPriority,
                $"{kind} production draw priority");
            AssertEqual(definition.CanDamageSamus, projectile.CanDamageSamus,
                $"{kind} production Samus-damage property");
            AssertEqual(definition.PersistsOnSamusContact, projectile.PersistsOnSamusContact,
                $"{kind} production persistence property");
            AssertEqual(definition.BlocksSamusProjectiles, projectile.BlocksSamusProjectiles,
                $"{kind} production projectile-blocking property");
            AssertEqual((ushort)0x0c00, projectile.GraphicsIndex,
                $"{kind} production graphics word");

            projectile.Position = projectile.Position.WithWhole(new(0x0100, 0x0100));
            projectile.Radius = new(1, 1);
            projectile.CanDamageSamus = true;
            projectile.PersistsOnSamusContact = true;
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x0100, 0x0100)),
                Health = ushort.MaxValue,
            };
            ushort initialList = projectile.InstructionPointer;
            AssertTrue(collideWithSamus(projectile, samus).HasValue,
                $"{kind} production touch admitted");
            AssertEqual(
                definition.TouchInstructionList == 0
                    ? initialList
                    : definition.TouchInstructionList,
                projectile.InstructionPointer,
                $"{kind} production touch list");

            initialize(projectile, kind, 0);
            projectile.Position = projectile.Position.WithWhole(new(0x0100, 0x0100));
            projectile.BlocksSamusProjectiles = true;
            projectile.CollisionOption = 0;
            var shots = new SamusProjectileSystem();
            SamusProjectileSlot shot = shots.Slots[0];
            shot.Type = 1;
            shot.Damage = 1;
            shot.Direction = (ushort)SamusProjectileDirection.Right;
            shot.Position = shot.Position.WithWhole(new(0x0100, 0x0100));
            int hits = enemies.ResolveEnemyProjectileSamusProjectileHits(
                guard,
                shots,
                new SamusBombProjectileSystem());
            AssertEqual(1, hits, $"{kind} production shot admitted");
            AssertEqual(definition.ShotInstructionList, projectile.InstructionPointer,
                $"{kind} production shot list");
        }

        AssertThrows<InvalidDataException>(
            () => EnemyProjectileDefinitionCatalog.Get(RoomEnemyProjectileKind.None),
            "inactive enemy-projectile identity has no definition");
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileDefinitionCatalog.Get((RoomEnemyProjectileKind)0x8000),
            "unknown enemy-projectile definition");

        Console.WriteLine(
            $"Enemy projectile definitions: {kinds.Length:N0} complete seven-word records, " +
            "common initialization, Samus touch, and projectile-shot consumers pass with " +
            "all definition bytes forbidden.");
    }

    private static CartridgeReadGuard NewEnemyProjectileDefinitionReadGuard(
        ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Enemy-projectile runtime reread compiled definition byte ${address:X6}.");
}
