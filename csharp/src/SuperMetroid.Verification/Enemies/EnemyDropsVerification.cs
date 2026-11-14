using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Audio;
using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Exercises native enemy drop selection, pickup effects, collision timing, and death conversion.</summary>
internal static class EnemyDropsVerification
{
    /// <summary>The first authored drop-chance table's chances, which the synthetic definition header selects.</summary>
    internal static readonly EnemyDropChances NativeDropChances = new(60, 60, 60, 5, 60, 10);

    /// <summary>
    /// Verifies the complete bank-$86 temporary-pickup path: selection, resource effects,
    /// collision timing, the physical slot-zero bug, and conversion from a generic enemy
    /// death. All test actors still use ROM-shaped projectile definitions and lists.
    /// </summary>
    internal static void VerifyEnemyDrops()
    {
        Suite(nameof(VerifyEveryEnemyPickupEffect), () => VerifyEveryEnemyPickupEffect());
        Suite(nameof(VerifyEnemyDropSelectionRules), () => VerifyEnemyDropSelectionRules());
        Suite(nameof(VerifyEnemyPickupLifetimeAndCollision), () => VerifyEnemyPickupLifetimeAndCollision());
        Suite(nameof(VerifyEnemyPickupGrappleDelay), () => VerifyEnemyPickupGrappleDelay());
        Suite(nameof(VerifyDirectEnemyDropReachesSlotZero), () => VerifyDirectEnemyDropReachesSlotZero());
        Suite(nameof(VerifyGenericEnemyDeathDropConversion), () => VerifyGenericEnemyDeathDropConversion());
        Suite(nameof(VerifyContactDeathStopsEnemyDispatch), () => VerifyContactDeathStopsEnemyDispatch());

        Console.WriteLine(
            "  Enemy drops: native random selection, five effects, collision/lifetime, grapple delay, slot-zero bug, and death conversion agree.");
    }

    /// <summary>Confirms contact death replaces a respawning enemy with inert AI before the next frame.</summary>
    internal static void VerifyContactDeathStopsEnemyDispatch()
    {
        var samus = CreateDropTestSamus();
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.PseudoScrew;
        var fixture = CreateEnemyDropFixture(samus, [1]);
        var enemy = fixture.System.SlotTable.Slots[0];
        enemy.Bind((EnemyDefinitionId)0x9000, enemy.Definition);
        enemy.Bind(enemy.DefinitionId, default(RoomEnemyDefinition) with
        {
            Touch = EnemyReaction.NormalEnemyTouch,
        });
        SeedContactDeathVulnerability(fixture.Bus);
        enemy.Position = enemy.Position.WithWhole(samus.Position.Whole);
        enemy.Radius = new(8, 8);
        enemy.Health = 1;
        enemy.Frame = EnemyFrameId.SkulteraSwimmingLeftLoopFirst;
        enemy.Contact = EnemyContact.None;
        enemy.Processing = EnemyProcessing.None;
        enemy.Presence = EnemyPresence.RespawnsIfKilled;
        var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        var samusColours = new SamusColours();
        fixture.System.StepStandaloneFrame(fixture.System.StandaloneFrame(fixture.Level) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, SamusProjectiles = projectiles, SamusColours = samusColours });
        AssertEqual(EnemyDefinitionId.Respawn, enemy.DefinitionId, "contact death retains respawn placeholder");
        AssertEqual(1, fixture.System.RoomStatus.EnemiesKilled, "contact death counted once");
        AssertEqual(1, enemy.FrameCounter, "death frame dispatches inert placeholder then advances native frame counter");
        // The definition no longer names the placeholder's empty main AI (#1738); its entity owns it.
        AssertEqual(RoomEnemyAuxiliaryDefinitionCatalog.Respawn, enemy.Definition,
            "cached definition follows the replacement identity to the respawn placeholder's");
        AssertTrue(enemy.Entity is SuperMetroid.Core.Enemies.Inert.InertEnemyEntity,
            "the replacement identity binds the placeholder entity, whose main AI takes no action");
        fixture.System.StepStandaloneFrame(fixture.System.StandaloneFrame(fixture.Level) with { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, SamusProjectiles = projectiles, SamusColours = samusColours });
        AssertEqual(0, fixture.System.ActiveEnemies.Count, "placeholder remains excluded next frame");
        AssertEqual(1, fixture.System.RoomStatus.EnemiesKilled, "placeholder cannot be killed twice");
    }

    /// <summary>Checks resource changes and sounds for the five enemy pickup kinds.</summary>
    private static void VerifyEveryEnemyPickupEffect()
    {
        SamusState smallEnergySamus = CreateDropTestSamus();
        smallEnergySamus.Health = 50;
        AssertEnemyPickupEffect(
            EnemyPickupKind.SmallEnergy,
            smallEnergySamus,
            expectedSound: 1,
            assertEffect: samus =>
                AssertEqual(55, samus.Health, "small-energy pickup restores five"));

        SamusState bigEnergySamus = CreateDropTestSamus();
        bigEnergySamus.Health = 95;
        bigEnergySamus.ReserveEnergy = 90;
        bigEnergySamus.MaxReserveEnergy = 100;
        AssertEnemyPickupEffect(
            EnemyPickupKind.BigEnergy,
            bigEnergySamus,
            expectedSound: 2,
            assertEffect: samus =>
            {
                AssertEqual(99, samus.Health,
                    "big-energy pickup caps ordinary energy");
                AssertEqual(100, samus.ReserveEnergy,
                    "big-energy overflow fills and caps reserve energy");
                AssertEqual(ReserveTankMode.Auto, samus.ReserveTankMode,
                    "first reserve overflow enables automatic reserve mode");
            });

        SamusState powerBombSamus = CreateDropTestSamus();
        powerBombSamus.PowerBombs = 4;
        powerBombSamus.MaxPowerBombs = 5;
        AssertEnemyPickupEffect(
            EnemyPickupKind.PowerBomb,
            powerBombSamus,
            expectedSound: 5,
            assertEffect: samus =>
                AssertEqual(5, samus.PowerBombs,
                    "power-bomb pickup restores one"));

        SamusState missileSamus = CreateDropTestSamus();
        missileSamus.Missiles = 4;
        missileSamus.MaxMissiles = 5;
        AssertEnemyPickupEffect(
            EnemyPickupKind.Missile,
            missileSamus,
            expectedSound: 3,
            assertEffect: samus =>
            {
                AssertEqual(5, samus.Missiles,
                    "missile pickup caps the visible missile count");
                AssertEqual(1, samus.ReserveMissiles,
                    "missile overflow enters native unused reserve-missile WRAM");
            });

        SamusState superSamus = CreateDropTestSamus();
        superSamus.SuperMissiles = 4;
        superSamus.MaxSuperMissiles = 5;
        AssertEnemyPickupEffect(
            EnemyPickupKind.SuperMissile,
            superSamus,
            expectedSound: 4,
            assertEffect: samus =>
                AssertEqual(5, samus.SuperMissiles,
                    "super-missile pickup restores one"));
    }

    /// <summary>Collects one spawned pickup and checks its sound and Samus resource effect.</summary>
    internal static void AssertEnemyPickupEffect(
        EnemyPickupKind kind,
        SamusState samus,
        ushort expectedSound,
        Action<SamusState> assertEffect)
    {
        ushort random = kind switch
        {
            EnemyPickupKind.SmallEnergy => 1,
            EnemyPickupKind.BigEnergy => 123,
            EnemyPickupKind.Missile => 1,
            EnemyPickupKind.SuperMissile => 196,
            EnemyPickupKind.PowerBomb => 246,
            EnemyPickupKind.None or
                EnemyPickupKind.NoDrop => throw new ArgumentOutOfRangeException(nameof(kind)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [random]);
        EnemyProjectile pickup = fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(
            samus.Position.Whole,
            NativeDropChances) ??
            throw new InvalidOperationException($"Could not allocate {kind} test pickup.");

        AssertEqual(kind, pickup.StateOf<EnemyPickupState>().Kind,
            $"{kind} pickup restores {kind}");
        AssertEqual(400, pickup.StateOf<EnemyPickupState>().Lifetime,
            $"{kind} starts with native 400-frame lifetime");
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);

        AssertEqual((SoundEffectId?)SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, expectedSound), fixture.System.Audio.LatestSound,
            $"{kind} library-two sound");
        AssertEqual(EnemyProjectileFrameBehavior.Stationary, pickup.FrameBehavior,
            $"{kind} enters shared blank/no-collision tail after collection");
        assertEffect(samus);
    }

    /// <summary>Checks native random selection and eligibility rules for temporary enemy drops.</summary>
    internal static void VerifyEnemyDropSelectionRules()
    {
        // Random zero is explicitly rejected by $86:F106. A following value of one must
        // select the first enabled accumulator entry rather than becoming a no-drop.
        SamusState rerollSamus = CreateDropTestSamus();
        rerollSamus.Health = 50;
        EnemyDropFixture reroll = CreateEnemyDropFixture(rerollSamus, randomValues: [0, 1]);
        EnemyProjectile rerolled = (reroll.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(
            new(50, 50), NativeDropChances) ?? throw new InvalidOperationException("Verification requires reroll.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable( new(50, 50), NativeDropChan...."));
        AssertEqual(EnemyPickupKind.SmallEnergy, rerolled.StateOf<EnemyPickupState>().Kind,
            "zero RNG sample rerolls before selecting small energy");

        // `$7E:0E1E` is a stateful critical-energy flag. Energy below 30 enables it,
        // 30..49 retains its previous value, and 50+ clears it. Native table 1 ($F1FA)
        // makes each of those three states observably distinct with the same RNG byte.
        SamusState biasSamus = CreateDropTestSamus();
        biasSamus.Health = 29;
        EnemyDropFixture bias = CreateEnemyDropFixture(
            biasSamus,
            randomValues: [200, 200, 200]);
        var biasDropChances = new EnemyDropChances(30, 80, 85, 40, 10, 10);

        EnemyProjectile belowThirty =
            (bias.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(10, 10), biasDropChances) ?? throw new InvalidOperationException("Verification requires bias.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(10, 10), biasDropChancePoi...."));
        AssertEqual(EnemyPickupKind.BigEnergy, belowThirty.StateOf<EnemyPickupState>().Kind,
            "critical energy suppresses no-drop and renormalizes energy weights");

        biasSamus.Health = 40;
        EnemyProjectile graceBand =
            (bias.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(20, 20), biasDropChances) ?? throw new InvalidOperationException("Verification requires bias.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(20, 20), biasDropChancePoi...."));
        AssertEqual(EnemyPickupKind.BigEnergy, graceBand.StateOf<EnemyPickupState>().Kind,
            "30..49 energy retains the previous critical-drop bias");

        biasSamus.Health = 50;
        EnemyProjectile clearedBias =
            (bias.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(30, 30), biasDropChances) ?? throw new InvalidOperationException("Verification requires bias.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(30, 30), biasDropChancePoi...."));
        AssertEqual(EnemyProjectileFrameBehavior.Stationary, clearedBias.FrameBehavior,
            "50 energy clears critical bias and permits the table's no-drop weight");

        SamusState fullSamus = CreateDropTestSamus();
        fullSamus.ReserveEnergy = fullSamus.MaxReserveEnergy = 100;
        fullSamus.Missiles = fullSamus.MaxMissiles = 5;
        fullSamus.SuperMissiles = fullSamus.MaxSuperMissiles = 5;
        fullSamus.PowerBombs = fullSamus.MaxPowerBombs = 5;
        EnemyDropFixture full = CreateEnemyDropFixture(fullSamus, randomValues: [1]);
        EnemyProjectile ineligible =
            (full.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(40, 40), NativeDropChances) ?? throw new InvalidOperationException("Verification requires full.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(40, 40), NativeDropChanceP...."));
        AssertEqual(EnemyProjectileFrameBehavior.Stationary, ineligible.FrameBehavior,
            "full resources make the native record's ammo weights ineligible");
    }

    /// <summary>Checks pickup contact timing, expiration, and transition to the shared blank tail.</summary>
    private static void VerifyEnemyPickupLifetimeAndCollision()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1, 1]);

        EnemyProjectile expiring =
            (fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(300, 300), NativeDropChances) ?? throw new InvalidOperationException("Verification requires fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(300, 300), NativeDropCh...."));
        expiring.StateOf<EnemyPickupState>().Lifetime = 1;
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertEqual(0, expiring.StateOf<EnemyPickupState>().Lifetime,
            "pickup lifetime decrements before its expiry comparison");
        AssertEqual(EnemyProjectileFrameBehavior.Stationary, expiring.FrameBehavior,
            "expired pickup enters native blank tail");
        AssertEqual(50, samus.Health,
            "expiry does not apply a pickup effect");

        EnemyProjectile boundary =
            (fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropChances) ?? throw new InvalidOperationException("Verification requires fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropCh...."));
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(
            boundary.Position.X.Whole + boundary.Radius.X + samus.Kinematics.Radius.X)), boundary.Position.Y.Whole));
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertEqual(50, samus.Health,
            "radius equality is not Samus/pickup contact");

        samus.Position -= new Offset16<RoomPixels>(1, 0);
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertTrue(samus.Health > 50,
            "one pixel inside strict radius sum collects pickup");
    }

    /// <summary>Checks the pickup's delayed collection while grapple interaction is active.</summary>
    private static void VerifyEnemyPickupGrappleDelay()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        samus.Position = samus.Position.WithWhole(new(500, 500));
        samus.Grapple.Phase = GrapplePhase.Firing;
        samus.Grapple.Anchor = samus.Grapple.Anchor.WithX(100);
        samus.Grapple.Anchor = samus.Grapple.Anchor.WithY(100);
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1]);
        EnemyProjectile pickup =
            (fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropChances) ?? throw new InvalidOperationException("Verification requires fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropCh...."));

        for (int frame = 0; frame < 16; frame++)
        {
            fixture.System.StepEnemyProjectiles(fixture.Level, samus);
            AssertEqual(50, samus.Health,
                $"grapple cannot collect pickup during protected frame {frame + 1}");
        }
        AssertEqual(384, pickup.StateOf<EnemyPickupState>().Lifetime,
            "sixteen protected frames still decrement pickup lifetime");

        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertTrue(samus.Health > 50,
            "grapple endpoint collects on the seventeenth pickup frame");
    }

    /// <summary>
    /// $86:EF29's branch tests the X restored by Random_Drop_Routine, which on this path is
    /// the spawn routine's projectile ID rather than a slot index, so slot zero still
    /// becomes a pickup. Crocomire's death drops in the 100% movie depend on it.
    /// </summary>
    private static void VerifyDirectEnemyDropReachesSlotZero()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        EnemyDropFixture fixture = CreateEnemyDropFixture(
            samus,
            randomValues: Enumerable.Repeat((ushort)1, 19).ToArray());

        for (int allocation = 0; allocation < 17; allocation++)
        {
            EnemyProjectile pickup =
                (fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropChances) ?? throw new InvalidOperationException("Verification requires fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropCh...."));
            AssertEqual(17 - allocation, pickup.Place,
                $"enemy projectile allocation {allocation} descends from native $22");
            AssertEqual(EnemyProjectileFrameBehavior.PickupLifetime, pickup.FrameBehavior,
                $"nonzero projectile slot {pickup.Place} becomes pickup");
        }

        EnemyProjectile slotZero =
            (fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropChances) ?? throw new InvalidOperationException("Verification requires fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropCh...."));
        AssertEqual(0, slotZero.Place, "eighteenth allocation reaches physical slot zero");
        AssertEqual(EnemyProjectileFrameBehavior.PickupLifetime, slotZero.FrameBehavior,
            "a directly spawned drop in physical slot zero becomes a pickup");
        AssertEqual<EnemyProjectile?>(null,
            fixture.System.DeathsAndDrops.SpawnEnemyDropFromChanceTable(new(100, 100), NativeDropChances),
            "nineteenth allocation observes the shared eighteen-slot pool as full");
    }

    /// <summary>Checks conversion of a generic death explosion into a pickup without replacing its projectile identity.</summary>
    private static void VerifyGenericEnemyDeathDropConversion()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1]);

        const ushort enemyHeader = 0x9000;
        SeedEnemyDropChancePointer(fixture.Bus, enemyHeader);
        RoomEnemySlot enemy = fixture.System.SlotTable.Slots[0];
        enemy.Bind((EnemyDefinitionId)enemyHeader, enemy.Definition);
        enemy.Position = enemy.Position.WithWhole(new(321, 123));
        enemy.Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook;
        enemy.Contact = EnemyContact.None;
        enemy.Processing = EnemyProcessing.None;
        enemy.Presence = EnemyPresence.RespawnsIfKilled;

        // A Fireflea touched with health nine left dies in the small explosion.
        fixture.System.DeathsAndDrops.StartGenericEnemyDeath(enemy,
            deathAnimation: FirefleaTouchDeathExplosion.AfterSurvivedTouch(9, EnemyDeathExplosionDefinitions.AuthoredOrder));
        AssertEqual(EnemyDefinitionId.Respawn, enemy.DefinitionId,
            "respawning death leaves native DAFF placeholder in physical enemy slot");
        AssertEqual((byte)0xa3, enemy.NativeHeader.Bank,
            "respawning death leaves native placeholder AI bank");
        AssertEqual(1, fixture.System.RoomStatus.EnemiesKilled,
            "generic enemy death increments room kill counter");

        EnemyProjectile explosion = fixture.System.Projectiles.EnemyProjectiles[17];
        AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind,
            "generic death allocates projectile definition F345");
        AssertEqual((EnemyDefinitionId)enemyHeader, explosion.DropDefinitionId,
            "death actor retains killed enemy header for later chance lookup");
        AssertEqual((ushort)321, explosion.Position.X.Whole,
            "death actor retains killed enemy X position");
        AssertEqual((ushort)123, explosion.Position.Y.Whole,
            "death actor retains killed enemy Y position");
        AssertEqual(KilledEnemy.Of(enemy, respawns: true), explosion.KilledEnemy,
            "respawning slot zero is retained as the killed enemy, marked to respawn");
        AssertEqual(
            EnemyDeathExplosions.Animation(EnemyDeathExplosion.Small),
            ProgramOf(explosion),
            "a Fireflea health above four starts the small explosion");
        AssertEqual(SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard, explosion.Graphics,
            "enemy-death initializer replaces the dying actor's room graphics with the cleared word's common colours");

        EnemyDeathProjectileSegment converted = fixture.System.DeathsAndDrops.ConvertEnemyDeathExplosionToPickup(explosion);
        AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind,
            "EEAF converts death actor in place without rewriting its definition identity");
        AssertEqual(EnemyPickupKind.SmallEnergy, explosion.StateOf<EnemyPickupState>().Kind,
            "converted death actor becomes a small-energy pickup");
        AssertEqual(EnemyProjectileFrameBehavior.PickupLifetime, explosion.FrameBehavior,
            "converted death actor enters shared pickup pre-instruction");
        AssertEqual(EnemyDeathProjectileSegment.SmallEnergy, converted,
            "converted death actor continues at the small-energy drop program");
    }

    /// <summary>Creates a centered Samus fixture with full ordinary energy for pickup comparisons.</summary>
    internal static SamusState CreateDropTestSamus() => new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
    {
        Position = new(new(100, 0), new(100, 0)),
        Health = 99,
        MaxHealth = 99,
    };

    /// <summary>Loads a one-block room with ROM-shaped pickup programs and a controlled random queue.</summary>
    internal static EnemyDropFixture CreateEnemyDropFixture(
        SamusState samus,
        IReadOnlyList<ushort> randomValues,
        IRoomEnemyDefinitionSource? definitions = null)
    {
        var bus = new TestAddressSpace();
        SeedEnemyPickupRom(bus);
        Queue<ushort> random = new(randomValues);
        int NextRandom() => random.Count == 0 ? 1 : random.Dequeue();

        RoomLevelData level = CreateEmptyRoom(new(1, 1));
        var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: definitions ?? bus, services: new StandaloneRoomEnemyServices(NextRandom));
        system.Load(new RoomEnemyLoadRequest(bus.ReadEnemyPopulation(0x9000), BgArtworkFixtures.For(new SnesVram())) { Level = level, Samus = samus });
        return new EnemyDropFixture(bus, system, level);
    }

    /// <summary>Seeds respawn enemy data and compiled pickup list pointers into the synthetic bus.</summary>
    private static void SeedEnemyPickupRom(TestAddressSpace bus)
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] placeholder = new byte[64];
        for (int i = 0; i < placeholder.Length; i++)
            placeholder[i] = retail.ReadCartridgeByte(0xa00000 + SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.Respawn) + i);
        bus.WriteBytes(0xa00000 + SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.Respawn), placeholder);
        // Empty enemy population: tests directly exercise the shared projectile subsystem.
        SeedEmptyRoomEnemyPopulation(bus, 0x9000);

        // The five compiled pickup identities select their retail list addresses. Long
        // synthetic frames keep the focused effect fixture independent of pickup artwork
        // while exercising the real immutable selectors.
        foreach (ushort list in new ushort[] { 0xed8d, 0xeda3, 0xedeb, 0xedb9, 0xeddd })
            bus.WriteBytes(0x860000 | list, [0xff, 0x7f, 0x00, 0x90]);

        // Shared blank tail: one blank frame, then EF10 respawn and 8154 delete. Most
        // checks inspect the tail immediately; this data also keeps a stepped tail valid.
        bus.WriteBytes(0x86eca3,
        [
            0x01, 0x00, 0x00, 0x00,
            0x10, 0xef,
            0x54, 0x81,
        ]);

    }

    /// <summary>
    /// Points the synthetic bank-$A0 enemy definition at <paramref name="enemyHeader"/> (its
    /// drop-chance pointer field, definition byte 58) at the native first drop-chance table,
    /// and seeds that table's six chances, which the import decodes.
    /// </summary>
    internal static void SeedEnemyDropChancePointer(TestAddressSpace bus, ushort enemyHeader)
    {
        WriteTestWord(bus, 0xa00000 | enemyHeader | 58, SuperMetroid.AssetExtraction.Enemies.EnemyHeaderTableDefinitions.FirstDropChances);
        NativeDropChances.Deconstruct(out int small, out int big, out int missile, out int nothing, out int super, out int powerBomb);
        bus.WriteBytes(SuperMetroid.AssetExtraction.Enemies.EnemyHeaderTableDefinitions.TableBank | SuperMetroid.AssetExtraction.Enemies.EnemyHeaderTableDefinitions.FirstDropChances,
            [(byte)small, (byte)big, (byte)missile, (byte)nothing, (byte)super, (byte)powerBomb]);
    }

    /// <summary>Seeds the bank-$B4 vulnerability byte the contact-death fixture enemy consults.</summary>
    private static void SeedContactDeathVulnerability(TestAddressSpace bus) =>
        bus.WriteBytes(0xb48014, [2]);

    /// <summary>Groups the synthetic bus, loaded enemy system, and one-block level for pickup checks.</summary>
    internal readonly record struct EnemyDropFixture(
        TestAddressSpace Bus,
        RoomEnemySystem System,
        RoomLevelData Level);

    /// <summary>Runs the complete enemy-drop verification command.</summary>
    [VerificationCommand("--enemy-drops")]
    internal static int EnemyDropsCommand()
    {
        Suite(nameof(VerifyEnemyDrops), () => VerifyEnemyDrops());
        return 0;
    }

    /// <summary>Runs the focused contact-death dispatch verification command.</summary>
    [VerificationCommand("--enemy-contact-death")]
    internal static int EnemyContactDeathCommand()
    {
        Suite(nameof(VerifyContactDeathStopsEnemyDispatch), () => VerifyContactDeathStopsEnemyDispatch());
        return 0;
    }
}
