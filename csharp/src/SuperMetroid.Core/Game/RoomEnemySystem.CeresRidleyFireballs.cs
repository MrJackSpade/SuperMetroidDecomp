using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The bank-$86 definition pointer occupying one live room-enemy projectile slot. Keeping
/// the cartridge pointer as the enum value makes debugger state directly comparable with
/// <c>eproj_id</c> rather than inventing a host-only identity layer.
/// </summary>
public enum RoomEnemyProjectileKind : ushort
{
    None = 0,
    YappingMawBody = 0xec95,
    SkreeParticleDownRight = 0x8bc2,
    SkreeParticleUpRight = 0x8bd0,
    SkreeParticleDownLeft = 0x8bde,
    SkreeParticleUpLeft = 0x8bec,
    MetareeParticleDownRight = 0x8bfa,
    MetareeParticleUpRight = 0x8c08,
    MetareeParticleDownLeft = 0x8c16,
    MetareeParticleUpLeft = 0x8c24,
    CrocomireProjectile = 0x8f8f,
    CrocomireBridgeCrumbling = 0x8f9d,
    CrocomireSpikeWallPieces = 0x90c1,
    KraidSpitRock = 0x9c45,
    KraidCeilingRock = 0x9c53,
    KraidRisingRockLeft = 0x9c61,
    KraidRisingRockRight = 0x9c6f,
    PhantoonDestroyableFlame = 0x9c29,
    PhantoonStartingFlame = 0x9c37,
    DraygonGoop = 0x8e50,
    DraygonWallTurret = 0x8e5e,
    MotherBrainRoomTurret = 0xc17e,
    MotherBrainRoomTurretBullet = 0xc18c,
    MotherBrainGlassShard = 0xcefc,
    MotherBrainGlassSparkle = 0xcf0a,
    MotherBrainOnionRing = 0xcb4b,
    MotherBrainBomb = 0xcb59,
    MotherBrainHandBeamCharging = 0xcb67,
    MotherBrainHandBeamFired = 0xcb75,
    MotherBrainRainbowBeamCharging = 0xcb83,
    MotherBrainPurpleBreathBig = 0xcb2f,
    MotherBrainDrool = 0xcb91,
    MotherBrainDyingDrool = 0xcb9f,
    MotherBrainRainbowBeamExplosion = 0xcbad,
    /// <summary>$86:CB13 body-relative death explosions; C914 follows the moving body.</summary>
    MotherBrainDeathExplosion = MotherBrainDeathRomData.ExplosionDefinition,
    MotherBrainEscapeDoorFragment = MotherBrainDeathRomData.DoorFragmentDefinition,
    MotherBrainEscapeSubtitle = MotherBrainDeathRomData.SubtitleDefinition,
    MotherBrainTopRightTube = 0xcc5b,
    MotherBrainTopLeftTube = 0xcc69,
    MotherBrainTopMiddleLeftTube = 0xcc77,
    MotherBrainTopMiddleRightTube = 0xcc85,
    CeresRidleyFireball = 0x9642,
    CeresRidleyHorizontalAfterburnCenter = 0x9650,
    CeresRidleyVerticalAfterburnCenter = 0x965e,
    CeresRidleyHorizontalAfterburnRight = 0x966c,
    CeresRidleyHorizontalAfterburnLeft = 0x967a,
    CeresRidleyVerticalAfterburnUp = 0x9688,
    CeresRidleyVerticalAfterburnDown = 0x9696,
    CeresFallingDebrisLight = 0x9734,
    CeresFallingDebrisDark = 0x9742,
    GunshipLiftoffDustCloud = 0xa379,
    AlcoonFireball = 0x9e90,
    PowampSpike = 0xd298,
    WorkRobotLaserUpLeft = 0xd2a6,
    WorkRobotLaserHorizontal = 0xd2b4,
    WorkRobotLaserDownLeft = 0xd2c2,
    WorkRobotLaserUpRight = 0xd2d0,
    WorkRobotLaserDownRight = 0xd2de,
    FallingSpark = 0xf498,
    NuclearWaffleBody = 0xbbc7,
    FakeKraidSpit = 0x9db0,
    FakeKraidSpikeLeft = 0x9dbe,
    FakeKraidSpikeRight = 0x9dcc,
    KiHunterAcidSpitLeft = 0xcf18,
    KiHunterAcidSpitRight = 0xcf26,
    PirateMotherBrainLaser = 0xa17b,
    PirateClaw = 0xa189,
    PolypRock = 0xbd5a,
    CacatacSpike = 0xdafe,
    StokeProjectile = 0xdbf2,
    NamiheFireball = 0xdfbc,
    FuneFireball = 0xdfca,
    LavaThrownByMagdollite = 0xe0e0,
    DragonFireball = 0xb5cb,
    EyeDoorProjectile = 0xb743,
    EyeDoorSweat = 0xb751,
    EyeDoorSmoke = 0xe517,
    MiscDustExplosion = 0xe509,
    EnemyDeathPickup = 0xf337,
    EnemyDeathExplosion = 0xf345,
    KagoBug = 0xd02e,
    BotwoonBody = 0xeba0,
    BotwoonSpit = 0xec48,
    BombTorizoLowHealthDrool = 0xa95b,
    BombTorizoInitialDrool = 0xa969,
    BombTorizoExplosiveSwipe = 0xa985,
    BombTorizoLowHealthExplosion = 0xa9a1,
    BombTorizoDeathExplosion = 0xa9af,
    BombTorizoStatueBreaking = 0xa993,
    BombTorizoChozoOrb = 0xad5e,
    BombTorizoSonicBoom = 0xaea8,
    GoldenTorizoChozoOrb = 0xad7a,
    GoldenTorizoSonicBoom = 0xaeb6,
    BombTorizoRightFootDust = 0xafe5,
    BombTorizoLeftFootDust = 0xaff3,
    GoldenTorizoEgg = 0xb1c0,
    GoldenTorizoSuperMissile = 0xb31a,
    GoldenTorizoEyeBeam = 0xb428,
    TourianStatueRidley = 0xbaa2,
    TourianStatuePhantoon = 0xbab0,
    TourianStatueBaseDecoration = 0xbabe,
    WreckedShipChozoSpikeFootstep = 0xaf68,
    WreckedShipChozoSpikeFootstepAlternate = 0xaf76,
    ShaktoolAttackFrontCircle = 0xbe25,
    ShaktoolAttackMiddleCircle = 0xbe33,
    ShaktoolAttackBackCircle = 0xbe41,
    SporeSpawnStalk = 0xde6c,
    SporeSpawnSpore = 0xde7a,
    SporeSpawnSpawner = 0xde88,
    SaveStationElectricity = 0xe6d2,
    /// <summary>Gate actor spawned when a downward gate begins closing.</summary>
    DownwardGateMoving = 0xe64b,
    /// <summary>Gate actor parked at the bottom when a room initially loads.</summary>
    DownwardGateClosed = 0xe659,
    NoobTubeCrack = 0xd904,
    NoobTubeShard = 0xd912,
    NoobTubeReleasedAirBubble = 0xd920,
}

/// <summary>
/// The mutually exclusive draw pass selected by enemy-projectile property bit
/// <c>$1000</c>. Bank <c>$86:8390/$83B2</c> traverses the same physical pool once for
/// each value, on opposite sides of Samus's OAM submission.
/// </summary>
public enum EnemyProjectileDrawPriority : byte
{
    Low = 0,
    High = 1,
}

/// <summary>
/// Debugger-visible projection of one of bank $86's eighteen fixed enemy-projectile slots.
/// Positions retain separate 16-bit subpositions because a fireball's signed 8.8 velocity
/// is added to the high byte of that fraction by the original movement helpers.
/// </summary>
public sealed class RoomEnemyProjectileSlot
{
    internal RoomEnemyProjectileSlot(int slotIndex) => SlotIndex = slotIndex;

    public int SlotIndex { get; }
    public RoomEnemyProjectileKind Kind { get; internal set; }
    public bool IsActive => Kind != RoomEnemyProjectileKind.None;
    public ushort XPosition { get; internal set; }
    public ushort XSubposition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort YSubposition { get; internal set; }
    public ushort XVelocity { get; internal set; }
    public ushort YVelocity { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort PreInstruction { get; internal set; }
    public ushort GraphicsIndex { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }
    public ushort Damage { get; internal set; }
    public ushort InvincibilityFrames { get; internal set; }
    /// <summary>
    /// Draw pass selected by native enemy-projectile property bit <c>$1000</c>.
    /// This is independent of the two-bit priority stored in each spritemap's OAM word.
    /// </summary>
    public EnemyProjectileDrawPriority DrawPriority { get; internal set; }
    /// <summary>
    /// Bank $86's independent <c>eproj_timers</c> word. This is not the frame-list
    /// instruction timer: projectile bytecode explicitly initializes and decrements this
    /// value for counted loops and randomized impact animations.
    /// </summary>
    public ushort GeneralTimer { get; internal set; }
    public ushort RemainingAfterburns { get; internal set; }
    public ushort NextAfterburnKind { get; internal set; }
    public ushort DirectionParameter { get; internal set; }
    /// <summary>Native generic enemy-projectile variable E at WRAM <c>$1AFF,x</c>.</summary>
    public ushort Variable0 { get; internal set; }
    /// <summary>Native generic enemy-projectile variable F at WRAM <c>$1B23,x</c>.</summary>
    public ushort Variable1 { get; internal set; }
    public bool CanDamageSamus { get; internal set; }
    /// <summary>Native projectile property $4000: contact does not delete this actor.</summary>
    public bool PersistsOnSamusContact { get; internal set; }
    /// <summary>Native projectile property $8000: Samus's shots test this actor for collision.</summary>
    public bool BlocksSamusProjectiles { get; internal set; }
    /// <summary>
    /// Native enemy-projectile initializer flag. Zero runs the actor's shot list, one
    /// creates an indestructible dud, and two suppresses the collision scan altogether.
    /// This is deliberately separate from property $8000: Kago enables that property only
    /// after its bug has moved far enough from the source shell, while retaining flag zero.
    /// </summary>
    public ushort CollisionOption { get; internal set; }
    /// <summary>
    /// Native <c>eproj_G</c> collision word after a destructible projectile is shot. Kago
    /// reuses the same word as its idle timer before collision, so the typed bug wrapper is
    /// the authority on which meaning is currently active.
    /// </summary>
    public ushort CollidedProjectileType { get; internal set; }
    /// <summary>
    /// Native <c>eproj_killed_enemy_index</c>. Enemy-death explosions set bit $8000 when
    /// their terminal instruction must rebuild this physical enemy slot.
    /// </summary>
    public ushort KilledEnemyNativeIndex { get; internal set; }
    /// <summary>
    /// Native bank-$A0 enemy-header pointer retained in the parallel bank-$7E projectile
    /// metadata. Death explosions use offset $3A of this record to find their six-byte
    /// bank-$B4 drop table.
    /// </summary>
    public ushort EnemyHeaderPointer { get; internal set; }
    /// <summary>
    /// Direct bank-$B4 drop-table pointer used by the few boss/projectile instructions
    /// which already resolved a special drop table before allocating pickup $F337. Zero
    /// means that <see cref="EnemyHeaderPointer"/> remains authoritative.
    /// </summary>
    public ushort ItemDropChancesPointerOverride { get; internal set; }

    internal void Clear()
    {
        Kind = RoomEnemyProjectileKind.None;
        XPosition = XSubposition = YPosition = YSubposition = 0;
        XVelocity = YVelocity = 0;
        InstructionPointer = InstructionTimer = SpritemapPointer = PreInstruction = 0;
        GraphicsIndex = XRadius = YRadius = Damage = InvincibilityFrames = GeneralTimer = 0;
        DrawPriority = EnemyProjectileDrawPriority.Low;
        RemainingAfterburns = NextAfterburnKind = 0;
        DirectionParameter = Variable0 = Variable1 = 0;
        CollisionOption = CollidedProjectileType = KilledEnemyNativeIndex = 0;
        EnemyHeaderPointer = ItemDropChancesPointerOverride = 0;
        CanDamageSamus = PersistsOnSamusContact = BlocksSamusProjectiles = false;
    }
}

public sealed partial class RoomEnemySystem
{
    // Super Metroid reserves native indexes $00..$22, in steps of two, for eighteen enemy
    // projectiles. Keeping the same capacity exposes saturation and spawn failure honestly.
    private const int RoomEnemyProjectileSlotCount = 18;
    private static readonly ushort FireballGraphicsIndex = EnemyPaletteBits.Palette5;

    private readonly RoomEnemyProjectileSlot[] _enemyProjectiles =
        Enumerable.Range(0, RoomEnemyProjectileSlotCount)
            .Select(index => new RoomEnemyProjectileSlot(index))
            .ToArray();
    private byte _currentEnemyProjectileFrame8;

    /// <summary>All eighteen physical bank-$86 slots, including currently inactive slots.</summary>
    public IReadOnlyList<RoomEnemyProjectileSlot> EnemyProjectiles => _enemyProjectiles;

    /// <summary>
    /// Last library-one dud sound requested when an indestructible enemy projectile blocked
    /// a Samus shot. This is a frame publication; the audio mixer remains an outer seam.
    /// </summary>
    public ushort? LastEnemyProjectileDudSoundEffect { get; private set; }

    /// <summary>Number of live actors in the shared bank-$86 enemy-projectile pool.</summary>
    public int ActiveEnemyProjectileCount =>
        _enemyProjectiles.Count(projectile => projectile.IsActive);

    /// <summary>
    /// Allocates the light or dark Ceres falling-tile actor requested by room main
    /// <c>$8F:E525</c>. The X coordinate and palette variant come directly from the room's
    /// current RNG word; this initializer owns only bank-$86's projectile fields.
    /// </summary>
    public void SpawnCeresFallingDebris(ushort xPosition, bool dark)
    {
        EnsureLoaded();
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RoomEnemyProjectileKind kind = dark
            ? RoomEnemyProjectileKind.CeresFallingDebrisDark
            : RoomEnemyProjectileKind.CeresFallingDebrisLight;
        InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: EnemyPaletteBits.Palette7);
        projectile.XPosition = xPosition;
        projectile.YPosition = 0x002a;
        projectile.XVelocity = 0;
        projectile.YVelocity = 0x0010;
        projectile.Variable0 = 0;
        projectile.Variable1 = 0;
    }

    /// <summary>
    /// Ports <c>SpawnEprojWithRoomGfx($E6D2, 0)</c> and initializer $86:E6AD.
    /// The cartridge derives the actor origin from the currently executing save-station
    /// PLM: one block right and two blocks above its trigger block.
    /// </summary>
    public void SpawnSaveStationElectricity(int plmBlockIndex, int roomWidthInBlocks)
    {
        EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        ArgumentOutOfRangeException.ThrowIfNegative(plmBlockIndex);

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
        {
            // SpawnEprojInner deliberately drops a spawn when all eighteen physical slots
            // are occupied. This is the cartridge's finite-pool behavior, not a host error.
            return;
        }

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.SaveStationElectricity,
            graphicsIndex: 0);
        int blockX = plmBlockIndex % roomWidthInBlocks;
        int blockY = plmBlockIndex / roomWidthInBlocks;
        projectile.XPosition = unchecked((ushort)(16 * (blockX + 1)));
        projectile.YPosition = unchecked((ushort)(16 * (blockY - 2)));
    }

    /// <summary>
    /// Allocates one of gunship function <c>$A2:AC1B</c>'s six room-graphics dust actors.
    /// Parameter values are even byte offsets <c>0..A</c> into the native X/list tables.
    /// </summary>
    private void SpawnGunshipLiftoffDustCloud(ushort parameter, SamusState samus)
    {
        if (parameter > 0x000a || (parameter & 1) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Gunship dust parameter must be 0,2,4,6,8,A.");
        }

        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.GunshipLiftoffDustCloud,
            graphicsIndex: 0);
        int tableIndex = parameter >> 1;
        projectile.XPosition = unchecked((ushort)(samus.XPosition +
            unchecked((short)ReadWord(
                _bus!,
                EnemyRomTablePointers.Ceres.FallingDebrisXOffsetWords + tableIndex * 2))));
        projectile.YPosition = unchecked((ushort)(samus.YPosition + 0x0050));
        projectile.XVelocity = 0;
        projectile.YVelocity = 0;
        projectile.Variable0 = parameter;
        projectile.InstructionPointer = ReadWord(
            _bus!,
            EnemyRomTablePointers.Ceres.FallingDebrisInstructionPointers + tableIndex * 2);
        projectile.InstructionTimer = 1;
    }

    /// <summary>
    /// Ports <c>EprojProjCollDet</c> and <c>HandleEprojCollWithProj</c> at
    /// $A0:996C-$9A30. Flag-one Nuclear Waffle links create duds and remain alive; flag-zero
    /// Kago bugs remember the incoming projectile type and switch to their ROM shot list.
    /// </summary>
    public int ResolveEnemyProjectileSamusProjectileHits(
        ISnesAddressSpace bus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(projectiles);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        EnsureLoaded();

        int hitCount = 0;
        for (int enemyProjectileIndex = _enemyProjectiles.Length - 1;
             enemyProjectileIndex >= 0;
             enemyProjectileIndex--)
        {
            RoomEnemyProjectileSlot enemyProjectile =
                _enemyProjectiles[enemyProjectileIndex];
            if (!enemyProjectile.IsActive ||
                !enemyProjectile.BlocksSamusProjectiles ||
                enemyProjectile.CollisionOption == 2)
                continue;

            foreach (SamusProjectileSlot shot in projectiles.Slots.Take(5))
            {
                if (!shot.IsActive)
                    continue;

                SamusProjectileTypeWord shotType = shot.PackedType;
                if (shotType.Family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb ||
                    shotType.FamilyValue >= (ushort)SamusProjectileFamily.BeamExplosion)
                    continue;

                // The native path intentionally compares only 32-pixel cells. It does not
                // run the radius overlap used by ordinary enemies, a coarse quirk preserved
                // here instead of “fixing” shots near a cell corner.
                if ((enemyProjectile.XPosition & 0xffe0) != (shot.XPosition & 0xffe0) ||
                    (enemyProjectile.YPosition & 0xffe0) != (shot.YPosition & 0xffe0))
                {
                    continue;
                }

                // `$A0:99F9-$9A07` only sets direction lifecycle bit $10 for non-plasma
                // shots. It does not call the ordinary enemy-impact routine and therefore
                // cannot rewrite a freshly fired Super Missile (or its invisible link) to
                // family $0800 in the producer frame. Its own pre-instruction consumes the
                // mark on the next projectile pass.
                ushort collidedProjectileType = shot.Type;
                if ((collidedProjectileType & 8) == 0)
                    shot.Direction = shot.PackedDirection.WithCollisionLifecycleState();

                if (enemyProjectile.CollisionOption == 1)
                {
                    LastEnemyProjectileDudSoundEffect = 0x003d;
                }
                else
                {
                    enemyProjectile.CollidedProjectileType = collidedProjectileType;
                    enemyProjectile.InstructionPointer = ReadWord(
                        bus,
                        0x860000 | unchecked((ushort)((ushort)enemyProjectile.Kind + 12)));
                    enemyProjectile.InstructionTimer = 1;
                    enemyProjectile.PreInstruction = EnemyProjectileCodePointers.RTS_8684FB;

                    // Native masks properties with $0FFF. The typed fields below are the
                    // three high property bits represented by this runtime, so clearing
                    // them is the exact structural equivalent rather than a Kago special.
                    enemyProjectile.BlocksSamusProjectiles = false;
                    enemyProjectile.PersistsOnSamusContact = false;
                    enemyProjectile.CanDamageSamus = true;
                }
                hitCount++;
            }
        }

        return hitCount;
    }

    /// <summary>
    /// Executes the projectile portion of the gameplay frame after enemy instructions have
    /// had their opportunity to spawn a fireball. This is the same producer/consumer order
    /// as EnemyMain followed by bank $86's enemy-projectile handler.
    /// </summary>
    public void StepEnemyProjectiles(
        RoomLevelData level,
        SamusState? samus,
        ushort controllerInput = 0,
        ushort cameraX = 0,
        ushort cameraY = 0,
        byte? nmiFrameCounter8 = null,
        SamusBombProjectileSystem? samusBombs = null)
    {
        ArgumentNullException.ThrowIfNull(level);
        EnsureLoaded();
        _samusForEnemyDrops = samus;
        LastEnemyPickupSoundEffect = null;
        LastCollectedEnemyPickup = null;
        LastEnemyDeathSoundEffectLibrary2 = null;

        // `$86:810D-$8122` starts X at physical byte index `$22`, executes that slot, reloads
        // the unchanged outer index from `$1991`, subtracts two, and continues through `$00`.
        // Array index 17 is native `$22`, so this must be a live descending scan rather than
        // LINQ's ascending enumeration or a frame-start snapshot.
        //
        // The distinction is observable whenever a pre-instruction or instruction opcode
        // allocates another projectile. `$86:8027` searches `$22 -> $00` without changing
        // the outer `$1991`. A child allocated below the current physical index is therefore
        // reached later in THIS pass; a child allocated above it waits until the next pass.
        // Reusing the current slot also lets the replacement's instruction list run in this
        // pass after the pre-instruction returns. Reading each array entry at loop time
        // preserves all three cases and prevents host collection semantics from inventing a
        // universal one-frame spawn delay.
        byte projectileFrame = nmiFrameCounter8 ?? _standaloneEnemyProjectileFrameCounter8++;
        _currentEnemyProjectileFrame8 = projectileFrame;
        for (int projectileIndex = _enemyProjectiles.Length - 1;
             projectileIndex >= 0;
             projectileIndex--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[projectileIndex];
            if (!projectile.IsActive)
                continue;

            RunEnemyProjectilePreInstruction(
                projectile,
                level,
                samus,
                cameraX,
                cameraY,
                projectileFrame,
                samusBombs);
            if (!projectile.IsActive)
                continue;

            ProcessEnemyProjectileInstructions(projectile, samus, cameraX, cameraY);
        }

        // Native gameplay runs `$86:868B` for every projectile first, then enters the
        // separate `$A0:9894` Samus-collision pass. That pass samples invincibility and
        // contact-damage state once at entry; damage from one overlapping projectile does
        // not abort the remaining descending-slot scan. This distinction is observable for
        // Puromi/Nuclear Waffle, whose four persistent body links begin co-located.
        bool collisionPassEnabled = samus is not null &&
            samus.InvincibilityTimer == 0 &&
            samus.HorizontalSpeed.ContactDamageIndex == 0;
        if (!collisionPassEnabled)
            return;

        ushort? finalKnockbackXDirection = null;
        for (int index = _enemyProjectiles.Length - 1; index >= 0; index--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[index];
            if (projectile.IsActive)
            {
                finalKnockbackXDirection =
                    ResolveEnemyProjectileSamusCollision(projectile, samus!) ??
                    finalKnockbackXDirection;
            }
        }

        // `$A0:9923` only publishes the five-frame request and overwrites its horizontal
        // direction for every hit. Bank $90 consumes those words once, after the complete
        // descending collision scan. Our typed initializer represents that later consumer,
        // so invoke it once with the final overlapping slot's direction after retaining all
        // per-slot damage above.
        if (finalKnockbackXDirection.HasValue)
        {
            SamusKnockbackMovement.Start(
                _bus!,
                samus!,
                controllerInput,
                finalKnockbackXDirection.Value,
                knockbackTimer: 5);
        }
    }

    /// <summary>
    /// Emits both native enemy-projectile priority passes for focused diagnostics which do
    /// not draw Samus between them. Gameplay must call the two phase-specific methods so
    /// property bit <c>$1000</c> determines which side of Samus owns each OAM record.
    /// </summary>
    public void DrawEnemyProjectiles(OamBuffer oam, ushort cameraX, ushort cameraY, bool timeIsFrozen = false)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();

        DrawRoomSpriteObjects(oam, cameraX, cameraY);
        DrawEnemyProjectilePass(
            oam,
            cameraX,
            cameraY,
            EnemyProjectileDrawPriority.High, timeIsFrozen);
        DrawEnemyProjectilePass(
            oam,
            cameraX,
            cameraY,
            EnemyProjectileDrawPriority.Low, timeIsFrozen);
    }

    /// <summary>
    /// Ports <c>Draw_HighPriority_EnemyProjectile</c> at <c>$86:8390</c>, including the
    /// preceding room-sprite-object phase at <c>$A0:8855</c>.
    /// </summary>
    public void DrawHighPriorityEnemyProjectiles(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY,
        bool timeIsFrozen = false)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();

        // Sprite objects precede only the high pass. Repeating them in the low pass would
        // duplicate explosions, dust, and Spark's trail in the same hardware OAM image.
        DrawRoomSpriteObjects(oam, cameraX, cameraY);
        DrawEnemyProjectilePass(
            oam,
            cameraX,
            cameraY,
            EnemyProjectileDrawPriority.High, timeIsFrozen);
    }

    /// <summary>Ports <c>Draw_LowPriority_EnemyProjectile</c> at <c>$86:83B2</c>.</summary>
    public void DrawLowPriorityEnemyProjectiles(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY,
        bool timeIsFrozen = false)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();

        DrawEnemyProjectilePass(
            oam,
            cameraX,
            cameraY,
            EnemyProjectileDrawPriority.Low, timeIsFrozen);
    }

    private void DrawEnemyProjectilePass(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY,
        EnemyProjectileDrawPriority priority,
        bool timeIsFrozen)
    {
        var (shakeX, shakeY) = GetEnemyProjectileShake(timeIsFrozen);
        // Both cartridge routines scan native indexes $22,$20,...,$00. Physical array
        // index seventeen therefore reaches OAM first and wins equal-priority overlap.
        for (int projectileIndex = _enemyProjectiles.Length - 1;
             projectileIndex >= 0;
             projectileIndex--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[projectileIndex];

            if (!projectile.IsActive ||
                projectile.DrawPriority != priority ||
                projectile.SpritemapPointer == 0)
                continue;

            ushort screenX = unchecked((ushort)(projectile.XPosition + shakeX - cameraX));
            ushort screenY = unchecked((ushort)(projectile.YPosition + shakeY - cameraY));
            if (((screenX + 128) & 0xfe00) != 0 || ((screenY + 128) & 0xfe00) != 0)
                continue;

            oam.AddEnemyProjectileSpritemap(
                _bus!,
                projectile.SpritemapPointer,
                screenX,
                screenY,
                projectile.GraphicsIndex,
                originYIsOnScreen: (screenY >> 8) == 0);
        }
    }

    /// <summary>Ports Ridley instruction $A6:E84D and retains its asymmetric aim clamps.</summary>
    private void CalculateRidleyFireballVelocity(RoomEnemySlot ridley, SamusState samus)
    {
        RidleyEnemyState state = RequireRidley(ridley);
        int muzzleX = ridley.XPosition + (state.FacingDirection == 0 ? -25 : 25);
        int muzzleY = ridley.YPosition - 43;
        byte cartridgeAngle = CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - muzzleX)),
            unchecked((short)(samus.YPosition - muzzleY)));
        byte angle = unchecked((byte)-(cartridgeAngle + 0x80));

        if (state.FacingDirection == 0)
        {
            if (angle < 0x40 || angle >= 0xeb)
                angle = 0xeb;
            else if (angle < 0xb0)
                angle = 0xb0;
        }
        else
        {
            if (angle < 0x15 || angle >= 0xc0)
                angle = 0x15;
            else if (angle >= 0x50)
                angle = 0x50;
        }

        // Math_MultBySin/Cos at $86:C26C reads signed words from the shared table at
        // $A0:B443. The apparently relevant $94:A957 address is executable grapple
        // code, not table data; treating that code as samples produces enormous bogus
        // velocities. The native index is `angle * 2` because X is a byte offset into
        // 16-bit entries; callers add $40 themselves when they want cosine. Its multiply
        // takes the absolute table word,
        // shifts by eight, then reapplies the sign. Speed $0500 therefore remains a
        // signed 8.8 velocity whose magnitude never exceeds $0500.
        state.FireballXVelocity = MultiplyCartridgeSinCos(0x0500, angle);
        state.FireballYVelocity = MultiplyCartridgeSinCos(0x0500, unchecked((byte)(angle + 64)));
    }

    private ushort MultiplyCartridgeSinCos(ushort speed, byte angle)
    {
        short tableValue = unchecked((short)ReadWord(
            _bus!, EnemyRomTablePointers.Common.SignedSineCosineWords + angle * 2));
        int magnitude = speed * Math.Abs((int)tableValue) >> 8;
        return unchecked((ushort)(tableValue < 0 ? -magnitude : magnitude));
    }

    /// <summary>Allocates and initializes enemy projectile $86:9642.</summary>
    private void SpawnRidleyFireball(RoomEnemySlot ridley, bool spawnAfterburn)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        RidleyEnemyState state = RequireRidley(ridley);
        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CeresRidleyFireball,
            FireballGraphicsIndex);
        projectile.XPosition = unchecked((ushort)(ridley.XPosition +
            (state.FacingDirection == 0 ? -25 : 25)));
        projectile.YPosition = unchecked((ushort)(ridley.YPosition - 43));
        projectile.XVelocity = state.FireballXVelocity;
        projectile.YVelocity = state.FireballYVelocity;
        projectile.RemainingAfterburns = spawnAfterburn ? (ushort)3 : (ushort)0;
    }

    private RoomEnemyProjectileSlot? AllocateEnemyProjectile()
    {
        // SpawnEnemyProjectile searches from native index $22 toward zero.
        for (int index = _enemyProjectiles.Length - 1; index >= 0; index--)
        {
            RoomEnemyProjectileSlot projectile = _enemyProjectiles[index];
            if (!projectile.IsActive)
            {
                projectile.Clear();
                return projectile;
            }
        }
        return null;
    }

    /// <summary>
    /// Copies the seven-word bank-$86 definition record installed by
    /// <c>SpawnEprojInner</c>. Family initializers may then replace fields exactly as their
    /// cartridge routine does; centralizing the copy prevents each projectile translation
    /// from inventing subtly different radius/property semantics.
    /// </summary>
    private void InitializeEnemyProjectileFromDefinition(
        RoomEnemyProjectileSlot projectile,
        RoomEnemyProjectileKind kind,
        ushort graphicsIndex)
    {
        int definition = 0x860000 | (ushort)kind;
        projectile.Kind = kind;
        projectile.PreInstruction = ReadWord(_bus!, definition + 2);
        projectile.InstructionPointer = ReadWord(_bus!, definition + 4);
        projectile.InstructionTimer = 1;

        // SpawnEprojInner initializes the drawable map to $8000 before the first list tick.
        // It is intentionally not the first list map; bank-$86 advances it on its own pass.
        projectile.SpritemapPointer = 0x8000;
        ushort radii = ReadWord(_bus!, definition + 6);
        projectile.XRadius = unchecked((byte)radii);
        projectile.YRadius = unchecked((byte)(radii >> 8));
        ushort properties = ReadWord(_bus!, definition + 8);
        projectile.Damage = unchecked((ushort)(properties & 0x0fff));
        projectile.InvincibilityFrames = 96;
        projectile.DrawPriority = (properties & 0x1000) != 0
            ? EnemyProjectileDrawPriority.High
            : EnemyProjectileDrawPriority.Low;
        projectile.CanDamageSamus = (properties & 0x2000) == 0;
        projectile.PersistsOnSamusContact = (properties & 0x4000) != 0;
        projectile.BlocksSamusProjectiles = (properties & 0x8000) != 0;
        projectile.CollisionOption = 0;
        projectile.CollidedProjectileType = 0;
        projectile.GraphicsIndex = graphicsIndex;
    }

    private void RunEnemyProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY,
        byte nmiFrameCounter8,
        SamusBombProjectileSystem? samusBombs)
    {
        if (TryStepTourianUnlockEffect(projectile)) return;
        switch (projectile.PreInstruction)
        {
            case 0:
            case EnemyProjectileCodePointers.RTS_868170:
            case EnemyProjectileCodePointers.RTS_86A327:
            case EnemyProjectileCodePointers.RTS_8684FB:
            case EnemyProjectileCodePointers.RTS_86EC94:
            case EnemyProjectileCodePointers.RTS_86D0EB:
            case EnemyProjectileCodePointers.RTS_868D54:
            case EnemyProjectileCodePointers.RTS_86950C:
            case EnemyProjectileCodePointers.RTS_869A44:
            case EnemyProjectileCodePointers.RTS_86BBC6:
            case EnemyProjectileCodePointers.RTS_86A05B:
            case EnemyProjectileCodePointers.RTS_86EFDF:
            case EnemyProjectileCodePointers.RTS_86A919:
            case EnemyProjectileCodePointers.PreInstruction_BombTorizoStatueFragment_Stopped:
            case EnemyProjectileCodePointers.RTS_86DD44:
            case EnemyProjectileCodePointers.RTS_86CAA3:
            case EnemyProjectileCodePointers.RTS_86C76D:
            case EnemyProjectileCodePointers.RTS_86E6D1:
            case DownwardGateEnemyProjectileRomData.InertPreInstruction:
            case EyeDoorEnemyProjectileRomData.SmokeInertPreInstruction:
                return;

            case DownwardGateEnemyProjectileRomData.MovementPreInstruction:
                RunDownwardGateProjectileMovement(projectile);
                return;

            case EyeDoorEnemyProjectileRomData.ProjectilePreInstruction:
                RunEyeDoorProjectilePreInstruction(projectile, level);
                return;

            case EyeDoorEnemyProjectileRomData.SweatPreInstruction:
                RunEyeDoorSweatPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProjectile_BombTorizoChozoBreaking_Falling:
                RunBombTorizoStatueBreakingPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_Pickup:
                // Lifetime, grapple endpoint, then Samus body.
                RunEnemyPickupPreInstruction(projectile, samus);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsTurrets:
                RunMotherBrainTurretPreInstruction(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsTurretBullets:
                RunMotherBrainTurretBulletPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProj_MotherBrainGlassShattering_Shard:
                RunMotherBrainGlassShardPreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsTubeFalling:
                RunMotherBrainTopTubePreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsDrool:
                RunMotherBrainAttachedDroolPreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsDrool_Falling:
                RunMotherBrainFallingDroolPreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsOnionRings:
                RunMotherBrainOnionRingPreInstruction(
                    projectile,
                    samus,
                    cameraX);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsBomb:
                RunMotherBrainBombPreInstruction(projectile, samusBombs);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProjectile_MotherBrainRainbowBeam_Charging:
                RunMotherBrainRainbowChargingPreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProj_MotherBrainsRainbowBeamExplosion:
                RunMotherBrainRainbowExplosionPreInstruction(projectile, samus);
                return;
            case MotherBrainDeathRomData.ExplosionPreInstruction:
                RunMotherBrainDeathExplosion(projectile);
                return;
            case MotherBrainDeathRomData.DoorFragmentPreInstruction:
                RunMotherBrainDoorFragment(projectile);
                return;
            case MotherBrainDeathRomData.SubtitlePreInstruction:
                PinMotherBrainEscapeSubtitle(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_DraygonGoop_StuckToSamus:
                RunAttachedDraygonGoop(projectile, samus);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProj_DraygonsWallTurretProjectile_Fired:
                RunDraygonProjectileFlight(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_DraygonGoop:
                RunFlyingDraygonGoop(projectile, samus);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_Spores:
                RunSporeSpawnSporePreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_SporeSpawner:
                RunSporeSpawnSpawnerPreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_BotwoonsBody:
                RunBotwoonBodyPreInstruction(projectile, nmiFrameCounter8);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_BotwoonsSpit:
                RunBotwoonSpitPreInstruction(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_BombTorizosChozoOrbs:
                RunBombTorizoChozoOrbPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizosChozoOrbs:
                RunGoldenTorizoChozoOrbPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_TorizoSonicBoom:
                RunBombTorizoSonicBoomPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProjectile_BombTorizoLowHealthDrool_Falling:
                RunBombTorizoDroolPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_Bouncing:
                RunGoldenTorizoEggPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_Hatched:
                RunGoldenTorizoEggHorizontalCharge(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEgg_HitWall:
                RunGoldenTorizoEggFall(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoSuperMissile_Held:
                RunGoldenTorizoSuperMissilePreInstruction(projectile);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProjectile_GoldenTorizoSuperMissile_Thrown:
                RunGoldenTorizoSuperMissileFlight(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_GoldenTorizoEyeBeam:
                RunGoldenTorizoEyeBeamPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProj_TourianStatueBaseDecoration_AllowProcess:
            case EnemyProjectileCodePointers.PreInst_EnemyProj_TourianStatue_Ridley_Phantoon_BaseDecor:
                PositionTourianEntranceStatueProjectile(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_ShaktoolsAttack_Front:
                RunShaktoolFrontCirclePreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProjectile_ShaktoolsAttack_MiddleBack_Moving:
                RunShaktoolLinkedCirclePreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MiscDust:
                CullMiscDustOutsideCamera(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_DragonFireball:
                RunDragonFireballPreInstruction(projectile, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_RidleyFireball:
            {
                bool horizontalCollision = MoveProjectileAxis(projectile, level, horizontal: true);
                bool verticalCollision = !horizontalCollision &&
                    MoveProjectileAxis(projectile, level, horizontal: false);
                if (horizontalCollision || verticalCollision)
                {
                    ushort x = projectile.XPosition;
                    ushort y = projectile.YPosition;
                    ushort count = projectile.RemainingAfterburns;
                    projectile.Clear();
                    if (count != 0)
                    {
                        SpawnAfterburnCenter(
                            horizontalCollision
                                ? RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter
                                : RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
                            x,
                            y,
                            count);
                    }
                }
                return;
            }

            case EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFlickering:
            case EnemyProjectileCodePointers.PreInstruction_NoobTubeCrackFalling:
            case EnemyProjectileCodePointers.PreInstruction_NoobTubeShardFlying:
            case EnemyProjectileCodePointers.PreInstruction_NoobTubeShardFalling:
            case EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFalling:
            case EnemyProjectileCodePointers.PreInstruction_NoobTubeBubbleFlying:
                RunNoobTubeProjectilePreInstruction(projectile, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_HorizontalAfterburn:
                // $86:950D first uses the raw 8.8 horizontal adder, not the room-collision
                // helper. Only the perpendicular vertical move may end this afterburn.
                (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
                    projectile.XPosition,
                    projectile.XSubposition,
                    projectile.XVelocity);
                if (MoveProjectileAxis(projectile, level, horizontal: false))
                    BeginAfterburnFinalAnimation(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_VerticalAfterburn:
                // $86:9522 is the transposed path: unrestricted vertical travel followed
                // by a horizontal room-collision test.
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
                    projectile.YPosition,
                    projectile.YSubposition,
                    projectile.YVelocity);
                if (MoveProjectileAxis(projectile, level, horizontal: true))
                    BeginAfterburnFinalAnimation(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MetalSkreeParticle:
                (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
                    projectile.XPosition,
                    projectile.XSubposition,
                    projectile.XVelocity);
                (projectile.YPosition, projectile.YSubposition) = AddEightBitVelocity(
                    projectile.YPosition,
                    projectile.YSubposition,
                    projectile.YVelocity);
                projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 0x0050));
                if (unchecked((ushort)(projectile.XPosition - cameraX)) >= 256 ||
                    unchecked((ushort)(projectile.YPosition - cameraY)) >= 256)
                {
                    projectile.Clear();
                }
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CrocomiresProjectile_Setup:
                StartCrocomireProjectileFlight(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CrocomiresProjectile_Fired:
                RunCrocomireProjectileFlight(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CrocomireSpikeWallPieces:
                RunCrocomireSpikeWallPiece(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KraidRocks:
                RunKraidRockPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KraidCeilingRocks:
                RunKraidCeilingRockPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KraidRockSpit_UsePalette0:
                projectile.GraphicsIndex = 0;
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PhantoonStartingFlames:
                RunPhantoonStartingFlameWaiting(projectile);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProjectile_PhantoonStartingFlames_Activated:
                RunPhantoonStartingFlameOrbit(projectile, nmiFrameCounter8);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Falling:
                RunPhantoonCasualFlameFalling(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround:
                RunPhantoonCasualFlameImpactPause(projectile, nmiFrameCounter8);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing:
                RunPhantoonCasualFlameBouncing(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Enraged:
                RunPhantoonEnragedFlame(projectile);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Rain:
                RunPhantoonRainFlame(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInst_EnemyProj_PhantoonDestroyableFlame_Spiral:
                RunPhantoonSpiralFlame(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CrocomireBridgeCrumbling:
                RunCrocomireBridgeFragment(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_AlcoonFireball:
                RunAlcoonFireballPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Moving:
                RunKiHunterAcidMovement(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Left:
                StartKiHunterAcidMovement(projectile, movingRight: false);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Right:
                StartKiHunterAcidMovement(projectile, movingRight: true);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PowampSpike:
                RunPowampSpikePreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_WreckedShipRobotLaser:
                RunWorkRobotLaserPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_StokeFireball:
                // Stoke shot: horizontal motion and viewport cull.
                RunStokeProjectilePreInstruction(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CacatacSpike:
                // Cacatac spike: ten direction-table movers.
                RunCacatacSpikePreInstruction(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PolypRock:
                // Polyp rock: quadratic rise/fall and viewport cull.
                RunPolypRockPreInstruction(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_NamiFuneFireball:
                // Fune/Namihe: directional 8.8 flight and cull.
                RunFuneNamiheFireballPreInstruction(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MagdolliteLava:
                RunMagdolliteLavaPreInstruction(projectile, cameraX, cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Idle:
                RunKagoBugIdle(projectile);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Jumping:
                RunKagoBugJumping(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KagoBug_Falling:
                RunKagoBugFalling(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_FallingSpark:
                RunFallingSparkPreInstruction(projectile, level, nmiFrameCounter8);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_CeresFallingTile:
                projectile.YVelocity = unchecked((ushort)(projectile.YVelocity + 0x0010));
                if (MoveProjectileAxis(projectile, level, horizontal: false))
                {
                    ushort impactX = projectile.XPosition;
                    ushort impactY = projectile.YPosition;
                    projectile.Clear();
                    SpawnRoomGraphicsDustExplosion(impactX, impactY, animationIndex: 9);
                    QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x006d), maximumQueued: 6);
                }
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MiniKraidSpit:
                RunFakeKraidSpitPreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MiniKraidSpikes:
                RunFakeKraidSpikePreInstruction(projectile, level);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_Pirate_MotherBrain_Laser_Left:
            case EnemyProjectileCodePointers.PreInst_EnemyProjectile_Pirate_MotherBrain_Laser_Right:
                RunPirateMotherBrainLaserPreInstruction(
                    projectile,
                    cameraX,
                    cameraY);
                return;

            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Left:
            case EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_PirateClaw_Right:
                RunNinjaPirateClawPreInstruction(projectile, cameraX, cameraY);
                return;

            default:
                throw new InvalidDataException(
                    $"Enemy projectile pre-instruction $86:{projectile.PreInstruction:X4} is not translated.");
        }
    }

    private bool MoveProjectileAxis(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level,
        bool horizontal)
    {
        ushort position = horizontal ? projectile.XPosition : projectile.YPosition;
        ushort subposition = horizontal ? projectile.XSubposition : projectile.YSubposition;
        short velocity = unchecked((short)(horizontal ? projectile.XVelocity : projectile.YVelocity));
        int fixedPosition = (position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (velocity << 8));
        ushort nextPosition = unchecked((ushort)(fixedPosition >> 16));
        ushort nextSubposition = unchecked((ushort)fixedPosition);

        ushort movementRadius = horizontal ? projectile.XRadius : projectile.YRadius;
        ushort perpendicularPosition = horizontal
            ? projectile.YPosition
            : projectile.XPosition;
        ushort perpendicularRadius = horizontal
            ? projectile.YRadius
            : projectile.XRadius;

        // Bank $86 checks every room block crossed by the projectile's perpendicular
        // diameter. The positive edge is inclusive, hence radius - 1; using +radius made
        // right/down projectiles collide one pixel earlier than their native counterparts.
        ushort movementEdge = unchecked((ushort)(nextPosition +
            (velocity < 0 ? -movementRadius : movementRadius - 1)));
        int firstPerpendicularBlock = (perpendicularPosition - perpendicularRadius) >> 4;
        int lastPerpendicularBlock = (perpendicularPosition + perpendicularRadius - 1) >> 4;
        for (int block = firstPerpendicularBlock; block <= lastPerpendicularBlock; block++)
        {
            int blockX = horizontal ? movementEdge >> 4 : block;
            int blockY = horizontal ? block : movementEdge >> 4;
            if (ProjectileAxisProbeHitsRoom(
                    level,
                    projectile,
                    blockX,
                    blockY,
                    horizontal,
                    movingNegative: velocity < 0,
                    targetEdge: movementEdge,
                    slopeAlignedPosition: out ushort? slopeAlignedPosition))
            {
                // `$86:894F-$897A` / `$86:8A0D-$8A38` do not merely reject the
                // attempted movement. They clear the subposition and place the
                // projectile flush against the 16-pixel block boundary it reached.
                // Without this correction a fast downward projectile retains its last
                // pre-collision coordinate and visibly hovers above the floor.
                ushort snappedPosition = slopeAlignedPosition ?? (velocity < 0
                    ? unchecked((ushort)((movementEdge | 0x000f) + movementRadius + 1))
                    : unchecked((ushort)((movementEdge & 0xfff0) - movementRadius)));
                bool snapDoesNotMoveBackwards = velocity < 0
                    ? snappedPosition <= position
                    : snappedPosition >= position;

                if (horizontal)
                {
                    projectile.XSubposition = 0;
                    if (snapDoesNotMoveBackwards)
                        projectile.XPosition = snappedPosition;
                }
                else
                {
                    projectile.YSubposition = 0;
                    if (snapDoesNotMoveBackwards)
                        projectile.YPosition = snappedPosition;
                }
                return true;
            }
        }

        if (horizontal)
        {
            projectile.XPosition = nextPosition;
            projectile.XSubposition = nextSubposition;
        }
        else
        {
            projectile.YPosition = nextPosition;
            projectile.YSubposition = nextSubposition;
        }
        return false;
    }

    private bool ProjectileAxisProbeHitsRoom(
        RoomLevelData level,
        RoomEnemyProjectileSlot projectile,
        int blockX,
        int blockY,
        bool horizontal,
        bool movingNegative,
        ushort targetEdge,
        out ushort? slopeAlignedPosition)
    {
        slopeAlignedPosition = null;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return true;
        }

        // Resolve type-$5/$D BTS links before dispatching the final block family, just as
        // the native projectile collision loop does. Type zero is air; type nine is a door
        // and remains a wall to an enemy projectile.
        int blockIndex = ResolveEnemyCollisionBlockIndex(level, blockX, blockY);
        if (blockIndex < 0)
            return true;

        RoomCollisionBlock collisionBlock = level.GetCollisionBlockByIndex(blockIndex);
        RoomCollisionType type = collisionBlock.CollisionType;
        if (type == RoomCollisionType.Slope && collisionBlock.Bts.IsNonSquareSlope)
        {
            // Horizontal bank-$86 motion ignores non-square slopes; the following
            // vertical pass owns their height profile. Treating one as a full wall can
            // consume every Kago falling frame before Y collision is ever attempted.
            if (horizontal)
                return false;

            // Bank $86's vertical projectile collision uses the same 16-sample slope
            // profiles as bank $94. Only the column containing the projectile center owns
            // non-square geometry; neighboring columns touched by its radius do not turn
            // the slope into a full-height wall.
            if (blockX != projectile.XPosition >> 4)
                return false;

            bool movingUp = movingNegative;
            if (movingUp != collisionBlock.Bts.SlopeFlipsVertically)
                return false;

            int height = ReadNonSquareSlopeHeight(
                collisionBlock.Bts,
                (collisionBlock.Bts.SlopeFlipsHorizontally
                    ? projectile.XPosition ^ 0x000f
                    : projectile.XPosition) & 0x000f);
            int edgeWithinBlock = movingUp
                ? (targetEdge & 0x000f) ^ 0x000f
                : targetEdge & 0x000f;
            int adjustment = height - edgeWithinBlock - 1;
            if (movingUp)
            {
                if (adjustment > 0)
                    return false;
            }
            else if (height - edgeWithinBlock != 1 && adjustment >= 0)
            {
                return false;
            }

            slopeAlignedPosition = movingUp
                ? unchecked((ushort)((blockY << 4) + 16 - height + projectile.YRadius))
                : unchecked((ushort)((blockY << 4) + height - projectile.YRadius));
            return true;
        }

        return type is
            RoomCollisionType.Slope or
            RoomCollisionType.HorizontalExtension or
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or
            RoomCollisionType.VerticalExtension or
            RoomCollisionType.GrappleBlock or
            RoomCollisionType.BombableBlock;
    }

    /// <summary>
    /// Point-form room collision used by projectile routines that do not carry an axis,
    /// radius, or movement direction and therefore cannot resolve non-square geometry.
    /// </summary>
    private static bool ProjectileProbeHitsRoom(RoomLevelData level, ushort x, ushort y)
    {
        int blockX = x >> 4;
        int blockY = y >> 4;
        if ((uint)blockX >= (uint)level.WidthInBlocks ||
            (uint)blockY >= (uint)level.HeightInBlocks)
        {
            return true;
        }

        int blockIndex = ResolveEnemyCollisionBlockIndex(level, blockX, blockY);
        if (blockIndex < 0)
            return true;

        RoomCollisionType type = level.GetCollisionBlockByIndex(blockIndex).CollisionType;
        return type is
            RoomCollisionType.Slope or
            RoomCollisionType.HorizontalExtension or
            RoomCollisionType.SolidBlock or
            RoomCollisionType.DoorBlock or
            RoomCollisionType.SpecialBlock or
            RoomCollisionType.ShootableBlock or
            RoomCollisionType.VerticalExtension or
            RoomCollisionType.GrappleBlock or
            RoomCollisionType.BombableBlock;
    }

    private ushort? ResolveEnemyProjectileSamusCollision(
        RoomEnemyProjectileSlot projectile,
        SamusState samus)
    {
        // `$A0:9894` already made the pass-level invincibility/contact-damage decision.
        // Do not re-read the timer here: the first hit writes it, but the original loop
        // deliberately continues testing the other projectile slots in this same pass.
        // $A0:98C7-98DB skips either zero radius before overlap testing. A zero
        // radius is not a point-shaped hazard: decorative wall shards use it to
        // remain harmless even while passing straight through Samus's body.
        if (!projectile.CanDamageSamus || projectile.XRadius == 0 || projectile.YRadius == 0)
            return null;

        int xDistance = Math.Abs(unchecked((short)(projectile.XPosition - samus.XPosition)));
        int yDistance = Math.Abs(unchecked((short)(projectile.YPosition - samus.YPosition)));
        if (xDistance >= projectile.XRadius + samus.Kinematics.XRadius ||
            yDistance >= projectile.YRadius + samus.Kinematics.YRadius)
        {
            return null;
        }

        samus.Health = samus.Health <= projectile.Damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - projectile.Damage));
        samus.InvincibilityTimer = projectile.InvincibilityFrames;
        ushort knockbackXDirection = unchecked((short)(
            samus.XPosition - projectile.XPosition)) >= 0
            ? (ushort)1
            : (ushort)0;

        // `$A0:9930-$993F` installs the definition's touch list before consulting property
        // $4000. Mother Brain's turret bullet depends on that ordering: it survives contact
        // but immediately changes to its smoke list. Keeping this in the common path also
        // prevents later persistent projectile families from needing bespoke hit effects.
        ushort touchInstruction = ReadWord(
            _bus!,
            0x860000 | unchecked((ushort)((ushort)projectile.Kind + 10)));
        if (touchInstruction != 0)
        {
            projectile.InstructionPointer = touchInstruction;
            projectile.InstructionTimer = 1;
        }

        // Generic enemy-projectile contact deletes Ridley's fireball. Afterburn is a wall-
        // impact feature from $86:940E, so a Samus contact does not create the wall bloom.
        if (!projectile.PersistsOnSamusContact)
            projectile.Clear();

        return knockbackXDirection;
    }

    private void ProcessEnemyProjectileInstructions(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
    {
        ushort oldTimer = projectile.InstructionTimer;
        projectile.InstructionTimer = unchecked((ushort)(projectile.InstructionTimer - 1));
        if (oldTimer != 1)
            return;

        ushort cursor = projectile.InstructionPointer;
        for (int operationCount = 0; operationCount < 24; operationCount++)
        {
            ushort word = ReadWord(_bus!, 0x860000 | cursor);
            if ((word & 0x8000) == 0)
            {
                if (word == 0)
                    throw new InvalidDataException($"Enemy projectile frame $86:{cursor:X4} has zero duration.");
                projectile.InstructionTimer = word;
                projectile.SpritemapPointer = ReadWord(
                    _bus!,
                    0x860000 | unchecked((ushort)(cursor + 2)));
                projectile.InstructionPointer = unchecked((ushort)(cursor + 4));
                return;
            }

            if (TryExecuteTourianUnlockInstruction(projectile, word, ref cursor)) continue;
            switch (word)
            {
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete:
                    projectile.Clear();
                    return;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_OrY:
                {
                    ushort mask = ReadWord(_bus!, 0x860000 |
                        unchecked((ushort)(cursor + 2)));
                    projectile.Damage = unchecked((ushort)(projectile.Damage | (mask & 0x0fff)));
                    if ((mask & 0x1000) != 0)
                        projectile.DrawPriority = EnemyProjectileDrawPriority.High;
                    if ((mask & 0x2000) != 0)
                        projectile.CanDamageSamus = false;
                    if ((mask & 0x4000) != 0)
                        projectile.PersistsOnSamusContact = true;
                    if ((mask & 0x8000) != 0)
                        projectile.BlocksSamusProjectiles = true;
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                }
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Properties_AndY:
                {
                    ushort mask = ReadWord(_bus!, 0x860000 |
                        unchecked((ushort)(cursor + 2)));
                    projectile.Damage = unchecked((ushort)(projectile.Damage & (mask & 0x0fff)));
                    if ((mask & 0x1000) == 0)
                        projectile.DrawPriority = EnemyProjectileDrawPriority.Low;
                    if ((mask & 0x2000) == 0)
                        projectile.CanDamageSamus = true;
                    if ((mask & 0x4000) == 0)
                        projectile.PersistsOnSamusContact = false;
                    if ((mask & 0x8000) == 0)
                        projectile.BlocksSamusProjectiles = false;
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                }
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProj_EnableCollisionWithSamusProj_868248:
                    projectile.BlocksSamusProjectiles = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_DisableCollisionWIthSamusProj:
                    projectile.BlocksSamusProjectiles = false;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_DisableCollisionWithSamus:
                    projectile.CanDamageSamus = false;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_EnableCollisionWithSamus_868266:
                    projectile.CanDamageSamus = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_SetToNotDieOnContact_868270:
                    projectile.PersistsOnSamusContact = true;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.UNUSED_Instruction_EnemyProjectile_SetToDieOnContact_86827A:
                    projectile.PersistsOnSamusContact = false;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_SetHighPriority:
                    projectile.DrawPriority = EnemyProjectileDrawPriority.High;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.UNUSED_Instruction_EnemyProjectile_SetLowPriority_86828E:
                    projectile.DrawPriority = EnemyProjectileDrawPriority.Low;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_XYRadiusInY:
                {
                    ushort radii = ReadWord(_bus!, 0x860000 |
                        unchecked((ushort)(cursor + 2)));
                    projectile.XRadius = unchecked((byte)radii);
                    projectile.YRadius = unchecked((byte)(radii >> 8));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                }
                case EnemyProjectileCodePointers.UNUSED_Instruction_EnemyProjectile_XYRadius_0:
                    projectile.XRadius = 0;
                    projectile.YRadius = 0;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.UNUSED_Instruction_EnemyProjectile_QueueMusicTrackInY:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max6_868309:
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max6:
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max6:
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib1_Max15:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max15_86832D:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max15_868336:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max3_86833F:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max3_868348:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib3_Max3_868351:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86835A:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib2_Max9_868363:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max9_86836C:
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProjectile_QueueSoundInY_Lib1_Max1_868375:
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib2_Max1:
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_QueueSoundInY_Lib3_Max1:
                    // Audio is an outer-runtime seam, but these commands are byte-packed.
                    // Advancing by three (two-byte opcode plus one-byte ID) is essential:
                    // rounding to a word would desynchronize every following frame.
                    cursor = unchecked((ushort)(cursor + 3));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep:
                    // The native command rewinds Y to its own opcode, stores that pointer,
                    // pops the instruction-handler return address, and leaves timer zero.
                    // Subsequent frames wrap zero to FFFF and therefore never parse again.
                    projectile.InstructionPointer = cursor;
                    projectile.InstructionTimer = 0;
                    return;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY:
                    projectile.PreInstruction = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_CalculateDirectionTowardsSamus:
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Enemy-projectile direction bytecode requires Samus.");
                    }
                    projectile.Variable0 = unchecked((ushort)(2 * CalculateCartridgeAngle(
                        unchecked((short)(samus.XPosition - projectile.XPosition)),
                        unchecked((short)(samus.YPosition - projectile.YPosition)))));
                    int eyeDoorAngle = projectile.Variable0 >> 1;
                    projectile.XVelocity = unchecked((ushort)ReadWord(
                        _bus!,
                        EnemyRomTablePointers.Common.SignedSineCosineWords +
                        ((eyeDoorAngle & 0xff) * 2)));
                    projectile.YVelocity = unchecked((ushort)ReadWord(
                        _bus!,
                        EnemyRomTablePointers.Common.SignedSineCosineWords +
                        (((eyeDoorAngle - 64) & 0xff) * 2)));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction:
                    projectile.PreInstruction = EnemyProjectileCodePointers.RTS_868170;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case DownwardGateEnemyProjectileRomData.SetYVelocityInstruction:
                    projectile.YVelocity = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_CallExternalFunctionInY:
                {
                    int externalFunction =
                        _bus!.ReadByte((int)new SnesAddress(
                            0x86, unchecked((ushort)(cursor + 2)))) |
                        (_bus.ReadByte((int)new SnesAddress(
                            0x86, unchecked((ushort)(cursor + 3)))) << 8) |
                        (_bus.ReadByte((int)new SnesAddress(
                            0x86, unchecked((ushort)(cursor + 4)))) << 16);
                    if (externalFunction != 0x86c7fb || projectile.Kind is not (
                            RoomEnemyProjectileKind.MotherBrainHandBeamCharging or
                            RoomEnemyProjectileKind.MotherBrainHandBeamFired))
                    {
                        throw new InvalidDataException(
                            $"Enemy projectile external function ${externalFunction:X6} " +
                            $"from $86:{cursor:X4} is not translated.");
                    }

                    SpawnMotherBrainHandBeamFired(projectile.Variable0);
                    cursor = unchecked((ushort)(cursor + 5));
                    break;
                }
                case EnemyProjectileCodePointers.Instruction_SetPreInst_DraygonsWallTurretProjectile_Fired when projectile.Kind == RoomEnemyProjectileKind.DraygonWallTurret:
                    projectile.PreInstruction =
                        EnemyProjectileCodePointers.PreInstruction_EnemyProj_DraygonsWallTurretProjectile_Fired;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_DraygonGoop_SamusCollision when projectile.Kind == RoomEnemyProjectileKind.DraygonGoop:
                    AttachDraygonGoopToSamus(projectile, samus);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY:
                    cursor = ReadWord(_bus!, 0x860000 | unchecked((ushort)(cursor + 2)));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY_Y:
                    cursor = unchecked((ushort)(cursor + 2 + unchecked((sbyte)_bus!.ReadByte(
                        0x860000 | unchecked((ushort)(cursor + 2))))));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero:
                {
                    ushort before = projectile.GeneralTimer;
                    projectile.GeneralTimer = unchecked((ushort)(before - 1));
                    cursor = before == 1
                        ? unchecked((ushort)(cursor + 4))
                        : ReadWord(_bus!, 0x860000 | unchecked((ushort)(cursor + 2)));
                    break;
                }
                case EnemyProjectileCodePointers.UNUSED_Inst_EnemyProj_DecrementTimer_GotoY_YIfNonZero_8681CE:
                {
                    ushort before = projectile.GeneralTimer;
                    projectile.GeneralTimer = unchecked((ushort)(before - 1));
                    cursor = before == 1
                        ? unchecked((ushort)(cursor + 3))
                        : unchecked((ushort)(cursor + 2 + unchecked((sbyte)_bus!.ReadByte(
                            0x860000 | unchecked((ushort)(cursor + 2))))));
                    break;
                }
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY:
                    projectile.GeneralTimer = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case EnemyProjectileCodePointers.Instruction_NoobTubeShardAssignFallingAngle:
                    projectile.XVelocity = unchecked((byte)(_nextRandom!() >> 8));
                    projectile.YVelocity = NoobTubeProjectileRomData.ShardFallYVelocity;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_NoobTubeBubbleAssignFallingAngle:
                    projectile.XVelocity = unchecked((byte)(_nextRandom!() >> 8));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_NoobTubeShardReflectFlicker:
                    projectile.XPosition = (_currentEnemyProjectileFrame8 & 1) != 0
                        ? projectile.Variable1
                        : unchecked((ushort)(0x0100 - projectile.Variable1));
                    projectile.SpritemapPointer = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor +
                            ((_currentEnemyProjectileFrame8 & 1) != 0 ? 2 : 4))));
                    projectile.InstructionPointer = unchecked((ushort)(cursor + 6));
                    projectile.InstructionTimer = 1;
                    return;
                case EnemyProjectileCodePointers.Instruction_NoobTubeShardFlicker:
                    projectile.XPosition = (_currentEnemyProjectileFrame8 & 1) != 0
                        ? projectile.Variable1
                        : NoobTubeProjectileRomData.HiddenXPosition;
                    projectile.SpritemapPointer = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor + 2)));
                    projectile.InstructionPointer = unchecked((ushort)(cursor + 4));
                    projectile.InstructionTimer = 1;
                    return;
                case EnemyProjectileCodePointers.RTS_8681DE:
                    // Bomb Torizo's impact list uses this address as a compact no-op before
                    // its counted branch. It is a real callable ROM entry, not a typo for
                    // MoveRandomlyWithinRadius at the following byte.
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_MoveRandomlyWithinXRadius_YRadius:
                    MoveEnemyProjectileRandomlyWithinRadius(projectile, cursor);
                    cursor = unchecked((ushort)(cursor + 6));
                    break;
                case EnemyProjectileCodePointers.Instruction_PreInstructionInY_ExecuteY:
                    projectile.PreInstruction = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor + 2)));

                    // A050 returns the argument cursor unchanged. The common interpreter
                    // consequently sees A05C/A07A as the next instruction, dispatches that
                    // movement routine once immediately, then resumes after the operand.
                    // Merely installing the pointer would leave every laser four/two pixels
                    // behind the cartridge for its entire lifetime.
                    RunPirateMotherBrainLaserPreInstruction(
                        projectile,
                        cameraX,
                        cameraY);
                    if (!projectile.IsActive)
                        return;
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Torizo_ResetPosition:
                    projectile.XPosition = projectile.Variable0;
                    projectile.YPosition = projectile.Variable1;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.UNUSED_Instruction_EnemyProj_MoveHorizontally_GotoY_86AD92:
                    (projectile.XPosition, projectile.XSubposition) = AddEightBitVelocity(
                        projectile.XPosition,
                        projectile.XSubposition,
                        projectile.XVelocity);
                    cursor = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor +
                            (unchecked((short)projectile.XVelocity) < 0 ? 2 : 4))));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched:
                    cursor = (projectile.Variable0 & 0x8000) != 0
                        ? EnemyProjectileInstructionLists.GoldenTorizoEggHatchTargetRight
                        : EnemyProjectileInstructionLists.GoldenTorizoEggHatchTargetLeft;
                    break;
                case EnemyProjectileCodePointers.Instruction_AimSuperMissile_Rightwards:
                case EnemyProjectileCodePointers.Instruction_AimSuperMissile_Leftwards:
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Golden Torizo super-missile aiming requires the active Samus actor.");
                    }
                    SetGoldenTorizoSuperMissileVelocity(
                        projectile,
                        samus,
                        awayFromSamus:
                            word == EnemyProjectileCodePointers.Instruction_AimSuperMissile_Leftwards);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoYIfEyeBeamExplosionsDisabled:
                {
                    TorizoEnemyState state = GoldenTorizo ??
                        throw new InvalidOperationException(
                            "Golden Torizo eye-beam bytecode has no owning Torizo state.");
                    cursor = (state.AttackFlags & 0x8000) == 0
                        ? ReadWord(_bus!, 0x860000 | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                }
                case EnemyProjectileCodePointers.UNUSED_Instruction_ResetPosition_86B436:
                    projectile.XPosition = projectile.Variable0;
                    projectile.YPosition = projectile.Variable1;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_MotherBrainsTurretBullets_GotoY when projectile.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet:
                    // The bullet initializer stores direction * 2 in variable E. The ROM
                    // opcode adds that byte offset to the eight-pointer table immediately
                    // following the opcode, then jumps to the selected one-frame map.
                    cursor = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor + 2 + projectile.Variable0)));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_UsePalette0 when projectile.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurretBullet:
                    // First instruction of the shared touch/shot smoke sequence.
                    projectile.GraphicsIndex = 0;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProj_MotherBrainsDrool_MoveDownCPixels
                    when projectile.Kind is
                    RoomEnemyProjectileKind.MotherBrainDrool or
                    RoomEnemyProjectileKind.MotherBrainDyingDrool:
                    // After changing to the falling pre-instruction, the list lowers the
                    // released sprite by twelve whole pixels before its first falling map.
                    projectile.YPosition = unchecked((ushort)(projectile.YPosition + 12));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY_Probability_1_4:
                    cursor = (_nextRandom!() & 0xc000) == 0xc000
                        ? ReadWord(_bus!, 0x860000 | unchecked((ushort)(cursor + 2)))
                        : unchecked((ushort)(cursor + 4));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_SpawnEnemyDropsWIthYDropChances:
                    RequestTorizoChozoOrbDrop(projectile, cursor);
                    cursor = unchecked((ushort)(cursor + 6));
                    break;
                case EnemyProjectileCodePointers.Instruction_Spawn_HorizontalAfterburn_EnemyProjectiles:
                    SpawnAfterburnPair(projectile, horizontal: true);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_Spawn_VerticalAfterburn_EnemyProjectiles:
                    SpawnAfterburnPair(projectile, horizontal: false);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_SpawnNext_Afterburn_EnemyProjectile:
                    SpawnNextAfterburn(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_SpawnPhantoonDrop:
                    RequestPhantoonFlameDrop(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_SpawnEnemyDropsWithDraygonEyeChances:
                    // The native callback allocates the pickup before its caller's goto/
                    // delete tail releases this projectile, preserving shared slot order.
                    SpawnEnemyDropFromEnemyHeader(projectile.XPosition, projectile.YPosition, DraygonEyeDefinition);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case KagoBugStartJumpInstruction:
                    StartKagoBugJump(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case KagoBugStartIdleInstruction:
                    StartKagoBugIdle(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case KagoBugUsePaletteZeroInstruction:
                    projectile.GraphicsIndex = 0;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case KagoBugSpawnDropInstruction:
                    RequestKagoBugDrop(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case MagdolliteLavaDropInstruction:
                    RequestMagdolliteLavaDrop(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20:
                    SpawnRandomEnemyDeathSprite(projectile, cursor, mask: 0x003f, center: 32);
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10:
                    SpawnRandomEnemyDeathSprite(projectile, cursor, mask: 0x001f, center: 16);
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX:
                    // The native dispatcher passes a pointer to the first byte after the
                    // opcode into EprojInstr_QueueSfx2_9, and that routine returns the same
                    // pointer unchanged. Therefore the timed duration begins immediately
                    // after $EE8B. Treating that duration as an operand skips two bytes and
                    // interprets the following spritemap pointer as another opcode.
                    LastEnemyDeathSoundEffectLibrary2 = 9;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX:
                    LastEnemyDeathSoundEffectLibrary2 = 0x0024;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProj_EDeathExplo_QueueContactKilledSoundFX:
                    LastEnemyDeathSoundEffectLibrary2 = 0x000b;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Spores_SetProperties3000:
                    SetSporeSpawnImpactProperties(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Spores_SpawnEnemyDrops:
                    RequestSporeSpawnSporeDrop(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_SporeSpawner_SpawnSpore:
                    SpawnSporeSpawnSpore(projectile.XPosition, projectile.YPosition);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_TorizoLandingDustClouds:
                    projectile.YPosition = unchecked((ushort)(projectile.YPosition - 4));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup:
                    if (projectile.Kind != RoomEnemyProjectileKind.EnemyDeathExplosion)
                    {
                        throw new InvalidDataException(
                            $"Enemy projectile $86:{(ushort)projectile.Kind:X4} reached death-drop opcode $EEAF.");
                    }
                    ConvertEnemyDeathExplosionToPickup(projectile);
                    cursor = projectile.InstructionPointer;
                    break;
                case EnemyProjectileCodePointers.Instruction_EnemyProjectile_Pickup_HandleRespawningEnemy:
                    if (unchecked((short)projectile.KilledEnemyNativeIndex) <= -2)
                    {
                        RespawnEnemyFromSnapshot(unchecked((ushort)(
                            projectile.KilledEnemyNativeIndex & 0x7fff)));
                    }
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                default:
                    throw new InvalidDataException(
                        $"Enemy projectile instruction $86:{word:X4} at $86:{cursor:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            "Enemy projectile list did not reach a timed frame within 24 operations.");
    }

    /// <summary>
    /// Ports $86:ECE3/$ED17. Both instructions consume one object-number operand and one
    /// RNG word; its low and high bytes offset X and Y around the death actor respectively.
    /// CreateSpriteAtPos owns a separate finite bank-$B4 pool, so these decorations never
    /// consume one of the eighteen pickup/death projectile slots.
    /// </summary>
    private void SpawnRandomEnemyDeathSprite(
        RoomEnemyProjectileSlot projectile,
        ushort instructionPointer,
        ushort mask,
        int center)
    {
        ushort random = _nextRandom!();
        ushort x = unchecked((ushort)(
            projectile.XPosition + (random & mask) - center));
        ushort y = unchecked((ushort)(
            projectile.YPosition + ((random & (mask << 8)) >> 8) - center));
        RoomSpriteObjectKind kind = (RoomSpriteObjectKind)ReadWord(
            _bus!,
            0x860000 | unchecked((ushort)(instructionPointer + 2)));
        _ = SpawnRoomSpriteObject(x, y, kind, graphicsIndex: 0);
    }

    private void MoveEnemyProjectileRandomlyWithinRadius(
        RoomEnemyProjectileSlot projectile,
        ushort instructionPointer)
    {
        // $86:81DF consumes four packed bytes: X mask/center followed by Y mask/center.
        // Each axis rejects negative candidate offsets, while two independent bits from the
        // first random sample choose the eventual signs. This peculiar rejection loop is
        // observable in Bomb Torizo's sonic-boom wall impact, so a host RNG approximation
        // would produce a visibly different debris cloud and desynchronize later randomness.
        int operands = 0x860000 | unchecked((ushort)(instructionPointer + 2));
        byte xMask = _bus!.ReadByte(operands);
        byte xCenter = _bus.ReadByte(operands + 1);
        byte yMask = _bus.ReadByte(operands + 2);
        byte yCenter = _bus.ReadByte(operands + 3);
        ushort signSample = _nextRandom!();

        int xOffset;
        do
        {
            xOffset = (xMask & unchecked((byte)_nextRandom())) - xCenter;
        }
        while (xOffset < 0);
        if ((signSample & 0x8000) != 0)
            xOffset = -xOffset;
        projectile.XPosition = unchecked((ushort)(projectile.XPosition + xOffset));

        int yOffset;
        do
        {
            yOffset = (yMask & unchecked((byte)_nextRandom())) - yCenter;
        }
        while (yOffset < 0);
        if ((signSample & 0x4000) != 0)
            yOffset = -yOffset;
        projectile.YPosition = unchecked((ushort)(projectile.YPosition + yOffset));
    }

    private void SpawnAfterburnCenter(
        RoomEnemyProjectileKind kind,
        ushort x,
        ushort y,
        ushort remaining)
    {
        RoomEnemyProjectileSlot? center = AllocateEnemyProjectile();
        if (center is null)
            return;
        InitializeEnemyProjectileFromDefinition(center, kind, FireballGraphicsIndex);
        center.XPosition = x;
        center.YPosition = y;
        center.RemainingAfterburns = remaining;
    }

    private void SpawnAfterburnPair(RoomEnemyProjectileSlot center, bool horizontal)
    {
        if (horizontal)
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight, 0x0e00, 0);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft, 0xf200, 0);
        }
        else
        {
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp, 0, 0xf200);
            SpawnDirectionalAfterburn(center, RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown, 0, 0x0e00);
        }
    }

    private void SpawnDirectionalAfterburn(
        RoomEnemyProjectileSlot source,
        RoomEnemyProjectileKind kind,
        ushort xVelocity,
        ushort yVelocity)
    {
        RoomEnemyProjectileSlot? afterburn = AllocateEnemyProjectile();
        if (afterburn is null)
            return;
        InitializeEnemyProjectileFromDefinition(afterburn, kind, FireballGraphicsIndex);
        afterburn.XPosition = source.XPosition;
        afterburn.YPosition = source.YPosition;
        afterburn.XVelocity = xVelocity;
        afterburn.YVelocity = yVelocity;
        afterburn.RemainingAfterburns = source.RemainingAfterburns;
        afterburn.NextAfterburnKind = (ushort)kind;
    }

    private void SpawnNextAfterburn(RoomEnemyProjectileSlot source)
    {
        byte lowCount = unchecked((byte)(source.RemainingAfterburns - 1));
        source.RemainingAfterburns = unchecked((ushort)((source.RemainingAfterburns & 0xff00) | lowCount));
        if ((lowCount & 0x80) != 0)
            return;
        SpawnDirectionalAfterburn(
            source,
            (RoomEnemyProjectileKind)source.NextAfterburnKind,
            source.XVelocity,
            source.YVelocity);
    }

    private static void BeginAfterburnFinalAnimation(RoomEnemyProjectileSlot projectile)
    {
        projectile.InstructionPointer = EnemyProjectileInstructionLists.RidleyCenterAfterburn;
        projectile.InstructionTimer = 1;
        projectile.XVelocity = 0;
        projectile.YVelocity = 0;
        projectile.CanDamageSamus = false;
    }

    /// <summary>Spawns the four bank-$86 particles emitted by a dying/burrowing Skree.</summary>
    private void SpawnSkreeParticleBurst(RoomEnemySlot skree)
    {
        SpawnSkreeOrMetareeParticle(
            skree,
            RoomEnemyProjectileKind.SkreeParticleDownRight,
            6,
            0x0140,
            0xfcff,
            instructionPointer: EnemyProjectileInstructionLists.SkreeParticle);
        SpawnSkreeOrMetareeParticle(
            skree,
            RoomEnemyProjectileKind.SkreeParticleUpRight,
            6,
            0x0060,
            0xfbff,
            instructionPointer: EnemyProjectileInstructionLists.SkreeParticle);
        SpawnSkreeOrMetareeParticle(
            skree,
            RoomEnemyProjectileKind.SkreeParticleDownLeft,
            -6,
            0xfec0,
            0xfcff,
            instructionPointer: EnemyProjectileInstructionLists.SkreeParticle);
        SpawnSkreeOrMetareeParticle(
            skree,
            RoomEnemyProjectileKind.SkreeParticleUpLeft,
            -6,
            0xffa0,
            0xfbff,
            instructionPointer: EnemyProjectileInstructionLists.SkreeParticle);
    }

    /// <summary>
    /// Spawns Metaree's four metal-particle definitions. Their motion initializers are
    /// shared byte-for-byte with Skree, but pointer <c>$86:8AC5</c> selects Metaree's ROM
    /// spritemap instead of silently reusing the visually different Skree debris.
    /// </summary>
    private void SpawnMetareeParticleBurst(RoomEnemySlot metaree)
    {
        SpawnSkreeOrMetareeParticle(
            metaree,
            RoomEnemyProjectileKind.MetareeParticleDownRight,
            6,
            0x0140,
            0xfcff,
            instructionPointer: EnemyProjectileInstructionLists.MetareeParticle);
        SpawnSkreeOrMetareeParticle(
            metaree,
            RoomEnemyProjectileKind.MetareeParticleUpRight,
            6,
            0x0060,
            0xfbff,
            instructionPointer: EnemyProjectileInstructionLists.MetareeParticle);
        SpawnSkreeOrMetareeParticle(
            metaree,
            RoomEnemyProjectileKind.MetareeParticleDownLeft,
            -6,
            0xfec0,
            0xfcff,
            instructionPointer: EnemyProjectileInstructionLists.MetareeParticle);
        SpawnSkreeOrMetareeParticle(
            metaree,
            RoomEnemyProjectileKind.MetareeParticleUpLeft,
            -6,
            0xffa0,
            0xfbff,
            instructionPointer: EnemyProjectileInstructionLists.MetareeParticle);
    }

    private void SpawnSkreeOrMetareeParticle(
        RoomEnemySlot source,
        RoomEnemyProjectileKind kind,
        int xOffset,
        ushort xVelocity,
        ushort yVelocity,
        ushort instructionPointer)
    {
        RoomEnemyProjectileSlot? particle = AllocateEnemyProjectile();
        if (particle is null)
            return;

        particle.Kind = kind;
        particle.XPosition = unchecked((ushort)(source.XPosition + xOffset));
        particle.YPosition = source.YPosition;
        particle.XVelocity = xVelocity;
        particle.YVelocity = yVelocity;
        particle.InstructionPointer = instructionPointer;
        particle.InstructionTimer = 1;
        particle.PreInstruction =
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MetalSkreeParticle;
        particle.GraphicsIndex = unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex));
        particle.XRadius = 2;
        particle.YRadius = 2;
    }

}
