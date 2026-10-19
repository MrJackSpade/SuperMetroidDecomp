using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
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
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Areas.Ceres;

/// <summary>Verification access to <see cref="RoomEnemySystem"/> members production does not use.</summary>
internal static class RoomEnemySystemAccess
{
    /// <summary>Exposes live enemy-slot state and focused collision, projectile, and draw steps for gameplay verification.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>
        /// Runs the native mouth pass followed by the outer mouth/body pass outside Kraid's AI.
        /// <c>MainAI_Kraid</c> itself interleaves palette handling between the two passes.
        /// </summary>
        internal int ResolveKraidProjectileHits(
            ISnesAddressSpace bus,
            SamusProjectileSystem projectiles,
            SamusBombProjectileSystem sharedProjectiles)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(projectiles);
            Ensure.NotNull(sharedProjectiles);
            var kraid = self.KraidBehavior.Kraid;
            RoomEnemySlot body = self.SlotTable.Slots[0];
            if (kraid is null || body.EnemyDefinitionPointer != EnemyDefinitionId.Kraid ||
                body.Properties.HasAny(EnemyProperties.Deleted))
                return 0;
            var shots = new KraidBehavior.KraidShotSlots(projectiles, sharedProjectiles);
            return self.KraidBehavior.ResolveKraidMouthProjectileHits(body, kraid, shots) +
                self.KraidBehavior.ResolveKraidBodyProjectileHits(body, kraid, shots);
        }

        /// <summary>Botwoon's live state while its room is loaded.</summary>
        internal BotwoonEnemyState? Botwoon => self.BotwoonBehavior._botwoonState;

        /// <summary>Per-slot Evir states, indexed by enemy slot.</summary>
        internal IReadOnlyList<EvirEnemyState?> EvirStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.SpeciesState<EvirEnemyState>())];

        /// <summary>Special drops a dying Metroid has requested this frame.</summary>
        internal IReadOnlyList<MetroidDropRequest> MetroidDropRequests =>
            self.MetroidBehavior._metroidDropRequests;

        /// <summary>Companion/corpse state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadSidehopperEnemyState?> DeadSidehoppers =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<DeadSidehopperEntity, DeadSidehopperEnemyState>())];

        /// <summary>The loaded Dead Torizo's typed extended-WRAM state.</summary>
        internal DeadTorizoEnemyState? DeadTorizo => self.DeadTorizoBehavior._deadTorizo;

        /// <summary>Alternating VRAM records authored by the most recent actor frame.</summary>
        internal IReadOnlyList<VramWriteEntry> LastDeadTorizoVramTransfers =>
            self.DeadTorizoBehavior._deadTorizoFrameVramTransfers;

        /// <summary>Typed state for every physical slot currently owned by a Skree.</summary>
        internal IReadOnlyList<SkreeEnemyState?> SkreeStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<SkreeEntity, SkreeEnemyState>())];

        /// <summary>Typed Spark state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SparkEnemyState?> SparkStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<SparkEntity, SparkEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by a Zoa.</summary>
        internal IReadOnlyList<ZoaEnemyState?> ZoaStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<ZoaEntity, ZoaEnemyState>())];

        /// <summary>Typed state for both graphics and hitbox records in all physical slots.</summary>
        internal IReadOnlyList<HibashiEnemyState?> HibashiStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<HibashiEntity, HibashiEnemyState>())];

        /// <summary>Typed actor extensions in fixed native slot order.</summary>
        internal IReadOnlyList<ElevatorEnemyState?> ElevatorStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<ElevatorEntity, ElevatorEnemyState>())];

        /// <summary>Kago extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<KagoEnemyState?> KagoStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<KagoEntity, KagoEnemyState>())];

        /// <summary>Typed Waver state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<WaverEnemyState?> WaverStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<WaverEntity, WaverEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by a crawler family.</summary>
        internal IReadOnlyList<CrawlerEnemyState?> CrawlerStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<CrawlerEntity, CrawlerEnemyState>())];

        /// <summary>Typed state for powered and deactivated Work Robots in all physical slots.</summary>
        internal IReadOnlyList<WorkRobotEnemyState?> WorkRobotStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<WorkRobotEntity, WorkRobotEnemyState>())];

        /// <summary>Typed state for every physical enemy slot currently owned by a Rio.</summary>
        internal IReadOnlyList<RioEnemyState?> RioStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<RioEntity, RioEnemyState>())];

        /// <summary>Typed state for each live or previously allocated physical enemy slot.</summary>
        internal IReadOnlyList<ZebetiteEnemyState?> ZebetiteStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<ZebetiteEntity, ZebetiteEnemyState>())];

        /// <summary>
        /// Per-physical-slot Shaktool state. A live retail group occupies seven consecutive
        /// non-null entries; every other room slot remains null.
        /// </summary>
        internal IReadOnlyList<ShaktoolSegmentState?> ShaktoolSegments =>
            [.. self.SlotTable.Slots.Select(slot => slot.SpeciesState<ShaktoolSegmentState>())];

        /// <summary>Typed state for every physical slot currently owned by a Mochtroid.</summary>
        internal IReadOnlyList<MochtroidEnemyState?> MochtroidStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<MochtroidEntity, MochtroidEnemyState>())];

        /// <summary>Typed state for each physical slot currently occupied by a Yard.</summary>
        internal IReadOnlyList<YardEnemyState?> YardStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<YardEntity, YardEnemyState>())];

        /// <summary>Typed Metaree state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MetareeEnemyState?> MetareeStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<MetareeEntity, MetareeEnemyState>())];

        /// <summary>Typed state for every physical Owtch-capable enemy slot.</summary>
        internal IReadOnlyList<OwtchEnemyState?> OwtchStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<OwtchEntity, OwtchEnemyState>())];

        /// <summary>Typed state for all physical slots occupied by Fune or Namihe.</summary>
        internal IReadOnlyList<FuneNamiheEnemyState?> FuneNamiheStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<FuneNamiheEntity, FuneNamiheEnemyState>())];

        /// <summary>Typed Stoke state for each of the 32 physical enemy slots.</summary>
        internal IReadOnlyList<StokeEnemyState?> StokeStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<StokeEntity, StokeEnemyState>())];

        /// <summary>Fake Kraid extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<FakeKraidEnemyState?> FakeKraidStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<FakeKraidEntity, FakeKraidEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by this family.</summary>
        internal IReadOnlyList<NorfairLavaJumpingEnemyState?> NorfairLavaJumpingEnemyStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<NorfairLavaJumpingEnemyEntity, NorfairLavaJumpingEnemyState>())];

        /// <summary>Typed Fireflea state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<FirefleaEnemyState?> FirefleaStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<FirefleaEntity, FirefleaEnemyState>())];

        /// <summary>All 32 physical bank-$B4 slots, including inactive ones.</summary>
        internal IReadOnlyList<RoomSpriteObjectSlot> RoomSpriteObjects => self.SpriteObjects.Slots;

        /// <summary>The live Spore Spawn extension, or null outside its room.</summary>
        internal SporeSpawnEnemyState? SporeSpawn => self.SporeSpawnBehavior._sporeSpawn;

        /// <summary>The Boyon state of each physical slot's occupant, or null where no initialized Boyon occupies it.</summary>
        internal IReadOnlyList<BoyonEnemyState?> BoyonStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<BoyonEntity, BoyonEnemyState>())];

        /// <summary>Typed state for each physical escape-Etecoon slot.</summary>
        internal IReadOnlyList<EscapeEtecoonEnemyState?> EscapeEtecoonStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<EscapeEtecoonEntity, EscapeEtecoonEnemyState>())];

        /// <summary>Each Ceres door slot's typed program cursor.</summary>
        internal IReadOnlyDictionary<int, AnimationProgramCursor<CeresDoorProgram>> CeresDoorPrograms =>
            self.CeresDoorBehavior._doorPrograms;

        /// <summary>Each Ceres steam vent slot's typed program cursor.</summary>
        internal IReadOnlyDictionary<int, AnimationProgramCursor<CeresSteamProgram>> CeresSteamPrograms =>
            self.CeresSteamBehavior._ventPrograms;

        /// <summary>Typed state for each physical escape-Dachora slot.</summary>
        internal IReadOnlyList<EscapeDachoraEnemyState?> EscapeDachoraStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<EscapeDachoraEntity, EscapeDachoraEnemyState>())];

        /// <summary>Nuclear Waffle extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<NuclearWaffleEnemyState?> NuclearWaffleStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<NuclearWaffleEntity, NuclearWaffleEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by the hopper family.</summary>
        internal IReadOnlyList<HopperEnemyState?> HopperStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<HoppersEntity, HopperEnemyState>())];

        /// <summary>Typed face-block state in fixed physical enemy-slot order.</summary>
        internal IReadOnlyList<BlueBrinstarFaceBlockEnemyState?> BlueBrinstarFaceBlockStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<BlueBrinstarFaceBlockEntity, BlueBrinstarFaceBlockEnemyState>())];

        /// <summary>Frame-local drops requested by destroyed free-floating spores.</summary>
        internal IReadOnlyList<SporeSpawnDropRequest> SporeSpawnDropRequests =>
            self.SporeSpawnBehavior._sporeSpawnDropRequests;

        /// <summary>Dead Zoomer/Ripper/Skree state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadTourianCorpseEnemyState?> DeadTourianCorpses =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<DeadTourianEntity, DeadTourianCorpseEnemyState>())];

        /// <summary>Typed Atomic state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AtomicEnemyState?> AtomicStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<AtomicEntity, AtomicEnemyState>())];

        /// <summary>Typed state for every physical Cacatac-capable enemy slot.</summary>
        internal IReadOnlyList<CacatacEnemyState?> CacatacStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<CacatacEntity, CacatacEnemyState>())];

        /// <summary>Typed state for every physical enemy slot currently owned by a Rinka.</summary>
        internal IReadOnlyList<RinkaEnemyState?> RinkaStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<RinkaEntity, RinkaEnemyState>())];

        /// <summary>Walking Space Pirate extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<WalkingSpacePirateEnemyState?> WalkingSpacePirateStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<WalkingSpacePirateEntity, WalkingSpacePirateEnemyState>())];

        /// <summary>Wall Space Pirate extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<WallSpacePirateEnemyState?> WallSpacePirateStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<WallSpacePirateEntity, WallSpacePirateEnemyState>())];

        /// <summary>Magdollite extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<MagdolliteEnemyState?> MagdolliteStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<MagdolliteEntity, MagdolliteEnemyState>())];

        /// <summary>Typed state for each physical enemy slot currently owned by a Metroid.</summary>
        internal IReadOnlyList<MetroidEnemyState?> MetroidStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<MetroidEntity, MetroidEnemyState>())];

        /// <summary>Baby Metroid actor state during the Mother Brain encounter.</summary>
        internal ShitroidEnemyState? Shitroid => self.ShitroidBehavior._shitroid;

        /// <summary>Native $40-byte slot offsets selected by the most recent activity scan.</summary>
        internal IReadOnlyList<int> ActiveEnemyIndexes => self.ProcessingWindow._activeEnemyIndexes;

        /// <summary>Typed native state for every physical Etecoon slot.</summary>
        internal IReadOnlyList<EtecoonEnemyState?> EtecoonStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<EtecoonEntity, EtecoonEnemyState>())];

        /// <summary>Native slot offsets admitted to solid-enemy interaction.</summary>
        internal IReadOnlyList<int> InteractiveEnemyIndexes => self.ProcessingWindow._interactiveEnemyIndexes;

        /// <summary>Typed Alcoon state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<AlcoonEnemyState?> AlcoonStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<AlcoonEntity, AlcoonEnemyState>())];

        /// <summary>Typed Dragon state by physical enemy slot, including cosmetic wing slots.</summary>
        internal IReadOnlyList<DragonEnemyState?> DragonStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<DragonEntity, DragonEnemyState>())];

        /// <summary>The room's terminated bank-$B4 graphics-set records.</summary>
        internal IReadOnlyList<RoomEnemyGraphicsSetEntry> GraphicsSet => self.PopulationLoader._graphicsSet;

        /// <summary>Typed state for all 32 physical Puyo-capable enemy slots.</summary>
        internal IReadOnlyList<PuyoEnemyState?> PuyoStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<PuyoEntity, PuyoEnemyState>())];

        /// <summary>Typed Bull state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BullEnemyState?> BullStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<BullEntity, BullEnemyState>())];

        /// <summary>Typed Skultera state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<SkulteraEnemyState?> SkulteraStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<SkulteraEntity, SkulteraEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by Sbug/Sbug2.</summary>
        internal IReadOnlyList<SbugEnemyState?> SbugStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<SbugEntity, SbugEnemyState>())];

        /// <summary>Typed state for every physical Pipe Bug enemy slot.</summary>
        internal IReadOnlyList<PipeBugEnemyState?> PipeBugStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.SpeciesState<PipeBugEnemyState>())];

        /// <summary>Typed Ki-Hunter state for every body and wing physical enemy slot.</summary>
        internal IReadOnlyList<KiHunterEnemyState?> KiHunterStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.SpeciesState<KiHunterEnemyState>())];

        /// <summary>Typed Choot state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<ChootEnemyState?> ChootStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<ChootEntity, ChootEnemyState>())];

        /// <summary>Pickup requests emitted by shot destroyable flames in this room load.</summary>
        internal IReadOnlyList<PhantoonFlameDropRequest> PhantoonFlameDropRequests =>
            self.PhantoonBehavior._phantoonFlameDropRequests;

        /// <summary>Typed native state for every loaded Yapping Maw slot.</summary>
        internal IReadOnlyList<YappingMawEnemyState?> YappingMawStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<YappingMawEntity, YappingMawEnemyState>())];

        /// <summary>Typed Oum state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<MaridiaLargeSnailEnemyState?> MaridiaLargeSnailStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<MaridiaLargeSnailEntity, MaridiaLargeSnailEnemyState>())];

        /// <summary>Typed Beetom state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<BeetomEnemyState?> BeetomStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<BeetomEntity, BeetomEnemyState>())];

        /// <summary>Typed Tripper/Kamer state for all 32 physical enemy slots.</summary>
        internal IReadOnlyList<PlatformEnemyState?> PlatformStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<PlatformsEntity, PlatformEnemyState>())];

        /// <summary>Bomb Torizo's state while definition $EEFF owns slot zero.</summary>
        internal TorizoEnemyState? BombTorizo =>
            self.BombTorizoBehavior._torizoState is { IsGolden: false } ? self.BombTorizoBehavior._torizoState : null;

        /// <summary>Growing shutter extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<GrowingShutterEnemyState?> GrowingShutterStates =>
            [.. self.SlotTable.Slots.Select(slot => slot.EntityState<ShuttersEntity, GrowingShutterEnemyState>())];

        /// <summary>Pickup requests emitted by shot Chozo orbs during this room load.</summary>
        internal IReadOnlyList<TorizoOrbDropRequest> TorizoOrbDropRequests =>
            self.BombTorizoBehavior._torizoOrbDropRequests;

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
            SamusState? samus,
            Position16<RoomPixels> camera = default,
            int? nmiFrameCounter8 = null,
            SamusBombProjectileSystem? samusBombs = null)
        {
            self.StepEnemyProjectileInstructions(level, samus, camera, nmiFrameCounter8, samusBombs);
            self.Projectiles.ResolveEnemyProjectileSamusHits(samus);
        }

        /// <summary>
        /// Emits both native enemy-projectile priority passes for focused diagnostics which do
        /// not draw Samus between them. Gameplay must call the two phase-specific methods so
        /// property bit <c>$1000</c> determines which side of Samus owns each OAM record.
        /// </summary>
        internal void DrawEnemyProjectiles(OamBuffer oam, Position16<RoomPixels> camera, bool timeIsFrozen = false)
        {
            Ensure.NotNull(oam);
            self.Room.EnsureLoaded();

            self.SpriteObjects.DrawRoomSpriteObjectsAsOam(oam, camera);
            self.Projectiles.DrawEnemyProjectilePassAsOam(oam, camera, EnemyProjectileDrawPriority.High, timeIsFrozen);
            self.Projectiles.DrawEnemyProjectilePassAsOam(oam, camera, EnemyProjectileDrawPriority.Low, timeIsFrozen);
        }

        /// <summary>Standalone contact probe; runtime dispatches body before AI and tail during tail update.</summary>
        internal bool ResolveRidleySamusContact(SamusState samus) =>
            (bool)self.CeresRidleyBehavior.ResolveRidleyBodySamusContact(samus) ||
            (bool)self.RidleyTail.ResolveRidleyTailSamusContact(samus);
    }
}
