using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // Expected values come from native-chainsaw-fire-probe, not from another C# path.
    // This remains separately invokable for focused diagnostics and also runs in the
    // default suite now that the cartridge-backed firing/lifetime slice is implemented.
    private static void VerifyChainsawFiring()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(16, 16, new ushort[256], new byte[256],
            new ushort[256], new byte[8]);
        var samus = new SamusState
        {
            CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
        };
        var pause = new PauseMenuState(
            bus,
            samus,
            new Bank80SystemState(),
            AreaId.Crateria,
            0,
            0);
        samus.CollectedBeams = 0x100f;
        samus.EquippedBeams = (ushort)(SamusBeamFlags.Wave | SamusBeamFlags.Spazer);
        pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int frame = 0; frame < 32; frame++)
            pause.Step(0, 0);
        pause.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)0x000d, samus.EquippedBeams,
            "same-frame Boots Left+A equips Wave, Spazer, and Plasma without Ice");
        samus.Pose = 1;
        samus.XPosition = 128;
        samus.YPosition = 128;
        samus.SelectedHudItem = 0;
        var shared = new SamusBombProjectileSystem();
        var projectiles = new SamusProjectileSystem();
        var result = projectiles.StepFrame(bus, level, samus,
            (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, shared);

        // A missing shot also leaves an empty slot, so checking deletion alone would
        // incorrectly pass the existing silent >=12 rejection in HandleBeamInput.
        AssertEqual((int?)0, result.FiredSlot, "native Chainsaw admits and allocates the shot");
        var spawn = projectiles.LastFiredProjectileSnapshot
            ?? throw new InvalidOperationException("Chainsaw firing has no producer snapshot.");
        AssertEqual(139, spawn.XPosition, "native Chainsaw muzzle X before its callback");
        AssertEqual(123, spawn.YPosition, "native Chainsaw muzzle Y before its callback");
        AssertEqual(-64, spawn.XVelocity, "native Chainsaw initial speed overread");
        AssertEqual(0, spawn.YVelocity, "native Chainsaw initial vertical speed");
        AssertEqual(0, projectiles.ProjectileCounter, "inactive Power Bomb deletes Chainsaw on first update");
        AssertEqual(0, projectiles.Slots[0].InstructionPointer, "deleted Chainsaw has no animation list");
        AssertEqual(0, projectiles.Slots[0].Damage, "deleted Chainsaw releases its damage sentinel");
        AssertEqual((byte)0x34, projectiles.ChainsawWindowRegisters.ReadByte(
            GameplayWindowRegisterAddresses.Window12Selection), "first callback stores inherited Y low byte");
        AssertEqual((byte)0x00, projectiles.ChainsawWindowRegisters.ReadByte(
            GameplayWindowRegisterAddresses.Window34Selection), "first callback stores inherited Y high byte");

        // PowerBomb_Func3 clears the inactive shot, but the native callback still falls
        // through to the bank-$94 boundary dispatcher. A bombable origin with no PLM owner
        // makes that otherwise invisible call fail loudly through the production path.
        ushort[] reactiveWords = new ushort[256];
        reactiveWords[0] = (ushort)((int)RoomCollisionType.BombableBlock << 12);
        var reactiveLevel = new RoomLevelData(16, 16, reactiveWords, new byte[256],
            new ushort[256], new byte[8]);
        var reactiveSamus = new SamusState
        {
            Pose = 1,
            XPosition = 128,
            YPosition = 128,
            EquippedBeams = 0x000d,
            SelectedHudItem = 0,
        };
        AssertThrows<InvalidOperationException>(() => new SamusProjectileSystem().StepFrame(
            bus,
            reactiveLevel,
            reactiveSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            new SamusBombProjectileSystem()), "cleared Chainsaw still reaches Power Bomb boundary reactions");

        // Wave collision scans do not kill their owner, but they still run every touched
        // block's shot reaction before the B0AC callback deletes the no-Power-Bomb shot.
        // This is how an invisible, zero-range Chainsaw shot opens a blue door.
        ushort[] doorWords = new ushort[256];
        byte[] doorBts = new byte[256];
        int doorOrigin = 6 * 16 + 9;
        doorWords[doorOrigin] = (ushort)((int)RoomCollisionType.ShootableBlock << 12);
        doorBts[doorOrigin] = 0x41;
        for (int row = 1; row < 4; row++)
        {
            doorWords[doorOrigin + row * 16] =
                (ushort)((int)RoomCollisionType.VerticalExtension << 12);
            doorBts[doorOrigin + row * 16] = unchecked((byte)-row);
        }
        var doorLevel = new RoomLevelData(16, 16, doorWords, doorBts,
            new ushort[256], new byte[8]);
        var doorSamus = new SamusState
        {
            Pose = 1,
            XPosition = 128,
            YPosition = 128,
            EquippedBeams = 0x000d,
            SelectedHudItem = 0,
        };
        var doorPlms = new RoomPlmSystem();
        var doorProjectiles = new SamusProjectileSystem();
        SamusProjectileFrameResult doorFrame = doorProjectiles.StepFrame(
            bus,
            doorLevel,
            doorSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            new SamusBombProjectileSystem(),
            roomPlms: doorPlms);
        AssertEqual(1, doorPlms.ActiveCount, "no-Power-Bomb Chainsaw opens blue door");
        AssertEqual(RoomCollisionType.SolidBlock,
            doorLevel.GetCollisionBlockByIndex(doorOrigin).CollisionType,
            "Chainsaw blue-door setup mutates cap synchronously");
        AssertTrue(!doorFrame.CollisionStartedExplosion,
            "Wave scan opens door without synthesizing a visible beam impact");
        AssertEqual(0, doorProjectiles.ProjectileCounter,
            "door-opening no-Power-Bomb Chainsaw still deletes in B0AC callback");

        var activeShared = new SamusBombProjectileSystem();
        activeShared.PowerBombExplosion.Arm();
        var activeProjectiles = new SamusProjectileSystem();
        ushort[] nextLists = [0x902f, 0x9037, 0x903f, 0x9047, 0x904f, 0x9057, 0x905f, 0x9067];
        ushort[] spritemaps = [0xaf4c, 0xaf62, 0xaf78, 0xafa2, 0xafcc, 0xaff6, 0xb020, 0xb04a];
        ushort[] yRadii = [12, 12, 16, 16, 20, 20, 23, 23];
        ushort[] inheritedY = [0x0034, 0x9027, 0x902f, 0x9037, 0x903f, 0x9047, 0x904f, 0x9057];
        for (int update = 0; update < nextLists.Length; update++)
        {
            SamusProjectileFrameResult frame = activeProjectiles.StepFrame(
                bus,
                level,
                samus,
                update == 0 ? (ushort)SnesButton.X : (ushort)0,
                update == 0 ? (ushort)SnesButton.X : (ushort)0,
                0,
                0,
                activeShared);
            if (update == 0)
                AssertEqual((int?)0, frame.FiredSlot, "active-Power-Bomb Chainsaw allocation");
            SamusProjectileSlot slot = activeProjectiles.Slots[0];
            AssertEqual(nextLists[update], slot.InstructionPointer, $"active Chainsaw next list {update}");
            AssertEqual(spritemaps[update], slot.SpritemapPointer, $"active Chainsaw spritemap {update}");
            AssertEqual((ushort)8, slot.XRadius, $"active Chainsaw X radius {update}");
            AssertEqual(yRadii[update], slot.YRadius, $"active Chainsaw Y radius {update}");
            AssertEqual((byte)inheritedY[update], activeProjectiles.ChainsawWindowRegisters.ReadByte(
                GameplayWindowRegisterAddresses.Window12Selection), $"active Chainsaw inherited Y low {update}");
            AssertEqual((byte)(inheritedY[update] >> 8), activeProjectiles.ChainsawWindowRegisters.ReadByte(
                GameplayWindowRegisterAddresses.Window34Selection), $"active Chainsaw inherited Y high {update}");
        }

        // Combination thirteen indexes the same vulnerability byte as Super Missiles.
        // The default native vulnerability record gives Super Missiles multiplier two.
        // Resolve a real production overlap through that compiled record. Native common
        // damage halves the slot's 150 damage word before applying the multiplier,
        // producing a 150-point hit.
        var combatShared = new SamusBombProjectileSystem();
        combatShared.PowerBombExplosion.Arm();
        var combatProjectiles = new SamusProjectileSystem();
        combatProjectiles.StepFrame(bus, level, samus,
            (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, combatShared);
        var combat = CreateEnemyDropFixture(samus, [1]);
        RoomEnemySlot target = combat.System.Slots[0];
        target.EnemyDefinitionPointer = 0x9000;
        target.Definition = default(RoomEnemyDefinition) with
        {
            Bank = 0xa3,
            ShotAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyShot,
            VulnerabilityPointer = EnemyVulnerabilityDefinitions.DefaultPointer,
        };
        target.XPosition = 139;
        target.YPosition = 123;
        target.XRadius = target.YRadius = 16;
        target.Health = 1000;
        target.SpritemapPointer = 0x8000;
        combat.System.StepFrame(0, 0, timeIsFrozen: true, samus, level: combat.Level);
        AssertEqual(1, combat.System.ResolveOrdinaryProjectileHits(
            combat.Bus, combatProjectiles, combatShared, samus),
            "active Chainsaw overlaps one ordinary enemy");
        AssertEqual((ushort)850, target.Health,
            "Chainsaw uses Super-Missile vulnerability byte for a 150-point hit");
        AssertEqual((ushort)0x800d, combatProjectiles.Slots[0].Type,
            "Plasma bit keeps Chainsaw alive after enemy damage");

        // The documented Orange Door interference is an ordering effect, not a special
        // door exception. Native processes the bomb-owned slots before the ordinary
        // Chainsaw slot. The real Power Bomb boundary visit therefore breaks terrain and
        // publishes family $0300 to the yellow-door actor; the following Chainsaw visit
        // observes the already-cleared terrain but overwrites that door's pending timer
        // with its beam-family word. The resident actor consumes only the last published
        // hit and rejects it. Reproduce both boundary walkers in that exact order.
        const ushort yellowDoorPopulation = 0x9300;
        const ushort yellowDoorArgument = 0xffff;
        const int yellowDoorBlock = 1 * 5 + 2;
        const int bombableBlock = 2 * 5 + 1;
        var interferenceBus = new TestAddressSpace();
        WriteTestWord(interferenceBus, 0x84c85c, 0xe000);
        WriteTestWord(interferenceBus, 0x84e002, 0xe100);
        WriteTestWord(interferenceBus, 0x84e006, 0xe200);
        WriteTestWord(interferenceBus, 0x84e00e, 0xe300);
        interferenceBus.WriteByte(0x84e202, 1);
        WriteTestWord(interferenceBus, 0x84e203, 0xe400);
        WriteTestWord(interferenceBus, 0x84e205, 1);
        WriteTestWord(interferenceBus, 0x84e207, 0xe320);
        WriteTestWord(interferenceBus, 0x84e400, 12);
        WriteTestWord(interferenceBus, 0x84e402, 0xe340);
        WriteTestWord(interferenceBus, 0x84e105, 0xe360);
        WriteDoorDrawList(interferenceBus, 0x84e300, [0x0001, 0xc000, 0x0000]);
        WriteDoorDrawList(interferenceBus, 0x84e320, [0x0001, 0xc000, 0x0000]);
        WriteDoorDrawList(interferenceBus, 0x84e340, [0x0001, 0xc000, 0x0000]);
        WriteDoorDrawList(interferenceBus, 0x84e360, [0x0001, 0x8000, 0x0000]);
        WriteTestWord(
            interferenceBus,
            0x8f0000 | yellowDoorPopulation,
            RoomPlmHeaders.YellowDoorFacingLeft);
        interferenceBus.WriteByte(0x8f0000 | (yellowDoorPopulation + 2), 2);
        interferenceBus.WriteByte(0x8f0000 | (yellowDoorPopulation + 3), 1);
        WriteTestWord(
            interferenceBus,
            0x8f0000 | (yellowDoorPopulation + 4),
            yellowDoorArgument);
        WriteTestWord(interferenceBus, 0x8f0000 | (yellowDoorPopulation + 6), 0);

        ushort[] interferenceWords = new ushort[25];
        byte[] interferenceBts = new byte[25];
        interferenceWords[yellowDoorBlock] = 0x8000;
        var interferenceLevel = new RoomLevelData(
            5,
            5,
            interferenceWords,
            interferenceBts,
            new ushort[25],
            new byte[8]);
        var interferencePlms = new RoomPlmSystem();
        var interferenceStreamer = interferenceLevel.CreateBackgroundStreamer();
        AssertEqual(1, interferencePlms.LoadRoomPopulation(
                interferenceBus,
                interferenceLevel,
                interferenceStreamer,
                new SnesVram(),
                yellowDoorPopulation,
                new Bank80SystemState(),
                AreaId.Crateria,
                () => new SamusState(),
                () => false),
            "Chainsaw interference fixture loads the resident yellow door");
        interferencePlms.Step(
            interferenceBus,
            interferenceLevel,
            interferenceStreamer,
            0,
            0,
            0);

        var interferenceReactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            interferenceLevel,
            32,
            32,
            0x1000,
            interferenceReactions,
            interferencePlms,
            AreaId.Crateria,
            (ushort)SamusProjectileFamily.PowerBomb);
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            interferenceLevel,
            32,
            32,
            0x1000,
            interferenceReactions,
            interferencePlms,
            AreaId.Crateria,
            0x800d);
        interferencePlms.Step(
            interferenceBus,
            interferenceLevel,
            interferenceStreamer,
            0,
            0,
            0);
        ColoredDoorPlmSnapshot suppressedDoor = interferencePlms.ColoredDoors.Single();
        AssertEqual(ColoredDoorPhase.Waiting, suppressedDoor.Phase,
            "later Chainsaw hit suppresses yellow-door opening");
        AssertEqual((byte)0, suppressedDoor.HitCounter,
            "suppressed yellow door never advances its opening threshold");

        ushort[] breakingWords = new ushort[25];
        breakingWords[bombableBlock] =
            (ushort)((int)RoomCollisionType.BombableBlock << 12);
        var breakingLevel = new RoomLevelData(
            5,
            5,
            breakingWords,
            new byte[25],
            new ushort[25],
            new byte[0x2000]);
        var breakingPlms = new RoomPlmSystem();
        var breakingReactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            breakingLevel,
            32,
            32,
            0x1000,
            breakingReactions,
            breakingPlms,
            AreaId.Crateria,
            (ushort)SamusProjectileFamily.PowerBomb);
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            breakingLevel,
            32,
            32,
            0x1000,
            breakingReactions,
            breakingPlms,
            AreaId.Crateria,
            0x800d);
        AssertEqual(RoomCollisionType.SolidBlock,
            breakingLevel.GetCollisionBlockByIndex(bombableBlock).CollisionType,
            "Chainsaw callback preserves the Power Bomb's synchronous break setup");
        AssertEqual(1, breakingPlms.ActiveCount,
            "Power Bomb terrain reaction remains owned after Chainsaw callback");
        breakingPlms.Step(
            bus,
            breakingLevel,
            breakingLevel.CreateBackgroundStreamer(),
            0,
            0,
            0);
        AssertEqual(RoomCollisionType.Air,
            breakingLevel.GetCollisionBlockByIndex(bombableBlock).CollisionType,
            "Power Bomb terrain completes its visible break after Chainsaw callback");

        // The charged table has only twelve authored entries. Combination thirteen reads
        // word $0A0A from FireChargedBeam's following instruction bytes, installs it, and
        // reaches that callback in the same descending projectile pass. On hardware the
        // bank-$90 JSR maps to mutable WRAM $7E:0A0A; the exact opcode stream depends on the
        // cached previous-Super-Missile word. The semantic port must preserve this unstable
        // destination and fail loudly rather than silently choosing ordinary Wave motion.
        var chargedSamus = new SamusState
        {
            Pose = 1,
            XPosition = 128,
            YPosition = 128,
            EquippedBeams = 0x100d,
        };
        var chargedShared = new SamusBombProjectileSystem();
        var chargedProjectiles = new SamusProjectileSystem();
        for (int frame = 0; frame < 60; frame++)
        {
            chargedShared.StepFrame(bus, level, chargedSamus, 0, 0);
            chargedProjectiles.StepFrame(
                bus,
                level,
                chargedSamus,
                (ushort)SnesButton.X,
                frame == 0 ? (ushort)SnesButton.X : (ushort)0,
                0,
                0,
                chargedShared);
        }
        chargedShared.StepFrame(bus, level, chargedSamus, 0, 0);
        NotSupportedException chargedFault = AssertThrows<NotSupportedException>(
            () => chargedProjectiles.StepFrame(
                bus,
                level,
                chargedSamus,
                0,
                0,
                0,
                0,
                chargedShared),
            "charged Chainsaw reaches mutable low-WRAM execution");
        AssertTrue(chargedFault.Message.Contains("$90:0A0A", StringComparison.Ordinal) &&
            chargedFault.Message.Contains("$7E:0A0A", StringComparison.Ordinal),
            "charged Chainsaw diagnostic identifies dispatcher and WRAM mirror");
        AssertEqual(SamusProjectilePreInstruction.ChargedChainsawLowWramExecution,
            chargedProjectiles.Slots[0].PreInstruction,
            "charged Chainsaw retains its native unstable callback identity");
        AssertEqual((ushort)0x901d, chargedProjectiles.Slots[0].Type,
            "charged Chainsaw is fully allocated before unstable callback execution");

        Console.WriteLine("Chainsaw firing: native admission, door/enemy reactions, Orange-Door interference, callback stores, Power-Bomb-gated lifetime and charged WRAM execution agree.");

        static void WriteDoorDrawList(
            TestAddressSpace targetBus,
            int address,
            ushort[] words)
        {
            foreach (ushort word in words)
            {
                WriteTestWord(targetBus, address, word);
                address += 2;
            }
        }
    }
}
