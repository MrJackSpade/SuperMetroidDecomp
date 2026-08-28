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
    SkreeParticleDownRight = 0x8bc2,
    SkreeParticleUpRight = 0x8bd0,
    SkreeParticleDownLeft = 0x8bde,
    SkreeParticleUpLeft = 0x8bec,
    MetareeParticleDownRight = 0x8bfa,
    MetareeParticleUpRight = 0x8c08,
    MetareeParticleDownLeft = 0x8c16,
    MetareeParticleUpLeft = 0x8c24,
    CeresRidleyFireball = 0x9642,
    CeresRidleyHorizontalAfterburnCenter = 0x9650,
    CeresRidleyVerticalAfterburnCenter = 0x965e,
    CeresRidleyHorizontalAfterburnRight = 0x966c,
    CeresRidleyHorizontalAfterburnLeft = 0x967a,
    CeresRidleyVerticalAfterburnUp = 0x9688,
    CeresRidleyVerticalAfterburnDown = 0x9696,
    AlcoonFireball = 0x9e90,
    PowampSpike = 0xd298,
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
    public ushort RemainingAfterburns { get; internal set; }
    public ushort NextAfterburnKind { get; internal set; }
    public ushort DirectionParameter { get; internal set; }
    public bool CanDamageSamus { get; internal set; }

    internal void Clear()
    {
        Kind = RoomEnemyProjectileKind.None;
        XPosition = XSubposition = YPosition = YSubposition = 0;
        XVelocity = YVelocity = 0;
        InstructionPointer = InstructionTimer = SpritemapPointer = PreInstruction = 0;
        GraphicsIndex = XRadius = YRadius = Damage = InvincibilityFrames = 0;
        RemainingAfterburns = NextAfterburnKind = 0;
        DirectionParameter = 0;
        CanDamageSamus = false;
    }
}

public sealed partial class RoomEnemySystem
{
    // Super Metroid reserves native indexes $00..$22, in steps of two, for eighteen enemy
    // projectiles. Keeping the same capacity exposes saturation and spawn failure honestly.
    private const int RoomEnemyProjectileSlotCount = 18;
    private const ushort FireballGraphicsIndex = 0x0a00;
    private const ushort FireballRadius = 6;
    private const ushort FireballDamage = 3;
    private const ushort FireballInvincibilityFrames = 96;

    private readonly RoomEnemyProjectileSlot[] _enemyProjectiles =
        Enumerable.Range(0, RoomEnemyProjectileSlotCount)
            .Select(index => new RoomEnemyProjectileSlot(index))
            .ToArray();

    /// <summary>All eighteen physical bank-$86 slots, including currently inactive slots.</summary>
    public IReadOnlyList<RoomEnemyProjectileSlot> EnemyProjectiles => _enemyProjectiles;

    /// <summary>Number of live actors in the shared bank-$86 enemy-projectile pool.</summary>
    public int ActiveEnemyProjectileCount =>
        _enemyProjectiles.Count(projectile => projectile.IsActive);

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
        ushort cameraY = 0)
    {
        ArgumentNullException.ThrowIfNull(level);
        EnsureLoaded();

        // Snapshot the active set. Afterburn instruction opcodes may allocate later slots;
        // native descending-slot iteration does not execute a newly spawned lower-priority
        // actor twice in the same logical instruction pass.
        RoomEnemyProjectileSlot[] activeAtFrameStart =
            _enemyProjectiles.Where(projectile => projectile.IsActive).ToArray();
        foreach (RoomEnemyProjectileSlot projectile in activeAtFrameStart)
        {
            if (!projectile.IsActive)
                continue;

            RunEnemyProjectilePreInstruction(projectile, level, cameraX, cameraY);
            if (!projectile.IsActive)
                continue;

            ResolveEnemyProjectileSamusCollision(projectile, samus, controllerInput);
            if (!projectile.IsActive)
                continue;

            ProcessEnemyProjectileInstructions(projectile);
        }
    }

    /// <summary>
    /// Emits all live Ridley projectiles through the common bank-$81 enemy-projectile
    /// spritemap writer. Their cartridge properties are $5003, selecting the first enemy-
    /// projectile draw phase used by the runtime.
    /// </summary>
    public void DrawEnemyProjectiles(OamBuffer oam, ushort cameraX, ushort cameraY)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();

        foreach (RoomEnemyProjectileSlot projectile in _enemyProjectiles)
        {
            if (!projectile.IsActive || projectile.SpritemapPointer == 0)
                continue;

            ushort screenX = unchecked((ushort)(projectile.XPosition - cameraX));
            ushort screenY = unchecked((ushort)(projectile.YPosition - cameraY));
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
    private void CalculateCeresRidleyFireballVelocity(RoomEnemySlot ridley, SamusState samus)
    {
        CeresRidleyState state = RequireCeresRidley(ridley);
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
        short tableValue = unchecked((short)ReadWord(_bus!, 0xa0b443 + (angle * 2)));
        int magnitude = speed * Math.Abs((int)tableValue) >> 8;
        return unchecked((ushort)(tableValue < 0 ? -magnitude : magnitude));
    }

    /// <summary>Allocates and initializes enemy projectile $86:9642.</summary>
    private void SpawnCeresRidleyFireball(RoomEnemySlot ridley, bool spawnAfterburn)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return;

        CeresRidleyState state = RequireCeresRidley(ridley);
        projectile.Kind = RoomEnemyProjectileKind.CeresRidleyFireball;
        projectile.XPosition = unchecked((ushort)(ridley.XPosition +
            (state.FacingDirection == 0 ? -25 : 25)));
        projectile.YPosition = unchecked((ushort)(ridley.YPosition - 43));
        projectile.XVelocity = state.FireballXVelocity;
        projectile.YVelocity = state.FireballYVelocity;
        projectile.RemainingAfterburns = spawnAfterburn ? (ushort)3 : (ushort)0;
        projectile.InstructionPointer = 0x9552;
        projectile.InstructionTimer = 1;
        projectile.PreInstruction = 0x940e;
        projectile.GraphicsIndex = FireballGraphicsIndex;
        projectile.XRadius = FireballRadius;
        projectile.YRadius = FireballRadius;
        projectile.Damage = FireballDamage;
        projectile.InvincibilityFrames = FireballInvincibilityFrames;
        projectile.CanDamageSamus = true;
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

    private void RunEnemyProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level,
        ushort cameraX,
        ushort cameraY)
    {
        switch (projectile.PreInstruction)
        {
            case 0:
            case 0x8170: // The common cleared-pre-instruction RTS.
            case 0x950c: // Center afterburn is stationary while its instruction list blooms.
                return;

            case 0x940e:
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

            case 0x950d:
                MoveProjectileAxis(projectile, level, horizontal: true);
                if (MoveProjectileAxis(projectile, level, horizontal: false))
                    BeginAfterburnFinalAnimation(projectile);
                return;

            case 0x9522:
                MoveProjectileAxis(projectile, level, horizontal: false);
                if (MoveProjectileAxis(projectile, level, horizontal: true))
                    BeginAfterburnFinalAnimation(projectile);
                return;

            case 0x8b5d: // Skree particle: signed 8.8 movement, gravity, camera deletion.
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

            case 0x9eff: // Alcoon fireball: Y then X collision, followed by horizontal drag.
                RunAlcoonFireballPreInstruction(projectile, level);
                return;

            case 0xd263: // Powamp spike: radial acceleration and X-then-Y room collision.
                RunPowampSpikePreInstruction(projectile, level);
                return;

            default:
                throw new NotSupportedException(
                    $"Enemy projectile pre-instruction $86:{projectile.PreInstruction:X4} is not translated.");
        }
    }

    private static bool MoveProjectileAxis(
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
            ushort probeX = horizontal ? movementEdge : unchecked((ushort)(block << 4));
            ushort probeY = horizontal ? unchecked((ushort)(block << 4)) : movementEdge;
            if (ProjectileProbeHitsRoom(level, probeX, probeY))
                return true;
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

    private static bool ProjectileProbeHitsRoom(RoomLevelData level, ushort x, ushort y)
    {
        int blockX = x >> 4;
        int blockY = y >> 4;
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

        byte type = level.GetCollisionBlockByIndex(blockIndex).CollisionType;
        return type is 1 or 5 or 8 or 9 or 0x0b or 0x0c or 0x0d or 0x0e or 0x0f;
    }

    private void ResolveEnemyProjectileSamusCollision(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus,
        ushort controllerInput)
    {
        if (!projectile.CanDamageSamus || samus is null || samus.InvincibilityTimer != 0)
            return;

        int xDistance = Math.Abs(unchecked((short)(projectile.XPosition - samus.XPosition)));
        int yDistance = Math.Abs(unchecked((short)(projectile.YPosition - samus.YPosition)));
        if (xDistance >= projectile.XRadius + samus.Kinematics.XRadius ||
            yDistance >= projectile.YRadius + samus.Kinematics.YRadius)
        {
            return;
        }

        samus.Health = samus.Health <= projectile.Damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - projectile.Damage));
        samus.InvincibilityTimer = projectile.InvincibilityFrames;
        ushort knockbackXDirection = unchecked((short)(
            samus.XPosition - projectile.XPosition)) >= 0
            ? (ushort)1
            : (ushort)0;

        // Generic enemy-projectile touch publishes the five-frame knockback request and
        // bank $90 consumes it through special prospective command one. In this runtime
        // the common initializer is the typed form of that consumer: it installs pose
        // $53/$54, hurt animation, vertical launch, and the 60-frame flash lifetime. Merely
        // writing $18AA/$0A54 left Samus in an ordinary pose while invincibility hid her,
        // which looked exactly like the actor had disappeared after a fireball impact.
        SamusKnockbackMovement.Start(
            _bus!,
            samus,
            controllerInput,
            knockbackXDirection,
            knockbackTimer: 5);

        // Generic enemy-projectile contact deletes Ridley's fireball. Afterburn is a wall-
        // impact feature from $86:940E, so a Samus contact does not create the wall bloom.
        projectile.Clear();
    }

    private void ProcessEnemyProjectileInstructions(RoomEnemyProjectileSlot projectile)
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

            switch (word)
            {
                case 0x8154: // Delete.
                    projectile.Clear();
                    return;
                case 0x8161: // Install the operand as pre-instruction.
                    projectile.PreInstruction = ReadWord(
                        _bus!,
                        0x860000 | unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;
                case 0x816a: // Clear pre-instruction to $8170 RTS.
                    projectile.PreInstruction = 0x8170;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x81ab: // Same-bank goto.
                    cursor = ReadWord(_bus!, 0x860000 | unchecked((ushort)(cursor + 2)));
                    break;
                case 0x95ba: // Spawn horizontal right/left afterburn pair.
                    SpawnAfterburnPair(projectile, horizontal: true);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x95ed: // Spawn vertical up/down afterburn pair.
                    SpawnAfterburnPair(projectile, horizontal: false);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case 0x9620: // Decrement count and spawn the next actor in this direction.
                    SpawnNextAfterburn(projectile);
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                default:
                    throw new NotSupportedException(
                        $"Enemy projectile instruction $86:{word:X4} at $86:{cursor:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            "Enemy projectile list did not reach a timed frame within 24 operations.");
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
        center.Kind = kind;
        center.XPosition = x;
        center.YPosition = y;
        center.RemainingAfterburns = remaining;
        center.InstructionPointer = kind == RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter
            ? (ushort)0x95a0
            : (ushort)0x95d3;
        center.InstructionTimer = 1;
        center.PreInstruction = 0x950c;
        center.GraphicsIndex = FireballGraphicsIndex;
        center.XRadius = FireballRadius;
        center.YRadius = FireballRadius;
        center.Damage = FireballDamage;
        center.InvincibilityFrames = FireballInvincibilityFrames;
        center.CanDamageSamus = true;
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
        afterburn.Kind = kind;
        afterburn.XPosition = source.XPosition;
        afterburn.YPosition = source.YPosition;
        afterburn.XVelocity = xVelocity;
        afterburn.YVelocity = yVelocity;
        afterburn.RemainingAfterburns = source.RemainingAfterburns;
        afterburn.NextAfterburnKind = (ushort)kind;
        afterburn.InstructionPointer = 0x9606;
        afterburn.InstructionTimer = 1;
        afterburn.PreInstruction = xVelocity != 0 ? (ushort)0x950d : (ushort)0x9522;
        afterburn.GraphicsIndex = FireballGraphicsIndex;
        afterburn.XRadius = FireballRadius;
        afterburn.YRadius = FireballRadius;
        afterburn.Damage = FireballDamage;
        afterburn.InvincibilityFrames = FireballInvincibilityFrames;
        afterburn.CanDamageSamus = true;
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
        projectile.InstructionPointer = 0x9574;
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
            instructionPointer: 0x8abd);
        SpawnSkreeOrMetareeParticle(
            skree,
            RoomEnemyProjectileKind.SkreeParticleUpRight,
            6,
            0x0060,
            0xfbff,
            instructionPointer: 0x8abd);
        SpawnSkreeOrMetareeParticle(
            skree,
            RoomEnemyProjectileKind.SkreeParticleDownLeft,
            -6,
            0xfec0,
            0xfcff,
            instructionPointer: 0x8abd);
        SpawnSkreeOrMetareeParticle(
            skree,
            RoomEnemyProjectileKind.SkreeParticleUpLeft,
            -6,
            0xffa0,
            0xfbff,
            instructionPointer: 0x8abd);
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
            instructionPointer: 0x8ac5);
        SpawnSkreeOrMetareeParticle(
            metaree,
            RoomEnemyProjectileKind.MetareeParticleUpRight,
            6,
            0x0060,
            0xfbff,
            instructionPointer: 0x8ac5);
        SpawnSkreeOrMetareeParticle(
            metaree,
            RoomEnemyProjectileKind.MetareeParticleDownLeft,
            -6,
            0xfec0,
            0xfcff,
            instructionPointer: 0x8ac5);
        SpawnSkreeOrMetareeParticle(
            metaree,
            RoomEnemyProjectileKind.MetareeParticleUpLeft,
            -6,
            0xffa0,
            0xfbff,
            instructionPointer: 0x8ac5);
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
        particle.PreInstruction = 0x8b5d;
        particle.GraphicsIndex = unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex));
        particle.XRadius = 2;
        particle.YRadius = 2;
    }

}
