using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Enemies;

using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.WorkRobots;
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
        internal void ResolveKraidProjectileHits(
            SamusProjectileSystem projectiles,
            SamusBombProjectileSystem samusBombs)
        {
            Ensure.NotNull(projectiles);
            Ensure.NotNull(samusBombs);
            var kraid = self.Kraid;
            RoomEnemyPlace body = self.Places.InUpdateOrder[0];
            if (kraid is null || body.Occupant?.DefinitionId != EnemyDefinitionId.Kraid ||
                body.Presence.HasAny(EnemyPresence.Deleted))
                return;
            IReadOnlyList<KraidBehavior.KraidPassProjectile> tested = KraidBehavior.TestedByShotPasses(projectiles, samusBombs);
            self.KraidBehavior.Combat.ResolveKraidMouthProjectileHits(body.HeldOccupant(), kraid, tested);
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

        /// <summary>Typed state for every physical slot currently owned by a crawler family.</summary>
        internal IReadOnlyList<CrawlerEnemyState?> CrawlerStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<CrawlerEntity, CrawlerEnemyState>())];

        /// <summary>Typed state for powered and deactivated Work Robots in all physical slots.</summary>
        internal IReadOnlyList<WorkRobotEnemyState?> WorkRobotStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<WorkRobotEntity, WorkRobotEnemyState>())];

        /// <summary>Typed state for each live or previously allocated physical enemy slot.</summary>
        internal IReadOnlyList<ZebetiteEnemyState?> ZebetiteStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<ZebetiteEntity, ZebetiteEnemyState>())];

        /// <summary>Typed state for all physical places occupied by a Fune.</summary>
        internal IReadOnlyList<FuneEnemyState?> FuneStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<FuneEntity, FuneEnemyState>())];

        /// <summary>Typed state for all physical places occupied by a Namihe.</summary>
        internal IReadOnlyList<NamiheEnemyState?> NamiheStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<NamiheEntity, NamiheEnemyState>())];

        /// <summary>Typed state for every physical slot currently owned by this family.</summary>
        internal IReadOnlyList<SqueeptState?> SqueeptStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<SqueeptEntity, SqueeptState>())];

        /// <summary>The room's live sprite objects, in spawn order.</summary>
        internal IReadOnlyList<RoomEffectActor> RoomEffectActors => [.. self.EffectActors.Live];

        /// <summary>The live Spore Spawn extension, or null outside its room.</summary>
        /// <remarks>His body holds his encounter from its initialization (#2231), so the check finds it there.</remarks>
        internal SporeSpawnEnemyState? SporeSpawn =>
            self.Places.InUpdateOrder.Select(place => place.SpeciesState<SporeSpawnEnemyState>()).FirstOrDefault(state => state is not null);

        /// <summary>Nuclear Waffle extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<NuclearWaffleEnemyState?> NuclearWaffleStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<NuclearWaffleEntity, NuclearWaffleEnemyState>())];

        /// <summary>Dead Zoomer/Ripper/Skree state indexed by physical enemy slot.</summary>
        internal IReadOnlyList<DeadTourianCorpseEnemyState?> DeadTourianCorpses =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<DeadTourianEntity, DeadTourianCorpseEnemyState>())];

        /// <summary>Magdollite extensions indexed by physical enemy slot.</summary>
        internal IReadOnlyList<MagdollitePartState?> MagdolliteStates =>
            [.. self.Places.InUpdateOrder.Select(place => (place.Entity as MagdollitePartEntity)?.State)];

        /// <summary>The state of the one Baby Metroid the room has placed, each Baby Metroid owning its own (#2030).</summary>
        /// <exception cref="InvalidOperationException">The room has placed no Baby Metroid, or more than one.</exception>
        internal BabyMetroidEnemyState BabyMetroid =>
            self.Places.InUpdateOrder.Select(place => place.EntityState<BabyMetroidEntity, BabyMetroidEnemyState>()).OfType<BabyMetroidEnemyState>().Single();

        /// <summary>Native $40-byte slot offsets selected by the most recent activity scan.</summary>
        internal IReadOnlyList<RoomEnemyOccupant> ActiveEnemies => self.ProcessingWindow.ActiveEnemies;

        /// <summary>Native slot offsets admitted to solid-enemy interaction.</summary>
        internal IReadOnlyList<RoomEnemyOccupant> InteractiveEnemies => self.ProcessingWindow.InteractiveEnemies;

        /// <summary>Typed Dragon state by physical enemy slot, including cosmetic wing slots.</summary>
        internal IReadOnlyList<DragonEnemyState?> DragonStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.EntityState<DragonEntity, DragonEnemyState>())];

        /// <summary>Typed Dragon body or wing state for every physical slot either occupies.</summary>
        internal IReadOnlyList<DragonPartState?> DragonPartStates =>
            [.. self.Places.InUpdateOrder.Select(place => (DragonPartState?)place.EntityState<DragonEntity, DragonEnemyState>() ?? place.EntityState<DragonWingEntity, DragonWingState>())];

        /// <summary>The Brinstar Pipe Bug state of each slot that holds a Zeb or Zebbo.</summary>
        internal IReadOnlyList<BrinstarPipeBugState?> BrinstarPipeBugStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<BrinstarPipeBugState>())];

        /// <summary>The Norfair Pipe Bug state of each slot that holds a Gamet.</summary>
        internal IReadOnlyList<NorfairPipeBugState?> NorfairPipeBugStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<NorfairPipeBugState>())];

        /// <summary>The Ki-Hunter wing state of each slot that holds wings.</summary>
        internal IReadOnlyList<KiHunterWingState?> KiHunterWingStates =>
            [.. self.Places.InUpdateOrder.Select(place => place.SpeciesState<KiHunterWingState>())];

        /// <summary>The state the room's Torizo, Bomb or Golden, owns as its entity's species state, or null in a room without one.</summary>
        internal TorizoEnemyState? Torizo =>
            self.Places.InUpdateOrder.Select(place => place.SpeciesState<TorizoEnemyState>()).OfType<TorizoEnemyState>().SingleOrDefault();

        /// <summary>The room's Bomb Torizo's state, or null in a room without one.</summary>
        internal TorizoEnemyState? BombTorizo => self.Torizo is { Variant: BombTorizoVariant } torizo ? torizo : null;

        /// <summary>The room's Golden Torizo's state, or null in a room without one.</summary>
        internal TorizoEnemyState? GoldenTorizo => self.Torizo is { Variant: GoldenTorizoVariant } torizo ? torizo : null;

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
            self.AdvanceEnemyProjectileAnimations(level, samus, camera, SuperMetroid.Core.Compatibility.ShortFrameCount.Zero.After(frame), samusBombs ?? SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateBombSystem(), frameCounter: frame);
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
        internal void ResolveRidleySamusContact(SamusState samus)
        {
            // Each body resolves its own contact with the encounter it holds (#2231).
            RoomEnemyPlace body = self.Places.InUpdateOrder.First(place => place.Entity is SuperMetroid.Core.Bosses.Ridley.RidleyEntity);
            SuperMetroid.Core.Bosses.Ridley.RidleyEncounterState ridley = SuperMetroid.Core.Bosses.Ridley.RidleyEntityConstants.EncounterOf(body);
            self.RidleyBody.Contact.ResolveSamusContact(body.HeldOccupant(), samus, RoomEnemyFrameFixtures.StandalonePseudoScrew());
            SuperMetroid.Core.Bosses.Ridley.RidleyTail.ResolveRidleyTailSamusContact(body, ridley, samus);
        }
    }
}
