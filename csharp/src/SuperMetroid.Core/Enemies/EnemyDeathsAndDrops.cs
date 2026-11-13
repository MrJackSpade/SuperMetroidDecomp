using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy deaths and item drops: generic death explosions, respawn placeholders, drop selection and scatter,
/// and pickup collection with its resource restoration.
/// </summary>
[RoomLifetime]
internal sealed class EnemyDeathsAndDrops
{
    /// <summary>Tracks the room's cumulative enemy-kill count.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Allocates death explosions and direct item-drop projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Supplies RNG and enemy header definitions for drop selection.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Reinitializes a respawned enemy from its saved population record.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Receives the pickup and death sounds, each queued as the drop or death program makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects death animation, pickup, and respawn processing to the room systems.</summary>
    internal EnemyDeathsAndDrops(EnemyRoomStatus roomStatus, EnemyProjectileEngine projectiles, EnemyRoomBinding room,
        IEnemyDispatch dispatch, PowerBombRadiusAfterKill powerBombRadius, EnemyAudioQueue audio)
    {
        _audio = audio;
        _roomStatus = roomStatus;
        _projectiles = projectiles;
        _room = room;
        _dispatch = dispatch;
        _powerBombRadius = powerBombRadius;
    }
    /// <summary>Callback that checks pickup lifetime, Grapple, and Samus contact.</summary>
    private const EnemyProjectileFrameBehavior EnemyPickupFrameBehavior =
        EnemyProjectileFrameBehavior.PickupLifetime;
    /// <summary>Frames a newly spawned enemy pickup remains collectible.</summary>
    private const int EnemyPickupLifetime = 400;
    /// <summary>Initial pickup frames during which a Grapple endpoint cannot collect it.</summary>
    private const int EnemyPickupGrappleDelay = 16;

    /// <summary>Live Samus owner needed when a dying grappled enemy drops the beam.</summary>
    internal SamusState? SamusForEnemyDrops { get; private set; }

    /// <summary>Binds the live Samus owner for the room load or enemy frame that follows.</summary>
    internal void BindSamus(SamusState? samus) => SamusForEnemyDrops = samus;

    /// <summary>Requests a collected drop's sound; it joins the queue only when the sound queue is idle.</summary>
    private void RequestPickupSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, priority: SoundRequestPriority.WhenIdle);

    /// <summary>
    /// Requests a death program's sound (the enemy-killed cry, the small explosion or the contact-killed sound); it joins
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
    internal void RecordKill(RoomEnemySlot enemy) =>
        _powerBombRadius.RecordKill(enemy.Presence.HasAny(EnemyPresence.RespawnsIfKilled));

    /// <summary>
    /// Starts an enemy's generic death. The enemy is cleared immediately,
    /// while the death explosion retains the header, position, and optional respawn index until
    /// its authored animation reaches its become-pickup cue.
    /// </summary>
    internal void StartGenericEnemyDeath(
        RoomEnemySlot enemy,
        EnemyDeathExplosion deathAnimation)
    {
        RecordKill(enemy);
        // The death installs the dropped-grapple function before clearing the enemy.
        // Equality is deliberate: this is not a general test for any set grapple bit.
        if (enemy.AiStates == EnemyAiStates.Grappled)
        {
            if (SamusForEnemyDrops is null)
                throw new InvalidOperationException("Grapple enemy death requires the live Samus owner.");
            SamusForEnemyDrops.Grapple.Phase = GrapplePhase.Dropped;
        }
        bool respawns = enemy.Presence.HasAny(EnemyPresence.RespawnsIfKilled);
        RoomEnemySpawnSnapshot survivingSpawn = enemy.Spawn;
        EnemyProjectile? explosion = _projectiles.AllocateEnemyProjectile();
        if (explosion is not null)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                explosion,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphics: default);
            explosion.Position = explosion.Position.WithWhole(enemy.Position.Whole);
            explosion.DropDefinitionId = enemy.DefinitionId;
            explosion.KilledEnemy = KilledEnemy.Of(enemy, respawns);
            explosion.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathExplosions.Program(deathAnimation)));
        }

        enemy.Clear();
        enemy.Spawn = survivingSpawn;
        if (respawns)
        {
            // The post-memset marker keeps the physical slot unavailable until the
            // explosion's respawn cue reconstructs it from the population record.
            InstallRespawnPlaceholder(enemy);
        }
        _roomStatus.RecordKill();
    }

    /// <summary>Keeps a killed respawning slot occupied by replacing its header with the respawn placeholder.</summary>
    internal void InstallRespawnPlaceholder(RoomEnemySlot enemy)
    {
        // The game reads the new definition on every dispatch.
        // Our cached definition must follow it even during the death frame.
        enemy.Bind(EnemyDefinitionId.Respawn, _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.Respawn));
    }

    /// <summary>Chooses the definition's death explosion, raising the smaller ones for Super Missile kills.</summary>
    internal static EnemyDeathExplosion SelectNormalShotDeathAnimation(
        RoomEnemySlot enemy,
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
    /// Shared implementation of the twelve item-drop routines.
    /// Each iteration advances RNG once for both coordinates; allocation
    /// and drop selection then advance it independently inside the pickup. Both scatters read
    /// their fields from the one sample.
    /// </summary>
    internal void SpawnEnemyDropScatter(
        EnemyDefinitionId definitionId,
        int count,
        int xBase,
        RandomSampleField xScatter,
        int yBase,
        RandomSampleField yScatter)
    {
        for (int drop = 0; drop < count; drop++)
        {
            int random = _room.Random.Generate();
            SpawnEnemyDropFromEnemyHeader(new(
                CoordinateAxis.Wrap(xScatter.Of(random) + xBase),
                CoordinateAxis.Wrap(yScatter.Of(random) + yBase)), definitionId);
        }
    }

    /// <summary>Scatters drops within a 32-pixel square around a defeated actor.</summary>
    internal void SpawnEnemyDropScatterAround(
        EnemyDefinitionId definitionId,
        int count,
        Position16<RoomPixels> origin)
    {
        for (int drop = 0; drop < count; drop++)
        {
            int random = _room.Random.Generate();
            SpawnEnemyDropFromEnemyHeader(new(
                CoordinateAxis.Wrap(origin.X + EnemyDropScatterDefinitions.HorizontalScatter.Of(random) - 16),
                CoordinateAxis.Wrap(origin.Y + EnemyDropScatterDefinitions.VerticalScatter.Of(random) - 16)), definitionId);
        }
    }

    /// <summary>
    /// Allocates a pickup projectile using an ordinary enemy header. The
    /// allocation search, slot-zero failure, random selection, and no-drop tail all match
    /// the original's enemy-projectile spawn followed by the pickup's initializer.
    /// </summary>
    internal EnemyProjectile? SpawnEnemyDropFromEnemyHeader(
        Position16<RoomPixels> position,
        EnemyDefinitionId definitionId)
    {
        _room.EnsureLoaded();
        EnemyProjectile? pickup = _projectiles.AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            graphics: default);
        pickup.Position = pickup.Position.WithWhole(position);
        pickup.DropDefinitionId = definitionId;
        pickup.KilledEnemy = KilledEnemy.None;
        InitializeDirectEnemyPickup(pickup);
        return pickup;
    }

    /// <summary>
    /// Allocates a pickup from a chance table already selected by specialized
    /// boss code. This represents routines such as Botwoon's Draygon-eye drop instruction
    /// without manufacturing a fake enemy header solely to get back to the same six bytes.
    /// </summary>
    internal EnemyProjectile? SpawnEnemyDropFromChanceTable(
        Position16<RoomPixels> position,
        EnemyDropChances? dropChances)
    {
        _room.EnsureLoaded();
        EnemyProjectile? pickup = _projectiles.AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            graphics: default);
        pickup.Position = pickup.Position.WithWhole(position);
        pickup.DropChancesOverride = dropChances;
        pickup.KilledEnemy = KilledEnemy.None;
        InitializeDirectEnemyPickup(pickup);
        return pickup;
    }

    /// <summary>Rolls a direct drop and either starts its pickup list or the no-drop tail.</summary>
    private void InitializeDirectEnemyPickup(EnemyProjectile pickup)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(pickup);

        // Unlike a finished death explosion, a direct drop becomes a pickup in any projectile
        // slot, slot zero included. The roll reads the drop's definition straight back from
        // the pickup that holds it.
        pickup.StartAnimation(new EnemyDeathProjectileAnimation(kind is EnemyPickupKind.None or EnemyPickupKind.NoDrop
            ? ConfigureDormantPickup(pickup)
            : ConfigurePickup(pickup, kind)));
    }

    /// <summary>
    /// Converts a completed enemy-death explosion in place and returns the program it continues at: its drop's, or the
    /// respawn tail's. Keeping the actor in its existing place keeps both its retained killed enemy and whether it drops
    /// at all: an explosion in the first place never converts (<see cref="FirstPlaceExplosionLeavesNoPickup"/>).
    /// </summary>
    internal EnemyDeathProjectileProgram ConvertEnemyDeathExplosionToPickup(EnemyProjectile projectile)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(projectile);
        return !FirstPlaceExplosionLeavesNoPickup.BecomesPickup(projectile.Place) || kind is EnemyPickupKind.None or EnemyPickupKind.NoDrop
            ? ConfigureDormantPickup(projectile)
            : ConfigurePickup(projectile, kind);
    }

    /// <summary>Gives the projectile the selected pickup's identity, 400-frame life and collision callback, and returns its program.</summary>
    internal static EnemyDeathProjectileProgram ConfigurePickup(EnemyProjectile projectile, EnemyPickupKind kind)
    {
        EnemyPickupAnimationDefinition animation = EnemyPickups.Animation(kind);
        projectile.State = new EnemyPickupState(kind, EnemyPickupLifetime);
        projectile.FrameBehavior = EnemyPickupFrameBehavior;

        // Becoming a pickup clears the persists-on-contact property but retains the other definition bits. The typed
        // representation stores the three collision flags separately from the low damage.
        projectile.PersistsOnSamusContact = false;
        return animation.Program ?? throw new InvalidOperationException($"Enemy pickup kind {kind} has no drop program.");
    }

    /// <summary>Switches a consumed or rejected drop to the blank, non-interactive deletion tail.</summary>
    private static void MakeEnemyPickupDormant(EnemyProjectile projectile) =>
        projectile.StartAnimation(new EnemyDeathProjectileAnimation(ConfigureDormantPickup(projectile)));

    /// <summary>Makes the projectile a blank, non-interactive drop and returns the deletion tail it runs.</summary>
    private static EnemyDeathProjectileProgram ConfigureDormantPickup(EnemyProjectile projectile)
    {
        // The blank frame lasts 64 frames before the respawn cue handles a retained respawning enemy
        // and the actor is deleted. Collection and natural expiry use this same tail.
        projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
        projectile.CanDamageSamus = false;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        return EnemyDeathExplosionDefinitions.NoDropTail;
    }

    /// <summary>
    /// Exact translation of the random drop selection, whose chances are counts of 0 through 255. Minor drops are
    /// renormalized into the budget left after enabled super/PB chances; the two major drops retain their raw
    /// table probabilities. Every addition wraps past 255, explicitly here. The drops Samus can use are offered in the chance table's column order (the minor
    /// columns first, then the two major ones), and the first whose accumulated chance reaches the random draw is
    /// chosen; the draw is read before anything else, as the game reads it.
    /// </summary>
    internal EnemyPickupKind SelectRandomEnemyDrop(EnemyProjectile projectile)
    {
        SamusState samus = SamusForEnemyDrops ?? throw new InvalidOperationException(
            "Enemy drop selection requires the active Samus state.");
        if (ResolveEnemyDropChances(projectile) is not { } table)
            return EnemyPickupKind.NoDrop;

        int random;
        do
        {
            random = _room.Random.Next(EnemyDropSelectionDefinitions.DropRoll);
        }
        while (random == 0);

        int majorAdjustedBudget = FullWeightBudget;
        int accumulated = 0;
        int combinedEnergy = samus.Health + samus.ReserveEnergy;
        if (combinedEnergy < 30)
            _room.Session.CriticalEnergyDropBias = true;
        else if (combinedEnergy >= 50)
            _room.Session.CriticalEnergyDropBias = false;
        // Values 30..49 deliberately retain the previous flag: this is the game's
        // hysteresis/grace period, not an omitted assignment.

        int pooledMinorWeight;
        var offered = new HashSet<EnemyPickupKind>();
        if (_room.Session.CriticalEnergyDropBias)
        {
            pooledMinorWeight = WithinWeightPool(table.SmallEnergy + table.BigEnergy);
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
                pooledMinorWeight = WithinWeightPool(
                    pooledMinorWeight + table.SmallEnergy + table.BigEnergy);
                offered.Add(EnemyPickupKind.SmallEnergy);
                offered.Add(EnemyPickupKind.BigEnergy);
            }
            if (samus.Missiles != samus.MaxMissiles)
            {
                pooledMinorWeight = WithinWeightPool(pooledMinorWeight + table.Missile);
                offered.Add(EnemyPickupKind.Missile);
            }
            if (samus.SuperMissiles != samus.MaxSuperMissiles)
            {
                majorAdjustedBudget = WithinWeightPool(
                    majorAdjustedBudget - table.SuperMissile);
                offered.Add(EnemyPickupKind.SuperMissile);
            }
            if (samus.PowerBombs != samus.MaxPowerBombs)
            {
                majorAdjustedBudget = WithinWeightPool(
                    majorAdjustedBudget - table.PowerBomb);
                offered.Add(EnemyPickupKind.PowerBomb);
            }
        }

        // With no pooled minor weight the minor columns are passed over and the major columns are offered alone.
        int column = pooledMinorWeight != 0 ? 0 : MinorDropColumnCount;
        for (; column < MinorDropColumnCount; column++)
        {
            EnemyPickupKind kind = EnemyDropTables.ForProbabilityColumn(column);
            if (!offered.Contains(kind))
                continue;

            // The multiply result is sixteen bits and division truncates toward
            // zero. Inputs are bytes, so ordinary integer arithmetic is identical.
            accumulated = CoordinateAxis.Wrap(accumulated + majorAdjustedBudget * table.Of(kind) / pooledMinorWeight);
            if (accumulated >= random)
                return kind;
        }

        for (; column < EnemyDropSelectionDefinitions.ProbabilityColumns.Length; column++)
        {
            EnemyPickupKind kind = EnemyDropTables.ForProbabilityColumn(column);
            if (!offered.Contains(kind))
                continue;

            accumulated = CoordinateAxis.Wrap(accumulated + table.Of(kind));
            if (accumulated >= random)
                return kind;
        }

        return EnemyPickupKind.NoDrop;
    }

    /// <summary>The chance table's minor drop columns (both energies, missile and nothing), offered before the major ones.</summary>
    private const int MinorDropColumnCount = 4;

    /// <summary>The whole weight budget the drop chances share, before the enabled major drops take their part.</summary>
    private const int FullWeightBudget = WeightPoolSpan - 1;

    /// <summary>
    /// Uses an explicit boss chance table, a saved enemy header's, or none. A death explosion from a cleared enemy slot
    /// drops nothing: the drop-chance table indexed by the empty definition reads a value
    /// the boot clears and the game never sets to anything but zero.
    /// </summary>
    private EnemyDropChances? ResolveEnemyDropChances(EnemyProjectile projectile)
    {
        if (projectile.DropChancesOverride is { } chances)
            return chances;
        return projectile.DropDefinitionId == EnemyDefinitionId.None
            ? null
            : _room.LoadedDefinitions.ReadEnemyDefinition(projectile.DropDefinitionId).DropChances;
    }

    /// <summary>Decrements pickup lifetime, then tests Grapple endpoint and strict Samus body overlap.</summary>
    internal void RunEnemyPickupFrameBehavior(
        EnemyProjectile projectile,
        SamusState? samus)
    {
        // The lifetime is decremented first; the pickup expires on the update it reaches zero.
        EnemyPickupState pickup = projectile.StateOf<EnemyPickupState>();
        pickup.Lifetime--;
        if (pickup.Lifetime == 0)
        {
            MakeEnemyPickupDormant(projectile);
            return;
        }

        if (samus is null)
            return;

        // The grapple query reports any non-inactive grapple function. A pickup is immune to its
        // endpoint for the first sixteen decrements, then uses strict 16-pixel axis tests.
        if (samus.Grapple.Phase != GrapplePhase.Inactive &&
            pickup.Lifetime < EnemyPickupLifetime - EnemyPickupGrappleDelay &&
            AbsoluteWrappedDelta(projectile.Position.X.Whole, samus.Grapple.Anchor.X) < 16 &&
            AbsoluteWrappedDelta(projectile.Position.Y.Whole, samus.Grapple.Anchor.Y) < 16)
        {
            CollectEnemyPickup(projectile, samus);
            return;
        }

        // The overlap test's subtract/borrow formulation is exactly strict radius-sum overlap for
        // valid actor radii. Equality is not contact in either axis.
        if (AbsoluteWrappedDelta(samus.Position.X.Whole, projectile.Position.X.Whole) >=
                samus.Kinematics.Radius.X + projectile.Radius.X ||
            AbsoluteWrappedDelta(samus.Position.Y.Whole, projectile.Position.Y.Whole) >=
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
                RestoreEnemyDropEnergy(samus, 5);
                RequestPickupSound(EnemySound.SmallEnergyPickup);
                break;
            case EnemyPickupKind.BigEnergy:
                RestoreEnemyDropEnergy(samus, 20);
                RequestPickupSound(EnemySound.BigEnergyPickup);
                break;
            case EnemyPickupKind.PowerBomb:
                RestoreEnemyDropPowerBombs(samus, 1);
                RequestPickupSound(EnemySound.PowerBombPickup);
                break;
            case EnemyPickupKind.Missile:
                RestoreEnemyDropMissiles(samus, 2);
                RequestPickupSound(EnemySound.MissilePickup);
                break;
            case EnemyPickupKind.SuperMissile:
                RestoreEnemyDropSuperMissiles(samus, 1);
                RequestPickupSound(EnemySound.SuperMissilePickup);
                break;
            case EnemyPickupKind.None:
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

    /// <summary>Fills missiles first, spilling excess into reserves capped by inventory and 99.</summary>
    private static void RestoreEnemyDropMissiles(SamusState samus, int amount)
    {
        int restored = samus.Missiles + amount;
        samus.Missiles = restored;
        if (restored < samus.MaxMissiles)
            return;

        samus.ReserveMissiles += restored - samus.MaxMissiles;
        int reserveCap = Math.Min(samus.MaxMissiles, 99);
        if (samus.ReserveMissiles >= reserveCap)
            samus.ReserveMissiles = reserveCap;
        samus.Missiles = samus.MaxMissiles;
    }

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

    /// <summary>Measures the absolute distance between two of the room's wrapping coordinates.</summary>
    private static int AbsoluteWrappedDelta(int left, int right) =>
        CoordinateAxis.Distance(left, right);

    /// <summary>The span of the drop-weight pool: every chance is out of 255.</summary>
    private const int WeightPoolSpan = 256;

    /// <summary>
    /// A pooled drop weight or remaining budget within the 256-unit pool: a pool summed past 255, or a budget taken below
    /// zero, continues from the other end, as it does for a table whose chances do not sum to 255.
    /// </summary>
    /// <param name="weight">The computed weight, which may lie outside the pool.</param>
    private static int WithinWeightPool(int weight) => (weight % WeightPoolSpan + WeightPoolSpan) % WeightPoolSpan;

    /// <summary>
    /// Runs the respawn cue for the respawning enemy a death actor retains.
    /// The method is deliberately population/snapshot based so later respawning families can
    /// share it without acquiring a Rinka-specific reconstruction path.
    /// </summary>
    internal void RespawnEnemyFromSnapshot(RoomEnemySlot slot)
    {
        RoomEnemySpawnSnapshot spawn = slot.Spawn;
        if (spawn.Population.DefinitionId == EnemyDefinitionId.None)
        {
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no surviving population snapshot to respawn.");
        }

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(spawn.Population.DefinitionId);
        // Unlike the room's enemy setup and an ordinary enemy spawn, a respawn
        // never writes the spritemap: the actor keeps the word its death cleared,
        // or what its init AI wrote, until its first instruction. A zero spritemap is also
        // the contact gate, so the respawned actor cannot touch Samus before then.
        _dispatch.InitializeSlotFromDefinition(slot, spawn.Population, definition);
        _dispatch.RunInitializationAi(slot, EnemySpawnSurroundings.None);
    }

    /// <summary>Connects pickup lifetime and contact handling to the projectile engine.</summary>
    internal void ConnectPickupFrameBehavior(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            // Lifetime, grapple endpoint, then Samus body.
            RunEnemyPickupFrameBehavior(context.Projectile, context.Samus);
        }, EnemyProjectileFrameBehavior.PickupLifetime);
    }

    /// <summary>Queues the enemy-killed sound, as the death explosion's cue does.</summary>
    internal void QueueEnemyKilledSound() => RequestDeathSound(EnemyKilledSound);

    /// <summary>Connects the death and drop programs' cues: sprite spawns, sounds, drop conversion and respawn.</summary>
    internal void ConnectDeathExplosionCues(EnemyProjectileHandlerTable table) =>
        table.AddBranchingCue<EnemyDeathProjectileProgram, EnemyDeathProjectileCue>((context, cue) => ApplyEnemyDeathProjectileCue(context.Projectile, cue));

    /// <summary>
    /// Carries out one cue of the death and drop programs, returning the program a converted explosion continues at, or
    /// null to run on.
    /// </summary>
    private EnemyDeathProjectileProgram? ApplyEnemyDeathProjectileCue(EnemyProjectile projectile, EnemyDeathProjectileCue cue)
    {
        switch (cue.Kind)
        {
            case EnemyDeathProjectileCueKind.Delete:
                projectile.Delete();
                return null;
            case EnemyDeathProjectileCueKind.SpawnSpriteWithin32:
                _projectiles.SpawnRandomEnemyDeathSprite(
                    projectile,
                    cue.SpriteObject,
                    EnemyDeathExplosionDefinitions.WideSpriteHorizontalScatter,
                    EnemyDeathExplosionDefinitions.WideSpriteVerticalScatter,
                    center: 32);
                return null;
            case EnemyDeathProjectileCueKind.SpawnSpriteWithin16:
                _projectiles.SpawnRandomEnemyDeathSprite(
                    projectile,
                    cue.SpriteObject,
                    EnemyDeathExplosionDefinitions.NarrowSpriteHorizontalScatter,
                    EnemyDeathExplosionDefinitions.NarrowSpriteVerticalScatter,
                    center: 16);
                return null;
            case EnemyDeathProjectileCueKind.PlayEnemyKilledSound:
                // The sound-queueing instruction returns to the next instruction, so the following duration starts the next frame.
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
                return ConvertEnemyDeathExplosionToPickup(projectile);
            case EnemyDeathProjectileCueKind.HandleRespawningEnemy:
                if (projectile.KilledEnemy.Respawns)
                    RespawnEnemyFromSnapshot(projectile.KilledEnemy.Enemy ?? throw new InvalidDataException("A respawning death explosion names no enemy."));
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EnemyDeathProjectileCueKind)} {cue.Kind}.");
        }
    }
}
