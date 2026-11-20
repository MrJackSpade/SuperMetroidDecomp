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
    {
        _audio = audio;
        _roomStatus = roomStatus;
        _projectiles = projectiles;
        _room = room;
        _lifecycle = lifecycle;
        _powerBombRadius = powerBombRadius;
    }

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
        var behavior = Explosion(new EnemyDeathExplosionState(KilledEnemy.Of(enemy, respawns), enemy.DefinitionId));
        EnemyProjectile? explosion = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.EnemyDeathExplosion, behavior, SpriteGraphics.SharedStandard);
        if (explosion is not null)
        {
            explosion.Position = explosion.Position.WithWhole(enemy.Position.Whole);
            explosion.StartAnimation(new EnemyDeathExplosionAnimation(EnemyDeathExplosions.Animation(deathAnimation), behavior));
        }

        enemy.Clear();
        if (respawns)
            enemy.AwaitRespawn();
    }

    /// <summary>Chooses the definition's death explosion, raising the smaller ones for Super Missile kills.</summary>
    internal static EnemyDeathExplosion ShotKillExplosion(
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
    /// random sample drawn before it spawns. The base is a corner of a boss's arena and the scatters only add, so every
    /// drop lies in the room.
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
            _ = spawn(new(xScatter.Of(random) + xBase, yScatter.Of(random) + yBase));
        }
    }

    /// <summary>
    /// Scatters drops within a 32-pixel square around a defeated actor: the gold ninja Pirate or Fake Kraid, each of which
    /// dies well inside its room, so every drop lies in the room.
    /// </summary>
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
                origin.X + EnemyDropScatterDefinitions.HorizontalScatter.Of(random) - EnemyDropScatterDefinitions.ScatterCentring,
                origin.Y + EnemyDropScatterDefinitions.VerticalScatter.Of(random) - EnemyDropScatterDefinitions.ScatterCentring), definitionId, samus);
        }
    }

    /// <summary>
    /// Spawns a pickup whose drop chances an enemy definition gives: it takes a free projectile place as any enemy
    /// projectile does, none when no place is free, and rolls its drop, lying dormant when nothing drops.
    /// </summary>
    internal EnemyProjectile? SpawnEnemyDropFromDefinition(
        Position16<RoomPixels> position,
        EnemyDefinitionId definitionId,
        SamusState samus)
    {
        EnemyProjectile? pickup = _projectiles.AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        InitializeDirectEnemyPickup(pickup, position, DropChancesOf(definitionId), samus);
        return pickup;
    }

    /// <summary>
    /// Allocates a pickup from a chance table a fight chose itself, such as the Torizos' orbs and Spore Spawn's spores,
    /// without manufacturing a fake enemy definition solely to reach the same six chances.
    /// </summary>
    internal EnemyProjectile? SpawnEnemyDropFromChanceTable(
        Position16<RoomPixels> position,
        EnemyDropChances dropChances,
        SamusState samus)
    {
        EnemyProjectile? pickup = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.EnemyDeathPickup, drop.Behavior, SpriteGraphics.SharedStandard);
        if (pickup is null)
            return null;

        InitializeDirectEnemyPickup(pickup, position, dropChances, samus);
        return pickup;
    }

    /// <summary>
    /// Rolls a direct drop's <paramref name="dropChances"/>, none when its enemy drops nothing, and either starts its
    /// pickup animation or the ending it plays when nothing drops.
    /// </summary>
    private void InitializeDirectEnemyPickup(EnemyProjectile pickup, Position16<RoomPixels> position, EnemyDropChances? dropChances,
        SamusState samus)
    {
        // The pickup takes its definition and place, then rolls its drop. Unlike a finished death explosion, a direct drop
        // becomes a pickup in any place for an enemy projectile, the first included. The roll reads the drop's definition
        // straight back from the pickup that holds it.
        EnemyPickupKind kind = SelectRandomEnemyDrop(dropChances, samus);
        DropBinding drop = kind == EnemyPickupKind.NoDrop
            ? DropBinding.Of(new DormantDropBehavior(this, KilledEnemy.None))
            : DropBinding.Of(Pickup(new EnemyPickupState(kind, EnemyPickupDefinitions.Lifetime, KilledEnemy.None)));
        pickup.Position = pickup.Position.WithWhole(position);
        pickup.StartAnimation(new EnemyDeathAndDropAnimation(kind == EnemyPickupKind.NoDrop
            ? ConfigureDormantPickup(pickup)
            : ConfigurePickup(pickup, kind), drop.Cues));
    }

    /// <summary>A direct drop's behaviour, both as the behaviour bound to it and as the owner of its animations' cues.</summary>
    /// <param name="Behavior">The behaviour bound to the drop.</param>
    /// <param name="Cues">The same behaviour, which owns the drop's animations' cues.</param>
    private sealed record DropBinding(EnemyProjectileBehavior Behavior, IEnemyProjectileCues<EnemyDeathAndDropCue> Cues)
    {
        /// <summary>The binding of <paramref name="drop"/>.</summary>
        /// <typeparam name="TDrop">The drop's behaviour.</typeparam>
        /// <param name="drop">The drop's behaviour.</param>
        internal static DropBinding Of<TDrop>(TDrop drop)
            where TDrop : EnemyProjectileBehavior, IEnemyProjectileCues<EnemyDeathAndDropCue> => new(drop, drop);
    }

    /// <summary>
    /// Converts a completed enemy-death explosion in place into its drop and returns the drop's animation, which the
    /// explosion's play goes on in (#2554): the projectile is handed to its pickup, or to a dormant drop that shows the
    /// respawn pause. Keeping the actor in its existing place keeps both its retained killed enemy and whether it drops at
    /// all: an explosion in the first place never converts (<see cref="FirstPlaceExplosionLeavesNoPickup"/>).
    /// </summary>
    /// <param name="projectile">The finished death explosion.</param>
    /// <param name="explosion">What the explosion keeps: the enemy it came from and the definition its drop rolls by.</param>
    /// <param name="samus">Samus, whose energy and ammunition the drop's roll reads.</param>
    internal EnemyDeathAndDropAnimation ConvertEnemyDeathExplosionToPickup(EnemyProjectile projectile, EnemyDeathExplosionState explosion,
        SamusState samus)
    {
        Ensure.NotNull(projectile);
        Ensure.NotNull(explosion);
        EnemyPickupKind kind = SelectRandomEnemyDrop(DropChancesOf(explosion.DropDefinitionId), samus);
        if (!FirstPlaceExplosionLeavesNoPickup.BecomesPickup(projectile.Place) || kind == EnemyPickupKind.NoDrop)
        {
            var dormant = new DormantDropBehavior(this, explosion.KilledEnemy);
            projectile.Rebind(dormant);
            return new EnemyDeathAndDropAnimation(ConfigureDormantPickup(projectile), dormant);
        }
        var pickup = Pickup(new EnemyPickupState(kind, EnemyPickupDefinitions.Lifetime, explosion.KilledEnemy));
        projectile.Rebind(pickup);
        return new EnemyDeathAndDropAnimation(ConfigurePickup(projectile, kind), pickup);
    }

    /// <summary>
    /// Gives the projectile, whose behaviour already holds the pickup and moves it as a pickup, the selected pickup's
    /// collision reaction, and returns its animation.
    /// </summary>
    internal static EnemyDeathAndDropSegment ConfigurePickup(EnemyProjectile projectile, EnemyPickupKind kind)
    {
        EnemyPickupAnimationDefinition animation = EnemyPickups.Animation(kind);

        // A pickup is gone when Samus touches it; it keeps the explosion's other behaviours.
        projectile.PersistsOnSamusContact = false;
        return animation.Animation;
    }

    /// <summary>Leaves a collected or expired drop blank and harmless until it is removed.</summary>
    private static void MakeEnemyPickupDormant(EnemyProjectile projectile, IEnemyProjectileCues<EnemyDeathAndDropCue> cues) =>
        projectile.StartAnimation(new EnemyDeathAndDropAnimation(ConfigureDormantPickup(projectile), cues));

    /// <summary>Makes the projectile a blank, harmless drop and returns the respawn pause it shows until it is removed.</summary>
    private static EnemyDeathAndDropSegment ConfigureDormantPickup(EnemyProjectile projectile)
    {
        // The blank frame lasts its respawn pause before a retained respawning enemy respawns and the drop is removed. A
        // collected or expired pickup ends the same way.
        projectile.HoldStill();
        projectile.CanDamageSamus = false;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        return EnemyDeathExplosionDefinitions.NoDropPause;
    }

    /// <summary>
    /// The random drop selection, whose chances are counts out of <see cref="EnemyDropSelectionDefinitions.FullWeightBudget"/>. Minor drops are renormalized into the budget
    /// left after the offered Super Missile and Power Bomb chances; the two major drops retain their own chances. Every
    /// authored table's chances sum to the whole budget, so the accumulated chances stay within twice it (#1620). The
    /// drops Samus can use are offered in order, the <see cref="EnemyDropSelectionDefinitions.MinorDrops"/> first and then
    /// the <see cref="EnemyDropSelectionDefinitions.MajorDrops"/>, and the first whose accumulated chance reaches the random
    /// draw is chosen; the draw is read before anything else, as the game reads it.
    /// </summary>
    /// <param name="dropChances">The chances rolled; none when the drop's enemy drops nothing.</param>
    /// <param name="samus">Samus, whose energy and ammunition decide which drops are offered.</param>
    internal EnemyPickupKind SelectRandomEnemyDrop(EnemyDropChances? dropChances, SamusState samus)
    {
        Ensure.NotNull(samus);
        if (dropChances is not { } table)
            return EnemyPickupKind.NoDrop;
        // Every authored table's chances sum to the whole budget, so the pooled weights and the budget left for the major
        // drops stay within it (#1620).
        if (table.Total > EnemyDropSelectionDefinitions.FullWeightBudget)
            throw new InvalidDataException($"A drop table's chances sum to {table.Total}, past the {EnemyDropSelectionDefinitions.FullWeightBudget} every chance is out of.");

        int sample;
        do
        {
            sample = _room.Random.Generate();
        }
        while (EnemyDropSelectionDefinitions.DropRollDrawnAgain.IsTakenBy(sample));
        int random = EnemyDropSelectionDefinitions.DropRoll.Of(sample);

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
    /// The drop chances of <paramref name="definitionId"/>'s definition, none when it drops nothing; a death explosion from
    /// a cleared enemy place names no enemy and drops nothing.
    /// </summary>
    private EnemyDropChances? DropChancesOf(EnemyDefinitionId definitionId) =>
        definitionId == EnemyDefinitionId.None ? null : _room.Definitions.ReadEnemyDefinition(definitionId).DropChances;

    /// <summary>Decrements pickup lifetime, then tests Grapple endpoint and strict Samus body overlap.</summary>
    internal void TendEnemyPickup(
        EnemyProjectile projectile,
        EnemyPickupState pickup,
        SamusState samus,
        IEnemyProjectileCues<EnemyDeathAndDropCue> cues)
    {
        // The lifetime is counted first; the pickup expires on the update it ends.
        if (pickup.Lifetime.Count())
        {
            MakeEnemyPickupDormant(projectile, cues);
            return;
        }

        // A pickup appears where its enemy died, and an enemy can die above or left of the room's origin: a fly that has
        // climbed through the ceiling, or a Bull braking past Samus, struck by a shot that lives on past the view's edge.
        // The grapple's endpoint and Samus reach such a pickup from the room's top row or left column the short way, as
        // DistanceAcrossRoomEdge.Between gives it.
        // The grapple query reports any grapple phase but inactive. A pickup is immune to its
        // endpoint for the first sixteen decrements, then uses strict 16-pixel axis tests.
        if (samus.Grapple.Phase != GrapplePhase.Inactive &&
            pickup.Lifetime.UpdatesLeft < EnemyPickupDefinitions.Lifetime - EnemyPickupDefinitions.GrappleDelay &&
            DistanceAcrossRoomEdge.Between(projectile.Position.X.Whole, samus.Grapple.Anchor.X) < EnemyPickupDefinitions.GrappleReach &&
            DistanceAcrossRoomEdge.Between(projectile.Position.Y.Whole, samus.Grapple.Anchor.Y) < EnemyPickupDefinitions.GrappleReach)
        {
            CollectEnemyPickup(projectile, pickup.Kind, samus, cues);
            return;
        }

        // The pickup touches Samus only while both axes' distances are strictly less than their
        // radii's sum. Equality is not contact in either axis.
        if (DistanceAcrossRoomEdge.Between(samus.Position.X.Whole, projectile.Position.X.Whole) >=
                samus.Kinematics.Radius.X + projectile.Radius.X ||
            DistanceAcrossRoomEdge.Between(samus.Position.Y.Whole, projectile.Position.Y.Whole) >=
                samus.Kinematics.Radius.Y + projectile.Radius.Y)
        {
            return;
        }

        CollectEnemyPickup(projectile, pickup.Kind, samus, cues);
    }

    /// <summary>Applies health or ammo from a touched pickup, publishes its sound, and makes it dormant.</summary>
    private void CollectEnemyPickup(
        EnemyProjectile projectile,
        EnemyPickupKind kind,
        SamusState samus,
        IEnemyProjectileCues<EnemyDeathAndDropCue> cues)
    {
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

        MakeEnemyPickupDormant(projectile, cues);
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
    /// Runs the respawn cue for the respawning enemy a death actor retains: the enemy is set up again from the population
    /// it spawned from.
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
        _lifecycle.RespawnEnemy(place);
    }


    /// <summary>What a drop's behaviour answers for its animations' cues: the enemy it came from.</summary>
    private interface IDropCues
    {
        /// <summary>The enemy the drop came from, which its respawn cue rebuilds when it respawns.</summary>
        KilledEnemy KilledEnemy { get; }
    }

    /// <summary>The behaviour of a death explosion keeping <paramref name="explosion"/>.</summary>
    /// <param name="explosion">What the explosion keeps: the enemy it came from and the definition its drop rolls by.</param>
    internal ExplosionBehavior Explosion(EnemyDeathExplosionState explosion) => new(this, explosion);

    /// <summary>The behaviour of a pickup keeping <paramref name="pickup"/>.</summary>
    /// <param name="pickup">What the pickup keeps: its kind, lifetime and the enemy it came from.</param>
    internal PickupBehavior Pickup(EnemyPickupState pickup) => new(this, pickup);

    /// <summary>A death explosion, holding the enemy it came from and the definition its drop rolls by.</summary>
    /// <param name="drops">The room's deaths and drops, which run the explosion's cues.</param>
    /// <param name="explosion">What the explosion keeps.</param>
    internal sealed class ExplosionBehavior(EnemyDeathsAndDrops drops, EnemyDeathExplosionState explosion)
        : EnemyProjectileBehavior, IEnemyProjectileHandingOverCues<EnemyDeathExplosionCue>
    {
        /// <inheritdoc/>
        public EnemyProjectileAnimation? RunCue(EnemyProjectileCueContext context, EnemyDeathExplosionCue cue) =>
            drops.ApplyEnemyDeathExplosionCue(context.Projectile, explosion, cue, context.Samus);
    }

    /// <summary>A pickup, holding what it restores, how long it lasts and the enemy it came from; it waits to be collected.</summary>
    /// <param name="drops">The room's deaths and drops, which tend the pickup and run its cues.</param>
    /// <param name="pickup">What the pickup keeps.</param>
    internal sealed class PickupBehavior(EnemyDeathsAndDrops drops, EnemyPickupState pickup)
        : EnemyProjectileMotionBehavior<SingleMotion>(initial: SingleMotion.Moving),
            IEnemyProjectileCues<EnemyDeathAndDropCue>, IDropCues
    {
        /// <inheritdoc/>
        public KilledEnemy KilledEnemy => pickup.KilledEnemy;

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            drops.TendEnemyPickup(context.Projectile, pickup, context.Samus, this);

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EnemyDeathAndDropCue cue) => drops.ApplyEnemyDeathAndDropCue(context.Projectile, this, cue);
    }

    /// <summary>A drop that rolled no pickup, or a collected one: it holds still through its pause, then ends.</summary>
    /// <param name="drops">The room's deaths and drops, which run the drop's cues.</param>
    /// <param name="killedEnemy">The enemy the drop came from.</param>
    private sealed class DormantDropBehavior(EnemyDeathsAndDrops drops, KilledEnemy killedEnemy)
        : EnemyProjectileBehavior, IEnemyProjectileCues<EnemyDeathAndDropCue>, IDropCues
    {
        /// <inheritdoc/>
        public KilledEnemy KilledEnemy => killedEnemy;

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, EnemyDeathAndDropCue cue) => drops.ApplyEnemyDeathAndDropCue(context.Projectile, this, cue);
    }

    /// <summary>Queues the enemy-killed sound, as the death explosion's cue does.</summary>
    internal void QueueEnemyKilledSound() => RequestDeathSound(EnemyKilledSound);

    /// <summary>
    /// Carries out one cue of <paramref name="explosion"/>'s animation, returning the drop's animation its play goes on in
    /// once it becomes its drop, or null to run on.
    /// </summary>
    private EnemyDeathAndDropAnimation? ApplyEnemyDeathExplosionCue(EnemyProjectile projectile, EnemyDeathExplosionState explosion,
        EnemyDeathExplosionCue cue, SamusState samus)
    {
        switch (cue)
        {
            case EnemyDeathExplosionCue.SpawnEffectActor spawn:
                _projectiles.SpawnRandomEnemyDeathSprite(projectile, spawn.EffectActor, spawn.Spread);
                return null;
            case EnemyDeathExplosionCue.Signal { Kind: EnemyDeathExplosionCueKind.PlayEnemyKilledSound }:
                // The sound-queueing cue continues to the next cue, so the following duration starts the next frame.
                QueueEnemyKilledSound();
                return null;
            case EnemyDeathExplosionCue.Signal { Kind: EnemyDeathExplosionCueKind.PlaySmallExplosionSound }:
                RequestDeathSound(EnemySoundEffectDefinitions.DeathSmallExplosionSound);
                return null;
            case EnemyDeathExplosionCue.Signal { Kind: EnemyDeathExplosionCueKind.PlayContactKilledSound }:
                RequestDeathSound(EnemySoundEffectDefinitions.DeathContactKilledSound);
                return null;
            case EnemyDeathExplosionCue.Signal { Kind: EnemyDeathExplosionCueKind.BecomePickup }:
                return ConvertEnemyDeathExplosionToPickup(projectile, explosion, samus);
            default:
                throw new InvalidOperationException($"Undefined {nameof(EnemyDeathExplosionCue)} {cue}.");
        }
    }

    /// <summary>Carries out one cue of the animation of <paramref name="drop"/>, the drop a death explosion became or a direct drop.</summary>
    private void ApplyEnemyDeathAndDropCue(EnemyProjectile projectile, IDropCues drop, EnemyDeathAndDropCue cue)
    {
        switch (cue)
        {
            case EnemyDeathAndDropCue.Signal { Kind: EnemyDeathAndDropCueKind.Delete }:
                projectile.Delete();
                return;
            case EnemyDeathAndDropCue.Signal { Kind: EnemyDeathAndDropCueKind.RespawnEnemy }:
                // The pickup a death explosion became rebuilds the enemy it came from when that enemy respawns; a drop
                // spawned directly came from no enemy.
                // Only a killed enemy respawns (KilledEnemy.Of), so a respawning drop always names its enemy.
                if (drop.KilledEnemy is { Respawns: true, Enemy: { } respawning })
                    RespawnEnemyFromSnapshot(respawning);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EnemyDeathAndDropCue)} {cue}.");
        }
    }
}
