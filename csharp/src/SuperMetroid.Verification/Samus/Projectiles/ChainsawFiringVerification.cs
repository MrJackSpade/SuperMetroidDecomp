using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Focused cartridge-probe comparison for Chainsaw shot admission, collision, lifetime and door interference.</summary>
internal static class ChainsawFiringVerification
{
    /// <summary>Checks the production firing and collision path against native-chainsaw-fire-probe, including bomb-owned slot ordering and the unstable charged callback.</summary>
    /// <remarks>This remains separately invokable for focused diagnostics and also runs in the default suite.</remarks>
    [PpuLayoutBoundary("exact hardware test: checks PPU register numbers")]
    internal static void VerifyChainsawFiring()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);
        var samus = new SamusState
        {
            CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            CollectedBeams = 0x100f,
            EquippedBeams = (ushort)(SamusBeamFlags.Wave | SamusBeamFlags.Spazer),
        };
        var pause = CreateRetailPauseFixture(bus, new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0) });
        EnterPauseEquipment(pause);
        SelectPauseBoots(pause);
        pause.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)0x000d, samus.EquippedBeams,
            "same-frame Boots Left+A equips Wave, Spazer, and Plasma without Ice");
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(128, 128));
        samus.SelectedHudItem = 0;
        var shared = CreateBombFixture();
        var projectiles = CreateProjectileFixture();
        projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = (ushort)SnesButton.X }, new(0, 0), shared);

        // A missing shot also leaves an empty slot, so checking deletion alone would
        // incorrectly pass the existing silent >=12 rejection in HandleBeamInput.
        AssertTrue(projectiles.LastFiredProjectileSnapshot is not null,
            "native Chainsaw admits and allocates the shot");
        AssertEqual(0, projectiles.SlotTable.ProjectileCounter, "inactive Power Bomb deletes Chainsaw on first update");
        AssertEqual(0, projectiles.Slots[0].InstructionPointer, "deleted Chainsaw has no animation list");
        AssertEqual(0, projectiles.Slots[0].Damage, "deleted Chainsaw releases its damage sentinel");
        AssertEqual((byte)0x34, projectiles.Chainsaw.ChainsawWindowRegisters.ReadByte(
            GameplayWindowRegisterAddresses.Window12Selection), "first callback stores inherited Y low byte");
        AssertEqual((byte)0x00, projectiles.Chainsaw.ChainsawWindowRegisters.ReadByte(
            GameplayWindowRegisterAddresses.Window34Selection), "first callback stores inherited Y high byte");

        // PowerBomb_Func3 clears the inactive shot, but the native callback still falls
        // through to the bank-$94 boundary dispatcher. A bombable origin with no PLM owner
        // makes that otherwise invisible call fail loudly through the production path.
        ushort[] reactiveWords = new ushort[256];
        reactiveWords[0] = (ushort)((int)RoomCollisionType.BombableBlock << 12);
        var reactiveLevel = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(reactiveWords, new byte[256], new ushort[256]), new byte[8]);
        var reactiveSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedBeams = 0x000d,
            SelectedHudItem = 0,
        };
        AssertThrows<InvalidOperationException>(() => CreateProjectileFixture().StepFrame(new SamusTerrain { Bus = bus, Level = reactiveLevel }, reactiveSamus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = (ushort)SnesButton.X }, new(0, 0), CreateBombFixture()), "cleared Chainsaw still reaches Power Bomb boundary reactions");

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
        var doorLevel = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(doorWords, doorBts, new ushort[256]), new byte[8]);
        var doorSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedBeams = 0x000d,
            SelectedHudItem = 0,
        };
        var doorPlms = new RoomPlmSystem();
        var doorProjectiles = CreateProjectileFixture();
        doorProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = doorLevel, Plms = doorPlms }, doorSamus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = (ushort)SnesButton.X }, new(0, 0), CreateBombFixture());
        AssertEqual(1, doorPlms.SlotTable.ActiveCount, "no-Power-Bomb Chainsaw opens blue door");
        AssertEqual(RoomCollisionType.SolidBlock,
            doorLevel.GetCollisionBlockByIndex(doorOrigin).CollisionType,
            "Chainsaw blue-door setup mutates cap synchronously");
        AssertTrue(doorProjectiles.Slots.All(slot =>
                !slot.PackedType.IsFamily(SamusProjectileFamily.BeamExplosion)),
            "Wave scan opens door without synthesizing a visible beam impact");
        AssertEqual(0, doorProjectiles.SlotTable.ProjectileCounter,
            "door-opening no-Power-Bomb Chainsaw still deletes in B0AC callback");

        var activeShared = CreateBombFixture();
        activeShared.PowerBombExplosion.Arm();
        var activeProjectiles = CreateProjectileFixture();
        ushort[] nextLists = [0x902f, 0x9037, 0x903f, 0x9047, 0x904f, 0x9057, 0x905f, 0x9067];
        ushort[] spritemaps = [0xaf4c, 0xaf62, 0xaf78, 0xafa2, 0xafcc, 0xaff6, 0xb020, 0xb04a];
        ushort[] yRadii = [12, 12, 16, 16, 20, 20, 23, 23];
        ushort[] inheritedY = [0x0034, 0x9027, 0x902f, 0x9037, 0x903f, 0x9047, 0x904f, 0x9057];
        for (int update = 0; update < nextLists.Length; update++)
        {
            activeProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, new ProjectileButtonInput { ControllerInput = update == 0 ? (ushort)SnesButton.X : (ushort)0, ControllerNewInput = update == 0 ? (ushort)SnesButton.X : (ushort)0 }, new(0, 0), activeShared);
            SamusProjectileSlot slot = activeProjectiles.Slots[0];
            if (update == 0)
                AssertTrue(activeProjectiles.LastFiredProjectileSnapshot is not null && slot.IsActive,
                    "active-Power-Bomb Chainsaw allocation");
            AssertEqual(nextLists[update], slot.InstructionPointer, $"active Chainsaw next list {update}");
            AssertEqual(spritemaps[update], slot.SpritemapPointer, $"active Chainsaw spritemap {update}");
            AssertEqual((ushort)8, slot.Radius.X, $"active Chainsaw X radius {update}");
            AssertEqual(yRadii[update], slot.Radius.Y, $"active Chainsaw Y radius {update}");
            AssertEqual((byte)inheritedY[update], activeProjectiles.Chainsaw.ChainsawWindowRegisters.ReadByte(
                GameplayWindowRegisterAddresses.Window12Selection), $"active Chainsaw inherited Y low {update}");
            AssertEqual(SnesLittleEndian.HighByte(inheritedY[update]), activeProjectiles.Chainsaw.ChainsawWindowRegisters.ReadByte(
                GameplayWindowRegisterAddresses.Window34Selection), $"active Chainsaw inherited Y high {update}");
        }

        // Combination thirteen indexes the same vulnerability byte as Super Missiles.
        // The default native vulnerability record gives Super Missiles multiplier two.
        // Resolve a real production overlap through that compiled record. Native common
        // damage halves the slot's 150 damage word before applying the multiplier,
        // producing a 150-point hit.
        var combatShared = CreateBombFixture();
        combatShared.PowerBombExplosion.Arm();
        var combatProjectiles = CreateProjectileFixture();
        combatProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = (ushort)SnesButton.X }, new(0, 0), combatShared);
        var combat = CreateEnemyDropFixture(samus, [1]);
        RoomEnemySlot target = combat.System.SlotTable.Slots[0];
        target.EnemyDefinitionPointer = (EnemyDefinitionId)0x9000;
        target.Definition = default(RoomEnemyDefinition) with
        {
            Bank = 0xa3,
            ShotAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyShot,
            VulnerabilityPointer = EnemyVulnerabilityDefinitions.DefaultPointer,
        };
        target.Position = target.Position.WithWhole(new(139, 123));
        target.Radius = new(16, 16);
        target.Health = 1000;
        target.SpritemapPointer = 0x8000;
        combat.System.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Samus = samus, Level = combat.Level });
        AssertEqual(1, combat.System.Collision.ResolveOrdinaryProjectileHits(
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
        const int yellowDoorBlock = 1 * 5 + 2;
        const int bombableBlock = 2 * 5 + 1;
        TestAddressSpace interferenceBus = CreateChainsawInterferenceCartridge(yellowDoorPopulation);

        ushort[] interferenceWords = new ushort[25];
        byte[] interferenceBts = new byte[25];
        interferenceWords[yellowDoorBlock] = 0x8000;
        var interferenceLevel = new RoomLevelData(new Extent16<RoomBlocks>(5, 5), new RoomLevelPlanes(interferenceWords, interferenceBts, new ushort[25]), new byte[8]);
        var interferencePlms = new RoomPlmSystem();
        var interferenceStreamer = interferenceLevel.CreateBackgroundStreamer();
        AssertEqual(1, interferencePlms.PopulationLoader.LoadRoomPopulation(new PlmRoom(interferenceBus, interferenceLevel, interferenceStreamer, new SnesVram(), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(interferenceBus, yellowDoorPopulation), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "Chainsaw interference fixture loads the resident yellow door");
        interferencePlms.Step(interferenceBus, interferenceLevel, interferenceStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });

        var interferenceReactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            interferenceLevel,
            new(32, 32),
            0x1000,
            interferenceReactions,
            interferencePlms,
            AreaId.Crateria,
            (ushort)SamusProjectileFamily.PowerBomb);
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            interferenceLevel,
            new(32, 32),
            0x1000,
            interferenceReactions,
            interferencePlms,
            AreaId.Crateria,
            0x800d);
        interferencePlms.Step(interferenceBus, interferenceLevel, interferenceStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        ColoredDoorPlmSnapshot suppressedDoor = interferencePlms.ColoredDoors.Single();
        AssertEqual(ColoredDoorPhase.Waiting, suppressedDoor.Phase,
            "later Chainsaw hit suppresses yellow-door opening");
        AssertEqual((byte)0, suppressedDoor.HitCounter,
            "suppressed yellow door never advances its opening threshold");

        ushort[] breakingWords = new ushort[25];
        breakingWords[bombableBlock] =
            (ushort)((int)RoomCollisionType.BombableBlock << 12);
        var breakingLevel = new RoomLevelData(new Extent16<RoomBlocks>(5, 5), new RoomLevelPlanes(breakingWords, new byte[25], new ushort[25]), new byte[0x2000]);
        var breakingPlms = new RoomPlmSystem();
        var breakingReactions = new List<BombBlockReaction>();
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            breakingLevel,
            new(32, 32),
            0x1000,
            breakingReactions,
            breakingPlms,
            AreaId.Crateria,
            (ushort)SamusProjectileFamily.PowerBomb);
        SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(
            breakingLevel,
            new(32, 32),
            0x1000,
            breakingReactions,
            breakingPlms,
            AreaId.Crateria,
            0x800d);
        AssertEqual(RoomCollisionType.SolidBlock,
            breakingLevel.GetCollisionBlockByIndex(bombableBlock).CollisionType,
            "Chainsaw callback preserves the Power Bomb's synchronous break setup");
        AssertEqual(1, breakingPlms.SlotTable.ActiveCount,
            "Power Bomb terrain reaction remains owned after Chainsaw callback");
        breakingPlms.Step(bus, breakingLevel, breakingLevel.CreateBackgroundStreamer(), new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
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
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedBeams = 0x100d,
        };
        var chargedShared = CreateBombFixture();
        var chargedProjectiles = CreateProjectileFixture();
        for (int frame = 0; frame < 60; frame++)
        {
            chargedShared.StepFrame(new SamusTerrain { Bus = bus, Level = level }, chargedSamus, 0, 0);
            chargedProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = level }, chargedSamus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = frame == 0 ? (ushort)SnesButton.X : (ushort)0 }, new(0, 0), chargedShared);
        }
        chargedShared.StepFrame(new SamusTerrain { Bus = bus, Level = level }, chargedSamus, 0, 0);
        NotSupportedException chargedFault = AssertThrows<NotSupportedException>(
            () => chargedProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = level }, chargedSamus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, new(0, 0), chargedShared),
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
    }

    /// <summary>
    /// Builds the Orange-Door interference cartridge: a synthetic bank-$84 yellow-door PLM
    /// program and its draw lists, plus one bank-$8F population entry at <paramref name="population"/>.
    /// </summary>
    [CpuBusBoundary("exact bus test: seeds the synthetic yellow-door PLM program and room population for the Chainsaw interference fixture")]
    private static TestAddressSpace CreateChainsawInterferenceCartridge(ushort population)
    {
        const ushort yellowDoorArgument = 0xffff;
        var interferenceBus = new TestAddressSpace();
        WriteTestWord(interferenceBus, 0x84c85c, 0x0400);
        WriteTestWord(interferenceBus, 0x840402, 0x0500);
        WriteTestWord(interferenceBus, 0x840406, 0x0600);
        WriteTestWord(interferenceBus, 0x84040e, 0x0700);
        interferenceBus.WriteByte(0x840602, 1);
        WriteTestWord(interferenceBus, 0x840603, 0x0800);
        WriteTestWord(interferenceBus, 0x840605, 1);
        WriteTestWord(interferenceBus, 0x840607, 0x0720);
        WriteTestWord(interferenceBus, 0x840800, 12);
        WriteTestWord(interferenceBus, 0x840802, 0x0740);
        WriteTestWord(interferenceBus, 0x840505, 0x0760);
        WriteDoorDrawList(interferenceBus, 0x840700, [0x0001, 0xc000, 0x0000]);
        WriteDoorDrawList(interferenceBus, 0x840720, [0x0001, 0xc000, 0x0000]);
        WriteDoorDrawList(interferenceBus, 0x840740, [0x0001, 0xc000, 0x0000]);
        WriteDoorDrawList(interferenceBus, 0x840760, [0x0001, 0x8000, 0x0000]);
        WriteTestWord(
            interferenceBus,
            0x8f0000 | population,
            (ushort)PlmHeaderId.YellowDoorFacingLeft);
        interferenceBus.WriteByte(0x8f0000 | (population + 2), 2);
        interferenceBus.WriteByte(0x8f0000 | (population + 3), 1);
        WriteTestWord(
            interferenceBus,
            0x8f0000 | (population + 4),
            yellowDoorArgument);
        WriteTestWord(interferenceBus, 0x8f0000 | (population + 6), 0);
        return interferenceBus;

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

    /// <summary>Runs the focused Chainsaw firing comparison from the command line.</summary>
    [VerificationCommand("--chainsaw-firing")]
    internal static int ChainsawFiringCommand()
    {
        Suite(nameof(VerifyChainsawFiring), () => VerifyChainsawFiring());
        return 0;
    }
}
