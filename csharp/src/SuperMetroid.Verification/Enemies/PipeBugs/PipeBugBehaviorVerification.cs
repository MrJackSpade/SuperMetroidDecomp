using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Input;
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
    /// <c>StartPipeBugAnimation</c> runs one pass and returns to its first pose on time; each family
    /// keeps its animation in its own state (#1845).
    /// </summary>
    internal static void VerifyPipeBugBehavior()
    {
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.ZebRisingLeft), Initialized(EnemyDefinitionId.Zeb, 0).Animation, "Zeb starts rising left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.ZebboRisingLeft), Initialized(EnemyDefinitionId.Zebbo, 1).Animation, "Zebbo starts rising left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.GametRisingLeft), Initialized(EnemyDefinitionId.Gamet, 0).Animation, "Gamet starts rising left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.GeegaFlyingLeft), Initialized(EnemyDefinitionId.Geega, 1).Animation, "Geega with parameter one starts flying left");
        AssertEqual(PipeBugAnimations.StartingWith(PipeBugSegment.GeegaFlyingRight), Initialized(EnemyDefinitionId.Geega, 0).Animation, "Geega with parameter zero starts flying right");

        foreach (PipeBugSegment program in Enum.GetValues<PipeBugSegment>())
        {
            (EnemyDefinitionId definition, ushort parameter) = program.ToString() switch
            {
                var name when name.StartsWith("Zebbo", StringComparison.Ordinal) => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Zebbo, 1),
                var name when name.StartsWith("Zeb", StringComparison.Ordinal) => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Zeb, 0),
                var name when name.StartsWith("Gamet", StringComparison.Ordinal) => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Gamet, 0),
                _ => new VerifyPipeBugBehaviorCase(EnemyDefinitionId.Geega, 0),
            };
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            Action animate;
            if (definition == EnemyDefinitionId.Geega)
            {
                // The yellow bug's setup builds the state its animation, which the check plays, runs on.
                YellowPipeBugState yellow = InitializeYellow(place, parameter);
                yellow.Playback = PipeBugsBehavior.StartPipeBugAnimation(place, program);
                animate = () => PipeBugsBehavior.AdvanceYellowPipeBugTypedAnimation(place, yellow);
            }
            else
            {
                Initialize(place, definition, parameter);
                place.PipeBugPlayback = PipeBugsBehavior.StartPipeBugAnimation(place, program);
                object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
                animate = () => EnemyInstructionRunner.Process(arguments);
            }
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(PipeBugAnimations.Segments, program, PipeBugFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(PipeBugAnimations.Segments, program), animate),
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
        foreach ((int waited, bool departs) in new[] { new VerifyPipeBugStateQuantitiesCase(70, false), new VerifyPipeBugStateQuantitiesCase(71, true) })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            Initialize(place, EnemyDefinitionId.Gamet, 0);
            NorfairPipeBugState state = place.RequireSpeciesState<NorfairPipeBugState>();
            state.Action = NorfairPipeBugAction.WaitForStagger;
            // The leader's role starts its wait for its departure count; its stagger wait counts one formation update on each
            // of its updates.
            state.Role = new NorfairPipeBugRole.Leader(StaggerTarget: 72);
<<<<<<< HEAD
            NorfairPipeBugSpawnSettings settings = place.HeldOccupant().SpawnSettings<NorfairPipeBugSpawnSettings>();
            for (int update = 0; update < waited; update++)
                PipeBugsBehavior.RunNorfairPipeBugMainAi(settings, place, state, Frame(new(200, 128)));
            AssertEqual(NorfairPipeBugAction.WaitForStagger, state.Action, $"Norfair Pipe Bug still waits after {waited} counted updates");
            PipeBugsBehavior.RunNorfairPipeBugMainAi(settings, place, state, Frame(new(200, 128)));
||||||| 418ca79935
            PrivateState.Set(state, "Departure", UpdateCountdownAccess.AtAge(72, before));
            enemies.PipeBugsBehavior.RunNorfairPipeBugMainAi(place.HeldOccupant(), place, Frame(new(200, 128)));
            AssertEqual(after, state.Departure.Age, $"Norfair Pipe Bug count after {before}");
=======
            // The member waits from a count of none: the main AI counts it up, one each update, to the count under test.
            for (int update = 0; update < before; update++)
            {
                enemies.PipeBugsBehavior.RunNorfairPipeBugMainAi(place.HeldOccupant(), place, Frame(new(200, 128)));
                AssertEqual(NorfairPipeBugAction.WaitForStagger, state.Action, $"Norfair Pipe Bug waits through count {update + 1}");
            }
            AssertEqual(before, state.Departure.Age, $"Norfair Pipe Bug counts {before} updates");
            enemies.PipeBugsBehavior.RunNorfairPipeBugMainAi(place.HeldOccupant(), place, Frame(new(200, 128)));
            AssertEqual(after, state.Departure.Age, $"Norfair Pipe Bug count after {before}");
>>>>>>> origin/main
            AssertEqual(departs ? NorfairPipeBugAction.Fly : NorfairPipeBugAction.WaitForStagger, state.Action,
                $"Norfair Pipe Bug departs only on reaching its target of 72 counted updates (update {waited + 1})");
            if (departs)
                AssertEqual(HorizontalDirection.Right, state.FlightDirection, "Norfair Pipe Bug departs toward Samus on its right");
        }

        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            Initialize(place, EnemyDefinitionId.Zeb, 0);
            BrinstarPipeBugState state = place.RequireSpeciesState<BrinstarPipeBugState>();
            state.Action = BrinstarPipeBugAction.WaitForSamus;
            PipeBugsBehavior.RunBrinstarPipeBugMainAi(place.HeldOccupant().SpawnSettings<BrinstarPipeBugSpawnSettings>(), place, place.RequireSpeciesState<BrinstarPipeBugState>(), Frame(new(28, 78)));
            AssertEqual(HorizontalDirection.Left, state.Flight, "Brinstar Pipe Bug flies left of Samus on her left");
            AssertEqual(BrinstarPipeBugAction.WaitForSamus, state.Action, "Brinstar Pipe Bug waits while Samus is 100 pixels away");
            PipeBugsBehavior.RunBrinstarPipeBugMainAi(place.HeldOccupant().SpawnSettings<BrinstarPipeBugSpawnSettings>(), place, place.RequireSpeciesState<BrinstarPipeBugState>(), Frame(new(148, 78)));
            AssertEqual(HorizontalDirection.Right, state.Flight, "Brinstar Pipe Bug flies right toward Samus on her right");
            AssertEqual(BrinstarPipeBugAction.Emerge, state.Action, "Brinstar Pipe Bug emerges for Samus 20 pixels away");
            AssertEqual(PipeBugSegment.ZebRisingRight, state.Playback.PlayingSegment, "Brinstar Pipe Bug emerges facing right");
        }

        {
            const int onePixel = 65536;
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            place.SetSpawnParameter2(EnemyDefinitionId.Geega, 24);
            YellowPipeBugState state = InitializeYellow(place, 0);
            AssertEqual(SixteenSixteenWords.ToDisplacement(onePixel + onePixel / 2), place.HeldOccupant().SpawnSettings<YellowPipeBugSpawnSettings>().Speed, "yellow Pipe Bug speed record 24 is 1.5 pixels");
            // The arc goes the way the placement faces (#2474): parameter one's zero faces right.
            AssertEqual(HorizontalDirection.Right, place.HeldOccupant().SpawnSettings<YellowPipeBugSpawnSettings>().Facing, "yellow Pipe Bug placement faces right");
            state.Action = YellowPipeBugAction.ArcDipping;
            state.ArcCurvePlace = QuadraticSpeedCurvePlace.FromMarks(1);
            PipeBugBehaviorExpected[] expected = [new PipeBugBehaviorExpected(0, true), new PipeBugBehaviorExpected(0, false), new PipeBugBehaviorExpected(1, false)];
            for (int update = 0; update < expected.Length; update++)
            {
                PipeBugsBehavior.RunYellowPipeBugMainAi(place.HeldOccupant().SpawnSettings<YellowPipeBugSpawnSettings>(), place, state, Frame(new(200, 128)));
                AssertEqual(SixteenSixteenWords.ToCoordinate(checked((uint)(128 * onePixel + (update + 1) * (onePixel + onePixel / 2)))), place.Position.X,
                    $"yellow Pipe Bug arc update {update} carries its 16.16 speed");
                AssertEqual(QuadraticSpeedCurvePlace.FromMarks(expected[update].Counter), state.ArcCurvePlace, $"yellow Pipe Bug arc record after update {update}");
                if (update < 2)
                    AssertEqual(expected[update].Rising ? YellowPipeBugAction.ArcDipping : YellowPipeBugAction.ArcClimbing, state.Action, $"yellow Pipe Bug arc phase after update {update}");
            }
        }
        Console.WriteLine("  Pipe Bug state: the formation count wraps and compares by sign, the direction bit stands alone, and the arc record never goes below zero.");

        static ClockedEnemyAiFrame Frame(Position16<RoomPixels> samusPosition) => new(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(samusPosition) } },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
    }

    /// <summary>
    /// #2071: a Norfair formation member's one stagger action moves it by its role's offset. For each member above or below
    /// the leader, the production main AI runs its stagger from where its rise ended until it waits: it stops exactly its
    /// role's offset from there, counting the formation once each update it moved.
    /// </summary>
    internal static void VerifyNorfairPipeBugStaggerOffsets()
    {
        ClockedEnemyAiFrame frame = new(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(200, 128)) } },
            new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
        foreach (NorfairPipeBugRole role in new[] { PipeBugDefinitions.UpperNearRole, PipeBugDefinitions.UpperFarRole, PipeBugDefinitions.LowerNearRole, PipeBugDefinitions.LowerFarRole })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace slot = enemies.Places.InUpdateOrder[0];
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128));
            Initialize(slot, EnemyDefinitionId.Gamet, 0);
            NorfairPipeBugState state = slot.RequireSpeciesState<NorfairPipeBugState>();
            NorfairPipeBugStagger stagger = role is NorfairPipeBugRole.Follower { Stagger: var moves } ? moves
                : throw new InvalidOperationException("Every member but the leader staggers.");
            // The member's place in the formation, from where its rise ended: up is toward smaller Y (#2473).
            int offset = stagger.Direction == VerticalDirection.Up ? -stagger.Distance : stagger.Distance;
            AssertEqual(NorfairPipeBugAction.Stagger, role.PostRiseAction, $"the member {offset} pixels from the leader staggers by its offset");
            state.Action = NorfairPipeBugAction.Stagger;
            state.Role = role;
            state.RiseEndY = 128;
            // The role's assignment starts the member's wait at its target, uncounted.
            AssertEqual(0, state.Departure.Age, $"the member staggering {offset} pixels starts uncounted");
            int updates = 0;
            while (state.Action == NorfairPipeBugAction.Stagger && updates <= stagger.Distance + 1)
            {
                PipeBugsBehavior.RunNorfairPipeBugMainAi(slot.HeldOccupant().SpawnSettings<NorfairPipeBugSpawnSettings>(), slot, slot.RequireSpeciesState<NorfairPipeBugState>(), frame);
                updates++;
            }
            AssertEqual(NorfairPipeBugAction.WaitForStagger, state.Action, $"the member staggering {offset} pixels then waits");
            AssertEqual(128 + offset, slot.Position.Y.Whole, $"the member staggering {offset} pixels stops there");
            AssertEqual(updates, state.Departure.Age, $"the member staggering {offset} pixels counts each update");
        }
        Console.WriteLine("  Norfair Pipe Bug staggers: each member moves by its role's offset and then waits.");
    }

    /// <summary>A freshly initialized Pipe Bug's animation.</summary>
    private static SuperMetroid.Core.Animation.AnimationPlayback<PipeBugSegment> Initialized(EnemyDefinitionId definition, ushort parameter)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        if (definition == EnemyDefinitionId.Geega)
            return InitializeYellow(place, parameter).Playback;
        Initialize(place, definition, parameter);
        return place.PipeBugPlayback ?? throw new InvalidOperationException("The Pipe Bug initializer left no state.");
    }

    /// <summary>Runs the yellow Pipe Bug's real initializer on <paramref name="place"/>, returning the state its setup builds.</summary>
    private static YellowPipeBugState InitializeYellow(RoomEnemyPlace place, ushort parameter)
    {
        place.Bind(EnemyDefinitionId.Geega, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnParameter1(EnemyDefinitionId.Geega, parameter);
        return PipeBugsBehavior.InitializeYellowPipeBug(place.HeldOccupant().SpawnSettings<YellowPipeBugSpawnSettings>(), place);
    }

    /// <summary>Runs <paramref name="definition"/>'s real initializer on <paramref name="place"/>.</summary>
    private static void Initialize(RoomEnemyPlace place, EnemyDefinitionId definition, ushort parameter)
    {
        place.Bind(definition, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnParameter1(definition, parameter);
        switch (definition)
        {
            // Each is set up as its room's population sets it up: its species builds its entity, whose setup builds its state.
            case EnemyDefinitionId.Zeb or EnemyDefinitionId.Zebbo or EnemyDefinitionId.Gamet:
                place.SetUpAsPlaced();
                break;
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
            case EnemyDefinitionId.MaridiaFly:
            case EnemyDefinitionId.Multiviola:
            case EnemyDefinitionId.Polyp:
            case EnemyDefinitionId.Rinka:
            case EnemyDefinitionId.Rio:
            case EnemyDefinitionId.Squeept:
            case EnemyDefinitionId.Geruta:
            case EnemyDefinitionId.Holtz:
            case EnemyDefinitionId.Oum:
            case EnemyDefinitionId.Choot:
            case EnemyDefinitionId.GrappleRipper:
            case EnemyDefinitionId.JetRipper:
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
            case EnemyDefinitionId.SuspensorPlatform:
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
            case EnemyDefinitionId.OrangeZoomer:
            case EnemyDefinitionId.Zeela:
            case EnemyDefinitionId.Sova:
            case EnemyDefinitionId.Zoomer:
            case EnemyDefinitionId.ArmoredZoomer:
            case EnemyDefinitionId.Metroid:
            case EnemyDefinitionId.Crocomire:
            case EnemyDefinitionId.CrocomireTongue:
            case EnemyDefinitionId.DraygonBody:
            case EnemyDefinitionId.DraygonEye:
            case EnemyDefinitionId.DraygonTail:
            case EnemyDefinitionId.DraygonArms:
            case EnemyDefinitionId.SporeSpawn:
            case EnemyDefinitionId.Boulder:
            case EnemyDefinitionId.KzanTop:
            case EnemyDefinitionId.KzanBottom:
            case EnemyDefinitionId.Hibashi:
            case EnemyDefinitionId.NuclearWaffle:
            case EnemyDefinitionId.FakeKraid:
            case EnemyDefinitionId.RidleyCeres:
            case EnemyDefinitionId.Ridley:
            case EnemyDefinitionId.RidleyBreakupPiece:
            case EnemyDefinitionId.SteamVent:
            case EnemyDefinitionId.CeresDoor:
            case EnemyDefinitionId.CeresRidleyRoomDoor:
            case EnemyDefinitionId.CeresElevatorDoor:
            case EnemyDefinitionId.CeresElevatorRoomWall:
            case EnemyDefinitionId.CeresGetawayChamberWall:
            case EnemyDefinitionId.Zebetite:
            case EnemyDefinitionId.Kraid:
            case EnemyDefinitionId.KraidArm:
            case EnemyDefinitionId.KraidLintTop:
            case EnemyDefinitionId.KraidLintMiddle:
            case EnemyDefinitionId.KraidLintBottom:
            case EnemyDefinitionId.KraidFoot:
            case EnemyDefinitionId.KraidFirstNail:
            case EnemyDefinitionId.KraidSecondNail:
            case EnemyDefinitionId.PhantoonBody:
            case EnemyDefinitionId.PhantoonEye:
            case EnemyDefinitionId.PhantoonTentacles:
            case EnemyDefinitionId.PhantoonMouth:
            case EnemyDefinitionId.Etecoon:
            case EnemyDefinitionId.Dachora:
            case EnemyDefinitionId.Evir:
            case EnemyDefinitionId.EvirProjectile:
            case EnemyDefinitionId.MorphBallEye:
            case EnemyDefinitionId.Fune:
            case EnemyDefinitionId.Namihe:
            case EnemyDefinitionId.Coven:
            case EnemyDefinitionId.YappingMaw:
            case EnemyDefinitionId.Kago:
            case EnemyDefinitionId.Magdollite:
            case EnemyDefinitionId.Beetom:
            case EnemyDefinitionId.Powamp:
            case EnemyDefinitionId.WorkRobot:
            case EnemyDefinitionId.UnpoweredWorkRobot:
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
            case EnemyDefinitionId.GoldenTorizo:
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
                PipeBugsBehavior.InitializeYellowPipeBug(place.HeldOccupant().SpawnSettings<YellowPipeBugSpawnSettings>(), place);
                break;
        }
    }

    /// <summary>Runs the Pipe Bug behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--pipe-bug-instruction-mechanics")]
    internal static int PipeBugBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPipeBugBehavior), () => VerifyPipeBugBehavior());
        Suite(nameof(VerifyPipeBugStateQuantities), () => VerifyPipeBugStateQuantities());
        Suite(nameof(VerifyNorfairPipeBugStaggerOffsets), () => VerifyNorfairPipeBugStaggerOffsets());
        Suite(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(rom));
        return 0;
    }
}
