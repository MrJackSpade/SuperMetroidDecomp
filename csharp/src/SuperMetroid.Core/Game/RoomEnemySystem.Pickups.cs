namespace SuperMetroid.Core.Game;

/// <summary>
/// Values returned by <c>Random_Drop_Routine</c> at $86:F106. These are deliberately the
/// cartridge values stored as <c>eproj_E / 2</c>; the table order is not the visual order of
/// the five pickup instruction lists.
/// </summary>
public enum EnemyPickupKind : ushort
{
    None = 0,
    SmallEnergy = 1,
    BigEnergy = 2,
    PowerBomb = 3,
    Missile = 4,
    SuperMissile = 5,
    NoDrop = 6,
}

public sealed partial class RoomEnemySystem
{
    private const ushort EnemyPickupPreInstruction =
        EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_Pickup;
    private const ushort EnemyPickupLifetime = 400;
    private const ushort EnemyPickupGrappleDelay = 16;
    private const int EnemyPickupInstructionPointerTable = 0x86ef04;
    private const int EnemyDropChancesBank = 0xb40000;

    // Indexes zero through five are the accumulator order used by $86:F106. Notice that
    // power bombs are last even though their returned pickup identity is three.
    private static readonly EnemyPickupKind[] EnemyDropAccumulatorOrder =
    [
        EnemyPickupKind.SmallEnergy,
        EnemyPickupKind.BigEnergy,
        EnemyPickupKind.Missile,
        EnemyPickupKind.NoDrop,
        EnemyPickupKind.SuperMissile,
        EnemyPickupKind.PowerBomb,
    ];

    // $7E:0E1E is intentionally stateful in the 30..49 energy grace band. Starting false
    // matches cleared WRAM; room transitions do not reset it on the cartridge.
    private bool _criticalEnergyDropBias;
    private SamusState? _samusForEnemyDrops;

    /// <summary>Last library-two sound requested by a collected enemy pickup this frame.</summary>
    public ushort? LastEnemyPickupSoundEffect { get; private set; }

    /// <summary>Last pickup kind applied to Samus this frame, useful while stepping AI.</summary>
    public EnemyPickupKind? LastCollectedEnemyPickup { get; private set; }

    /// <summary>
    /// Last library-two request made by the shared death/pickup instruction lists this
    /// frame. Values nine, $24, and $0B are authored directly in bank $86.
    /// </summary>
    public ushort? LastEnemyDeathSoundEffectLibrary2 { get; private set; }

    /// <summary>
    /// Ports <c>EnemyDeathAnimation</c> at $A0:A3AF. The enemy is cleared immediately,
    /// while projectile $F345 retains the header, position, and optional respawn index until
    /// its ROM animation reaches opcode $EEAF and becomes a pickup.
    /// </summary>
    internal void StartGenericEnemyDeath(
        RoomEnemySlot enemy,
        ushort deathAnimation)
    {
        if (deathAnimation >= 5)
            deathAnimation = 0;

        bool respawns = enemy.Properties.HasAny(EnemyProperties.RespawnIfKilled);
        RoomEnemySpawnSnapshot survivingSpawn = enemy.Spawn;
        RoomEnemyProjectileSlot? explosion = AllocateEnemyProjectile();
        if (explosion is not null)
        {
            InitializeEnemyProjectileFromDefinition(
                explosion,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphicsIndex: 0);
            explosion.XPosition = enemy.XPosition;
            explosion.YPosition = enemy.YPosition;
            explosion.EnemyHeaderPointer = enemy.EnemyDefinitionPointer;
            explosion.KilledEnemyNativeIndex = respawns
                ? unchecked((ushort)(enemy.NativeIndex | 0x8000))
                : enemy.NativeIndex;
            explosion.InstructionPointer = ReadWord(
                _bus!,
                0x860000 | unchecked((ushort)(
                    EnemyDeathInstructionPointerTable + deathAnimation * 2)));
            explosion.InstructionTimer = 1;
        }

        enemy.Clear();
        enemy.Spawn = survivingSpawn;
        if (respawns)
        {
            // The native post-memset marker keeps the physical slot unavailable until the
            // projectile's $EF10 instruction reconstructs it from the population record.
            enemy.EnemyDefinitionPointer = 0xdaff;
            enemy.AiBank = 0xa3;
        }
        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
    }

    private static ushort SelectNormalShotDeathAnimation(
        RoomEnemySlot enemy,
        ushort projectileType,
        bool forcePirateBigExplosion = false)
    {
        if (forcePirateBigExplosion)
            return 4;

        ushort causeOfDeath = unchecked((ushort)((projectileType >> 8) & 0x000f));
        if (causeOfDeath != 2)
            return enemy.Definition.DeathAnimation;

        // Missiles normally force explosion two, except headers whose authored value is
        // zero, one, or two. This is the exact signed `< 3` branch in $A0:A67F.
        return enemy.Definition.DeathAnimation < 3
            ? enemy.Definition.DeathAnimation
            : (ushort)2;
    }

    /// <summary>
    /// Shared implementation of the twelve <c>Enemy_ItemDrop_*</c> routines at
    /// $A0:B8EE-$BB9A. Each iteration advances RNG once for both coordinates; allocation
    /// and drop selection then advance it independently inside pickup $F337.
    /// </summary>
    private void SpawnEnemyDropScatter(
        ushort enemyHeaderPointer,
        int count,
        ushort xBase,
        ushort xMask,
        ushort yBase,
        ushort yMask)
    {
        for (int drop = 0; drop < count; drop++)
        {
            ushort random = _nextRandom!();
            ushort x = unchecked((ushort)((random & xMask) + xBase));
            ushort y = unchecked((ushort)(((random & yMask) >> 8) + yBase));
            SpawnEnemyDropFromEnemyHeader(x, y, enemyHeaderPointer);
        }
    }

    private void SpawnEnemyDropScatterAround(
        ushort enemyHeaderPointer,
        int count,
        ushort originX,
        ushort originY)
    {
        for (int drop = 0; drop < count; drop++)
        {
            ushort random = _nextRandom!();
            ushort x = unchecked((ushort)(originX + (random & 0x001f) - 16));
            ushort y = unchecked((ushort)(originY + ((random & 0x1f00) >> 8) - 16));
            SpawnEnemyDropFromEnemyHeader(x, y, enemyHeaderPointer);
        }
    }

    /// <summary>
    /// Allocates pickup projectile $F337 using an ordinary bank-$A0 enemy header. The
    /// allocation search, slot-zero failure, random selection, and no-drop tail all match
    /// <c>SpawnEprojWithRoomGfx</c> followed by <c>InitAI_EnemyProjectile_Pickup</c>.
    /// </summary>
    internal RoomEnemyProjectileSlot? SpawnEnemyDropFromEnemyHeader(
        ushort x,
        ushort y,
        ushort enemyHeaderPointer)
    {
        EnsureLoaded();
        RoomEnemyProjectileSlot? pickup = AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            graphicsIndex: 0);
        pickup.XPosition = x;
        pickup.YPosition = y;
        pickup.EnemyHeaderPointer = enemyHeaderPointer;
        pickup.KilledEnemyNativeIndex = 0xffff;
        InitializeDirectEnemyPickup(pickup);
        return pickup;
    }

    /// <summary>
    /// Allocates pickup $F337 from a bank-$B4 chance table already selected by specialized
    /// boss code. This represents routines such as Botwoon's Draygon-eye drop instruction
    /// without manufacturing a fake enemy header solely to get back to the same six bytes.
    /// </summary>
    internal RoomEnemyProjectileSlot? SpawnEnemyDropFromChanceTable(
        ushort x,
        ushort y,
        ushort itemDropChancesPointer)
    {
        EnsureLoaded();
        RoomEnemyProjectileSlot? pickup = AllocateEnemyProjectile();
        if (pickup is null)
            return null;

        InitializeEnemyProjectileFromDefinition(
            pickup,
            RoomEnemyProjectileKind.EnemyDeathPickup,
            graphicsIndex: 0);
        pickup.XPosition = x;
        pickup.YPosition = y;
        pickup.ItemDropChancesPointerOverride = itemDropChancesPointer;
        pickup.KilledEnemyNativeIndex = 0xffff;
        InitializeDirectEnemyPickup(pickup);
        return pickup;
    }

    private void InitializeDirectEnemyPickup(RoomEnemyProjectileSlot pickup)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(pickup);

        // Both $86:EF29 and $86:EEAF accidentally branch on the physical projectile index
        // after the random routine restores X. Native byte index zero is array slot zero,
        // making that one slot categorically unable to become a pickup.
        if (pickup.SlotIndex == 0 || kind is EnemyPickupKind.None or EnemyPickupKind.NoDrop)
        {
            MakeEnemyPickupDormant(pickup);
            return;
        }

        BeginEnemyPickup(pickup, kind);
    }

    /// <summary>
    /// Converts a completed enemy-death explosion in place. Keeping the actor in its
    /// existing slot preserves both the slot-zero bug and the retained respawn index.
    /// </summary>
    internal void ConvertEnemyDeathExplosionToPickup(RoomEnemyProjectileSlot projectile)
    {
        EnemyPickupKind kind = SelectRandomEnemyDrop(projectile);
        if (projectile.SlotIndex == 0 ||
            kind is EnemyPickupKind.None or EnemyPickupKind.NoDrop)
        {
            MakeEnemyPickupDormant(projectile);
            return;
        }

        BeginEnemyPickup(projectile, kind);
    }

    private void BeginEnemyPickup(
        RoomEnemyProjectileSlot projectile,
        EnemyPickupKind kind)
    {
        ushort tableOffset = checked((ushort)((ushort)kind * 2));
        projectile.Variable0 = tableOffset;
        projectile.InstructionPointer = ReadWord(
            _bus!,
            EnemyPickupInstructionPointerTable + tableOffset);
        projectile.InstructionTimer = 1;
        projectile.Variable1 = EnemyPickupLifetime;
        projectile.PreInstruction = EnemyPickupPreInstruction;

        // $86:EEAF clears property $4000 but retains the other definition bits. The typed
        // representation stores the three collision flags separately from the low damage.
        projectile.PersistsOnSamusContact = false;
    }

    private static void MakeEnemyPickupDormant(RoomEnemyProjectileSlot projectile)
    {
        // The blank map lasts 64 frames before $EF10 handles a retained respawning enemy
        // and $8154 deletes the actor. Collection and natural expiry use this same tail.
        projectile.InstructionPointer = EnemyDeathNoDropTail;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = EnemyProjectileCodePointers.RTS_86EFDF;
        projectile.CanDamageSamus = false;
        projectile.PersistsOnSamusContact = false;
        projectile.BlocksSamusProjectiles = false;
    }

    /// <summary>
    /// Exact byte-accumulator translation of $86:F106. Minor drops are renormalized into
    /// the budget left after enabled super/PB chances; the two major drops retain their raw
    /// table probabilities. All additions which occurred in eight-bit accumulator mode are
    /// explicitly wrapped here.
    /// </summary>
    internal EnemyPickupKind SelectRandomEnemyDrop(RoomEnemyProjectileSlot projectile)
    {
        SamusState samus = _samusForEnemyDrops ?? throw new InvalidOperationException(
            "Enemy drop selection requires the active Samus state.");
        ushort chancesPointer = ResolveEnemyDropChancesPointer(projectile);
        if (chancesPointer == 0)
            return EnemyPickupKind.NoDrop;

        Span<byte> chances = stackalloc byte[6];
        for (int index = 0; index < chances.Length; index++)
        {
            chances[index] = _bus!.ReadByte(
                EnemyDropChancesBank | unchecked((ushort)(chancesPointer + index)));
        }

        byte random;
        do
        {
            random = unchecked((byte)_nextRandom!());
        }
        while (random == 0);

        byte majorAdjustedBudget = 0xff;
        ushort accumulated = 0;
        ushort combinedEnergy = unchecked((ushort)(
            samus.Health + samus.ReserveEnergy));
        if (combinedEnergy < 30)
            _criticalEnergyDropBias = true;
        else if (combinedEnergy >= 50)
            _criticalEnergyDropBias = false;
        // Values 30..49 deliberately retain the previous flag: this is the cartridge's
        // hysteresis/grace period, not an omitted assignment.

        byte pooledMinorWeight;
        byte enabledMask;
        if (_criticalEnergyDropBias)
        {
            pooledMinorWeight = unchecked((byte)(chances[0] + chances[1]));
            enabledMask = 0x03;
        }
        else
        {
            pooledMinorWeight = chances[3];
            enabledMask = 0x08;
            if (samus.Health != samus.MaxHealth ||
                samus.ReserveEnergy != samus.MaxReserveEnergy)
            {
                pooledMinorWeight = unchecked((byte)(
                    pooledMinorWeight + chances[0] + chances[1]));
                enabledMask |= 0x03;
            }
            if (samus.Missiles != samus.MaxMissiles)
            {
                pooledMinorWeight = unchecked((byte)(pooledMinorWeight + chances[2]));
                enabledMask |= 0x04;
            }
            if (samus.SuperMissiles != samus.MaxSuperMissiles)
            {
                majorAdjustedBudget = unchecked((byte)(
                    majorAdjustedBudget - chances[4]));
                enabledMask |= 0x10;
            }
            if (samus.PowerBombs != samus.MaxPowerBombs)
            {
                majorAdjustedBudget = unchecked((byte)(
                    majorAdjustedBudget - chances[5]));
                enabledMask |= 0x20;
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
                    return EnemyDropAccumulatorOrder[accumulatorIndex];
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
                return EnemyDropAccumulatorOrder[accumulatorIndex];
        }

        return EnemyPickupKind.NoDrop;
    }

    private ushort ResolveEnemyDropChancesPointer(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.ItemDropChancesPointerOverride != 0)
            return projectile.ItemDropChancesPointerOverride;
        if (projectile.EnemyHeaderPointer == 0)
            return 0;
        return ReadDefinition(_bus!, projectile.EnemyHeaderPointer).ItemDropChancesPointer;
    }

    private void RunEnemyPickupPreInstruction(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus)
    {
        projectile.Variable1 = unchecked((ushort)(projectile.Variable1 - 1));
        if (projectile.Variable1 == 0)
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
            AbsoluteWrappedDelta(projectile.XPosition, samus.Grapple.AnchorX) < 16 &&
            AbsoluteWrappedDelta(projectile.YPosition, samus.Grapple.AnchorY) < 16)
        {
            CollectEnemyPickup(projectile, samus);
            return;
        }

        // $86:F057's subtract/borrow formulation is exactly strict radius-sum overlap for
        // valid actor radii. Equality is not contact, matching the BCS exits in both axes.
        if (AbsoluteWrappedDelta(samus.XPosition, projectile.XPosition) >=
                samus.Kinematics.XRadius + projectile.XRadius ||
            AbsoluteWrappedDelta(samus.YPosition, projectile.YPosition) >=
                samus.Kinematics.YRadius + projectile.YRadius)
        {
            return;
        }

        CollectEnemyPickup(projectile, samus);
    }

    private void CollectEnemyPickup(
        RoomEnemyProjectileSlot projectile,
        SamusState samus)
    {
        EnemyPickupKind kind = (EnemyPickupKind)(projectile.Variable0 >> 1);
        switch (kind)
        {
            case EnemyPickupKind.SmallEnergy:
                RestoreEnemyDropEnergy(samus, 5);
                LastEnemyPickupSoundEffect = 1;
                break;
            case EnemyPickupKind.BigEnergy:
                RestoreEnemyDropEnergy(samus, 20);
                LastEnemyPickupSoundEffect = 2;
                break;
            case EnemyPickupKind.PowerBomb:
                RestoreEnemyDropPowerBombs(samus, 1);
                LastEnemyPickupSoundEffect = 5;
                break;
            case EnemyPickupKind.Missile:
                RestoreEnemyDropMissiles(samus, 2);
                LastEnemyPickupSoundEffect = 3;
                break;
            case EnemyPickupKind.SuperMissile:
                RestoreEnemyDropSuperMissiles(samus, 1);
                LastEnemyPickupSoundEffect = 4;
                break;
            default:
                throw new InvalidDataException(
                    $"Enemy pickup slot {projectile.SlotIndex} contains invalid type ${projectile.Variable0:X4}.");
        }

        LastCollectedEnemyPickup = kind;
        MakeEnemyPickupDormant(projectile);
    }

    private static void RestoreEnemyDropEnergy(SamusState samus, ushort amount)
    {
        ushort restored = unchecked((ushort)(samus.Health + amount));
        samus.Health = restored;
        if (unchecked((short)(restored - samus.MaxHealth)) < 0)
            return;

        ushort reserve = unchecked((ushort)(
            samus.ReserveEnergy + restored - samus.MaxHealth));
        if (unchecked((short)(reserve - samus.MaxReserveEnergy)) >= 0)
            reserve = samus.MaxReserveEnergy;
        samus.ReserveEnergy = reserve;
        if (reserve != 0 && samus.ReserveTankMode == 0)
            samus.ReserveTankMode = 1;
        samus.Health = samus.MaxHealth;
    }

    private static void RestoreEnemyDropMissiles(SamusState samus, ushort amount)
    {
        ushort restored = unchecked((ushort)(samus.Missiles + amount));
        samus.Missiles = restored;
        if (unchecked((short)(restored - samus.MaxMissiles)) < 0)
            return;

        samus.ReserveMissiles = unchecked((ushort)(
            samus.ReserveMissiles + restored - samus.MaxMissiles));
        ushort reserveCap = unchecked((short)(samus.MaxMissiles - 99)) < 0
            ? samus.MaxMissiles
            : (ushort)99;
        if (unchecked((short)(samus.ReserveMissiles - reserveCap)) >= 0)
            samus.ReserveMissiles = reserveCap;
        samus.Missiles = samus.MaxMissiles;
    }

    private static void RestoreEnemyDropSuperMissiles(SamusState samus, ushort amount)
    {
        ushort restored = unchecked((ushort)(samus.SuperMissiles + amount));
        samus.SuperMissiles = restored;
        if (restored != samus.MaxSuperMissiles &&
            unchecked((short)(restored - samus.MaxSuperMissiles)) >= 0)
        {
            samus.SuperMissiles = samus.MaxSuperMissiles;
        }
    }

    private static void RestoreEnemyDropPowerBombs(SamusState samus, ushort amount)
    {
        ushort restored = unchecked((ushort)(samus.PowerBombs + amount));
        samus.PowerBombs = restored;
        if (restored != samus.MaxPowerBombs &&
            unchecked((short)(restored - samus.MaxPowerBombs)) >= 0)
        {
            samus.PowerBombs = samus.MaxPowerBombs;
        }
    }

    private static int AbsoluteWrappedDelta(ushort left, ushort right) =>
        Math.Abs((int)unchecked((short)(left - right)));
}
