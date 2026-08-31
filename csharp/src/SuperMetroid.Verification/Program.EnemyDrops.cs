using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Verifies the complete bank-$86 temporary-pickup path: selection, resource effects,
    /// collision timing, the physical slot-zero bug, and conversion from a generic enemy
    /// death. All test actors still use ROM-shaped projectile definitions and lists.
    /// </summary>
    private static void VerifyEnemyDrops()
    {
        VerifyEveryEnemyPickupEffect();
        VerifyEnemyDropSelectionRules();
        VerifyEnemyPickupLifetimeAndCollision();
        VerifyEnemyPickupGrappleDelay();
        VerifyEnemyProjectileSlotZeroDropBug();
        VerifyGenericEnemyDeathDropConversion();

        Console.WriteLine(
            "  Enemy drops: native random selection, five effects, collision/lifetime, grapple delay, slot-zero bug, and death conversion agree.");
    }

    private static void VerifyEveryEnemyPickupEffect()
    {
        SamusState smallEnergySamus = CreateDropTestSamus();
        smallEnergySamus.Health = 50;
        AssertEnemyPickupEffect(
            EnemyPickupKind.SmallEnergy,
            smallEnergySamus,
            expectedSound: 1,
            assertEffect: samus =>
                AssertEqual((ushort)55, samus.Health, "small-energy pickup restores five"));

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
                AssertEqual((ushort)99, samus.Health,
                    "big-energy pickup caps ordinary energy");
                AssertEqual((ushort)100, samus.ReserveEnergy,
                    "big-energy overflow fills and caps reserve energy");
                AssertEqual((ushort)1, samus.ReserveTankMode,
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
                AssertEqual((ushort)5, samus.PowerBombs,
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
                AssertEqual((ushort)5, samus.Missiles,
                    "missile pickup caps the visible missile count");
                AssertEqual((ushort)1, samus.ReserveMissiles,
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
                AssertEqual((ushort)5, samus.SuperMissiles,
                    "super-missile pickup restores one"));
    }

    private static void AssertEnemyPickupEffect(
        EnemyPickupKind kind,
        SamusState samus,
        ushort expectedSound,
        Action<SamusState> assertEffect)
    {
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1]);
        WriteSingleDropChance(fixture.Bus, kind);
        RoomEnemyProjectileSlot pickup = fixture.System.SpawnEnemyDropFromChanceTable(
            samus.XPosition,
            samus.YPosition,
            itemDropChancesPointer: 0x8000) ??
            throw new InvalidOperationException($"Could not allocate {kind} test pickup.");

        AssertEqual(unchecked((ushort)((ushort)kind * 2)), pickup.Variable0,
            $"{kind} uses native eproj_E identity");
        AssertEqual((ushort)400, pickup.Variable1,
            $"{kind} starts with native 400-frame lifetime");
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);

        AssertEqual(kind, fixture.System.LastCollectedEnemyPickup,
            $"{kind} collection publication");
        AssertEqual(expectedSound, fixture.System.LastEnemyPickupSoundEffect,
            $"{kind} library-two sound");
        AssertEqual((ushort)0xefdf, pickup.PreInstruction,
            $"{kind} enters shared blank/no-collision tail after collection");
        assertEffect(samus);
    }

    private static void VerifyEnemyDropSelectionRules()
    {
        // Random zero is explicitly rejected by $86:F106. A following value of one must
        // select the first enabled accumulator entry rather than becoming a no-drop.
        SamusState rerollSamus = CreateDropTestSamus();
        rerollSamus.Health = 50;
        EnemyDropFixture reroll = CreateEnemyDropFixture(rerollSamus, randomValues: [0, 1]);
        WriteSingleDropChance(reroll.Bus, EnemyPickupKind.SmallEnergy);
        RoomEnemyProjectileSlot rerolled = reroll.System.SpawnEnemyDropFromChanceTable(
            50, 50, 0x8000)!;
        AssertEqual((ushort)2, rerolled.Variable0,
            "zero RNG sample rerolls before selecting small energy");

        // `$7E:0E1E` is a stateful critical-energy flag. Energy below 30 enables it,
        // 30..49 retains its previous value, and 50+ clears it. A mostly-no-drop table
        // makes each of those three states observably distinct with the same RNG byte.
        SamusState biasSamus = CreateDropTestSamus();
        biasSamus.Health = 29;
        EnemyDropFixture bias = CreateEnemyDropFixture(
            biasSamus,
            randomValues: [200, 200, 200]);
        bias.Bus.WriteBytes(0xb48000, [1, 1, 0, 253, 0, 0]);

        RoomEnemyProjectileSlot belowThirty =
            bias.System.SpawnEnemyDropFromChanceTable(10, 10, 0x8000)!;
        AssertEqual((ushort)4, belowThirty.Variable0,
            "critical energy suppresses no-drop and renormalizes energy weights");

        biasSamus.Health = 40;
        RoomEnemyProjectileSlot graceBand =
            bias.System.SpawnEnemyDropFromChanceTable(20, 20, 0x8000)!;
        AssertEqual((ushort)4, graceBand.Variable0,
            "30..49 energy retains the previous critical-drop bias");

        biasSamus.Health = 50;
        RoomEnemyProjectileSlot clearedBias =
            bias.System.SpawnEnemyDropFromChanceTable(30, 30, 0x8000)!;
        AssertEqual((ushort)0xefdf, clearedBias.PreInstruction,
            "50 energy clears critical bias and permits the table's no-drop weight");

        SamusState fullSamus = CreateDropTestSamus();
        fullSamus.ReserveEnergy = fullSamus.MaxReserveEnergy = 100;
        fullSamus.Missiles = fullSamus.MaxMissiles = 5;
        fullSamus.SuperMissiles = fullSamus.MaxSuperMissiles = 5;
        fullSamus.PowerBombs = fullSamus.MaxPowerBombs = 5;
        EnemyDropFixture full = CreateEnemyDropFixture(fullSamus, randomValues: [1]);
        WriteSingleDropChance(full.Bus, EnemyPickupKind.Missile);
        RoomEnemyProjectileSlot ineligible =
            full.System.SpawnEnemyDropFromChanceTable(40, 40, 0x8000)!;
        AssertEqual((ushort)0xefdf, ineligible.PreInstruction,
            "full resources make an otherwise certain ammo drop ineligible");
    }

    private static void VerifyEnemyPickupLifetimeAndCollision()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1, 1]);
        WriteSingleDropChance(fixture.Bus, EnemyPickupKind.SmallEnergy);

        RoomEnemyProjectileSlot expiring =
            fixture.System.SpawnEnemyDropFromChanceTable(300, 300, 0x8000)!;
        expiring.Variable1 = 1;
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertEqual((ushort)0, expiring.Variable1,
            "pickup lifetime decrements before its expiry comparison");
        AssertEqual((ushort)0xefdf, expiring.PreInstruction,
            "expired pickup enters native blank tail");
        AssertEqual<EnemyPickupKind?>(null, fixture.System.LastCollectedEnemyPickup,
            "expiry does not apply a pickup effect");

        RoomEnemyProjectileSlot boundary =
            fixture.System.SpawnEnemyDropFromChanceTable(100, 100, 0x8000)!;
        samus.XPosition = unchecked((ushort)(
            boundary.XPosition + boundary.XRadius + samus.Kinematics.XRadius));
        samus.YPosition = boundary.YPosition;
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertEqual<EnemyPickupKind?>(null, fixture.System.LastCollectedEnemyPickup,
            "radius equality is not Samus/pickup contact");

        samus.XPosition--;
        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertEqual(EnemyPickupKind.SmallEnergy, fixture.System.LastCollectedEnemyPickup,
            "one pixel inside strict radius sum collects pickup");
    }

    private static void VerifyEnemyPickupGrappleDelay()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        samus.XPosition = samus.YPosition = 500;
        samus.Grapple.Phase = GrapplePhase.Firing;
        samus.Grapple.AnchorX = 100;
        samus.Grapple.AnchorY = 100;
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1]);
        WriteSingleDropChance(fixture.Bus, EnemyPickupKind.SmallEnergy);
        RoomEnemyProjectileSlot pickup =
            fixture.System.SpawnEnemyDropFromChanceTable(100, 100, 0x8000)!;

        for (int frame = 0; frame < 16; frame++)
        {
            fixture.System.StepEnemyProjectiles(fixture.Level, samus);
            AssertEqual<EnemyPickupKind?>(null, fixture.System.LastCollectedEnemyPickup,
                $"grapple cannot collect pickup during protected frame {frame + 1}");
        }
        AssertEqual((ushort)384, pickup.Variable1,
            "sixteen protected frames still decrement pickup lifetime");

        fixture.System.StepEnemyProjectiles(fixture.Level, samus);
        AssertEqual(EnemyPickupKind.SmallEnergy, fixture.System.LastCollectedEnemyPickup,
            "grapple endpoint collects on the seventeenth pickup frame");
    }

    private static void VerifyEnemyProjectileSlotZeroDropBug()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        EnemyDropFixture fixture = CreateEnemyDropFixture(
            samus,
            randomValues: Enumerable.Repeat((ushort)1, 19).ToArray());
        WriteSingleDropChance(fixture.Bus, EnemyPickupKind.SmallEnergy);

        for (int allocation = 0; allocation < 17; allocation++)
        {
            RoomEnemyProjectileSlot pickup =
                fixture.System.SpawnEnemyDropFromChanceTable(100, 100, 0x8000)!;
            AssertEqual(17 - allocation, pickup.SlotIndex,
                $"enemy projectile allocation {allocation} descends from native $22");
            AssertEqual((ushort)0xefe0, pickup.PreInstruction,
                $"nonzero projectile slot {pickup.SlotIndex} becomes pickup");
        }

        RoomEnemyProjectileSlot slotZero =
            fixture.System.SpawnEnemyDropFromChanceTable(100, 100, 0x8000)!;
        AssertEqual(0, slotZero.SlotIndex, "eighteenth allocation reaches physical slot zero");
        AssertEqual((ushort)0xefdf, slotZero.PreInstruction,
            "physical slot zero cannot become a pickup despite a successful random roll");
        AssertEqual<RoomEnemyProjectileSlot?>(null,
            fixture.System.SpawnEnemyDropFromChanceTable(100, 100, 0x8000),
            "nineteenth allocation observes the shared eighteen-slot pool as full");
    }

    private static void VerifyGenericEnemyDeathDropConversion()
    {
        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1]);
        WriteSingleDropChance(fixture.Bus, EnemyPickupKind.SmallEnergy);

        const ushort enemyHeader = 0x9000;
        WriteWord(fixture.Bus, 0xa00000 | enemyHeader | 58, 0x8000);
        RoomEnemySlot enemy = fixture.System.Slots[0];
        enemy.EnemyDefinitionPointer = enemyHeader;
        enemy.XPosition = 321;
        enemy.YPosition = 123;
        enemy.VramTilesIndex = 0x0200;
        enemy.PaletteIndex = 0x0c00;
        enemy.Properties = (ushort)EnemyProperties.RespawnIfKilled;

        // Variant nine proves the common routine applies the native >=5 clamp before it
        // indexes the five-pointer death-animation table.
        fixture.System.StartGenericEnemyDeath(enemy, deathAnimation: 9);
        AssertEqual((ushort)0xdaff, enemy.EnemyDefinitionPointer,
            "respawning death leaves native DAFF placeholder in physical enemy slot");
        AssertEqual((byte)0xa3, enemy.AiBank,
            "respawning death leaves native placeholder AI bank");
        AssertEqual((ushort)1, fixture.System.EnemiesKilled,
            "generic enemy death increments room kill counter");

        RoomEnemyProjectileSlot explosion = fixture.System.EnemyProjectiles[17];
        AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind,
            "generic death allocates projectile definition F345");
        AssertEqual(enemyHeader, explosion.EnemyHeaderPointer,
            "death actor retains killed enemy header for later chance lookup");
        AssertEqual((ushort)321, explosion.XPosition,
            "death actor retains killed enemy X position");
        AssertEqual((ushort)123, explosion.YPosition,
            "death actor retains killed enemy Y position");
        AssertEqual((ushort)0x8000, explosion.KilledEnemyNativeIndex,
            "respawning slot zero is retained as high-bit native enemy index");
        AssertEqual((ushort)0xf200, explosion.InstructionPointer,
            "out-of-range death variant clamps to ROM table entry zero");
        AssertEqual((ushort)0, explosion.GraphicsIndex,
            "enemy-death initializer replaces the dying actor's room graphics with zero");

        fixture.System.ConvertEnemyDeathExplosionToPickup(explosion);
        AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind,
            "EEAF converts death actor in place without rewriting its definition identity");
        AssertEqual((ushort)2, explosion.Variable0,
            "converted death actor stores selected small-energy identity");
        AssertEqual((ushort)0xefe0, explosion.PreInstruction,
            "converted death actor enters shared pickup pre-instruction");
        AssertEqual((ushort)0xf100, explosion.InstructionPointer,
            "converted death actor reads the real EF04 pickup-list table");
    }

    private static SamusState CreateDropTestSamus() => new()
    {
        XPosition = 100,
        YPosition = 100,
        Health = 99,
        MaxHealth = 99,
    };

    private static EnemyDropFixture CreateEnemyDropFixture(
        SamusState samus,
        IReadOnlyList<ushort> randomValues)
    {
        var bus = new TestAddressSpace();
        SeedEnemyPickupRom(bus);
        Queue<ushort> random = new(randomValues);
        ushort NextRandom() => random.Count == 0 ? (ushort)1 : random.Dequeue();

        RoomLevelData level = CreateRoom(
            1,
            1,
            new ushort[1],
            new byte[1]);
        var system = new RoomEnemySystem();
        system.Load(
            bus,
            populationPointer: 0x9000,
            tilesetPointer: 0,
            new SnesVram(),
            new SnesCgram(),
            NextRandom,
            level: level,
            samus: samus);
        return new EnemyDropFixture(bus, system, level);
    }

    private static void SeedEnemyPickupRom(TestAddressSpace bus)
    {
        // Empty enemy population: tests directly exercise the shared projectile subsystem.
        bus.WriteBytes(0xa19000, [0xff, 0xff]);

        // Seven-word $F337/$F345 definitions. Only pre-instruction, list, radii, and packed
        // collision properties are consumed by SpawnEprojInner, but retaining all words
        // keeps this fixture shaped exactly like the bank-$86 data it substitutes for.
        byte[] definition =
        [
            0x00, 0x00,
            0xdf, 0xef,
            0xa3, 0xec,
            0x10, 0x10,
            0x00, 0x30,
            0x00, 0x00,
            0x00, 0x00,
        ];
        bus.WriteBytes(0x86f337, definition);
        bus.WriteBytes(0x86f345, definition);

        // EF04 is indexed by pickup identity * 2. Index zero is unused; the five live
        // identities point at distinct long-lived fixture frames so pointer selection is
        // observable without needing to fabricate their retail art.
        bus.WriteBytes(0x86ef04,
        [
            0x00, 0x00,
            0x00, 0xf1,
            0x10, 0xf1,
            0x20, 0xf1,
            0x30, 0xf1,
            0x40, 0xf1,
            0x00, 0x00,
        ]);
        for (int list = 0; list < 5; list++)
            bus.WriteBytes(0x86f100 + list * 0x10, [0xff, 0x7f, 0x00, 0x90]);

        // Shared blank tail: one blank frame, then EF10 respawn and 8154 delete. Most
        // checks inspect the tail immediately; this data also keeps a stepped tail valid.
        bus.WriteBytes(0x86eca3,
        [
            0x01, 0x00, 0x00, 0x00,
            0x10, 0xef,
            0x54, 0x81,
        ]);

        // Five generic-death list pointers. Variant zero deliberately selects F200.
        bus.WriteBytes(0x86efd5,
        [
            0x00, 0xf2,
            0x10, 0xf2,
            0x20, 0xf2,
            0x30, 0xf2,
            0x40, 0xf2,
        ]);
    }

    private static void WriteSingleDropChance(TestAddressSpace bus, EnemyPickupKind kind)
    {
        byte[] chances = new byte[6];
        int chanceIndex = kind switch
        {
            EnemyPickupKind.SmallEnergy => 0,
            EnemyPickupKind.BigEnergy => 1,
            EnemyPickupKind.Missile => 2,
            EnemyPickupKind.NoDrop => 3,
            EnemyPickupKind.SuperMissile => 4,
            EnemyPickupKind.PowerBomb => 5,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        chances[chanceIndex] = 0xff;
        bus.WriteBytes(0xb48000, chances);
    }

    private readonly record struct EnemyDropFixture(
        TestAddressSpace Bus,
        RoomEnemySystem System,
        RoomLevelData Level);
}
