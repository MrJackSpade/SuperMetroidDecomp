using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Pipe Bugs' initial animations and that each of their sixteen loops runs through the real behavior.</summary>
internal static class PipeBugBehaviorVerification
{
    /// <summary>
    /// The Zeb, Zebbo, Gamet and Geega initializers start their native first loops, and every loop installed through
    /// <see cref="PipeBugsBehavior.StartPipeBugAnimation"/> runs one pass and returns to its first pose on time.
    /// </summary>
    internal static void VerifyPipeBugBehavior()
    {
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.ZebRisingLeft), Initialized(EnemyDefinitionId.Zeb, 0).Playback.Animation, "Zeb starts rising left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.ZebboRisingLeft), Initialized(EnemyDefinitionId.Zebbo, 1).Playback.Animation, "Zebbo starts rising left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.GametRisingLeft), Initialized(EnemyDefinitionId.Gamet, 0).Playback.Animation, "Gamet starts rising left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.GeegaFlyingLeft), Initialized(EnemyDefinitionId.Geega, 1).Playback.Animation, "Geega with parameter one starts flying left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.GeegaFlyingRight), Initialized(EnemyDefinitionId.Geega, 0).Playback.Animation, "Geega with parameter zero starts flying right");

        foreach (PipeBugSegment program in Enum.GetValues<PipeBugSegment>())
        {
            (EnemyDefinitionId definition, ushort parameter) = program.ToString() switch
            {
                var name when name.StartsWith("Zebbo", StringComparison.Ordinal) => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Zebbo, 1),
                var name when name.StartsWith("Zeb", StringComparison.Ordinal) => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Zeb, 0),
                var name when name.StartsWith("Gamet", StringComparison.Ordinal) => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Gamet, 0),
                _ => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Geega, 0),
            };
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PipeBugEnemyState state = Initialize(enemies, slot, definition, parameter);
            PipeBugsBehavior.StartPipeBugAnimation(slot, state, program);
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(PipeBugAnimations.Segments, program, PipeBugFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(PipeBugAnimations.Segments, program)),
                $"Pipe Bug {program} loop");
        }
        Console.WriteLine("  Pipe Bug behavior: four initializers start their native loops and all sixteen loops run on time.");
    }

    /// <summary>
    /// Drives the production main AI through the Pipe Bugs' typed state. A Norfair member waiting for its stagger target
    /// of 72 counts one per update; reaching it departs toward Samus and clears the count. A Brinstar bug
    /// keeps its flight direction in variable A's sign bit, leaving the word's other bits alone, and emerges facing the
    /// side Samus is on. A yellow bug flies at its 1.5-pixel 16.16 speed, and its arc counts the speed record down to
    /// zero, ends the rise without going below it, then counts back up.
    /// </summary>
    internal static void VerifyPipeBugStateQuantities()
    {
        foreach ((int before, int after, bool departs) in new[] { new VerifyPipeBugStateQuantitiesCase(70, 71, false), new VerifyPipeBugStateQuantitiesCase(71, 0, true) })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            PipeBugEnemyState state = Initialize(enemies, slot, EnemyDefinitionId.Gamet, 0);
            state.Action = PipeBugAction.NorfairWaitForStagger;
            state.StaggerTarget = 72;
            state.FormationCounter = before;
            enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(200, 128)));
            AssertEqual(after, state.FormationCounter, $"Norfair Pipe Bug count after {before}");
            AssertEqual(departs ? PipeBugAction.NorfairFlyRight : PipeBugAction.NorfairWaitForStagger, state.Action,
                $"Norfair Pipe Bug departs only on reaching its target (count {before})");
        }

        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            PipeBugEnemyState state = Initialize(enemies, slot, EnemyDefinitionId.Zeb, 0);
            state.Action = PipeBugAction.BrinstarWaitForSamus;
            enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(28, 78)));
            AssertTrue(state.BrinstarFliesLeft, "Brinstar Pipe Bug flies left of Samus on her left");
            AssertEqual(PipeBugAction.BrinstarWaitForSamus, state.Action, "Brinstar Pipe Bug waits while Samus is 100 pixels away");
            enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(148, 78)));
            AssertTrue(!state.BrinstarFliesLeft, "Brinstar Pipe Bug flies right toward Samus on her right");
            AssertEqual(PipeBugAction.BrinstarEmerge, state.Action, "Brinstar Pipe Bug emerges for Samus 20 pixels away");
            AssertEqual(PipeBugSegment.ZebRisingRight, state.RequestedAnimation, "Brinstar Pipe Bug emerges facing right");
        }

        {
            const int onePixel = 65536;
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            slot.SetSpawnParameter2(EnemyDefinitionId.Geega, 24);
            PipeBugEnemyState state = Initialize(enemies, slot, EnemyDefinitionId.Geega, 0);
            AssertEqual(SixteenSixteenWords.ToDisplacement(onePixel + onePixel / 2), state.FlightSpeed, "yellow Pipe Bug speed record 24 is 1.5 pixels");
            state.Action = PipeBugAction.YellowArcRight;
            state.ArcRising = true;
            state.ArcCounter = QuadraticSpeedCurvePlace.FromMarks(1);
            PipeBugBehaviorExpected[] expected = [new PipeBugBehaviorExpected(0, true), new PipeBugBehaviorExpected(0, false), new PipeBugBehaviorExpected(1, false)];
            for (int update = 0; update < expected.Length; update++)
            {
                enemies.PipeBugsBehavior.RunPipeBugsMainAi(slot, Frame(new(200, 128)));
                AssertEqual(SixteenSixteenWords.ToCoordinate(checked((uint)(128 * onePixel + (update + 1) * (onePixel + onePixel / 2)))), slot.Position.X,
                    $"yellow Pipe Bug arc update {update} carries its 16.16 speed");
                AssertEqual(QuadraticSpeedCurvePlace.FromMarks(expected[update].Counter), state.ArcCounter, $"yellow Pipe Bug arc record after update {update}");
                if (update < 2)
                    AssertEqual(expected[update].Rising, state.ArcRising, $"yellow Pipe Bug arc phase after update {update}");
            }
        }
        Console.WriteLine("  Pipe Bug state: the formation count wraps and compares by sign, the direction bit stands alone, and the arc record never goes below zero.");

        static ClockedEnemyAiFrame Frame(Position16<RoomPixels> samusPosition) => new(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(samusPosition) } },
            new FrameCounters(0));
    }

    /// <summary>A freshly initialized Pipe Bug's state.</summary>
    private static PipeBugEnemyState Initialized(EnemyDefinitionId definition, ushort parameter)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        return Initialize(enemies, enemies.SlotTable.Slots[0], definition, parameter);
    }

    /// <summary>Runs <paramref name="definition"/>'s real initializer on <paramref name="slot"/>.</summary>
    private static PipeBugEnemyState Initialize(RoomEnemySystem enemies, RoomEnemySlot slot, EnemyDefinitionId definition, ushort parameter)
    {
        slot.Bind(definition, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(definition, parameter);
        switch (definition)
        {
            case EnemyDefinitionId.Zeb or EnemyDefinitionId.Zebbo:
                PipeBugsBehavior.InitializeBrinstarPipeBug(slot);
                break;
            case EnemyDefinitionId.Gamet:
                enemies.PipeBugsBehavior.InitializeNorfairPipeBug(slot);
                break;
            case EnemyDefinitionId.None:
            case EnemyDefinitionId.Boyon:
            case EnemyDefinitionId.MamaTurtle:
            case EnemyDefinitionId.BabyTurtle:
            case EnemyDefinitionId.Puyo:
            case EnemyDefinitionId.Cacatac:
            case EnemyDefinitionId.Owtch:
            case EnemyDefinitionId.ShipTop:
            case EnemyDefinitionId.ShipBottomEntrance:
            case EnemyDefinitionId.Mellow:
            case EnemyDefinitionId.Mella:
            case EnemyDefinitionId.Menu:
            case EnemyDefinitionId.Multiviola:
            case EnemyDefinitionId.LavaRocks:
            case EnemyDefinitionId.Rinka:
            case EnemyDefinitionId.Rio:
            case EnemyDefinitionId.Squeept:
            case EnemyDefinitionId.Geruta:
            case EnemyDefinitionId.Holtz:
            case EnemyDefinitionId.Oum:
            case EnemyDefinitionId.Choot:
            case EnemyDefinitionId.GRipper:
            case EnemyDefinitionId.Ripper2:
            case EnemyDefinitionId.Ripper:
            case EnemyDefinitionId.Dragon:
            case EnemyDefinitionId.ShutterGrowing:
            case EnemyDefinitionId.ShutterShootable:
            case EnemyDefinitionId.ShutterDestroyable:
            case EnemyDefinitionId.Kamer:
            case EnemyDefinitionId.Waver:
            case EnemyDefinitionId.Metaree:
            case EnemyDefinitionId.Fireflea:
            case EnemyDefinitionId.Skultera:
            case EnemyDefinitionId.Elevator:
            case EnemyDefinitionId.Sciser:
            case EnemyDefinitionId.Zero:
            case EnemyDefinitionId.Tripper:
            case EnemyDefinitionId.Kamer2:
            case EnemyDefinitionId.Sbug:
            case EnemyDefinitionId.Mochtroid:
            case EnemyDefinitionId.Sidehopper:
            case EnemyDefinitionId.Dessgeega:
            case EnemyDefinitionId.SidehopperLarge:
            case EnemyDefinitionId.SidehopperTourian:
            case EnemyDefinitionId.DessgeegaLarge:
            case EnemyDefinitionId.Zoa:
            case EnemyDefinitionId.Viola:
            case EnemyDefinitionId.Skree:
            case EnemyDefinitionId.Yard:
            case EnemyDefinitionId.HZoomer:
            case EnemyDefinitionId.Zeela:
            case EnemyDefinitionId.Sova:
            case EnemyDefinitionId.Zoomer:
            case EnemyDefinitionId.MZoomer:
            case EnemyDefinitionId.Metroid:
            case EnemyDefinitionId.Crocomire:
            case EnemyDefinitionId.CrocomireTongue:
            case EnemyDefinitionId.DraygonBody:
            case EnemyDefinitionId.DraygonEye:
            case EnemyDefinitionId.DraygonTail:
            case EnemyDefinitionId.DraygonArms:
            case EnemyDefinitionId.SporeSpawn:
            case EnemyDefinitionId.SporeSpawnStalk:
            case EnemyDefinitionId.Boulder:
            case EnemyDefinitionId.KzanTop:
            case EnemyDefinitionId.KzanBottom:
            case EnemyDefinitionId.Hibashi:
            case EnemyDefinitionId.Puromi:
            case EnemyDefinitionId.MiniKraid:
            case EnemyDefinitionId.RidleyCeres:
            case EnemyDefinitionId.Ridley:
            case EnemyDefinitionId.RidleyExplosion:
            case EnemyDefinitionId.Steam:
            case EnemyDefinitionId.CeresDoor:
            case EnemyDefinitionId.Zebetite:
            case EnemyDefinitionId.Kraid:
            case EnemyDefinitionId.KraidArm:
            case EnemyDefinitionId.KraidLintTop:
            case EnemyDefinitionId.KraidLintMiddle:
            case EnemyDefinitionId.KraidLintBottom:
            case EnemyDefinitionId.KraidFoot:
            case EnemyDefinitionId.KraidNail:
            case EnemyDefinitionId.KraidNailBad:
            case EnemyDefinitionId.PhantoonBody:
            case EnemyDefinitionId.PhantoonEye:
            case EnemyDefinitionId.PhantoonTentacles:
            case EnemyDefinitionId.PhantoonMouth:
            case EnemyDefinitionId.Etecoon:
            case EnemyDefinitionId.Dachora:
            case EnemyDefinitionId.Evir:
            case EnemyDefinitionId.EvirProjectile:
            case EnemyDefinitionId.Eye:
            case EnemyDefinitionId.Fune:
            case EnemyDefinitionId.Namihe:
            case EnemyDefinitionId.Coven:
            case EnemyDefinitionId.YappingMaw:
            case EnemyDefinitionId.Kago:
            case EnemyDefinitionId.Magdollite:
            case EnemyDefinitionId.Beetom:
            case EnemyDefinitionId.Powamp:
            case EnemyDefinitionId.Robot:
            case EnemyDefinitionId.RobotNoPower:
            case EnemyDefinitionId.Bull:
            case EnemyDefinitionId.Alcoon:
            case EnemyDefinitionId.Atomic:
            case EnemyDefinitionId.Spark:
            case EnemyDefinitionId.FaceBlock:
            case EnemyDefinitionId.KihunterGreen:
            case EnemyDefinitionId.KihunterGreenWings:
            case EnemyDefinitionId.KihunterYellow:
            case EnemyDefinitionId.KihunterYellowWings:
            case EnemyDefinitionId.KihunterRed:
            case EnemyDefinitionId.KihunterRedWings:
            case EnemyDefinitionId.MotherBrainHead:
            case EnemyDefinitionId.MotherBrainBody:
            case EnemyDefinitionId.BabyMetroidCutscene:
            case EnemyDefinitionId.MotherBrainTubes:
            case EnemyDefinitionId.CorpseTorizo:
            case EnemyDefinitionId.CorpseSidehopper:
            case EnemyDefinitionId.CorpseZoomer:
            case EnemyDefinitionId.CorpseRipper:
            case EnemyDefinitionId.CorpseSkree:
            case EnemyDefinitionId.BabyMetroid:
            case EnemyDefinitionId.BombTorizo:
            case EnemyDefinitionId.BombTorizoOrb:
            case EnemyDefinitionId.GoldenTorizo:
            case EnemyDefinitionId.GoldenTorizoOrb:
            case EnemyDefinitionId.TourianStatue:
            case EnemyDefinitionId.Shaktool:
            case EnemyDefinitionId.NoobTubeCrack:
            case EnemyDefinitionId.Chozo:
            case EnemyDefinitionId.Geega:
            case EnemyDefinitionId.Botwoon:
            case EnemyDefinitionId.EtecoonEscape:
            case EnemyDefinitionId.DachoraEscape:
            case EnemyDefinitionId.PirateGreyWall:
            case EnemyDefinitionId.PirateGoldWall:
            case EnemyDefinitionId.PirateMagentaWall:
            case EnemyDefinitionId.PirateSilverWall:
            case EnemyDefinitionId.PirateGoldNinja:
            case EnemyDefinitionId.PirateSilverNinja:
            case EnemyDefinitionId.PirateGreyWalking:
            case EnemyDefinitionId.PirateGreenWalking:
            case EnemyDefinitionId.PirateRedWalking:
            case EnemyDefinitionId.PirateGoldWalking:
            case EnemyDefinitionId.PirateMagentaWalking:
            case EnemyDefinitionId.PirateSilverWalking:
            default:
                PipeBugsBehavior.InitializeYellowPipeBug(slot);
                break;
        }
        return enemies.PipeBugStates[slot.SlotIndex] ?? throw new InvalidOperationException("The Pipe Bug initializer left no state.");
    }

    /// <summary>Runs the Pipe Bug behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--pipe-bug-instruction-mechanics")]
    internal static int PipeBugBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPipeBugBehavior), () => VerifyPipeBugBehavior());
        Suite(nameof(VerifyPipeBugStateQuantities), () => VerifyPipeBugStateQuantities());
        Suite(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(rom));
        return 0;
    }
}
