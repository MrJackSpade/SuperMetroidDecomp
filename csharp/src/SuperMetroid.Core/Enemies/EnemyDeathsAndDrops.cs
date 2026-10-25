using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Projectiles;
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
    /// <summary>Finds a retained enemy slot when a death actor executes respawn.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Reinitializes a respawned enemy from its saved population record.</summary>
    private readonly IEnemyDispatch _dispatch;

    /// <summary>Connects death animation, pickup, and respawn processing to the room systems.</summary>
    internal EnemyDeathsAndDrops(EnemyRoomStatus roomStatus, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemySlotTable slotTable, IEnemyDispatch dispatch)
    {
        _roomStatus = roomStatus;
        _projectiles = projectiles;
        _room = room;
        _slotTable = slotTable;
        _dispatch = dispatch;
    }
    /// <summary>Bank-$86 callback that checks pickup lifetime, Grapple, and Samus contact.</summary>
    private const EnemyProjectilePreInstruction EnemyPickupPreInstruction =
        EnemyProjectilePreInstruction.Pickup;
    /// <summary>Frames a newly spawned enemy pickup remains collectible.</summary>
    private const ushort EnemyPickupLifetime = 400;
    /// <summary>Initial pickup frames during which a Grapple endpoint cannot collect it.</summary>
    private const ushort EnemyPickupGrappleDelay = 16;

    /// <summary>Live Samus owner needed when a dying grappled enemy drops the beam.</summary>
    internal SamusState? SamusForEnemyDrops { get; private set; }

    /// <summary>Binds the live Samus owner for the room load or enemy frame that follows.</summary>
    internal void BindSamus(SamusState? samus) => SamusForEnemyDrops = samus;

    /// <summary>Last library-two sound requested by a collected enemy pickup this frame.</summary>
    public SoundEffectId? LastEnemyPickupSoundEffect { get; internal set; }

    /// <summary>
    /// Last library-two sound requested by the shared death/pickup instruction lists this
    /// frame: the enemy-killed cry, the small explosion or the contact-killed sound, each
    /// authored directly in bank $86.
    /// </summary>
    public SoundEffectId? LastEnemyDeathSoundEffectLibrary2 { get; internal set; }

    /// <summary>The library-two enemy-killed cry that <c>EprojInstr_QueueSfx2_9</c> ($86:EE8B) queues.</summary>
    internal static readonly SoundEffectId EnemyKilledSound = Library2Sound.EnemyKilled;

    /// <summary>
    /// Scratch word `$12` as last written by <c>EnemyDeath</c> ($A0:A3E6) or
    /// <c>RinkaDeath</c> ($A0:A437): the dying actor's respawn property bit as its word,
    /// zero or 16384 (<see cref="RespawnScratchWord"/> is the only producer). The
    /// power-bomb walker at $A0:A306 keeps its horizontal radius in that same word, so a
    /// kill inside its loop resizes the explosion for every lower slot that frame; the
    /// walker reads it back as an ordinary radius, so no wider value can reach it.
    /// </summary>
    private int? _enemyDeathRespawnScratch;

    /// <summary>Records a dying actor's respawn word in scratch <c>$12</c>, as <c>EnemyDeath</c> and <c>RinkaDeath</c> do.</summary>
    internal void RecordRespawnScratch(RoomEnemySlot enemy) => _enemyDeathRespawnScratch = RespawnScratchWord(enemy);

    /// <summary>Clears scratch <c>$12</c>'s death record.</summary>
    internal void ClearRespawnScratch() => _enemyDeathRespawnScratch = null;

    /// <summary>Takes the death record a kill left in scratch <c>$12</c>, clearing it.</summary>
    internal int? TakeRespawnScratch()
    {
        int? scratch = _enemyDeathRespawnScratch;
        _enemyDeathRespawnScratch = null;
        return scratch;
    }

    /// <summary>
    /// Ports <c>EnemyDeathAnimation</c> at $A0:A3AF. The enemy is cleared immediately,
    /// while projectile $F345 retains the header, position, and optional respawn index until
    /// its ROM animation reaches opcode $EEAF and becomes a pickup.
    /// </summary>
    internal void StartGenericEnemyDeath(
        RoomEnemySlot enemy,
        int deathAnimation)
    {
        RecordRespawnScratch(enemy);
        // $A0:A3AF installs the dropped-grapple function before clearing the enemy.
        // Equality is deliberate: this is not a general test for any set grapple bit.
        if (enemy.AiStates == EnemyAiStates.Grappled)
        {
            if (SamusForEnemyDrops is null)
                throw new InvalidOperationException("Grapple enemy death requires the live Samus owner.");
            SamusForEnemyDrops.Grapple.Phase = GrapplePhase.Dropped;
        }
        if (deathAnimation >= 5)
            deathAnimation = 0;

        bool respawns = enemy.Properties.HasAny(EnemyProperties.RespawnIfKilled);
        RoomEnemySpawnSnapshot survivingSpawn = enemy.Spawn;
        RoomEnemyProjectileSlot? explosion = _projectiles.AllocateEnemyProjectile();
        if (explosion is not null)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                explosion,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphicsIndex: 0);
            explosion.Position = explosion.Position.WithWhole(enemy.Position.Whole);
            explosion.DropDefinitionId = enemy.DefinitionId;
            explosion.KilledEnemy = KilledEnemy.Of(enemy, respawns);
            explosion.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathExplosionDefinitions.Program(deathAnimation)));
        }

        enemy.Clear();
        enemy.Spawn = survivingSpawn;
        if (respawns)
        {
            // The native post-memset marker keeps the physical slot unavailable until the
            // projectile's $EF10 instruction reconstructs it from the population record.
            InstallRespawnPlaceholder(enemy);
        }
        _roomStatus.RecordKill();
    }

    /// <summary>
    /// Publishes the dying enemy's respawn property bit, as the word the cartridge stores, into the shared scratch
    /// word: 16384 for a respawning enemy, otherwise zero.
    /// </summary>
    internal static int RespawnScratchWord(RoomEnemySlot enemy) =>
        enemy.Properties.HasAny(EnemyProperties.RespawnIfKilled)
            ? EnemyProperties.RespawnIfKilled.ToNativeWord()
            : 0;

    /// <summary>Keeps a killed respawning slot occupied by replacing its header with the respawn placeholder.</summary>
    internal void InstallRespawnPlaceholder(RoomEnemySlot enemy)
    {
        // The cartridge reads the header through the new pointer on every dispatch.
        // Our cached definition must follow that pointer even during the death frame.
        enemy.Bind(EnemyDefinitionId.Respawn, _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.Respawn));
    }

    /// <summary>Chooses header death art, upgrading a small explosion for Super Missile kills.</summary>
    internal static ushort SelectNormalShotDeathAnimation(
        RoomEnemySlot enemy,
        SamusProjectileTypeWord projectileType,
        ShotDeathAnimationRule rule = ShotDeathAnimationRule.ByProjectile)
    {
        if (rule == ShotDeathAnimationRule.PirateBigExplosion)
            return 4;

        ushort causeOfDeath = (ushort)SamusProjectileFamilies.IndexOf(projectileType.Raw);
        if (causeOfDeath != 2)
            return enemy.Definition.DeathAnimation;

        // A Super Missile raises headers zero, one and two to explosion two and keeps three
        // and four: $A0:A67B loads Y = 2, and the signed `< 3` branch at $A0:A698 keeps it.
        return enemy.Definition.DeathAnimation < 3
            ? (ushort)2
            : enemy.Definition.DeathAnimation;
    }

    /// <summary>
    /// Shared implementation of the twelve <c>Enemy_ItemDrop_*</c> routines at
    /// $A0:B8EE-$BB9A. Each iteration advances RNG once for both coordinates; allocation
    /// and drop selection then advance it independently inside pickup $F337.
    /// </summary>
    internal void SpawnEnemyDropScatter(
        EnemyDefinitionId definitionId,
        int count,
        ushort xBase,
        ushort xMask,
        ushort yBase,
        ushort yMask)
    {
        for (int drop = 0; drop < count; drop++)
        {
            int random = _room.Random.Generate();
            ushort x = unchecked((ushort)((random & xMask) + xBase));
            ushort y = unchecked((ushort)(SnesLittleEndian.HighByte(unchecked((ushort)(random & yMask))) + yBase));
            SpawnEnemyDropFromEnemyHeader(new(x, y), definitionId);
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
            ushort x = unchecked((ushort)(origin.X + (random & EnemyDropMasks.ScatterBits) - 16));
            ushort y = unchecked((ushort)(origin.Y + ((SnesLittleEndian.HighByte(random) & EnemyDropMasks.ScatterBits)) - 16));
            SpawnEnemyDropFromEnemyHeader(new(x, y), definitionId);
        }
    }

    /// <summary>
    /// Allocates pickup projectile $F337 using an ordinary bank-$A0 enemy header. The
    /// allocation search, slot-zero failure, random selection, and no-drop tail all match
    /// <c>SpawnEprojWithRoomGfx</c> followed by <c>InitAI_EnemyProjectile_Pickup</c>.
    /// </summary>
    internal RoomEnemyProjectileSlot? SpawnEnemyDropFromEnemyHeader(
        Position16<RoomPixels> position,
        EnemyDefinitionId definitionId)
    {
        _room.EnsureLoaded();
        RoomEnemyProjectileSlot? pickup = _projectiles.AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            graphicsIndex: 0);
        pickup.Position = pickup.Position.WithWhole(position);
        pickup.DropDefinitionId = definitionId;
        pickup.KilledEnemy = KilledEnemy.None;
        InitializeDirectEnemyPickup(pickup);
        return pickup;
    }

    /// <summary>
    /// Allocates pickup $F337 from a bank-$B4 chance table already selected by specialized
    /// boss code. This represents routines such as Botwoon's Draygon-eye drop instruction
    /// without manufacturing a fake enemy header solely to get back to the same six bytes.
    /// </summary>
    internal RoomEnemyProjectileSlot? SpawnEnemyDropFromChanceTable(
        Position16<RoomPixels> position,
        EnemyDropTable? dropChances)
    {
        _room.EnsureLoaded();
        RoomEnemyProjectileSlot? pickup = _projectiles.AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            graphicsIndex: 0);
        pickup.Position = pickup.Position.WithWhole(position);
        pickup.DropChancesOverride = dropChances is { } table ? EnemyDropChanceDefinitions.At(table) : null;
        pickup.KilledEnemy = KilledEnemy.None;
        InitializeDirectEnemyPickup(pickup);
        return pickup;
    }

    /// <summary>Rolls a direct drop and either starts its pickup list or the no-drop tail.</summary>
    private void InitializeDirectEnemyPickup(RoomEnemyProjectileSlot pickup)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(pickup);

        // $86:EF29 also branches on the X that Random_Drop_Routine restores, but on this
        // path X still holds the spawn routine's projectile ID ($F337), never zero, so any
        // slot can become a pickup. Its `...,X` header store therefore lands at $7F:E6FF,
        // intro/credits tilemap scratch, and the drop roll reads it back from there.
        pickup.StartAnimation(new EnemyDeathProjectileAnimation(kind is EnemyPickupKind.None or EnemyPickupKind.NoDrop
            ? ConfigureDormantPickup(pickup)
            : ConfigurePickup(pickup, kind)));
    }

    /// <summary>
    /// Converts a completed enemy-death explosion in place and returns the program it continues at: its drop's, or the
    /// respawn tail's. Keeping the actor in its existing slot preserves both the slot-zero bug and the retained respawn
    /// index: $86:EEAF branches on the restored projectile index, so slot zero never converts.
    /// </summary>
    internal EnemyDeathProjectileProgram ConvertEnemyDeathExplosionToPickup(RoomEnemyProjectileSlot projectile)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(projectile);
        return projectile.SlotIndex == 0 || kind is EnemyPickupKind.None or EnemyPickupKind.NoDrop
            ? ConfigureDormantPickup(projectile)
            : ConfigurePickup(projectile, kind);
    }

    /// <summary>Gives the projectile the selected pickup's identity, 400-frame life and collision callback, and returns its program.</summary>
    internal static EnemyDeathProjectileProgram ConfigurePickup(RoomEnemyProjectileSlot projectile, EnemyPickupKind kind)
    {
        EnemyPickupAnimationDefinition animation = EnemyPickupDefinitions.Animation(kind);
        projectile.Variable0 = animation.TableOffset;
        projectile.Variable1 = EnemyPickupLifetime;
        projectile.PreInstruction = EnemyPickupPreInstruction;

        // $86:EEAF clears property $4000 but retains the other definition bits. The typed
        // representation stores the three collision flags separately from the low damage.
        projectile.PersistsOnSamusContact = false;
        return animation.Program ?? throw new InvalidOperationException($"Enemy pickup kind {kind} has no drop program.");
    }

    /// <summary>Switches a consumed or rejected drop to the blank, non-interactive deletion tail.</summary>
    private static void MakeEnemyPickupDormant(RoomEnemyProjectileSlot projectile) =>
        projectile.StartAnimation(new EnemyDeathProjectileAnimation(ConfigureDormantPickup(projectile)));

    /// <summary>Makes the projectile a blank, non-interactive drop and returns the deletion tail it runs.</summary>
    private static EnemyDeathProjectileProgram ConfigureDormantPickup(RoomEnemyProjectileSlot projectile)
    {
        // The blank map lasts 64 frames before $EF10 handles a retained respawning enemy
        // and $8154 deletes the actor. Collection and natural expiry use this same tail.
        projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_86EFDF;
        projectile.CanDamageSamus = false;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
        return EnemyDeathExplosionDefinitions.NoDropTail;
    }

    /// <summary>
    /// Exact byte-accumulator translation of $86:F106. Minor drops are renormalized into
    /// the budget left after enabled super/PB chances; the two major drops retain their raw
    /// table probabilities. All additions which occurred in eight-bit accumulator mode are
    /// explicitly wrapped here.
    /// </summary>
    internal EnemyPickupKind SelectRandomEnemyDrop(RoomEnemyProjectileSlot projectile)
    {
        SamusState samus = SamusForEnemyDrops ?? throw new InvalidOperationException(
            "Enemy drop selection requires the active Samus state.");
        if (ResolveEnemyDropChances(projectile) is not { } table)
            return EnemyPickupKind.NoDrop;

        Span<byte> chances = stackalloc byte[EnemyDropChanceDefinitions.RecordSize];
        table.CopyTo(chances);

        byte random;
        do
        {
            random = unchecked((byte)_room.Random.Generate());
        }
        while (random == 0);

        byte majorAdjustedBudget = byte.MaxValue;
        ushort accumulated = 0;
        ushort combinedEnergy = unchecked((ushort)(
            samus.Health + samus.ReserveEnergy));
        if (combinedEnergy < 30)
            _room.Session.CriticalEnergyDropBias = true;
        else if (combinedEnergy >= 50)
            _room.Session.CriticalEnergyDropBias = false;
        // Values 30..49 deliberately retain the previous flag: this is the cartridge's
        // hysteresis/grace period, not an omitted assignment.

        byte pooledMinorWeight;
        byte enabledMask;
        if (_room.Session.CriticalEnergyDropBias)
        {
            pooledMinorWeight = unchecked((byte)(chances[0] + chances[1]));
            enabledMask = EnemyDropMasks.Energy;
        }
        else
        {
            pooledMinorWeight = chances[3];
            enabledMask = EnemyDropMasks.Nothing;
            if (samus.Health != samus.MaxHealth ||
                samus.ReserveEnergy != samus.MaxReserveEnergy)
            {
                pooledMinorWeight = unchecked((byte)(
                    pooledMinorWeight + chances[0] + chances[1]));
                enabledMask |= EnemyDropMasks.Energy;
            }
            if (samus.Missiles != samus.MaxMissiles)
            {
                pooledMinorWeight = unchecked((byte)(pooledMinorWeight + chances[2]));
                enabledMask |= EnemyDropMasks.Missile;
            }
            if (samus.SuperMissiles != samus.MaxSuperMissiles)
            {
                majorAdjustedBudget = unchecked((byte)(
                    majorAdjustedBudget - chances[4]));
                enabledMask |= EnemyDropMasks.SuperMissile;
            }
            if (samus.PowerBombs != samus.MaxPowerBombs)
            {
                majorAdjustedBudget = unchecked((byte)(
                    majorAdjustedBudget - chances[5]));
                enabledMask |= EnemyDropMasks.PowerBomb;
            }
        }

        int accumulatorIndex = 0;
        if (pooledMinorWeight != 0)
        {
            for (; accumulatorIndex < 4; accumulatorIndex++)
            {
                bool enabled = (enabledMask & 1) != 0;
                enabledMask >>= 1;
                if (!enabled)
                    continue;

                // The SNES multiply result is sixteen bits and division truncates toward
                // zero. Inputs are bytes, so ordinary integer arithmetic is identical.
                accumulated = unchecked((ushort)(accumulated +
                    majorAdjustedBudget * chances[accumulatorIndex] / pooledMinorWeight));
                if (accumulated >= random)
                    return EnemyDropSelectionDefinitions.ForProbabilityColumn(accumulatorIndex);
            }
        }
        else
        {
            // The assembly jumps directly to major index four after shifting away all four
            // minor availability bits when their pooled divisor is zero.
            enabledMask >>= 4;
            accumulatorIndex = 4;
        }

        for (; accumulatorIndex < 6; accumulatorIndex++)
        {
            bool enabled = (enabledMask & 1) != 0;
            enabledMask >>= 1;
            if (!enabled)
                continue;

            accumulated = unchecked((ushort)(accumulated + chances[accumulatorIndex]));
            if (accumulated >= random)
                return EnemyDropSelectionDefinitions.ForProbabilityColumn(accumulatorIndex);
        }

        return EnemyPickupKind.NoDrop;
    }

    /// <summary>
    /// Uses an explicit boss chance table, a saved enemy header's, or none. A death explosion from a cleared enemy slot
    /// drops nothing: <c>EnemyHeaders_dropChances</c> (<c>$86:F120</c>) indexed by header zero reads <c>$A0:003A</c>,
    /// which LoROM mirrors to the unnamed direct-page word <c>$003A</c>. The boot clears it (<c>$80:844E</c>) and the
    /// pinned revision never writes it; only the 16-bit <c>STA.B DP_BlocksToUpdate+2</c> at <c>$80:AA0E</c>/<c>$80:ABA8</c>
    /// touches the neighbouring <c>$39</c>, with zero.
    /// </summary>
    private EnemyDropChances? ResolveEnemyDropChances(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.DropChancesOverride is { } chances)
            return chances;
        EnemyDropTable? table = projectile.DropDefinitionId == EnemyDefinitionId.None
            ? null
            : _room.LoadedDefinitions.ReadEnemyDefinition(projectile.DropDefinitionId).DropChances;
        return table is { } selected ? EnemyDropChanceDefinitions.At(selected) : null;
    }

    /// <summary>Decrements pickup lifetime, then tests Grapple endpoint and strict Samus body overlap.</summary>
    internal void RunEnemyPickupPreInstruction(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus)
    {
        // The lifetime is decremented first; the pickup expires on the update it reaches zero.
        CountdownStep lifetime = WrappingCountdown.FromRaw(checked((ushort)projectile.Variable1)).Decrement();
        projectile.Variable1 = lifetime.Value;
        if (lifetime.IsZero)
        {
            MakeEnemyPickupDormant(projectile);
            return;
        }

        if (samus is null)
            return;

        // Command $0D reports any non-inactive grapple function. A pickup is immune to its
        // endpoint for the first sixteen decrements, then uses strict 16-pixel axis tests.
        if (samus.Grapple.Phase != GrapplePhase.Inactive &&
            projectile.Variable1 < EnemyPickupLifetime - EnemyPickupGrappleDelay &&
            AbsoluteWrappedDelta(projectile.Position.X.Whole, samus.Grapple.Anchor.X) < 16 &&
            AbsoluteWrappedDelta(projectile.Position.Y.Whole, samus.Grapple.Anchor.Y) < 16)
        {
            CollectEnemyPickup(projectile, samus);
            return;
        }

        // $86:F057's subtract/borrow formulation is exactly strict radius-sum overlap for
        // valid actor radii. Equality is not contact, matching the BCS exits in both axes.
        if (AbsoluteWrappedDelta(samus.Position.X.Whole, projectile.Position.X.Whole) >=
                samus.Kinematics.Radius.X + projectile.Radius.X ||
            AbsoluteWrappedDelta(samus.Position.Y.Whole, projectile.Position.Y.Whole) >=
                samus.Kinematics.Radius.Y + projectile.Radius.Y)
        {
            return;
        }

        CollectEnemyPickup(projectile, samus);
    }

    /// <summary>The pickup kind whose table offset <paramref name="projectile"/> keeps in variable 0.</summary>
    /// <exception cref="InvalidDataException">No pickup table entry has that offset.</exception>
    private static EnemyPickupKind PickupKindAt(RoomEnemyProjectileSlot projectile)
    {
        foreach (EnemyPickupKind kind in EnemyPickupDefinitions.TableKinds)
            if (EnemyPickupDefinitions.Animation(kind).TableOffset == projectile.Variable0)
                return kind;
        throw new InvalidDataException(
            $"Enemy pickup slot {projectile.SlotIndex} contains invalid type ${projectile.Variable0:X4}.");
    }

    /// <summary>Applies health or ammo from a touched pickup, publishes its sound, and makes it dormant.</summary>
    private void CollectEnemyPickup(
        RoomEnemyProjectileSlot projectile,
        SamusState samus)
    {
        EnemyPickupKind kind = PickupKindAt(projectile);
        switch (kind)
        {
            case EnemyPickupKind.SmallEnergy:
                RestoreEnemyDropEnergy(samus, 5);
                LastEnemyPickupSoundEffect = Library2Sound.SmallEnergyPickup;
                break;
            case EnemyPickupKind.BigEnergy:
                RestoreEnemyDropEnergy(samus, 20);
                LastEnemyPickupSoundEffect = Library2Sound.BigEnergyPickup;
                break;
            case EnemyPickupKind.PowerBomb:
                RestoreEnemyDropPowerBombs(samus, 1);
                LastEnemyPickupSoundEffect = Library2Sound.PowerBombPickup;
                break;
            case EnemyPickupKind.Missile:
                RestoreEnemyDropMissiles(samus, 2);
                LastEnemyPickupSoundEffect = Library2Sound.MissilePickup;
                break;
            case EnemyPickupKind.SuperMissile:
                RestoreEnemyDropSuperMissiles(samus, 1);
                LastEnemyPickupSoundEffect = Library2Sound.SuperMissilePickup;
                break;
            case EnemyPickupKind.None:
            case EnemyPickupKind.NoDrop:
            default:
                throw new InvalidDataException(
                    $"Enemy pickup slot {projectile.SlotIndex} contains invalid type ${projectile.Variable0:X4}.");
        }

        MakeEnemyPickupDormant(projectile);
    }

    /// <summary>Fills health first, spilling excess into capped reserves and enabling reserve auto mode.</summary>
    private static void RestoreEnemyDropEnergy(SamusState samus, ushort amount)
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
    private static void RestoreEnemyDropMissiles(SamusState samus, ushort amount)
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
    private static void RestoreEnemyDropSuperMissiles(SamusState samus, ushort amount)
    {
        int restored = samus.SuperMissiles + amount;
        samus.SuperMissiles = restored;
        if (restored > samus.MaxSuperMissiles)
        {
            samus.SuperMissiles = samus.MaxSuperMissiles;
        }
    }

    /// <summary>Adds a Power Bomb and clamps it to the owned maximum.</summary>
    private static void RestoreEnemyDropPowerBombs(SamusState samus, ushort amount)
    {
        int restored = samus.PowerBombs + amount;
        samus.PowerBombs = restored;
        if (restored > samus.MaxPowerBombs)
        {
            samus.PowerBombs = samus.MaxPowerBombs;
        }
    }

    /// <summary>Measures the absolute distance between native 16-bit coordinates.</summary>
    private static int AbsoluteWrappedDelta(ushort left, ushort right) =>
        Math.Abs((int)unchecked((short)(left - right)));

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastEnemyPickupSoundEffect is { } pickupSound)
            audio.QueueEnemySound(pickupSound, maximumQueued: SoundQueueLimit.Max1);
        if (LastEnemyDeathSoundEffectLibrary2 is { } deathSound)
            audio.QueueEnemySound(deathSound, maximumQueued: SoundQueueLimit.Max1);
    }

    /// <summary>
    /// Executes instruction $86:EF10 for the respawning enemy a death actor retains.
    /// The method is deliberately population/snapshot based so later respawning families can
    /// share it without acquiring a Rinka-specific reconstruction path.
    /// </summary>
    internal void RespawnEnemyFromSnapshot(int slotIndex)
    {
        RoomEnemySlot slot = _slotTable.Slots[slotIndex];
        RoomEnemySpawnSnapshot spawn = slot.Spawn;
        if (spawn.Population.DefinitionId == EnemyDefinitionId.None)
        {
            throw new InvalidDataException(
                $"Enemy slot {slotIndex} has no surviving population snapshot to respawn.");
        }

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(spawn.Population.DefinitionId);
        // Unlike Initialise_Enemies ($A0:8B93) and Spawn_Enemy ($A0:93D9), Respawn_Enemy
        // ($86:F264) never writes the spritemap: the actor keeps EnemyDeath's cleared word,
        // or what its init AI wrote, until its first instruction. A zero spritemap is also
        // $A0:A08C's contact gate, so the respawned actor cannot touch Samus before then.
        _dispatch.InitializeSlotFromDefinition(slot, spawn.Population, definition);
        _dispatch.RunInitializationAi(slot, EnemySpawnSurroundings.None);
    }

    /// <summary>Connects pickup lifetime and contact handling to the projectile engine.</summary>
    internal void ConnectPickupPreInstruction(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            // Lifetime, grapple endpoint, then Samus body.
            RunEnemyPickupPreInstruction(context.Projectile, context.Samus);
        }, EnemyProjectilePreInstruction.Pickup);
    }

    /// <summary>Queues the library-two enemy-killed sound 9, as <c>EprojInstr_QueueSfx2_9</c> ($86:EE8B) does.</summary>
    internal void QueueEnemyKilledSound() => LastEnemyDeathSoundEffectLibrary2 = EnemyKilledSound;

    /// <summary>Connects the death and drop programs' cues: sprite spawns, sounds, drop conversion and respawn.</summary>
    internal void ConnectDeathExplosionCues(EnemyProjectileHandlerTable table) =>
        table.AddBranchingCue<EnemyDeathProjectileProgram, EnemyDeathProjectileCue>((context, cue) => ApplyEnemyDeathProjectileCue(context.Projectile, cue));

    /// <summary>
    /// Carries out one cue of the death and drop programs, returning the program a converted explosion continues at, or
    /// null to run on.
    /// </summary>
    private EnemyDeathProjectileProgram? ApplyEnemyDeathProjectileCue(RoomEnemyProjectileSlot projectile, EnemyDeathProjectileCue cue)
    {
        switch (cue.Kind)
        {
            case EnemyDeathProjectileCueKind.Delete:
                projectile.Clear();
                return null;
            case EnemyDeathProjectileCueKind.SpawnSpriteWithin32:
                _projectiles.SpawnRandomEnemyDeathSprite(projectile, cue.SpriteObject, mask: 63, center: 32);
                return null;
            case EnemyDeathProjectileCueKind.SpawnSpriteWithin16:
                _projectiles.SpawnRandomEnemyDeathSprite(projectile, cue.SpriteObject, mask: 31, center: 16);
                return null;
            case EnemyDeathProjectileCueKind.PlayEnemyKilledSound:
                // EprojInstr_QueueSfx2_9 returns the pointer it was passed, so the following duration starts the next frame.
                QueueEnemyKilledSound();
                return null;
            case EnemyDeathProjectileCueKind.PlaySmallExplosionSound:
                LastEnemyDeathSoundEffectLibrary2 = EnemySoundEffectDefinitions.DeathSmallExplosionSound;
                return null;
            case EnemyDeathProjectileCueKind.PlayContactKilledSound:
                LastEnemyDeathSoundEffectLibrary2 = EnemySoundEffectDefinitions.DeathContactKilledSound;
                return null;
            case EnemyDeathProjectileCueKind.BecomePickup:
                if (projectile.Kind != RoomEnemyProjectileKind.EnemyDeathExplosion)
                    throw new InvalidDataException($"Enemy projectile {projectile.Kind} reached the death-drop command $EEAF.");
                return ConvertEnemyDeathExplosionToPickup(projectile);
            case EnemyDeathProjectileCueKind.HandleRespawningEnemy:
                if (projectile.KilledEnemy.Respawns)
                    RespawnEnemyFromSnapshot(projectile.KilledEnemy.SlotIndex);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EnemyDeathProjectileCueKind)} {cue.Kind}.");
        }
    }
}
