using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Enemies;

using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

/// <summary>Verification access to <see cref="RoomEnemySystem"/> members production does not use.</summary>
internal static class RoomEnemySystemAccess
{
    /// <summary>How many frames a check's standalone projectile clock counts before it returns to zero, as a byte does.</summary>
    private const int StandaloneProjectileClockPeriod = 256;

    /// <summary>Each enemy system's standalone projectile clock, which a check's projectile pass steps when it names no frame.</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<RoomEnemySystem, System.Runtime.CompilerServices.StrongBox<int>>
        StandaloneProjectileClocks = [];

    /// <summary>Exposes live enemy-slot state and focused collision, projectile, and draw steps for gameplay verification.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>
        /// The next frame of the system's standalone projectile clock, 0 through 255, for a check's projectile pass that
        /// names no frame: the clock advances one frame a pass, as the byte-wide frame counter does.
        /// </summary>
        internal int NextStandaloneProjectileFrame()
        {
            System.Runtime.CompilerServices.StrongBox<int> clock = StandaloneProjectileClocks.GetValue(self, _ => new(0));
            int frame = clock.Value;
            clock.Value = (frame + 1) % StandaloneProjectileClockPeriod;
            return frame;
        }

        /// <summary>
        /// Runs the native mouth pass followed by the outer mouth/body pass outside Kraid's AI.
        /// <c>MainAI_Kraid</c> itself interleaves palette handling between the two passes.
        /// </summary>
        internal int ResolveKraidProjectileHits(
            SamusProjectileSystem projectiles,
            SamusBombProjectileSystem sharedProjectiles)
        {
            Ensure.NotNull(projectiles);
            Ensure.NotNull(sharedProjectiles);
            var kraid = self.Kraid;
            RoomEnemyPlace body = self.Places.InUpdateOrder[0];
            if (kraid is null || body.DefinitionId != EnemyDefinitionId.Kraid ||
                body.Presence.HasAny(EnemyPresence.Deleted))
                return 0;
            IReadOnlyList<KraidBehavior.KraidPassProjectile> tested = KraidBehavior.TestedByShotPasses(projectiles, sharedProjectiles);
            return self.KraidBehavior.Combat.ResolveKraidMouthProjectileHits(body, kraid, tested) +
                self.KraidBehavior.Combat.ResolveKraidBodyProjectileHits(body, kraid, tested);
        }

        /// <summary>Per-place Evir states, indexed by enemy place.</summary>
        internal IReadOnlyList<EvirBodyState?> EvirStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<EvirBodyState>())];

        /// <summary>The enemy drops waiting in the projectile pool: each drop an enemy makes takes a pickup's place there.</summary>
        internal int ActiveEnemyDropPickups =>
            self.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);

        /// <summary>Companion/corpse state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadSidehopperEnemyState?> DeadSidehoppers =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<DeadSidehopperEntity, DeadSidehopperEnemyState>())];

        /// <summary>Dead Torizo corpse state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadTorizoEnemyState?> DeadTorizos =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<DeadTorizoEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by a Skree.</summary>
        internal IReadOnlyList<SkreeEnemyState?> SkreeStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<SkreeEntity, SkreeEnemyState>())];

        /// <summary>Typed Spark state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SparkEnemyState?> SparkStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<SparkEntity, SparkEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by a Zoa.</summary>
        internal IReadOnlyList<ZoaEnemyState?> ZoaStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<ZoaEntity, ZoaEnemyState>())];

        /// <summary>Typed state for both graphics and hitbox records in all physical slots.</summary>
        internal IReadOnlyList<HibashiEnemyState?> HibashiStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<HibashiEntity, HibashiEnemyState>())];

        /// <summary>Typed Hibashi collision-part state for every physical slot a collision part occupies.</summary>
        internal IReadOnlyList<HibashiCollisionPartState?> HibashiCollisionPartStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<HibashiCollisionPartEntity, HibashiCollisionPartState>())];

        /// <summary>Typed actor extensions in fixed native slot order.</summary>
        internal IReadOnlyList<ElevatorEnemyState?> ElevatorStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<ElevatorEntity, ElevatorEnemyState>())];

        /// <summary>Kago extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<KagoEnemyState?> KagoStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<KagoEntity, KagoEnemyState>())];

        /// <summary>Typed Waver state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<WaverEnemyState?> WaverStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<WaverEntity, WaverEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by a crawler family.</summary>
        internal IReadOnlyList<CrawlerEnemyState?> CrawlerStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<CrawlerEntity, CrawlerEnemyState>())];

        /// <summary>Typed state for powered and deactivated Work Robots in all physical slots.</summary>
        internal IReadOnlyList<WorkRobotEnemyState?> WorkRobotStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<WorkRobotEntity, WorkRobotEnemyState>())];

        /// <summary>Typed state for every physical enemy slot currently owned by a Rio.</summary>
        internal IReadOnlyList<RioEnemyState?> RioStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<RioEntity, RioEnemyState>())];

        /// <summary>Typed state for each live or previously allocated physical enemy slot.</summary>
        internal IReadOnlyList<ZebetiteEnemyState?> ZebetiteStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<ZebetiteEntity, ZebetiteEnemyState>())];

        /// <summary>
        /// Per-physical-slot Shaktool state. A live retail group occupies seven consecutive
        /// non-null entries; every other room slot remains null.
        /// </summary>
        internal IReadOnlyList<ShaktoolSegmentState?> ShaktoolSegments =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<ShaktoolSegmentState>())];

        /// <summary>Typed state for every physical slot currently owned by a Mochtroid.</summary>
        internal IReadOnlyList<MochtroidEnemyState?> MochtroidStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<MochtroidEntity, MochtroidEnemyState>())];

        /// <summary>Typed state for each physical slot currently occupied by a Yard.</summary>
        internal IReadOnlyList<YardEnemyState?> YardStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<YardEntity, YardEnemyState>())];

        /// <summary>Typed Metaree state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MetareeEnemyState?> MetareeStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<MetareeEntity, MetareeEnemyState>())];

        /// <summary>Typed state for every physical Owtch-capable enemy slot.</summary>
        internal IReadOnlyList<OwtchEnemyState?> OwtchStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<OwtchEntity, OwtchEnemyState>())];

        /// <summary>Typed state for all physical places occupied by a Fune.</summary>
        internal IReadOnlyList<FuneEnemyState?> FuneStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<FuneEntity, FuneEnemyState>())];

        /// <summary>Typed state for all physical places occupied by a Namihe.</summary>
        internal IReadOnlyList<NamiheEnemyState?> NamiheStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<NamiheEntity, NamiheEnemyState>())];

        /// <summary>Fake Kraid extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<FakeKraidEnemyState?> FakeKraidStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<FakeKraidEntity, FakeKraidEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by this family.</summary>
        internal IReadOnlyList<NorfairLavaJumpingEnemyState?> NorfairLavaJumpingEnemyStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<NorfairLavaJumpingEnemyEntity, NorfairLavaJumpingEnemyState>())];

        /// <summary>Typed Squeept follower state for every physical slot a follower occupies.</summary>
        internal IReadOnlyList<NorfairLavaJumperFollowerState?> NorfairLavaJumperFollowerStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<NorfairLavaJumperFollowerEntity, NorfairLavaJumperFollowerState>())];

        /// <summary>Typed Fireflea state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<FirefleaEnemyState?> FirefleaStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<FirefleaEntity, FirefleaEnemyState>())];

        /// <summary>The room's live sprite objects, in spawn order.</summary>
        internal IReadOnlyList<RoomEffectActor> RoomEffectActors => [.. self.EffectActors.Live];

        /// <summary>The live Spore Spawn extension, or null outside its room.</summary>
        /// <remarks>His body holds his encounter from its initialization (#2231), so the check finds it there.</remarks>
        internal SporeSpawnEnemyState? SporeSpawn =>
            self.Places.InUpdateOrder.Select(place => place.SpeciesState<SporeSpawnEnemyState>()).FirstOrDefault(state => state is not null);

        /// <summary>The Boyon state of each physical slot's occupant, or null where no initialized Boyon occupies it.</summary>
        internal IReadOnlyList<BoyonEnemyState?> BoyonStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<BoyonEntity, BoyonEnemyState>())];

        /// <summary>Typed state for each physical escape-Etecoon slot.</summary>
        internal IReadOnlyList<EscapeEtecoonEnemyState?> EscapeEtecoonStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<EscapeEtecoonEntity, EscapeEtecoonEnemyState>())];

        /// <summary>Typed state for each physical escape-Dachora slot.</summary>
        internal IReadOnlyList<EscapeDachoraEnemyState?> EscapeDachoraStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<EscapeDachoraEntity, EscapeDachoraEnemyState>())];

        /// <summary>Nuclear Waffle extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<NuclearWaffleEnemyState?> NuclearWaffleStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<NuclearWaffleEntity, NuclearWaffleEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by the hopper family.</summary>
        internal IReadOnlyList<HopperEnemyState?> HopperStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<HoppersEntity, HopperEnemyState>())];

        /// <summary>Typed face-block state in fixed physical enemy-slot order.</summary>
        internal IReadOnlyList<BlueBrinstarFaceBlockEnemyState?> BlueBrinstarFaceBlockStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<BlueBrinstarFaceBlockEntity, BlueBrinstarFaceBlockEnemyState>())];

        /// <summary>Dead Zoomer/Ripper/Skree state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadTourianCorpseEnemyState?> DeadTourianCorpses =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<DeadTourianEntity, DeadTourianCorpseEnemyState>())];

        /// <summary>Typed Atomic state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AtomicEnemyState?> AtomicStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<AtomicEntity, AtomicEnemyState>())];

        /// <summary>Typed state for every physical Cacatac-capable enemy slot.</summary>
        internal IReadOnlyList<CacatacEnemyState?> CacatacStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<CacatacEntity, CacatacEnemyState>())];

        /// <summary>Typed state for every physical enemy slot currently owned by a Rinka.</summary>
        internal IReadOnlyList<RinkaEnemyState?> RinkaStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<RinkaEntity, RinkaEnemyState>())];

        /// <summary>Walking Space Pirate extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<WalkingSpacePirateEnemyState?> WalkingSpacePirateStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>())];

        /// <summary>Wall Space Pirate extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<WallSpacePirateEnemyState?> WallSpacePirateStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<WallSpacePirateEntity, WallSpacePirateEnemyState>())];

        /// <summary>Magdollite extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<MagdollitePartState?> MagdolliteStates =>
            [.. self.Places.InUpdateOrder.Select(place => (place.Entity as MagdollitePartEntity)?.State)];

        /// <summary>Typed state for each physical enemy slot currently owned by a Metroid.</summary>
        internal IReadOnlyList<MetroidEnemyState?> MetroidStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<MetroidEntity, MetroidEnemyState>())];

        /// <summary>The state of the one Shitroid the room has placed, each Shitroid owning its own (#2030).</summary>
        /// <exception cref="InvalidOperationException">The room has placed no Shitroid, or more than one.</exception>
        internal ShitroidEnemyState Shitroid =>
            self.Places.InUpdateOrder.Select(place => place.EntityState<ShitroidEntity, ShitroidEnemyState>()).OfType<ShitroidEnemyState>().Single();

        /// <summary>Native $40-byte slot offsets selected by the most recent activity scan.</summary>
        internal IReadOnlyList<RoomEnemyPlace> ActiveEnemies => self.ProcessingWindow.ActiveEnemies;

        /// <summary>Typed native state for every physical Etecoon slot.</summary>
        internal IReadOnlyList<EtecoonEnemyState?> EtecoonStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<EtecoonEntity, EtecoonEnemyState>())];

        /// <summary>Native slot offsets admitted to solid-enemy interaction.</summary>
        internal IReadOnlyList<RoomEnemyPlace> InteractiveEnemies => self.ProcessingWindow.InteractiveEnemies;

        /// <summary>Typed Alcoon state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AlcoonEnemyState?> AlcoonStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<AlcoonEntity, AlcoonEnemyState>())];

        /// <summary>Typed Dragon state by physical enemy slot, including cosmetic wing slots.</summary>
        internal IReadOnlyList<DragonEnemyState?> DragonStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<DragonEntity, DragonEnemyState>())];

        /// <summary>Typed Dragon wing state for every physical slot a wing occupies.</summary>
        internal IReadOnlyList<DragonWingState?> DragonWingStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<DragonWingEntity, DragonWingState>())];

        /// <summary>Typed Dragon body or wing state for every physical slot either occupies.</summary>
        internal IReadOnlyList<DragonPartState?> DragonPartStates =>
            [.. self.Places.InUpdateOrder.Select(place => (DragonPartState?)place.EntityState<DragonEntity, DragonEnemyState>() ?? place.EntityState<DragonWingEntity, DragonWingState>())];

        /// <summary>Typed state for all 32 physical Puyo-capable enemy slots.</summary>
        internal IReadOnlyList<PuyoEnemyState?> PuyoStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<PuyoEntity, PuyoEnemyState>())];

        /// <summary>Typed Bull state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BullEnemyState?> BullStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<BullEntity, BullEnemyState>())];

        /// <summary>Typed Skultera state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SkulteraEnemyState?> SkulteraStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<SkulteraEntity, SkulteraEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by Sbug/Sbug2.</summary>
        internal IReadOnlyList<SbugEnemyState?> SbugStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<SbugEntity, SbugEnemyState>())];

        /// <summary>The Brinstar Pipe Bug state of each slot that holds a Zeb or Zebbo.</summary>
        internal IReadOnlyList<BrinstarPipeBugState?> BrinstarPipeBugStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<BrinstarPipeBugState>())];

        /// <summary>The Norfair Pipe Bug state of each slot that holds a Gamet.</summary>
        internal IReadOnlyList<NorfairPipeBugState?> NorfairPipeBugStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<NorfairPipeBugState>())];

        /// <summary>The Ki-Hunter body state of each slot that holds a body.</summary>
        internal IReadOnlyList<KiHunterBodyState?> KiHunterBodyStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<KiHunterBodyState>())];

        /// <summary>The Ki-Hunter wing state of each slot that holds wings.</summary>
        internal IReadOnlyList<KiHunterWingState?> KiHunterWingStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<KiHunterWingState>())];

        /// <summary>Typed Choot state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<ChootEnemyState?> ChootStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<ChootEntity, ChootEnemyState>())];

        /// <summary>Typed native state for every loaded Yapping Maw slot.</summary>
        internal IReadOnlyList<YappingMawEnemyState?> YappingMawStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<YappingMawEntity, YappingMawEnemyState>())];

        /// <summary>Typed Oum state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MaridiaLargeSnailEnemyState?> MaridiaLargeSnailStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<MaridiaLargeSnailEntity, MaridiaLargeSnailEnemyState>())];

        /// <summary>Typed Beetom state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BeetomEnemyState?> BeetomStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<BeetomEntity, BeetomEnemyState>())];

        /// <summary>Typed Tripper/Kamer state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<PlatformEnemyState?> PlatformStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<PlatformsEntity, PlatformEnemyState>())];

        /// <summary>The state the room's Torizo, Bomb or Golden, owns as its entity's species state, or null in a room without one.</summary>
        internal TorizoEnemyState? Torizo =>
            self.Places.InUpdateOrder.Select(place => place.SpeciesState<TorizoEnemyState>()).OfType<TorizoEnemyState>().SingleOrDefault();

        /// <summary>The room's Bomb Torizo's state, or null in a room without one.</summary>
        internal TorizoEnemyState? BombTorizo => self.Torizo is { Variant: BombTorizoVariant } torizo ? torizo : null;

        /// <summary>The room's Golden Torizo's state, or null in a room without one.</summary>
        internal TorizoEnemyState? GoldenTorizo => self.Torizo is { Variant: GoldenTorizoVariant } torizo ? torizo : null;

        /// <summary>Growing shutter extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<GrowingShutterEnemyState?> GrowingShutterStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<ShuttersEntity, GrowingShutterEnemyState>())];

        /// <summary>Number of live actors in the shared bank-$86 enemy-projectile pool.</summary>
        internal int ActiveEnemyProjectileCount =>
            self.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);

        /// <summary>
        /// Executes the projectile portion of the gameplay frame after enemy instructions have
        /// had their opportunity to spawn a fireball. This is the same producer/consumer order
        /// as EnemyMain followed by bank $86's enemy-projectile handler.
        /// </summary>
        internal void StepEnemyProjectiles(
            RoomLevelData level,
            SamusState samus,
            Position16<RoomPixels> camera = default,
            int? frameCounterByte = null,
            SamusBombProjectileSystem? samusBombs = null)
        {
            // A check that names no frame steps the system's own standalone clock, one frame a pass.
            int frame = frameCounterByte ?? self.NextStandaloneProjectileFrame();
            self.AdvanceEnemyProjectileAnimations(level, samus, camera, frame, samusBombs ?? SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateBombSystem(), frameCounter: frame);
            self.Projectiles.ResolveEnemyProjectileSamusHits(samus);
        }

        /// <summary>
        /// Draws the projectiles in front of Samus and then those behind her, for focused diagnostics which do
        /// not draw Samus between them. Gameplay calls the two depth-specific methods around Samus.
        /// </summary>
        internal void DrawEnemyProjectiles(OamBuffer oam, Position16<RoomPixels> camera, bool timeIsFrozen = false)
        {
            Ensure.NotNull(oam);

            self.EffectActors.DrawRoomEffectActorsAsOam(oam, camera);
            self.Projectiles.DrawEnemyProjectilesAtAsOam(oam, camera, EnemyProjectileDepth.InFrontOfSamus, timeIsFrozen);
            self.Projectiles.DrawEnemyProjectilesAtAsOam(oam, camera, EnemyProjectileDepth.BehindSamus, timeIsFrozen);
        }

        /// <summary>Standalone contact probe; runtime dispatches body before AI and tail during tail update.</summary>
        internal bool ResolveRidleySamusContact(SamusState samus)
        {
            // Each body resolves its own contact with the encounter it holds (#2231).
            RoomEnemyPlace body = self.Places.InUpdateOrder.First(place => place.Entity is SuperMetroid.Core.Bosses.Ridley.RidleyEntity);
            SuperMetroid.Core.Bosses.Ridley.RidleyEncounterState ridley = SuperMetroid.Core.Bosses.Ridley.RidleyEntityConstants.EncounterOf(body);
            return self.RidleyBody.Contact.ResolveSamusContact(body, samus, RoomEnemyFrameFixtures.StandalonePseudoScrew()) ||
                SuperMetroid.Core.Bosses.Ridley.RidleyTail.ResolveRidleyTailSamusContact(body, ridley, samus);
        }
    }
}
