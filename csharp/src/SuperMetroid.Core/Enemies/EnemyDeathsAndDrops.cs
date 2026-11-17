using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy deaths and item drops: generic death explosions, respawn placeholders, drop selection and scatter,
/// and pickup collection with its resource restoration. It is the death explosions' and pickups' projectile behaviour
/// (#1801): it runs the projectiles it spawns.
/// </summary>
[RoomLifetime]
internal sealed class EnemyDeathsAndDrops
    : EnemyProjectileMotionBehavior<SingleMotion>, IEnemyProjectileBranchingCues<EnemyDeathProjectileSegment, EnemyDeathProjectileCue>
{
    /// <summary>Tracks the room's cumulative enemy-kill count.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Allocates death explosions and direct item-drop projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies RNG and enemy definitions for drop selection.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Reinitializes a respawned enemy from its saved population entry.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Receives the pickup and death sounds, each queued as the drop or death animation makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects death animation, pickup, and respawn processing to the room systems.</summary>
    internal EnemyDeathsAndDrops(EnemyRoomStatus roomStatus, EnemyProjectileEngine projectiles, EnemyRoomBinding room,
        IEnemyLifecycle lifecycle, PowerBombRadiusAfterKill powerBombRadius, EnemyAudioQueue audio)
        : base(StartingMotions)
    {
        _audio = audio;
        _roomStatus = roomStatus;
        _projectiles = projectiles;
        _room = room;
        _lifecycle = lifecycle;
        _powerBombRadius = powerBombRadius;
    }
    /// <summary>A pickup starts counting its lifetime; a death explosion holds still while its animation plays.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, SingleMotion?> StartingMotions = new()
    {
        [RoomEnemyProjectileKind.EnemyDeathPickup] = SingleMotion.Moving,
        [RoomEnemyProjectileKind.EnemyDeathExplosion] = null,
    };

    /// <summary>Requests a collected drop's sound; it joins the queue only when the sound queue is idle.</summary>
    private void RequestPickupSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.WhenIdle);

    /// <summary>
    /// Requests a death animation's sound (the enemy-killed cry, the small explosion or the contact-killed sound); it joins
    /// the queue only when the sound queue is idle.
    /// </summary>
    private void RequestDeathSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.WhenIdle);

    /// <summary>The enemy-library enemy-killed cry the death explosion queues.</summary>
    internal static readonly SoundEffectId EnemyKilledSound = EnemySound.EnemyKilled;

    /// <summary>Tells a power bomb's sweep in progress of each kill, which resizes its horizontal reach.</summary>
    private readonly PowerBombRadiusAfterKill _powerBombRadius;

    /// <summary>
    /// Tells a power bomb's sweep in progress that <paramref name="enemy"/> is dying, as every death does when it starts
    /// (<see cref="PowerBombRadiusAfterKill"/>).
    /// </summary>
    internal void RecordKill(RoomEnemyPlace enemy) =>
        _powerBombRadius.RecordKill(enemy.Presence.HasAny(EnemyPresence.RespawnsIfKilled));

    /// <summary>
    /// Starts an enemy's generic death. The enemy is cleared immediately,
    /// while the death explosion retains the definition, position, and optional respawn index until
    /// its authored animation reaches its become-pickup cue.
    /// </summary>
    /// <param name="enemy">The dying enemy.</param>
    /// <param name="deathAnimation">The explosion it dies in.</param>
    /// <param name="samus">Samus, whose grapple a grappled enemy's death drops.</param>
    internal void StartGenericEnemyDeath(
        RoomEnemyPlace enemy,
        EnemyDeathExplosion deathAnimation,
        SamusState samus)
    {
        Ensure.NotNull(samus);
        RecordKill(enemy);
        // The death puts the grapple into its dropped phase before clearing the enemy.
        // Only an enemy the grapple kills outright counts, not one it reacts to otherwise.
        if (enemy.AiStates == EnemyAiStates.Grappled)
            samus.Grapple.Phase = GrapplePhase.Dropped;
        RemoveKilledEnemy(enemy, deathAnimation, respawns: enemy.Presence.HasAny(EnemyPresence.RespawnsIfKilled));
        _roomStatus.RecordKill();
    }

    /// <summary>
    /// Removes a killed enemy from its place, the one removal every death runs (#2256): its death explosion starts where
    /// it stood, naming the enemy and whether it respawns, and the place is emptied. The enemy's population entry survives
    /// the kill (<see cref="RoomEnemyPlace.Clear"/>); a respawning enemy's place stays taken until the explosion's respawn
    /// cue rebuilds the enemy from that entry. When every projectile place is held the explosion is lost, and with it the
    /// enemy's drop and its only respawn cue.
    /// </summary>
    /// <param name="enemy">The killed enemy.</param>
    /// <param name="deathAnimation">The explosion it dies in.</param>
    /// <param name="respawns">Whether the explosion's respawn cue rebuilds the enemy.</param>
    internal void RemoveKilledEnemy(RoomEnemyPlace enemy, EnemyDeathExplosion deathAnimation, bool respawns)
    {
        Ensure.NotNull(enemy);
        EnemyProjectile? explosion = _projectiles.AllocateEnemyProjectile();
        if (explosion is not null)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                explosion,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                DeathProjectiles,
                graphics: default);
            explosion.Position = explosion.Position.WithWhole(enemy.Position.Whole);
            explosion.DropDefinitionId = enemy.DefinitionId;
            explosion.KilledEnemy = KilledEnemy.Of(enemy, respawns);
            explosion.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathExplosions.Animation(deathAnimation)));
        }

        enemy.Clear();
        if (respawns)
            enemy.AwaitRespawn();
    }

    /// <summary>Chooses the definition's death explosion, raising the smaller ones for Super Missile kills.</summary>
    internal static EnemyDeathExplosion SelectNormalShotDeathAnimation(
        RoomEnemyPlace enemy,
        SamusProjectileType projectileType,
        ShotDeathAnimationRule rule = ShotDeathAnimationRule.ByProjectile)
    {
        if (rule == ShotDeathAnimationRule.PirateBigExplosion)
            return EnemyDeathExplosion.Big;

        return projectileType.Family == SamusProjectileFamily.SuperMissile
            ? EnemyDeathExplosions.RaisedBySuperMissile(enemy.Definition.DeathExplosion)
            : enemy.Definition.DeathExplosion;
    }

    /// <summary>
    /// The twelve item drops' shared behaviour.
    /// Each pickup advances RNG once for both coordinates; spawning the pickup
    /// and choosing its drop then advance it on their own inside the pickup. Both scatters read
    /// their fields from the one sample.
    /// </summary>
    internal void SpawnEnemyDropScatter(
        EnemyDefinitionId definitionId,
        int count,
        int xBase,
        RandomSampleField xScatter,
        int yBase,
        RandomSampleField yScatter,
        SamusState samus) =>
        ScatterDrops(count, xBase, xScatter, yBase, yScatter, position => SpawnEnemyDropFromDefinition(position, definitionId, samus));

    /// <summary>
    /// The twelve item drops' shared behaviour, each rolling <paramref name="dropChances"/> (#2244): a fight whose drops
    /// roll a table of their own rather than an enemy's.
    /// </summary>
    internal void SpawnEnemyDropScatter(
        EnemyDropChances dropChances,
        int count,
        int xBase,
        RandomSampleField xScatter,
        int yBase,
        RandomSampleField yScatter,
        SamusState samus) =>
        ScatterDrops(count, xBase, xScatter, yBase, yScatter, position => SpawnEnemyDropFromChanceTable(position, dropChances, samus));

    /// <summary>
    /// Spawns <paramref name="count"/> drops by <paramref name="spawn"/>, each at the base plus the two scatters of one
    /// random sample drawn before it spawns.
    /// </summary>
    private void ScatterDrops(
        int count,
        int xBase,
        RandomSampleField xScatter,
        int yBase,
        RandomSampleField yScatter,
        Func<Position16<RoomPixels>, EnemyProjectile?> spawn)
    {
        for (int drop = 0; drop < count; drop++)
        {
            int random = _room.Random.Generate();
            _ = spawn(new(
                CoordinateAxis.Wrap(xScatter.Of(random) + xBase),
                CoordinateAxis.Wrap(yScatter.Of(random) + yBase)));
        }
    }

    /// <summary>Scatters drops within a 32-pixel square around a defeated actor.</summary>
    internal void SpawnEnemyDropScatterAround(
        EnemyDefinitionId definitionId,
        int count,
        Position16<RoomPixels> origin,
        SamusState samus)
    {
        for (int drop = 0; drop < count; drop++)
        {
            int random = _room.Random.Generate();
            SpawnEnemyDropFromDefinition(new(
                CoordinateAxis.Wrap(origin.X + EnemyDropScatterDefinitions.HorizontalScatter.Of(random) - EnemyDropScatterDefinitions.ScatterCentring),
                CoordinateAxis.Wrap(origin.Y + EnemyDropScatterDefinitions.VerticalScatter.Of(random) - EnemyDropScatterDefinitions.ScatterCentring)), definitionId, samus);
        }
    }

    /// <summary>
    /// Allocates a pickup projectile using an ordinary enemy definition. The
    /// search for a free place, failure at the first place, random selection, and no-drop tail follow
    /// the enemy-projectile spawn and then the pickup's setup.
    /// </summary>
    internal EnemyProjectile? SpawnEnemyDropFromDefinition(
        Position16<RoomPixels> position,
        EnemyDefinitionId definitionId,
        SamusState samus)
    {
        EnemyProjectile? pickup = _projectiles.AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            DeathProjectiles,
            graphics: default);
        pickup.Position = pickup.Position.WithWhole(position);
        pickup.DropDefinitionId = definitionId;
        pickup.KilledEnemy = KilledEnemy.None;
        InitializeDirectEnemyPickup(pickup, samus);
        return pickup;
    }

    /// <summary>
    /// Allocates a pickup from a chance table already selected by specialized
    /// boss code, such as Botwoon's Draygon-eye drop cue, without manufacturing a fake enemy definition solely to reach
    /// the same six chances.
    /// </summary>
    internal EnemyProjectile? SpawnEnemyDropFromChanceTable(
        Position16<RoomPixels> position,
        EnemyDropChances? dropChances,
        SamusState samus)
    {
        EnemyProjectile? pickup = _projectiles.AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            DeathProjectiles,
            graphics: default);
        pickup.Position = pickup.Position.WithWhole(position);
        pickup.DropChancesOverride = dropChances;
        pickup.KilledEnemy = KilledEnemy.None;
        InitializeDirectEnemyPickup(pickup, samus);
        return pickup;
    }

    /// <summary>Rolls a direct drop and either starts its pickup animation or the no-drop tail.</summary>
    private void InitializeDirectEnemyPickup(EnemyProjectile pickup, SamusState samus)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(pickup, samus);

        // Unlike a finished death explosion, a direct drop becomes a pickup in any place for
        // an enemy projectile, the first included. The roll reads the drop's definition straight back from
        // the pickup that holds it.
        pickup.StartAnimation(new EnemyDeathProjectileAnimation(kind == EnemyPickupKind.NoDrop
            ? ConfigureDormantPickup(pickup)
            : ConfigurePickup(pickup, kind)));
    }

    /// <summary>
    /// Converts a completed enemy-death explosion in place and returns the animation it continues at: its drop's, or the
    /// respawn tail's. Keeping the actor in its existing place keeps both its retained killed enemy and whether it drops
    /// at all: an explosion in the first place never converts (<see cref="FirstPlaceExplosionLeavesNoPickup"/>).
    /// </summary>
    /// <param name="projectile">The finished death explosion.</param>
    /// <param name="samus">Samus, whose energy and ammunition the drop's roll reads.</param>
    internal EnemyDeathProjectileSegment ConvertEnemyDeathExplosionToPickup(EnemyProjectile projectile, SamusState samus)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(projectile, samus);
        return !FirstPlaceExplosionLeavesNoPickup.BecomesPickup(projectile.Place) || kind == EnemyPickupKind.NoDrop
            ? ConfigureDormantPickup(projectile)
            : ConfigurePickup(projectile, kind);
    }

    /// <summary>Gives the projectile the selected pickup's identity, 400-frame life and collision reaction, and returns its animation.</summary>
    internal static EnemyDeathProjectileSegment ConfigurePickup(EnemyProjectile projectile, EnemyPickupKind kind)
    {
        EnemyPickupAnimationDefinition animation = EnemyPickups.Animation(kind);
        projectile.State = new EnemyPickupState(kind, EnemyPickupDefinitions.Lifetime);
        projectile.StartMotion(SingleMotion.Moving);

        // Becoming a pickup clears the persists-on-contact property and keeps the projectile's other properties.
        projectile.PersistsOnSamusContact = false;
        return animation.Animation;
    }

    /// <summary>Switches a consumed or rejected drop to the blank, non-interactive deletion tail.</summary>
    private static void MakeEnemyPickupDormant(EnemyProjectile projectile) =>
        projectile.StartAnimation(new EnemyDeathProjectileAnimation(ConfigureDormantPickup(projectile)));

    /// <summary>Makes the projectile a blank, non-interactive drop and returns the deletion tail it runs.</summary>
    private static EnemyDeathProjectileSegment ConfigureDormantPickup(EnemyProjectile projectile)
    {
        // The blank frame lasts 64 frames before the respawn cue handles a retained respawning enemy
        // and the actor is deleted. Collection and natural expiry use this same tail.
        projectile.HoldStill();
        projectile.CanDamageSamus = false;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        return EnemyDeathExplosionDefinitions.NoDropTail;
    }

    /// <summary>
    /// The random drop selection, whose chances are counts of 0 through 255. Minor drops are renormalized into the budget
    /// left after the offered Super Missile and Power Bomb chances; the two major drops retain their own chances. Every
    /// authored table's chances sum to the whole budget, so the accumulated chances stay within twice it (#1620). The
    /// drops Samus can use are offered in order, the <see cref="EnemyDropSelectionDefinitions.MinorDrops"/> first and then
    /// the <see cref="EnemyDropSelectionDefinitions.MajorDrops"/>, and the first whose accumulated chance reaches the random
    /// draw is chosen; the draw is read before anything else, as the game reads it.
    /// </summary>
    /// <param name="projectile">The drop whose chances are rolled.</param>
    /// <param name="samus">Samus, whose energy and ammunition decide which drops are offered.</param>
    internal EnemyPickupKind SelectRandomEnemyDrop(EnemyProjectile projectile, SamusState samus)
    {
        Ensure.NotNull(samus);
        if (ResolveEnemyDropChances(projectile) is not { } table)
            return EnemyPickupKind.NoDrop;
        // Every authored table's chances sum to the whole budget, so the pooled weights and the budget left for the major
        // drops stay within it (#1620).
        if (table.Total > EnemyDropSelectionDefinitions.FullWeightBudget)
            throw new InvalidDataException($"A drop table's chances sum to {table.Total}, past the {EnemyDropSelectionDefinitions.FullWeightBudget} every chance is out of.");

        int random;
        do
        {
            random = _room.Random.Next(EnemyDropSelectionDefinitions.DropRoll);
        }
        while (random == 0);

        int majorAdjustedBudget = EnemyDropSelectionDefinitions.FullWeightBudget;
        int accumulated = 0;
        int combinedEnergy = samus.Health + samus.ReserveEnergy;
        if (combinedEnergy < EnemyDropSelectionDefinitions.CriticalEnergy)
            _room.Session.CriticalEnergyDropBias = true;
        else if (combinedEnergy >= EnemyDropSelectionDefinitions.RecoveredEnergy)
            _room.Session.CriticalEnergyDropBias = false;
        // Energy between the two deliberately retains the previous flag: this is the game's
        // hysteresis/grace period, not an omitted assignment.

        int pooledMinorWeight;
        var offered = new HashSet<EnemyPickupKind>();
        if (_room.Session.CriticalEnergyDropBias)
        {
            pooledMinorWeight = table.SmallEnergy + table.BigEnergy;
            offered.Add(EnemyPickupKind.SmallEnergy);
            offered.Add(EnemyPickupKind.BigEnergy);
        }
        else
        {
            pooledMinorWeight = table.Nothing;
            offered.Add(EnemyPickupKind.NoDrop);
            if (samus.Health != samus.MaxHealth ||
                samus.ReserveEnergy != samus.MaxReserveEnergy)
            {
                pooledMinorWeight += table.SmallEnergy + table.BigEnergy;
                offered.Add(EnemyPickupKind.SmallEnergy);
                offered.Add(EnemyPickupKind.BigEnergy);
            }
            if (samus.Missiles != samus.MaxMissiles)
            {
                pooledMinorWeight += table.Missile;
                offered.Add(EnemyPickupKind.Missile);
            }
            if (samus.SuperMissiles != samus.MaxSuperMissiles)
            {
                majorAdjustedBudget -= table.SuperMissile;
                offered.Add(EnemyPickupKind.SuperMissile);
            }
            if (samus.PowerBombs != samus.MaxPowerBombs)
            {
                majorAdjustedBudget -= table.PowerBomb;
                offered.Add(EnemyPickupKind.PowerBomb);
            }
        }

        // With no pooled minor weight the minor drops are passed over and the major drops are offered alone.
        if (pooledMinorWeight != 0)
        {
            foreach (EnemyPickupKind kind in EnemyDropSelectionDefinitions.MinorDrops)
            {
                if (!offered.Contains(kind))
                    continue;

                // A minor drop's share of the budget the major drops leave, rounded down; the shares and the major chances
                // sum to at most the whole budget twice over.
                accumulated += majorAdjustedBudget * table.Of(kind) / pooledMinorWeight;
                if (accumulated >= random)
                    return kind;
            }
        }

        foreach (EnemyPickupKind kind in EnemyDropSelectionDefinitions.MajorDrops)
        {
            if (!offered.Contains(kind))
                continue;

            accumulated += table.Of(kind);
            if (accumulated >= random)
                return kind;
        }

        return EnemyPickupKind.NoDrop;
    }

    /// <summary>
    /// Uses an explicit boss chance table, a saved enemy definition's, or none. A death explosion from a cleared enemy place
    /// drops nothing.
    /// </summary>
    private EnemyDropChances? ResolveEnemyDropChances(EnemyProjectile projectile)
    {
        if (projectile.DropChancesOverride is { } chances)
            return chances;
        return projectile.DropDefinitionId == EnemyDefinitionId.None
            ? null
            : _room.Definitions.ReadEnemyDefinition(projectile.DropDefinitionId).DropChances;
    }

    /// <summary>Decrements pickup lifetime, then tests Grapple endpoint and strict Samus body overlap.</summary>
    internal void TendEnemyPickup(
        EnemyProjectile projectile,
        SamusState samus)
    {
        // The lifetime is decremented first; the pickup expires on the update it reaches zero.
        EnemyPickupState pickup = projectile.StateOf<EnemyPickupState>();
        pickup.Lifetime--;
        if (pickup.Lifetime == 0)
        {
            MakeEnemyPickupDormant(projectile);
            return;
        }

        // The grapple query reports any grapple phase but inactive. A pickup is immune to its
        // endpoint for the first sixteen decrements, then uses strict 16-pixel axis tests.
        if (samus.Grapple.Phase != GrapplePhase.Inactive &&
            pickup.Lifetime < EnemyPickupDefinitions.Lifetime - EnemyPickupDefinitions.GrappleDelay &&
            CoordinateAxis.Distance(projectile.Position.X.Whole, samus.Grapple.Anchor.X) < EnemyPickupDefinitions.GrappleReach &&
            CoordinateAxis.Distance(projectile.Position.Y.Whole, samus.Grapple.Anchor.Y) < EnemyPickupDefinitions.GrappleReach)
        {
            CollectEnemyPickup(projectile, samus);
            return;
        }

        // The pickup touches Samus only while both axes' distances are strictly less than their
        // radii's sum. Equality is not contact in either axis.
        if (CoordinateAxis.Distance(samus.Position.X.Whole, projectile.Position.X.Whole) >=
                samus.Kinematics.Radius.X + projectile.Radius.X ||
            CoordinateAxis.Distance(samus.Position.Y.Whole, projectile.Position.Y.Whole) >=
                samus.Kinematics.Radius.Y + projectile.Radius.Y)
        {
            return;
        }

        CollectEnemyPickup(projectile, samus);
    }

    /// <summary>Applies health or ammo from a touched pickup, publishes its sound, and makes it dormant.</summary>
    private void CollectEnemyPickup(
        EnemyProjectile projectile,
        SamusState samus)
    {
        EnemyPickupKind kind = projectile.StateOf<EnemyPickupState>().Kind;
        switch (kind)
        {
            case EnemyPickupKind.SmallEnergy:
                RestoreEnemyDropEnergy(samus, EnemyPickupDefinitions.SmallEnergyAmount);
                RequestPickupSound(EnemySound.SmallEnergyPickup);
                break;
            case EnemyPickupKind.BigEnergy:
                RestoreEnemyDropEnergy(samus, EnemyPickupDefinitions.BigEnergyAmount);
                RequestPickupSound(EnemySound.BigEnergyPickup);
                break;
            case EnemyPickupKind.PowerBomb:
                RestoreEnemyDropPowerBombs(samus, EnemyPickupDefinitions.PowerBombAmount);
                RequestPickupSound(EnemySound.PowerBombPickup);
                break;
            case EnemyPickupKind.Missile:
                RestoreEnemyDropMissiles(samus, EnemyPickupDefinitions.MissileAmount);
                RequestPickupSound(EnemySound.MissilePickup);
                break;
            case EnemyPickupKind.SuperMissile:
                RestoreEnemyDropSuperMissiles(samus, EnemyPickupDefinitions.SuperMissileAmount);
                RequestPickupSound(EnemySound.SuperMissilePickup);
                break;
            case EnemyPickupKind.NoDrop:
            default:
                throw new InvalidDataException(
                    $"The enemy pickup in place {projectile.Place} holds no restorable pickup ({kind}).");
        }

        MakeEnemyPickupDormant(projectile);
    }

    /// <summary>Fills health first, spilling excess into capped reserves and enabling reserve auto mode.</summary>
    private static void RestoreEnemyDropEnergy(SamusState samus, int amount)
    {
        int restored = samus.Health + amount;
        samus.Health = restored;
        if (restored < samus.MaxHealth)
            return;

        int reserve = samus.ReserveEnergy + restored - samus.MaxHealth;
        if (reserve >= samus.MaxReserveEnergy)
            reserve = samus.MaxReserveEnergy;
        samus.ReserveEnergy = reserve;
        if (reserve != 0 && samus.ReserveTankMode == ReserveTankMode.None)
            samus.ReserveTankMode = ReserveTankMode.Auto;
        samus.Health = samus.MaxHealth;
    }

    /// <summary>Adds missiles and caps them at Samus's capacity.</summary>
    private static void RestoreEnemyDropMissiles(SamusState samus, int amount) =>
        samus.Missiles = Math.Min(samus.Missiles + amount, samus.MaxMissiles);

    /// <summary>Adds a Super Missile and clamps it to the owned maximum.</summary>
    private static void RestoreEnemyDropSuperMissiles(SamusState samus, int amount)
    {
        int restored = samus.SuperMissiles + amount;
        samus.SuperMissiles = restored;
        if (restored > samus.MaxSuperMissiles)
        {
            samus.SuperMissiles = samus.MaxSuperMissiles;
        }
    }

    /// <summary>Adds a Power Bomb and clamps it to the owned maximum.</summary>
    private static void RestoreEnemyDropPowerBombs(SamusState samus, int amount)
    {
        int restored = samus.PowerBombs + amount;
        samus.PowerBombs = restored;
        if (restored > samus.MaxPowerBombs)
        {
            samus.PowerBombs = samus.MaxPowerBombs;
        }
    }

    /// <summary>
    /// Runs the respawn cue for the respawning enemy a death actor retains.
    /// The method is deliberately population/snapshot based so later respawning families can
    /// share it without acquiring a Rinka-specific reconstruction path.
    /// </summary>
    internal void RespawnEnemyFromSnapshot(RoomEnemyPlace place)
    {
        RoomEnemySpawnSnapshot spawn = place.Spawn;
        if (spawn.Population.DefinitionId == EnemyDefinitionId.None)
        {
            throw new InvalidDataException(
                $"Enemy place {place.Ordinal} has no surviving population snapshot to respawn.");
        }

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(spawn.Population.DefinitionId);
        // Unlike the room's enemy setup and an ordinary enemy spawn, a respawn
        // never sets the frame: the actor keeps the empty frame its death left,
        // or what its init AI set, until its first cue. An empty frame is also
        // the contact gate, so the respawned actor cannot touch Samus before then.
        _lifecycle.InitializePlaceFromDefinition(place, spawn.Population, definition);
        _lifecycle.RunInitializationAi(place, EnemySpawnSurroundings.None);
    }

    /// <summary>
    /// The death explosions and the pickups they and direct drops become (#1760). Each spawner starts the explosion or
    /// pickup animation it chooses.
    /// </summary>
    internal EnemyProjectileBehavior DeathProjectiles => this;

    /// <summary>
    /// Moves a pickup, the one kind here that moves: it counts its lifetime, then tests its grapple endpoint, then Samus's
    /// body contact, and is collected on contact. A death explosion, and a pickup gone dormant, hold still.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The pickup's one motion.</param>
    private protected override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
        TendEnemyPickup(context.Projectile, context.Samus);

    /// <summary>
    /// Carries out a cue of the death and drop animations (sprite spawns, sounds, drop conversion and respawn), returning
    /// where a converted explosion continues (the start of the drop's animation), or null to run on.
    /// </summary>
    /// <param name="context">The projectile, Samus and the camera of the pass running the animation.</param>
    /// <param name="cue">The cue the animation published.</param>
    /// <param name="pastCue">The place just past the cue; a converted explosion starts its drop's animation instead.</param>
    public AnimationContinuation<EnemyDeathProjectileSegment>? RunCue(EnemyProjectileCueContext context, EnemyDeathProjectileCue cue,
        AnimationPlayback<EnemyDeathProjectileSegment> pastCue) =>
        ApplyEnemyDeathProjectileCue(context.Projectile, cue, context.Samus) is { } converted
            ? new(EnemyDeathProjectileAnimations.StartingWith(converted), AnimationPlace.StartOfAnimation)
            : null;

    /// <summary>Queues the enemy-killed sound, as the death explosion's cue does.</summary>
    internal void QueueEnemyKilledSound() => RequestDeathSound(EnemyKilledSound);

    /// <summary>
    /// Carries out one cue of the death and drop animations, returning the segment a converted explosion continues at, or
    /// null to run on.
    /// </summary>
    private EnemyDeathProjectileSegment? ApplyEnemyDeathProjectileCue(EnemyProjectile projectile, EnemyDeathProjectileCue cue,
        SamusState samus)
    {
        if (cue is EnemyDeathProjectileCue.SpawnEffectActor spawn)
        {
            _projectiles.SpawnRandomEnemyDeathSprite(projectile, spawn.EffectActor, spawn.Spread);
            return null;
        }
        EnemyDeathProjectileCueKind kind = ((EnemyDeathProjectileCue.Signal)cue).Kind;
        switch (kind)
        {
            case EnemyDeathProjectileCueKind.Delete:
                projectile.Delete();
                return null;
            case EnemyDeathProjectileCueKind.PlayEnemyKilledSound:
                // The sound-queueing cue continues to the next cue, so the following duration starts the next frame.
                QueueEnemyKilledSound();
                return null;
            case EnemyDeathProjectileCueKind.PlaySmallExplosionSound:
                RequestDeathSound(EnemySoundEffectDefinitions.DeathSmallExplosionSound);
                return null;
            case EnemyDeathProjectileCueKind.PlayContactKilledSound:
                RequestDeathSound(EnemySoundEffectDefinitions.DeathContactKilledSound);
                return null;
            case EnemyDeathProjectileCueKind.BecomePickup:
                if (projectile.Kind != RoomEnemyProjectileKind.EnemyDeathExplosion)
                    throw new InvalidDataException($"Enemy projectile {projectile.Kind} reached the become-pickup cue, which only a death explosion runs.");
                return ConvertEnemyDeathExplosionToPickup(projectile, samus);
            case EnemyDeathProjectileCueKind.HandleRespawningEnemy:
                if (projectile.KilledEnemy.Respawns)
                    RespawnEnemyFromSnapshot(projectile.KilledEnemy.Enemy ?? throw new InvalidDataException("A respawning death explosion names no enemy."));
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EnemyDeathProjectileCueKind)} {kind}.");
        }
    }
}
