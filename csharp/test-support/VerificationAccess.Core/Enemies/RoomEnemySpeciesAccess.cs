using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.NoobTube;
using SuperMetroid.Core.Enemies.OldTourianWall;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Verification's view of each owner the room's species composition built (#2571): the room system keeps no member per
/// species, so a check reaches an owner as the game does, through the composition's frame participants (every species with
/// room or frame lifecycle state) and the capability holders that keep the owners they command (#2554), and a boss
/// service through the composition itself.
/// </summary>
internal static class RoomEnemySpeciesAccess
{
    /// <summary>Every owner the composition hands out or keeps behind a capability, each once or more.</summary>
    private static IEnumerable<object> OwnersOf(RoomEnemySystem enemies)
    {
        RoomEnemySpecies species = enemies.SpeciesComposition;
        foreach (IEnemySpecies participant in species.FrameParticipants)
        {
            yield return participant;
            if (participant is SparkBehavior spark)
                yield return spark.FallingSparks;
            if (participant is MotherBrainBehavior motherBrain)
                yield return motherBrain.Body.RoomActors.BabyMetroid;
        }
        yield return ((RoomEnemySpecies.RoomLoadPreparation)species.LoadPreparation).Gunship;
        yield return ((CeresEncounter)species.Ceres).CeresRidley;
        var spawns = (RoomEnemySpecies.ObjectSpawns)species.RoomObjectSpawns;
        yield return spawns.NoobTube;
        yield return spawns.EyeDoor;
        yield return spawns.OldTourianWall;
        yield return spawns.EscapeExplosions;
        yield return ((RoomEnemySpecies.ObjectRequests)species.RoomObjectRequests).DownwardGate;
        yield return ((RoomEnemySpecies.LoaderOperations)species.LoaderHooks).PrePhantoonRoom;
        if (species.TourianStatue?.Actors is TourianStatueActors statue)
            yield return statue.Unlock;
    }

    /// <summary>The one owner of type <typeparamref name="T"/> the room's composition built, or none.</summary>
    private static T? PresentOwner<T>(RoomEnemySystem enemies) where T : class =>
        OwnersOf(enemies).OfType<T>().Distinct().SingleOrDefault();

    /// <summary>The one owner of type <typeparamref name="T"/> the room's composition built.</summary>
    /// <exception cref="InvalidOperationException">The room's composition built none.</exception>
    private static T Owner<T>(RoomEnemySystem enemies) where T : class =>
        PresentOwner<T>(enemies) ?? throw new InvalidOperationException($"The room's species composition built no {typeof(T).Name}.");

    /// <summary>The owners of the room's species, by the names the room system once held them under.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>Samus's gunship: its landing, save prompt, boarding and exit, and its lift-offs after Ceres and the escape.</summary>
        internal GunshipBehavior GunshipBehavior => Owner<GunshipBehavior>(self);

        /// <summary>Ridley's fireballs and the afterburn that trails them, in Ceres and Norfair, which Mother Brain's afterburn reuses.</summary>
        internal RidleyProjectiles RidleyProjectiles => self.SpeciesComposition.RidleyProjectiles;

        /// <summary>What the room sees of Ridley's encounters in Ceres and Lower Norfair, and the Ceres escape's progress.</summary>
        internal RidleyEncounter RidleyEncounter => self.SpeciesComposition.RidleyEncounter;

        /// <summary>Coordinates Ridley's multipart tail through the shared place and projectile services.</summary>
        internal RidleyTail RidleyTail => self.SpeciesComposition.RidleyTail;

        /// <summary>The parts of Ridley's body both his encounters run (#2229).</summary>
        internal RidleyBody RidleyBody => self.SpeciesComposition.RidleyBody;

        /// <summary>The station's escape from Ceres Ridley's room: his getaway and the self-destruct warning (#2228).</summary>
        internal CeresEscape CeresEscape => self.SpeciesComposition.CeresEscape;

        /// <summary>Raises the dust of dead monsters' fallen corpse rows through the same frame as enemy processing.</summary>
        internal CorpseRowDust CorpseRowDust => self.SpeciesComposition.CorpseRowDust;

        /// <summary>Owns Mother Brain's room encounter and its cutscene state.</summary>
        internal MotherBrainEncounter MotherBrainEncounter => self.SpeciesComposition.MotherBrainEncounter;

        /// <summary>The Zoas, which hide until Samus comes near, rise to her height and dart across the room.</summary>
        internal ZoaBehavior ZoaBehavior => Owner<ZoaBehavior>(self);

        /// <summary>The Zebetites barring the way to Mother Brain, which Samus's missiles wear down.</summary>
        internal ZebetiteBehavior ZebetiteBehavior => Owner<ZebetiteBehavior>(self);

        /// <summary>The Yards, Maridia's shelled crawlers that curl into a ball Samus can kick.</summary>
        internal YardEnemyBehavior YardBehavior => Owner<YardEnemyBehavior>(self);

        /// <summary>The Yapping Maws, mouths on stalks that bite at Samus and carry her.</summary>
        internal YappingMawBehavior YappingMawBehavior => Owner<YappingMawBehavior>(self);

        /// <summary>The Wrecked Ship's ghosts, which flicker in and out.</summary>
        internal WreckedShipGhostBehavior WreckedShipGhostBehavior => Owner<WreckedShipGhostBehavior>(self);

        /// <summary>The Wrecked Ship's work robots, which patrol and fire lasers.</summary>
        internal WorkRobotBehavior WorkRobotBehavior => Owner<WorkRobotBehavior>(self);

        /// <summary>The Wavers, flyers that weave up and down as they cross the room.</summary>
        internal WaverBehavior WaverBehavior => Owner<WaverBehavior>(self);

        /// <summary>The space pirates that cling to walls, climb and fire at Samus.</summary>
        internal WallSpacePirateBehavior WallSpacePirateBehavior => Owner<WallSpacePirateBehavior>(self);

        /// <summary>The space pirates that walk the floors and fire at Samus.</summary>
        internal WalkingSpacePirateBehavior WalkingSpacePirateBehavior => Owner<WalkingSpacePirateBehavior>(self);

        /// <summary>The Tourian entrance statue scene: its decorations, which follow the statue down, and its look as each boss statue releases. Only the statue room builds it, with that room (#2426); null in every other room, which places no statue.</summary>
        internal TourianEntranceStatueBehavior? TourianEntranceStatueBehavior => PresentOwner<TourianEntranceStatueBehavior>(self);

        /// <summary>The Tourian statues' unlock effects in the statue room: the eyes' glows, the released souls and particles, their splashes at its water and the descent's dust (#2382). Only the statue room builds them, with its statue scene; null in every other room (#2486).</summary>
        internal TourianUnlockBehavior? TourianUnlockEffects => PresentOwner<TourianUnlockBehavior>(self);

        /// <summary>Spore Spawn, Brinstar's mini-boss, which swings its core and releases spores.</summary>
        internal SporeSpawnBehavior SporeSpawnBehavior => Owner<SporeSpawnBehavior>(self);

        /// <summary>The Wrecked Ship's sparks, which flicker on and off and drop falling sparks.</summary>
        internal SparkBehavior SparkBehavior => Owner<SparkBehavior>(self);

        /// <summary>The Skulteras, skull-faced fish that swim back and forth in Maridia.</summary>
        internal SkulteraBehavior SkulteraBehavior => Owner<SkulteraBehavior>(self);

        /// <summary>The Skrees, which hang from ceilings, dive at Samus and burst on the ground.</summary>
        internal SkreeBehavior SkreeBehavior => Owner<SkreeBehavior>(self);

        /// <summary>The shutters, the gates that rise, fall or grow out of the walls on their own.</summary>
        internal ShuttersBehavior ShuttersBehavior => Owner<ShuttersBehavior>(self);

        /// <summary>The Baby Metroid in Mother Brain's fight, which drains her and gives Samus its life.</summary>
        internal BabyMetroidBehavior BabyMetroidBehavior => Owner<BabyMetroidBehavior>(self);

        /// <summary>Shaktool, the Maridia digger that clears the sand from its tunnel.</summary>
        internal ShaktoolBehavior ShaktoolBehavior => Owner<ShaktoolBehavior>(self);

        /// <summary>The Sbugs, small bugs that wake when Samus comes near and dart about her.</summary>
        internal SbugBehavior SbugBehavior => Owner<SbugBehavior>(self);

        /// <summary>The Rippers, armoured flyers that cross the room and turn at walls, the jet-powered ones among them.</summary>
        internal RipperBehavior RipperBehavior => Owner<RipperBehavior>(self);

        /// <summary>The Rios, Brinstar's flyers that swoop at Samus from the ceiling.</summary>
        internal RioBehavior RioBehavior => Owner<RioBehavior>(self);

        /// <summary>The Rinkas, the rings that spawn in Tourian and fly at Samus.</summary>
        internal RinkaBehavior RinkaBehavior => Owner<RinkaBehavior>(self);

        /// <summary>Ridley in Lower Norfair: his fight and his death.</summary>
        internal NorfairRidleyBehavior NorfairRidleyBehavior => Owner<NorfairRidleyBehavior>(self);

        /// <summary>The Puyos, Norfair's small blobs that hop along floors and ceilings.</summary>
        internal PuyoBehavior PuyoBehavior => Owner<PuyoBehavior>(self);

        /// <summary>The room before Phantoon's, whose setup holds one enemy-projectile place empty for its first updates.</summary>
        internal PrePhantoonRoomBehavior PrePhantoonRoomBehavior => Owner<PrePhantoonRoomBehavior>(self);

        /// <summary>The Powamps, Maridia's balloon creatures that inflate and shoot spikes.</summary>
        internal PowampBehavior PowampBehavior => Owner<PowampBehavior>(self);

        /// <summary>The Polyps, which throw rocks out of Norfair's lava.</summary>
        internal PolypBehavior PolypBehavior => Owner<PolypBehavior>(self);

        /// <summary>The Trippers and suspensor platforms Samus rides.</summary>
        internal PlatformsBehavior PlatformsBehavior => Owner<PlatformsBehavior>(self);

        /// <summary>The pipe bugs, the Zebs, Zebbos, Gamets and Geegas that stream out of pipes at Samus.</summary>
        internal PipeBugsBehavior PipeBugsBehavior => Owner<PipeBugsBehavior>(self);

        /// <summary>Phantoon, the Wrecked Ship's boss.</summary>
        internal PhantoonBehavior PhantoonBehavior => Owner<PhantoonBehavior>(self);

        /// <summary>The Owtches, spiky creatures that crawl over Maridia's sand.</summary>
        internal OwtchBehavior OwtchBehavior => Owner<OwtchBehavior>(self);

        /// <summary>The wall of old Tourian's escape shaft, which the escape blows open.</summary>
        internal OldTourianEscapeShaftWallBehavior OldTourianEscapeShaftWallBehavior => Owner<OldTourianEscapeShaftWallBehavior>(self);

        /// <summary>The Nuclear Waffles, Lower Norfair's chains of fire that sweep an arc.</summary>
        internal NuclearWaffleBehavior NuclearWaffleBehavior => Owner<NuclearWaffleBehavior>(self);

        /// <summary>Norfair's Rios, which swoop at Samus from the ceiling.</summary>
        internal GerutaBehavior GerutaBehavior => Owner<GerutaBehavior>(self);

        /// <summary>The enemies that leap out of Norfair's lava and fall back in.</summary>
        internal SqueeptBehavior SqueeptBehavior => Owner<SqueeptBehavior>(self);

        /// <summary>Maridia's glass tube, which a power bomb shatters.</summary>
        internal NoobTubeBehavior NoobTubeBehavior => Owner<NoobTubeBehavior>(self);

        /// <summary>The ninja space pirates, which leap across their rooms and throw their claws.</summary>
        internal NinjaSpacePirateBehavior NinjaSpacePirateBehavior => Owner<NinjaSpacePirateBehavior>(self);

        /// <summary>The Multiviolas, Norfair's fireballs that bounce around their rooms.</summary>
        internal MultiviolaBehavior MultiviolaBehavior => Owner<MultiviolaBehavior>(self);

        /// <summary>Mother Brain, the final boss.</summary>
        internal MotherBrainBehavior MotherBrainBehavior => Owner<MotherBrainBehavior>(self);

        /// <summary>The wall-mounted eyes that track Samus and shine their beam on her.</summary>
        internal MorphBallEyeBehavior MorphBallEyeBehavior => Owner<MorphBallEyeBehavior>(self);

        /// <summary>The Mochtroids, Maridia's Metroid look-alikes.</summary>
        internal MochtroidBehavior MochtroidBehavior => Owner<MochtroidBehavior>(self);

        /// <summary>The Metroids of Tourian, which latch on to Samus and drain her energy.</summary>
        internal MetroidBehavior MetroidBehavior => Owner<MetroidBehavior>(self);

        /// <summary>The Metarees, which drop from ceilings and drill into the floor.</summary>
        internal MetareeBehavior MetareeBehavior => Owner<MetareeBehavior>(self);

        /// <summary>Maridia's large snails, which crawl over the room's surfaces.</summary>
        internal OumBehavior OumBehavior => Owner<OumBehavior>(self);

        /// <summary>Maridia's mama turtle and her babies.</summary>
        internal MamaTurtleBehavior MamaTurtleBehavior => Owner<MamaTurtleBehavior>(self);

        /// <summary>The Magdollites, which rise out of Lower Norfair's lava and throw fire.</summary>
        internal MagdolliteBehavior MagdolliteBehavior => Owner<MagdolliteBehavior>(self);

        /// <summary>Lower Norfair's Rios, which swoop at Samus from the ceiling.</summary>
        internal HoltzBehavior HoltzBehavior => Owner<HoltzBehavior>(self);

        /// <summary>The Kzans, spiked crushers that drop and rise again, carrying Samus on top.</summary>
        internal KzanBehavior KzanBehavior => Owner<KzanBehavior>(self);

        /// <summary>Kraid, Brinstar's boss.</summary>
        internal KraidBehavior KraidBehavior => Owner<KraidBehavior>(self);

        /// <summary>The Ki-Hunters, winged insects that swoop at Samus, spit acid and fight on once their wings are shot off.</summary>
        internal KiHunterBehavior KiHunterBehavior => Owner<KiHunterBehavior>(self);

        /// <summary>The Kagos, hives that release swarms of bugs when shot.</summary>
        internal KagoBehavior KagoBehavior => Owner<KagoBehavior>(self);

        /// <summary>The Sidehoppers and Dessgeegas, which hop along floors, walls and ceilings.</summary>
        internal HoppersBehavior HoppersBehavior => Owner<HoppersBehavior>(self);

        /// <summary>The Hibashis, Norfair's pillars of fire that erupt from the floor.</summary>
        internal HibashiBehavior HibashiBehavior => Owner<HibashiBehavior>(self);

        /// <summary>The Funes and Namihes, heads in Norfair's walls that spit fireballs.</summary>
        internal FuneNamiheBehavior FuneNamiheBehavior => Owner<FuneNamiheBehavior>(self);

        /// <summary>The Mellows, Mellas and Memus, swarms of flies that circle and dive at Samus.</summary>
        internal FliesBehavior FliesBehavior => Owner<FliesBehavior>(self);

        /// <summary>The Firefleas of Lower Norfair, whose deaths darken the room.</summary>
        internal FirefleaBehavior FirefleaBehavior => Owner<FirefleaBehavior>(self);

        /// <summary>The sparks that fall from the Wrecked Ship's sparks and skitter along the floor.</summary>
        internal FallingSparksBehavior FallingSparksBehavior => Owner<FallingSparksBehavior>(self);

        /// <summary>Fake Kraid, the small Kraid in the room before Kraid's.</summary>
        internal FakeKraidBehavior FakeKraidBehavior => Owner<FakeKraidBehavior>(self);

        /// <summary>The eyes guarding some doors, which fire at Samus until she destroys them.</summary>
        internal EyeDoorBehavior EyeDoorBehavior => Owner<EyeDoorBehavior>(self);

        /// <summary>The Evirs, Maridia's shrimp that throw their arms at Samus.</summary>
        internal EvirBehavior EvirBehavior => Owner<EvirBehavior>(self);

        /// <summary>The Etecoons, the friendly animals that show Samus the wall jump.</summary>
        internal EtecoonBehavior EtecoonBehavior => Owner<EtecoonBehavior>(self);

        /// <summary>The explosions that go off around Samus during the escape.</summary>
        internal EscapeExplosionsBehavior EscapeExplosionsBehavior => Owner<EscapeExplosionsBehavior>(self);

        /// <summary>The Dachora and Etecoons escaping Zebes, and the wall Samus can free them through.</summary>
        internal EscapeAnimalsBehavior EscapeAnimalsBehavior => Owner<EscapeAnimalsBehavior>(self);

        /// <summary>The elevators that carry Samus between areas.</summary>
        internal ElevatorBehavior ElevatorBehavior => Owner<ElevatorBehavior>(self);

        /// <summary>Draygon, Maridia's boss, with the turrets on its room's walls.</summary>
        internal DraygonBehavior DraygonBehavior => Owner<DraygonBehavior>(self);

        /// <summary>The Dragons, which rise out of Norfair's lava and spit fireballs.</summary>
        internal DragonBehavior DragonBehavior => Owner<DragonBehavior>(self);

        /// <summary>The gates that close downward over a passage and the gates already closed.</summary>
        internal DownwardGateBehavior DownwardGateBehavior => Owner<DownwardGateBehavior>(self);

        /// <summary>The drained corpses lying in Tourian, which crumble away when disturbed.</summary>
        internal DeadTourianBehavior DeadTourianBehavior => Owner<DeadTourianBehavior>(self);

        /// <summary>The drained Torizo lying in Tourian, which crumbles away when disturbed.</summary>
        internal DeadTorizoBehavior DeadTorizoBehavior => Owner<DeadTorizoBehavior>(self);

        /// <summary>The giant Sidehopper the Baby Metroid drains in Tourian, and the corpse it leaves.</summary>
        internal DeadSidehopperBehavior DeadSidehopperBehavior => Owner<DeadSidehopperBehavior>(self);

        /// <summary>The Dachora, the friendly animal that shows Samus the shinespark.</summary>
        internal DachoraBehavior DachoraBehavior => Owner<DachoraBehavior>(self);

        /// <summary>Crocomire, Norfair's boss.</summary>
        internal CrocomireBehavior CrocomireBehavior => Owner<CrocomireBehavior>(self);

        /// <summary>The Geemers, Zeelas and Sovas that crawl around the room's surfaces.</summary>
        internal CrawlersBehavior CrawlersBehavior => Owner<CrawlersBehavior>(self);

        /// <summary>The Chozo statues that come to life: the one that carries Samus and the one that drains the acid.</summary>
        internal ChozoStatueBehavior ChozoStatueBehavior => Owner<ChozoStatueBehavior>(self);

        /// <summary>The Choots, Maridia's flat fish that leap when Samus comes near and glide back down.</summary>
        internal ChootBehavior ChootBehavior => Owner<ChootBehavior>(self);

        /// <summary>Ridley in Ceres station: his fight over the Baby Metroid and his getaway.</summary>
        internal CeresRidleyBehavior CeresRidleyBehavior => Owner<CeresRidleyBehavior>(self);

        /// <summary>Ceres station's doors and walls, and the quake that shakes them during the escape.</summary>
        internal CeresDoorBehavior CeresDoorBehavior => Owner<CeresDoorBehavior>(self);

        /// <summary>The Cacatacs, Maridia's cacti that shoot spikes.</summary>
        internal CacatacBehavior CacatacBehavior => Owner<CacatacBehavior>(self);

        /// <summary>The Bulls, floating creatures that drift after Samus and recoil from shots.</summary>
        internal BullBehavior BullBehavior => Owner<BullBehavior>(self);

        /// <summary>The Boyons, Norfair's jelly blobs that bounce in place.</summary>
        internal BoyonBehavior BoyonBehavior => Owner<BoyonBehavior>(self);

        /// <summary>The boulders that roll and bounce down at Samus.</summary>
        internal BoulderBehavior BoulderBehavior => Owner<BoulderBehavior>(self);

        /// <summary>Botwoon, Maridia's mini-boss, with its body and spit.</summary>
        internal BotwoonBehavior BotwoonBehavior => Owner<BotwoonBehavior>(self);

        /// <summary>The Torizos, Bomb Torizo and Golden Torizo.</summary>
        internal TorizoBehavior TorizoBehavior => Owner<TorizoBehavior>(self);

        /// <summary>The face blocks in Blue Brinstar and their turning animation.</summary>
        internal BlueBrinstarFaceBlockBehavior BlueBrinstarFaceBlockBehavior => Owner<BlueBrinstarFaceBlockBehavior>(self);

        /// <summary>The Beetoms, which leap at Samus, latch on and drain her energy.</summary>
        internal BeetomBehavior BeetomBehavior => Owner<BeetomBehavior>(self);

        /// <summary>The Baby Metroid's meeting with Samus in Tourian, where it latches on and lets her go.</summary>
        internal BabyMetroidCutsceneBehavior BabyMetroidCutsceneBehavior => Owner<BabyMetroidCutsceneBehavior>(self);

        /// <summary>The Atomics, the Wrecked Ship's spinning atoms that drift after Samus.</summary>
        internal AtomicBehavior AtomicBehavior => Owner<AtomicBehavior>(self);

        /// <summary>The Alcoons, Norfair's lizards that leap out at Samus and spit fireballs.</summary>
        internal AlcoonBehavior AlcoonBehavior => Owner<AlcoonBehavior>(self);
    }
}
