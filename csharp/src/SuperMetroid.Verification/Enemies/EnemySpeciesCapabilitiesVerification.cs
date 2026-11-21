using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Confirms that the species entities answer the collision capabilities, private shot reactions and Ceres Ridley scene
/// drawing exactly as the shared definition-keyed catalog and the Ridley room's door-identity test did before those answers
/// moved onto the entities (#1312), with the private shot reactions in the definitions' shared-response form (#1759). The
/// expected tables below are the removed catalog's answers, written out per definition; every definition is set up as the
/// game sets it up (#2554), a part after its body, and asked through its entity.
/// </summary>
internal static class EnemySpeciesCapabilitiesVerification
{
    /// <summary>Both extended-spritemap walkers: Samus contact and projectile collision.</summary>
    private const EnemyCapabilities BothWalkers =
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The ordinary Pirate definitions: the wall, walking and ninja families in every colour.</summary>
    private static readonly EnemyDefinitionId[] OrdinaryPirates =
    [
        EnemyDefinitionId.PirateGreyWall, EnemyDefinitionId.PirateGoldWall, EnemyDefinitionId.PirateMagentaWall, EnemyDefinitionId.PirateSilverWall,
        EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateRedWalking,
        EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateSilverWalking,
        EnemyDefinitionId.PirateGoldNinja, EnemyDefinitionId.PirateSilverNinja,
    ];

    /// <summary>Crocomire's body and tongue, whose hitboxes name Crocomire's own shot reactions.</summary>
    private static readonly EnemyDefinitionId[] CrocomireParts = [EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue];

    /// <summary>The removed catalog's capabilities per definition; every definition not listed takes none.</summary>
    private static Dictionary<EnemyDefinitionId, EnemyCapabilities> ExpectedCapabilities()
    {
        var expected = new Dictionary<EnemyDefinitionId, EnemyCapabilities>
        {
            [EnemyDefinitionId.Oum] = BothWalkers,
            [EnemyDefinitionId.BombTorizo] = BothWalkers,
            [EnemyDefinitionId.GoldenTorizo] = BothWalkers,
            [EnemyDefinitionId.Crocomire] = BothWalkers,
            [EnemyDefinitionId.CrocomireTongue] = BothWalkers,
            [EnemyDefinitionId.SporeSpawn] = BothWalkers,
            [EnemyDefinitionId.SteamVent] = BothWalkers,
            [EnemyDefinitionId.KraidArm] = BothWalkers,
            [EnemyDefinitionId.KraidFoot] = BothWalkers,
            [EnemyDefinitionId.DraygonBody] = BothWalkers,
            [EnemyDefinitionId.PhantoonBody] = EnemyCapabilities.SamusFrameHitboxes,
            [EnemyDefinitionId.Kraid] = EnemyCapabilities.ProjectileFrameHitboxes,
            [EnemyDefinitionId.Ridley] = EnemyCapabilities.ProjectileFrameHitboxes,
            [EnemyDefinitionId.MotherBrainBody] = EnemyCapabilities.ProjectileFrameHitboxes,
            [EnemyDefinitionId.MotherBrainHead] = EnemyCapabilities.ProjectileFrameHitboxes,
        };
        foreach (EnemyDefinitionId pirate in OrdinaryPirates)
            expected.Add(pirate, BothWalkers);
        return expected;
    }

    /// <summary>
    /// The private shot reactions each definition owns (#1759, #2558): the species' own reaction its definition names, and
    /// the species' own identities where its hitboxes name them. The removed catalog's named callbacks map one to one onto
    /// these: a species' definition-level callback is the reaction its definition names. Every definition not listed owns
    /// none.
    /// </summary>
    private static Dictionary<EnemyDefinitionId, EnemyReaction[]> ExpectedPrivateShotReactions()
    {
        var expected = new Dictionary<EnemyDefinitionId, EnemyReaction[]>
        {
            [EnemyDefinitionId.BabyTurtle] = [TurtleReactions.Shot],
            [EnemyDefinitionId.Owtch] = [OwtchReactions.Shot],
            [EnemyDefinitionId.Rinka] = [RinkaReactions.Shot],
            [EnemyDefinitionId.Oum] = [OumReactions.Shot],
            [EnemyDefinitionId.GrappleRipper] = [RipperReactions.Shot],
            [EnemyDefinitionId.JetRipper] = [RipperReactions.Shot],
            [EnemyDefinitionId.Dragon] = [DragonReactions.Shot],
            [EnemyDefinitionId.ShutterShootable] = [ShutterReactions.Shot],
            [EnemyDefinitionId.Kamer] = [ShutterReactions.Shot],
            [EnemyDefinitionId.ShutterDestroyable] = [ShutterReactions.Shot],
            [EnemyDefinitionId.Metaree] = [MetareeReactions.Shot],
            [EnemyDefinitionId.Fireflea] = [FirefleaReactions.Shot],
            [EnemyDefinitionId.Tripper] = [PlatformsReactions.TripperShot],
            [EnemyDefinitionId.Mochtroid] = [MochtroidReactions.Shot],
            [EnemyDefinitionId.Skree] = [SkreeReactions.Shot],
            [EnemyDefinitionId.Yard] = [YardReactions.Shot],
            [EnemyDefinitionId.Evir] = [EvirReactions.Shot],
            [EnemyDefinitionId.YappingMaw] = [YappingMawReactions.Shot],
            [EnemyDefinitionId.Kago] = [KagoReactions.Shot],
            [EnemyDefinitionId.Magdollite] = [MagdolliteReactions.Shot],
            [EnemyDefinitionId.Beetom] = [BeetomReactions.Shot],
            [EnemyDefinitionId.Powamp] = [PowampReactions.Shot],
            [EnemyDefinitionId.WorkRobot] = [WorkRobotReactions.Shot],
            [EnemyDefinitionId.UnpoweredWorkRobot] = [WorkRobotReactions.Shot],
            [EnemyDefinitionId.Bull] = [BullReactions.Shot],
            [EnemyDefinitionId.Spark] = [SparkReactions.Shot],
            [EnemyDefinitionId.FaceBlock] = [FaceBlockReactions.Shot],
            [EnemyDefinitionId.KihunterGreen] = [KiHunterReactions.Shot],
            [EnemyDefinitionId.KihunterYellow] = [KiHunterReactions.Shot],
            [EnemyDefinitionId.KihunterRed] = [KiHunterReactions.Shot],
            [EnemyDefinitionId.FakeKraid] = [KraidReactions.FakeKraidShot],
            [EnemyDefinitionId.Zebetite] = [ZebetiteReactions.Shot],
            [EnemyDefinitionId.CorpseTorizo] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseSidehopper] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseZoomer] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseRipper] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseSkree] = [CorpseReactions.Shot],
            [EnemyDefinitionId.DraygonBody] = [DraygonReactions.Shot, EnemyReaction.DudShot],
            [EnemyDefinitionId.SporeSpawn] = [SporeSpawnReactions.Shot, EnemyReaction.DudShot],
            [EnemyDefinitionId.PhantoonBody] = [PhantoonReactions.Shot],
            [EnemyDefinitionId.MotherBrainBody] = [MotherBrainReactions.BodyShot],
            [EnemyDefinitionId.MotherBrainHead] = [MotherBrainReactions.HeadShot],
            [EnemyDefinitionId.BombTorizo] = [TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot],
            [EnemyDefinitionId.GoldenTorizo] = [TorizoReactions.GoldenShot, TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot],
            [EnemyDefinitionId.Shaktool] = [ShaktoolReactions.Shot],
            [EnemyDefinitionId.KraidArm] = [KraidReactions.ArmShot],
            [EnemyDefinitionId.RidleyCeres] = [RidleyReactions.Shot],
            [EnemyDefinitionId.Ridley] = [RidleyReactions.Shot],
            [EnemyDefinitionId.BabyMetroid] = [BabyMetroidReactions.Shot],
            [EnemyDefinitionId.Botwoon] = [BotwoonReactions.Shot],
            [EnemyDefinitionId.Metroid] = [MetroidReactions.Shot],
        };
        foreach (EnemyDefinitionId pirate in OrdinaryPirates)
        {
            expected.Add(pirate,
            [
                SpacePirateReactions.Shot, SpacePirateReactions.GoldNinjaVulnerableHitboxShot,
                SpacePirateReactions.GoldNinjaInvincibleHitboxShot,
            ]);
        }
        foreach (EnemyDefinitionId part in CrocomireParts)
        {
            expected.Add(part,
            [
                CrocomireReactions.AbsorbedShot, CrocomireReactions.DustShot, CrocomireReactions.MouthShot,
            ]);
        }
        return expected;
    }

    /// <summary>Every reaction a shot can select: the shared ones and each species' own.</summary>
    private static readonly EnemyReaction[] ProbedReactions =
    [
        .. EnemyReactionConstants.Shared,
        BotwoonReactions.Touch, BotwoonReactions.Shot, BabyMetroidReactions.Touch, BabyMetroidReactions.Shot,
        BabyMetroidReactionsConstants.CutsceneTouch, RinkaReactions.Touch, RinkaReactions.Shot, ZebetiteReactions.Touch,
        ZebetiteReactions.Shot, BeetomReactions.Touch, BeetomReactions.Shot, BullReactions.Shot, CorpseReactions.Touch,
        CorpseReactions.Shot, DragonReactions.Touch, DragonReactions.Shot, EvirReactions.Touch, EvirReactions.Shot,
        FaceBlockReactions.Shot, FirefleaReactions.Touch, FirefleaReactions.Shot, KagoReactions.Shot, KiHunterReactions.Shot,
        MagdolliteReactions.Touch, MagdolliteReactions.Shot, TurtleReactions.Touch, TurtleReactions.Shot,
        MetareeReactions.Shot, MetroidReactions.Touch, MetroidReactions.Shot, MochtroidReactions.Touch,
        MochtroidReactions.Shot, OwtchReactions.Shot, PlatformsReactions.TripperShot, PowampReactions.Touch,
        PowampReactions.Shot, RipperReactions.Shot, ShaktoolReactions.Touch, ShaktoolReactions.Shot,
        ShutterReactions.Touch, ShutterReactions.Shot, SkreeReactions.Shot, SparkReactions.Shot, WorkRobotReactions.Touch,
        WorkRobotReactions.Shot, YappingMawReactions.Touch, YappingMawReactions.Shot, YardReactions.Touch,
        YardReactions.Shot, CrocomireReactionsConstants.Shot, KraidReactions.NailTouch, KraidReactions.FakeKraidTouch,
        KraidReactions.FakeKraidShot,
        CeresSteamReactions.Touch,
        CrocomireReactions.ClawTouch, CrocomireReactions.AbsorbedShot, CrocomireReactions.DustShot,
        CrocomireReactions.MouthShot,
        DraygonReactions.Touch, DraygonReactions.Shot,
        KraidReactions.BodyTouch, KraidReactions.ArmTouch, KraidReactions.ArmShot,
        MotherBrainReactions.BodyShot, MotherBrainReactions.HeadShot, MotherBrainReactions.HeadTouch,
        PhantoonReactions.Touch, PhantoonReactions.Shot,
        RidleyReactions.FrameHitboxTouch, RidleyReactions.Shot,
        SporeSpawnReactions.Touch, SporeSpawnReactions.Shot,
        TorizoReactions.Touch, TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot, TorizoReactions.GoldenShot,
        OumReactions.DamagingTouch, OumReactions.NonDamagingTouch, OumReactions.Shot,
        SpacePirateReactions.Touch, SpacePirateReactions.Shot, SpacePirateReactions.GoldNinjaVulnerableHitboxShot,
        SpacePirateReactions.GoldNinjaInvincibleHitboxShot,
    ];

    /// <summary>
    /// Sets every room and auxiliary definition, and every definition the expected tables name, up as the game does, then
    /// compares its entity's capabilities, its answer for every probed shot reaction, and whether it shows Ceres Ridley's
    /// room door's overlay, against the expected tables. A definition a room's population places is set up from the
    /// population that lists it soonest, the entries listed before it set up first by the population load, so a part is set
    /// up after its body; a definition only spawned joining an encounter is spawned by that encounter as its spawner spawns
    /// it; any other is set up alone from the placement its spawner gives it.
    /// </summary>
    internal static void VerifyEnemySpeciesCapabilities()
    {
        Dictionary<EnemyDefinitionId, EnemyCapabilities> capabilities = ExpectedCapabilities();
        Dictionary<EnemyDefinitionId, EnemyReaction[]> privateReactions = ExpectedPrivateShotReactions();
        List<EnemyDefinitionId> definitions = [.. RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Concat(RoomEnemyAuxiliaryDefinitionCatalog.Definitions.Keys)
            .Concat(capabilities.Keys)
            .Concat(privateReactions.Keys)
            .Distinct()];
        Dictionary<EnemyDefinitionId, PopulationListing> listings = SoonestListings();
        var checkedDefinitions = new HashSet<EnemyDefinitionId>();
        int answers = 0;

        foreach (IGrouping<RoomEnemyPopulationDefinition, EnemyDefinitionId> listed in definitions
            .Where(listings.ContainsKey).GroupBy(definition => listings[definition].Population))
        {
            RoomEnemySystem enemies = LoadThrough(listed.Key, listed.Max(definition => listings[definition].Index));
            foreach (EnemyDefinitionId definition in listed)
                Check(definition, enemies, enemies.Places.InUpdateOrder[listings[definition].Index]);
        }

        // Her fake death's tubes and her late fight's Baby join the encounter her body begins.
        PopulationListing motherBrainListing = listings[EnemyDefinitionId.MotherBrainHead];
        RoomEnemySystem motherBrainRoom = LoadThrough(motherBrainListing.Population, motherBrainListing.Index);
        MotherBrainEnemyState motherBrain = motherBrainRoom.Places.InUpdateOrder[listings[EnemyDefinitionId.MotherBrainBody].Index]
            .EntityOf<MotherBrainBodyEntity>().Encounter;
        RoomEnemyPlace tube = ((IEnemyLifecycle)motherBrainRoom.Lifecycle).SpawnEnemy(
            new JoiningEnemySpawn<MotherBrainEnemyState, MotherBrainTubeSpawnSettings>(
                CompiledRoomEnemyDefinitions.Instance.ReadFallingTubePopulation(MotherBrainTubePiece.BottomLeft),
                new MotherBrainTubeSpawnSettings(MotherBrainTubePiece.BottomLeft), motherBrainRoom.MotherBrainBehavior, motherBrain))
            ?? throw new InvalidOperationException("Mother Brain's room had no free place for her falling tube.");
        Check(EnemyDefinitionId.MotherBrainTubes, motherBrainRoom, tube);
        MotherBrainRainbowBeamAttackSequence lateFight = MotherBrainBabyArrival.BringInBaby(motherBrain, RoomEnemyFrameFixtures.StandaloneSamus());
        Check(EnemyDefinitionId.BabyMetroidCutscene, motherBrainRoom, MotherBrainBabyArrival.BabyOf(lateFight).Place);

        // His breakup's pieces join the breakup his death begins, each spawned as his death spawns it.
        PopulationListing ridleyListing = listings[EnemyDefinitionId.Ridley];
        RoomEnemySystem ridleyRoom = LoadThrough(ridleyListing.Population, ridleyListing.Index);
        RoomEnemyPlace ridley = ridleyRoom.Places.InUpdateOrder[ridleyListing.Index];
        var pieceSettings = new RidleyExplosionSpawnSettings(RidleyExplosionPart.TailBase);
        RoomEnemyPlace piece = ((IEnemyLifecycle)ridleyRoom.Lifecycle).SpawnEnemy(
            new JoiningEnemySpawn<RidleyBreakupSource, RidleyExplosionSpawnSettings>(
                new RoomEnemyPlacement(EnemyDefinitionId.RidleyBreakupPiece, EnemyPlacing.At(Position16<RoomPixels>.Zero),
                    EnemyContact.IgnoresSamus, EnemyProcessing.Animates | EnemyProcessing.OffScreen, EnemyPresence.None,
                    CollidesByFrameHitboxes: false, pieceSettings),
                pieceSettings, ridleyRoom.NorfairRidleyBehavior.Death,
                new RidleyBreakupSource(ridley.EntityOf<NorfairRidleyEntity>().Encounter, ridley.Position.Whole)))
            ?? throw new InvalidOperationException("Ridley's room had no free place for his breakup's piece.");
        Check(EnemyDefinitionId.RidleyBreakupPiece, ridleyRoom, piece);

        // The rest are placed by their spawners from placements of their own, each set up alone.
        foreach (EnemyDefinitionId definition in definitions.Where(definition => !checkedDefinitions.Contains(definition)).ToList())
        {
            RoomEnemySystem enemies = NewRoomEnemies();
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            LeadingPartPlacement.Prepare(place, definition);
            place.Bind(definition, HeaderOf(definition));
            place.SetUpAsPlaced();
            Check(definition, enemies, place);
        }

        AssertEqual(definitions.Count, checkedDefinitions.Count, "every definition is set up and asked once");
        Console.WriteLine($"Enemy species capabilities: {checkedDefinitions.Count} definitions set up as the game sets them up answer the " +
            $"removed catalog's collision capabilities, {answers} private shot reaction queries and Ceres Ridley's room door overlay through their entities.");

        void Check(EnemyDefinitionId definition, RoomEnemySystem enemies, RoomEnemyPlace place)
        {
            AssertEqual(definition, place.HeldOccupant().DefinitionId, $"{definition} is set up where the check asks it");
            AssertTrue(checkedDefinitions.Add(definition), $"{definition} is asked once");
            IEnemyEntity entity = place.Entity ?? throw new InvalidOperationException($"{definition}'s setup built no entity.");

            AssertEqual(capabilities.GetValueOrDefault(definition, EnemyCapabilities.None), entity.Capabilities,
                $"{definition} collision capabilities");

            EnemyReaction[] owned = privateReactions.GetValueOrDefault(definition, []);
            foreach (EnemyReaction reaction in ProbedReactions)
            {
                bool expected = owned.Contains(reaction);
                bool actual = entity is IEnemyPrivateShotReaction owner && owner.OwnsPrivateShotReaction(reaction);
                AssertEqual(expected, actual, $"{definition} owns private shot reaction {reaction}");
                answers++;
            }

            // Only the door of Ceres Ridley's room is built as it; its setup attaches it to the room's Ceres doors, which
            // show its overlay picture while its animation's look is the overlay.
            if (entity is not CeresRidleyRoomDoorEntity door)
            {
                AssertTrue(definition != EnemyDefinitionId.CeresRidleyRoomDoor, $"{definition} is built as Ceres Ridley's room door");
                return;
            }
            AssertEqual(EnemyDefinitionId.CeresRidleyRoomDoor, definition, "only Ceres Ridley's room door is built as it");
            foreach (CeresRidleyRoomDoorLook look in new[] { CeresRidleyRoomDoorLook.Actor, CeresRidleyRoomDoorLook.Overlay })
            {
                door.State.Look = look;
                var sprites = new SpriteFrameBuffer();
                sprites.BeginFrame();
                enemies.CeresDoorBehavior.PublishRidleyRoomDoorOverlay(sprites, Position16<RoomPixels>.Zero);
                sprites.FinalizeFrame();
                AssertEqual(look == CeresRidleyRoomDoorLook.Overlay, !sprites.Draws.IsEmpty,
                    $"Ceres Ridley's room door shows its overlay when its look is {look}");
            }
        }
    }

    /// <summary>Where a room's population lists a definition: the population and the entry's place in it.</summary>
    /// <param name="Population">The population.</param>
    /// <param name="Index">The entry's place in the population's list, which is the room place it is set up in.</param>
    private readonly record struct PopulationListing(RoomEnemyPopulationDefinition Population, int Index);

    /// <summary>
    /// For every definition a room's population places, the population that lists it soonest, the fewest entries before
    /// it, and the first such in the game's list order.
    /// </summary>
    private static Dictionary<EnemyDefinitionId, PopulationListing> SoonestListings()
    {
        var listings = new Dictionary<EnemyDefinitionId, PopulationListing>();
        foreach (RoomEnemyPopulationDefinition population in RoomEnemyPopulationDefinitions.Definitions)
        {
            ReadOnlySpan<RoomEnemyPlacement> placements = population.Placements.Span;
            for (int index = 0; index < placements.Length; index++)
            {
                if (!listings.TryGetValue(placements[index].DefinitionId, out PopulationListing listed) || index < listed.Index)
                    listings[placements[index].DefinitionId] = new PopulationListing(population, index);
            }
        }
        return listings;
    }

    /// <summary>
    /// A new room's enemies with the entries of <paramref name="population"/> through the one at <paramref name="last"/> set
    /// up by the population load, in an open room holding every one of their places.
    /// </summary>
    private static RoomEnemySystem LoadThrough(RoomEnemyPopulationDefinition population, int last)
    {
        RoomEnemyPlacement[] placements = population.Placements.Span[..(last + 1)].ToArray();
        RoomEnemySystem enemies = NewRoomEnemies();
        enemies.PopulationLoader.LoadPopulation(enemies.Species, CompiledRoomEnemyDefinitions.Instance,
            new PlacedRoomEnemyPopulationDefinition(population.Name, placements, deathQuota: 0),
            new EnemyPopulationSurroundings(OpenRoomHolding(placements), RoomEnemyFrameFixtures.StandaloneSamus(), GameplayAction.None,
                Position16<RoomPixels>.Zero));
        return enemies;
    }

    /// <summary>
    /// A room with no terrain, at least four screens each way, that holds a screen past every place
    /// <paramref name="placements"/> put their enemies at inside the room; a place counted back from the far end of the
    /// room's coordinates lies before the room (<see cref="OffRoomPlaces"/>).
    /// </summary>
    private static RoomLevelData OpenRoomHolding(RoomEnemyPlacement[] placements)
    {
        int columns = RoomGridConversions.BlocksPerScreen * 4, rows = RoomGridConversions.BlocksPerScreen * 4;
        foreach (RoomEnemyPlacement placement in placements)
        {
            if (placement.Placing is not EnemyPlacedAt { Position: var at } || CoordinateAxis.AsSigned(at.X) < 0 || CoordinateAxis.AsSigned(at.Y) < 0)
                continue;
            columns = Math.Max(columns, RoomGridConversions.PixelToBlock(at.X) + RoomGridConversions.BlocksPerScreen);
            rows = Math.Max(rows, RoomGridConversions.PixelToBlock(at.Y) + RoomGridConversions.BlocksPerScreen);
        }
        return RoomEnemyFrameFixtures.OpenRoom(new(columns, rows));
    }

    /// <summary>
    /// A new room's enemies, in a room that can place any enemy, in a new game: no boss, mini-boss or Torizo of its area is
    /// defeated, no story event has happened and no Zebetite is destroyed; the room's scrolls are its own.
    /// </summary>
    private static RoomEnemySystem NewRoomEnemies()
    {
        var services = new VerificationRoomEnemyServices(null)
        {
            IsAreaBossDefeated = () => false,
            IsAreaMiniBossDefeated = () => false,
            IsAreaTorizoDefeated = () => false,
            HasEvent = _ => false,
            ReadZebetitesDestroyed = () => 0,
            SetRoomScrollState = (_, _) => { },
            NextRandom = () => 0,
            ReadRandom = () => 0,
        };
        return EnemySystemFixtures.PlacingAnyEnemy(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: services);
    }

    /// <summary>Runs the species capability equivalence check.</summary>
    [VerificationCommand("--enemy-species-capabilities")]
    internal static int EnemySpeciesCapabilitiesCommand()
    {
        Suite(nameof(VerifyEnemySpeciesCapabilities), () => VerifyEnemySpeciesCapabilities());
        return 0;
    }

    /// <summary>The definition's retail header, or the empty header for a definition no catalog pins.</summary>
    /// <param name="definition">The definition to bind.</param>
    private static RoomEnemyDefinition HeaderOf(EnemyDefinitionId definition)
    {
        if (RoomEnemyAuxiliaryDefinitionCatalog.TryGet(definition, out RoomEnemyDefinition auxiliary))
            return auxiliary;
        foreach ((EnemyDefinitionId pointer, RoomEnemyDefinition header) in RoomEnemyDefinitionCatalog.Definitions)
        {
            if (pointer == definition)
                return header;
        }
        return default;
    }
}
