using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Bosses.Torizo;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Bosses.SporeSpawn;
namespace SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Fx;

/// <summary>Checks all compiled enemy projectile definitions and their production collision consumers.</summary>
internal static class EnemyProjectileDefinitionsVerification
{
    /// <summary>Walks 123 native records, then checks initialization, Samus touch, and projectile shot handoffs.</summary>
    internal static void VerifyEnemyProjectileDefinitions(SuperMetroidAddressSpace rom)
    {
        RoomEnemyProjectileKind[] kinds = Enum.GetValues<RoomEnemyProjectileKind>()
            .Where(kind => kind != RoomEnemyProjectileKind.None)
            .Distinct()
            .ToArray();
        // #1269 added Mother Brain's small purple breath and the Climb's fake-wall explosion.
        AssertEqual(123, kinds.Length, "compiled enemy-projectile definition count");

        HashSet<int> forbidden = VerifyEnemyProjectileDefinitionRecords(rom, kinds);

        Action<RoomEnemyProjectileSlot, RoomEnemyProjectileKind, ushort> initialize =
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition;

        const ushort populationPointer = 0x9400;
        TestAddressSpace setupBus = CreateEmptyEnemyPopulationCartridge(populationPointer);
        var enemies = new RoomEnemySystem();
        enemies.Load(new RoomEnemyLoadRequest(setupBus, populationPointer, 0xffff, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(() => 0)));
        var guard = NewEnemyProjectileDefinitionReadGuard(setupBus, forbidden);
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
        Func<RoomEnemyProjectileSlot, SamusState, ushort?> collideWithSamus =
            enemies.Projectiles.ResolveEnemyProjectileSamusCollision;

        foreach (RoomEnemyProjectileKind kind in kinds)
        {
            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            projectile.Clear();
            initialize(projectile, kind, 0x0c00);
            AssertEqual(kind, projectile.Kind, $"{kind} production kind");
            AssertEqual(definition.PreInstruction, projectile.PreInstruction,
                $"{kind} production pre-instruction");
            // A kind with a typed initial program (#641) runs it in place of the definition's native list.
            bool typedStart = EnemyProjectileTypedStarts.Initial(kind) is not null;
            AssertEqual(typedStart, projectile.Animation is not null, $"{kind} production typed start");
            AssertEqual(typedStart ? (ushort)0 : definition.InitialInstructionList, projectile.InstructionPointer,
                $"{kind} production initial list");
            AssertEqual((ushort)1, projectile.InstructionTimer.RawValue,
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
            if (kind == RoomEnemyProjectileKind.DraygonGoop)
            {
                // Draygon's goop touch program is typed (#641); VerifyDraygonProjectileAnimations matches it to the native list.
                AssertTrue(projectile.Animation is DraygonProjectileAnimation { Cursor.Program: DraygonProjectileProgram.GoopTouch },
                    $"{kind} production touch starts the typed touch program");
            }
            else if (kind == RoomEnemyProjectileKind.MotherBrainOnionRing)
            {
                // The onion ring's impact program is typed (#1292); VerifyMotherBrainEffectAnimations matches it.
                AssertTrue(projectile.Animation is MotherBrainEffectAnimation { Cursor.Program: MotherBrainEffectProgram.OnionRingHit },
                    $"{kind} production touch starts the typed touch program");
            }
            else if (kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet)
            {
                // The turret bullet's impact program is typed (#1285); VerifyMotherBrainProjectileAnimations matches it.
                AssertTrue(projectile.Animation is MotherBrainTurretAnimation,
                    $"{kind} production touch starts the typed impact program");
            }
            else if (definition.TouchInstructionList != 0)
            {
                // Every other touch list is the shared delete list; the owner coverage check proves each one deletes.
                AssertTrue(projectile.Animation is EnemyProjectileDeleteAnimation, $"{kind} production touch starts the shared delete reaction");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed touch keeps no native list");
            }
            else
            {
                AssertEqual(initialList, projectile.InstructionPointer, $"{kind} production touch keeps its list");
            }

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
            int hits = enemies.Projectiles.ResolveEnemyProjectileSamusProjectileHits(
                shots,
                new SamusBombProjectileSystem());
            AssertEqual(1, hits, $"{kind} production shot admitted");
            if (kind == RoomEnemyProjectileKind.DraygonGoop)
            {
                AssertTrue(projectile.Animation is DraygonProjectileAnimation { Cursor.Program: DraygonProjectileProgram.GoopShot },
                    $"{kind} production shot starts the typed shot program");
            }
            else if (kind == RoomEnemyProjectileKind.CrocomireProjectile)
            {
                // Crocomire's shot program is typed (#641); VerifyCrocomireAnimations matches it to the native list.
                AssertTrue(projectile.Animation is CrocomireProjectileAnimation { Kind: CrocomireProjectileFrameKind.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.KraidSpitRock)
            {
                // Kraid's rock burst is typed (#641); VerifyKraidRockAnimations matches it to the native list.
                AssertTrue(projectile.Animation is KraidRockAnimation { Program: KraidRockProgram.SpitRockShot },
                    $"{kind} production shot starts the typed shot program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet)
            {
                // The turret bullet's impact is typed (#1285); VerifyMotherBrainProjectileAnimations matches it.
                AssertTrue(projectile.Animation is MotherBrainTurretAnimation,
                    $"{kind} production shot starts the typed impact program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.PhantoonDestroyableFlame or RoomEnemyProjectileKind.PhantoonStartingFlame)
            {
                // Phantoon's flames are typed (#1286); VerifyPhantoonFlameAnimations matches them to the native lists.
                PhantoonFlameProgram expected = kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame
                    ? PhantoonFlameProgram.DestroyableShot
                    : PhantoonFlameProgram.DestroyableIdle;
                AssertTrue(projectile.Animation is PhantoonFlameAnimation started
                        && started.Cursor == AnimationProgramCursor<PhantoonFlameProgram>.Start(expected),
                    $"{kind} production shot starts the typed {expected} program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.SporeSpawnStalk or RoomEnemyProjectileKind.SporeSpawnSpore or
                RoomEnemyProjectileKind.SporeSpawnSpawner)
            {
                // Spore Spawn's projectiles are typed (#1288); VerifySporeSpawnAnimations matches them to the native lists.
                SporeSpawnProjectileProgram expected = kind switch
                {
                    RoomEnemyProjectileKind.SporeSpawnStalk => SporeSpawnProjectileProgram.Stalk,
                    RoomEnemyProjectileKind.SporeSpawnSpore => SporeSpawnProjectileProgram.SporeShot,
                    _ => SporeSpawnProjectileProgram.SpawnerClosed,
                };
                AssertTrue(projectile.Animation is SporeSpawnProjectileAnimation started
                        && started.Cursor == AnimationProgramCursor<SporeSpawnProjectileProgram>.Start(expected),
                    $"{kind} production shot starts the typed {expected} program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind is RoomEnemyProjectileKind.BombTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoChozoOrb or RoomEnemyProjectileKind.GoldenTorizoEgg or RoomEnemyProjectileKind.GoldenTorizoSuperMissile)
            {
                // Torizo's shot programs are typed (#1289); VerifyTorizoProjectileAnimations matches them to the native lists.
                TorizoProjectileProgram expected = kind switch
                {
                    RoomEnemyProjectileKind.GoldenTorizoEgg => TorizoProjectileProgram.ChozoOrbWallImpact,
                    RoomEnemyProjectileKind.GoldenTorizoSuperMissile => TorizoProjectileProgram.SuperMissileImpact,
                    _ => TorizoProjectileProgram.ChozoOrbShot,
                };
                AssertTrue(projectile.Animation is TorizoProjectileAnimation started
                        && started.Cursor == AnimationProgramCursor<TorizoProjectileProgram>.Start(expected),
                    $"{kind} production shot starts the typed {expected} program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.PowampSpike)
            {
                // The spike's shot list is its private delete list, typed with its programs (#1291).
                AssertTrue(projectile.Animation is PowampSpikeAnimation { Cursor.Program: PowampSpikeProgram.Delete },
                    $"{kind} production shot starts the typed delete program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.KagoBug)
            {
                // The bug's shot list spawns its drop; it is typed with its programs (#1292).
                AssertTrue(projectile.Animation is KagoBugProjectileAnimation { Cursor.Program: KagoBugProjectileProgram.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.DraygonWallTurret)
            {
                // A shot wall turret runs the shared misc-dust small explosion (#1292).
                AssertTrue(projectile.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.SmallExplosion },
                    $"{kind} production shot starts the typed small explosion");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.EyeDoorProjectile)
            {
                // The eye-door shot's shot list is typed with its programs (#1292).
                AssertTrue(projectile.Animation is EyeDoorProjectileAnimation { Cursor.Program: EyeDoorProjectileProgram.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else if (kind == RoomEnemyProjectileKind.LavaThrownByMagdollite)
            {
                // The lava's shot list drops its pickup; it is typed with its programs (#1291).
                AssertTrue(projectile.Animation is MagdolliteLavaAnimation { Cursor.Program: MagdolliteLavaProgram.Shot },
                    $"{kind} production shot starts the typed shot program");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
            else
            {
                // Every other shot list is the shared delete list; the owner coverage check proves each one deletes.
                AssertTrue(projectile.Animation is EnemyProjectileDeleteAnimation, $"{kind} production shot starts the shared delete reaction");
                AssertEqual((ushort)0, projectile.InstructionPointer, $"{kind} typed shot keeps no native list");
            }
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

    /// <summary>
    /// Checks every compiled enemy-projectile definition against its seven-word bank-$86 record
    /// and returns the record bytes, which the runtime must then never reread.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the bank-$86 enemy-projectile definition records")]
    private static HashSet<int> VerifyEnemyProjectileDefinitionRecords(
        SuperMetroidAddressSpace rom, RoomEnemyProjectileKind[] kinds)
    {

        var forbidden = new HashSet<int>();
        foreach (RoomEnemyProjectileKind kind in kinds)
        {
            int address = 0x860000 | (ushort)kind;
            for (int index = 0; index < 14; index++)
                forbidden.Add(address + index);

            EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)), definition.PreInstruction,
                $"{kind} pre-instruction");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4)), definition.InitialInstructionList,
                $"{kind} initial list");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)), definition.RadiusWord,
                $"{kind} radius word");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 8)), definition.Properties,
                $"{kind} properties");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 10)), definition.TouchInstructionList,
                $"{kind} touch list");
        }
        return forbidden;
    }

    /// <summary>A sparse cartridge whose bank-$A1 population at <paramref name="populationPointer"/> is empty.</summary>
    [CpuBusBoundary("exact bus test: seeds an empty synthetic bank-$A1 enemy population")]
    private static TestAddressSpace CreateEmptyEnemyPopulationCartridge(ushort populationPointer)
    {
        var bus = new TestAddressSpace();
        bus.WriteByte(0xa10000 | populationPointer, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 1, 0xff);
        bus.WriteByte((0xa10000 | populationPointer) + 2, 0);
        return bus;
    }

    /// <summary>Rejects live reads of every compiled bank-$86 definition byte during collision handling.</summary>
    private static CartridgeReadGuard NewEnemyProjectileDefinitionReadGuard(
        ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Enemy-projectile runtime reread compiled definition byte ${address:X6}.");

    /// <summary>Loads the retail cartridge and runs the definition audit from the command dispatcher.</summary>
    [VerificationCommand("--enemy-projectile-definitions")]
    internal static int EnemyProjectileDefinitionsCommand()
    {
        Suite(nameof(VerifyEnemyProjectileDefinitions), () => VerifyEnemyProjectileDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
