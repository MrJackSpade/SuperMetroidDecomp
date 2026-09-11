using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The eighteen shared bank-$86 enemy-projectile slots used by Mother Brain's translated
/// blue rings, phase-three bombs, exploded escape-door fragments, and the alternate-language
/// subtitle.
/// </summary>
/// <remarks>
/// This is intentionally a projectile system instead of a timer hidden in the boss actor.
/// Retail code spawns definition <c>$86:CB4B</c> into the highest free projectile slot,
/// then the gameplay loop runs every occupied slot from high to low after all enemy AI.
/// A ring spawned by Mother Brain's head therefore receives its first delayed pre-instruction
/// and its first animation-list frame on that same gameplay frame.
/// </remarks>
public sealed partial class MotherBrainEnemyProjectileSystem
{
    /// <summary>Physical enemy-projectile capacity at WRAM <c>$1997-$19B9</c>.</summary>
    public const int SlotCount = 18;

    /// <summary>Projectile definition pointer used by head opcode <c>$A9:9E29</c>.</summary>
    public const ushort ProjectileDefinition = 0xcb4b;

    /// <summary>Mother Brain bomb definition spawned by head opcode <c>$A9:9EBD</c>.</summary>
    public const ushort BombDefinition = 0xcb59;

    /// <summary>Large purple-breath definition spawned beside Mother Brain bombs.</summary>
    public const ushort PurpleBreathBigDefinition = 0xcb2f;

    /// <summary>Generic room-coordinate dust/explosion definition at <c>$86:E509</c>.</summary>
    public const ushort MiscDustDefinition = 0xe509;

    /// <summary>Exploded escape-door fragment definition at <c>$86:CB21</c>.</summary>
    public const ushort EscapeDoorParticleDefinition = 0xcb21;

    /// <summary>Alternate-language “time bomb set” subtitle definition at <c>$86:CBBB</c>.</summary>
    public const ushort TimeBombSetSubtitleDefinition = 0xcbbb;

    /// <summary>Initial animation-list pointer stored by definition <c>$86:CB4B</c>.</summary>
    public const ushort InitialInstructionList = 0xc432;

    private const ushort SetXAndYRadiusInstruction = 0x8298;
    private const ushort DeleteInstruction = 0x8154;
    private const ushort SleepInstruction = 0x8159;
    private const ushort ClearPreInstruction = 0x816a;
    private const ushort GotoInstruction = 0x81ab;
    private const int MiscDustInstructionPointerTable = 0x86e42c;
    private static ReadOnlySpan<ushort> BombYAccelerations =>
        [0x0007, 0x0010, 0x0020, 0x0040, 0x0070, 0x00b0, 0x00f0, 0x0130, 0x0170, 0x0000];
    private static readonly short[] EscapeDoorParticleYOffsets =
        [-0x20, -0x18, -0x10, -0x08, 0x00, 0x08, 0x10, 0x18];
    private static readonly short[] EscapeDoorParticleYVelocities =
        [-0x0200, -0x0100, -0x0100, -0x0080, -0x0080, 0x0080, -0x0100, 0x0200];
    private readonly MotherBrainEnemyProjectileSlot[] _slots =
        Enumerable.Range(0, SlotCount)
            .Select(index => new MotherBrainEnemyProjectileSlot(index))
            .ToArray();

    /// <summary>Slots in ascending WRAM order; native processing visits them in reverse.</summary>
    public IReadOnlyList<MotherBrainEnemyProjectileSlot> Slots => _slots;

    /// <summary>
    /// Mother Brain body word incremented by <c>$86:C381</c>. The Baby's next enemy-AI call
    /// consumes the flag as a cry request; exposing the count preserves simultaneous hits.
    /// </summary>
    public ushort PendingBabyCryCount { get; private set; }

    /// <summary>Room-impact earthquake type written by <c>$86:C404</c>.</summary>
    public ushort EarthquakeType { get; private set; }

    /// <summary>Room-impact earthquake timer written by <c>$86:C404</c>.</summary>
    public ushort EarthquakeTimer { get; private set; }

    /// <summary>
    /// Host witness for Samus's global invincibility word. Rings normally target the Baby,
    /// but the native collision priority still checks Samus second and writes `$60` here.
    /// </summary>
    public ushort SamusInvincibilityTimer { get; private set; }

    /// <summary>Allocates and initializes one native blue-ring slot.</summary>
    public int? Spawn(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        MotherBrainOnionRingSpawnRequest request)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(motherBrain);

        // `$86:8027` starts at byte index `$22` (logical slot 17) and subtracts two until
        // it finds an ID word equal to zero. A full allocation fails without replacing a
        // live projectile.
        int slotIndex = SlotCount - 1;
        while (slotIndex >= 0 && _slots[slotIndex].IsActive)
            slotIndex--;
        if (slotIndex < 0)
            return null;

        MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
        slot.Clear();
        slot.ProjectileId = ProjectileDefinition;
        slot.Properties = 0x3050;
        slot.GraphicsIndex = 0x0400;
        slot.DelayTimer = 0x0008;
        slot.Angle = request.Angle;
        slot.XRadius = 6;
        slot.YRadius = 6;
        slot.InstructionPointer = InitialInstructionList;
        slot.InstructionTimer = 1;
        slot.SpritemapPointer = 0x8000;

        // `$86:C27A` multiplies `$0450` by a sign-extended ROM sine entry, shifts the
        // unsigned magnitude right eight, then restores the sign. Cosine is sine+`$40`.
        slot.XVelocity = CalculateVelocityComponent(0x0450, request.Angle.TableIndex);
        slot.YVelocity = CalculateVelocityComponent(
            0x0450,
            request.Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        PinToBrain(slot, motherBrain);
        return slotIndex;
    }

    /// <summary>
    /// Allocates and initializes one native Mother Brain bomb from <c>$86:C482-C4C7</c>.
    /// </summary>
    public int? SpawnBomb(
        MotherBrainRainbowBeamAttackSequence motherBrain,
        MotherBrainBombSpawnRequest request)
    {
        ArgumentNullException.ThrowIfNull(motherBrain);

        // `$86:8027` is shared by every enemy-projectile definition in this class. A full
        // pool drops the spawn request and, critically, does not increment the body counter.
        int slotIndex = SlotCount - 1;
        while (slotIndex >= 0 && _slots[slotIndex].IsActive)
            slotIndex--;
        if (slotIndex < 0)
            return null;

        MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
        slot.Clear();
        slot.ProjectileId = BombDefinition;
        slot.Properties = 0x40a0;
        slot.SpawnParameter = request.AfterburnCount;
        slot.GraphicsIndex = 0x0400;
        slot.XPosition = unchecked((ushort)(motherBrain.BrainXPosition + 0x000c));
        slot.YPosition = unchecked((ushort)(motherBrain.BrainYPosition + 0x0010));

        // The initializer performs an eight-bit store into the LOW byte of X subposition.
        // The common 8.8 mover owns only the high byte, so this low byte survives as the
        // natural-expiry afterburn parameter even while fractional X motion accumulates.
        slot.XSubposition = unchecked((byte)request.AfterburnCount);
        slot.XVelocity = 0x00e0;
        slot.YVelocity = 0x0100;
        slot.XRadius = 6;
        slot.YRadius = 6;
        slot.BounceHorizontalSpeed = 0x0070;
        slot.BounceTableOffset = 0;
        slot.InstructionPointer = 0xc76e;
        slot.InstructionTimer = 1;
        slot.SpritemapPointer = 0x8000;
        motherBrain.RegisterBombSpawn();
        return slotIndex;
    }

    /// <summary>
    /// Allocates the stationary large purple breath from <c>$86:CA6A-CA82</c>.
    /// </summary>
    public int? SpawnPurpleBreathBig(MotherBrainRainbowBeamAttackSequence motherBrain)
    {
        ArgumentNullException.ThrowIfNull(motherBrain);

        int slotIndex = SlotCount - 1;
        while (slotIndex >= 0 && _slots[slotIndex].IsActive)
            slotIndex--;
        if (slotIndex < 0)
            return null;

        MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
        slot.Clear();
        slot.ProjectileId = PurpleBreathBigDefinition;
        slot.Properties = 0x3000;
        slot.GraphicsIndex = 0;
        slot.XPosition = unchecked((ushort)(motherBrain.BrainXPosition + 6));
        slot.YPosition = unchecked((ushort)(motherBrain.BrainYPosition + 0x0010));
        slot.InstructionPointer = 0xcaa4;
        slot.InstructionTimer = 1;
        slot.SpritemapPointer = 0x8000;
        return slotIndex;
    }

    /// <summary>
    /// Allocates `$86:E509` at an explicit room coordinate and selects one of its thirty
    /// ROM animation lists through `$86:E42C`. This is the ordinary producer used by the
    /// dying Baby, Mother Brain corpse rows, bombs, and the exploding escape door.
    /// </summary>
    public int? SpawnMiscDust(
        ISnesAddressSpace bus,
        ushort xPosition,
        ushort yPosition,
        ushort animationIndex)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (animationIndex > 0x001d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex),
                animationIndex,
                "Bank-$86 misc-dust animation index must be in the native $00..$1D range.");
        }

        int slotIndex = SlotCount - 1;
        while (slotIndex >= 0 && _slots[slotIndex].IsActive)
            slotIndex--;
        if (slotIndex < 0)
            return null;

        MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
        slot.Clear();
        slot.ProjectileId = MiscDustDefinition;
        slot.Properties = 0x1000;
        slot.GraphicsIndex = 0;
        slot.SpawnParameter = animationIndex;
        slot.XPosition = xPosition;
        slot.YPosition = yPosition;

        // `$E468` doubles the parameter and follows the literal bank-$86 word table. Native
        // SpawnEnemyProjectile cleared the slot and installed instruction timer one before
        // calling that initializer; reproduce those shared effects around its three stores.
        slot.InstructionPointer = ReadWord(
            bus,
            MiscDustInstructionPointerTable + animationIndex * 2);
        slot.InstructionTimer = 1;
        slot.SpritemapPointer = 0x8000;
        return slotIndex;
    }

    /// <summary>
    /// Allocates and initializes one exploded escape-door fragment from <c>$86:C961-$C991</c>.
    /// </summary>
    public int? SpawnEscapeDoorParticle(MotherBrainEscapeDoorParticleSpawnRequest request)
    {
        if (request.Parameter >= 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                request.Parameter,
                "Mother Brain escape-door particle parameter must be in the native 0..7 range.");
        }

        // This is the same `$86:8027` scan used by blue rings: all Mother Brain enemy
        // projectiles compete for the same highest free physical slot. Keeping one pool is
        // important when a debugger deliberately leaves old projectiles alive at the door.
        int slotIndex = SlotCount - 1;
        while (slotIndex >= 0 && _slots[slotIndex].IsActive)
            slotIndex--;
        if (slotIndex < 0)
            return null;

        MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
        slot.Clear();
        slot.ProjectileId = EscapeDoorParticleDefinition;
        slot.Properties = 0x3000;
        slot.SpawnParameter = request.Parameter;
        slot.GraphicsIndex = 0;
        slot.XPosition = 0x0010;

        // `$C965` multiplies the parameter by four because the native table interleaves
        // one X word and one Y word per record. Every X offset is zero and every X velocity
        // is `$0500`; only the Y offset/velocity vary across the eight fragments.
        slot.YPosition = unchecked((ushort)(
            0x0080 + EscapeDoorParticleYOffsets[request.Parameter]));
        slot.XVelocity = 0x0500;
        slot.YVelocity = unchecked((ushort)EscapeDoorParticleYVelocities[request.Parameter]);
        slot.Lifetime = 0x0020;
        slot.InstructionPointer = 0xca22;
        slot.InstructionTimer = 1;
        slot.SpritemapPointer = 0x8000;
        return slotIndex;
    }

    /// <summary>
    /// Allocates the persistent alternate-language subtitle from <c>$86:CAF6-$CB11</c>.
    /// </summary>
    public int? SpawnTimeBombSetSubtitle()
    {
        int slotIndex = SlotCount - 1;
        while (slotIndex >= 0 && _slots[slotIndex].IsActive)
            slotIndex--;
        if (slotIndex < 0)
            return null;

        MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
        slot.Clear();
        slot.ProjectileId = TimeBombSetSubtitleDefinition;
        slot.Properties = 0x1000;
        slot.GraphicsIndex = 0;
        slot.XVelocity = 0;
        slot.YVelocity = 0;
        slot.XPosition = 0x0080;
        slot.YPosition = 0x00c0;
        slot.InstructionPointer = 0xcb0d;
        slot.InstructionTimer = 1;
        slot.SpritemapPointer = 0x8000;
        return slotIndex;
    }

    /// <summary>
    /// Runs <c>$86:8104-$8160</c>'s translated Mother Brain projectile subset for one frame.
    /// </summary>
    public MotherBrainEnemyProjectileFrameResult StepFrame(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        BabyMetroidCutsceneState? baby,
        SamusState samus,
        ushort layer1X = 0,
        ushort layer1Y = 0,
        SamusBombProjectileSystem? samusBombs = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(motherBrain);
        ArgumentNullException.ThrowIfNull(samus);

        SamusInvincibilityTimer = NativeWordCounter.DecrementSaturating(SamusInvincibilityTimer);
        EarthquakeTimer = NativeWordCounter.DecrementSaturating(EarthquakeTimer);

        var events = new List<MotherBrainOnionRingEvent>();
        var escapeDoorDustRequests = new List<MotherBrainEscapeDoorParticleDustRequest>();
        var bombEvents = new List<MotherBrainBombEvent>();

        // EprojRunAll scans physical byte indices `$22,$20,...,$00`. Keeping this order
        // matters when several rings overlap the Baby on the same frame: each live slot
        // applies its own `$50` subtraction before a later slot observes zero health.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
            if (!slot.IsActive)
                continue;

            if (slot.ProjectileId == EscapeDoorParticleDefinition)
            {
                bool deleted = RunEscapeDoorParticlePreInstruction(
                    slot,
                    out MotherBrainEscapeDoorParticleDustRequest? dustRequest);
                if (dustRequest is { } dust)
                {
                    escapeDoorDustRequests.Add(dust);

                    // `$86:CA03-CA1D` clears the fragment ID before calling the shared
                    // allocator. The allocator starts at physical slot seventeen every
                    // time, so it either chooses a free slot already visited by this
                    // descending loop or reuses the current slot. `$86:8128` then reloads
                    // the ORIGINAL loop index. Only the reuse case therefore advances the
                    // newborn dust's instruction list during this same outer pass.
                    int? allocatedSlot = SpawnMiscDust(
                        bus,
                        dust.XPosition,
                        dust.YPosition,
                        dust.ProjectileParameter);
                    if (allocatedSlot == slotIndex)
                        RunMiscDustForCurrentPass(bus, slot, layer1X, layer1Y);
                }

                // An ordinary live fragment retains its looping CA22 animation. A deleted
                // fragment was either replaced above (and handled as dust) or remains a
                // dead current slot because the newborn occupied a higher free slot.
                if (!deleted)
                    RunEscapeDoorParticleInstructionHandler(bus, slot);
                continue;
            }

            if (slot.ProjectileId == TimeBombSetSubtitleDefinition)
            {
                // `$86:CAFA` rewrites all four motion words every frame. This is stronger
                // than “velocity zero”: external debugger edits cannot move the subtitle,
                // because the native pre-instruction pins it back to screen (128,192).
                slot.XVelocity = 0;
                slot.YVelocity = 0;
                slot.XPosition = 0x0080;
                slot.YPosition = 0x00c0;
                RunInstructionHandler(bus, slot);
                continue;
            }

            if (slot.ProjectileId == BombDefinition)
            {
                bool deleted = RunBombPreInstruction(
                    slot,
                    motherBrain,
                    samusBombs,
                    out MotherBrainBombEvent? bombEvent);
                if (bombEvent is { } translatedBombEvent)
                {
                    bombEvents.Add(translatedBombEvent);

                    if (translatedBombEvent.Kind == MotherBrainBombEventKind.DestroyedBySamusBomb)
                    {
                        // `$86:C595-C5A8` clears the bomb before allocating parameter-nine
                        // dust. As with terminal door fragments, a same-slot allocation is
                        // immediately seen by `$8128-$8150`; a higher slot has already had
                        // its turn. Natural expiry first allocates Ridley afterburn, whose
                        // definition is not yet in this translated subset, so its later
                        // parameter-three dust remains an explicit event until that owner is
                        // implemented rather than lying about shared-pool competition.
                        int? allocatedSlot = SpawnMiscDust(
                            bus,
                            translatedBombEvent.XPosition,
                            translatedBombEvent.YPosition,
                            translatedBombEvent.DustParameter);
                        if (allocatedSlot == slotIndex)
                            RunMiscDustForCurrentPass(bus, slot, layer1X, layer1Y);
                    }
                }

                // `$86:C585` deliberately removes the caller's return address so a Samus-
                // bomb collision exits the entire pre-instruction immediately. The generic
                // dispatcher would still enter animation processing afterward, but a zero-ID
                // slot cannot draw; skipping dead bytecode retains every observable effect.
                if (!deleted)
                    RunLoopingInstructionHandler(bus, slot, "Mother Brain bomb");
                continue;
            }

            if (slot.ProjectileId == PurpleBreathBigDefinition)
            {
                // `$86:CAA3` is an RTS pre-instruction: the breath remains fixed at the
                // coordinates captured at spawn while its finite ROM animation runs.
                RunFiniteTimedInstructionHandler(bus, slot, "Mother Brain purple breath");
                continue;
            }

            if (slot.ProjectileId == MiscDustDefinition)
            {
                RunMiscDustForCurrentPass(bus, slot, layer1X, layer1Y);
                continue;
            }

            if (slot.ProjectileId != ProjectileDefinition)
            {
                throw new InvalidDataException(
                    $"Mother Brain projectile slot {slotIndex} contains untranslated definition " +
                    $"$86:{slot.ProjectileId:X4}.");
            }

            bool deletedByPreInstruction = RunPreInstruction(
                slot,
                motherBrain,
                baby,
                samus,
                layer1X,
                out MotherBrainOnionRingCollisionKind collision,
                out BabyMetroidOnionRingHitResult babyHit);

            if (collision != MotherBrainOnionRingCollisionKind.None)
            {
                events.Add(new MotherBrainOnionRingEvent(
                    slotIndex,
                    collision,
                    slot.XPosition,
                    slot.YPosition,
                    babyHit.HealthBefore,
                    babyHit.HealthAfter));
            }

            // EprojRunOne does call the instruction handler even if a pre-instruction just
            // zeroed the ID. No observable state can survive that deleted slot, so the host
            // may skip parsing its now-dead animation without changing gameplay behavior.
            if (!deletedByPreInstruction)
                RunInstructionHandler(bus, slot);
        }

        int activeCount = _slots.Count(slot => slot.IsActive);
        return new MotherBrainEnemyProjectileFrameResult(
            activeCount,
            events.ToArray(),
            escapeDoorDustRequests.ToArray(),
            bombEvents.ToArray());
    }

    /// <summary>Draws definitions with property bit <c>$1000</c>, matching <c>$86:8390</c>.</summary>
    public void DrawHighPriority(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        short shakeX = 0,
        short shakeY = 0) =>
        DrawPriority(bus, oam, layer1X, layer1Y, true, shakeX, shakeY);

    /// <summary>Draws definitions without property bit <c>$1000</c>, matching <c>$86:83B2</c>.</summary>
    public void DrawLowPriority(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        short shakeX = 0,
        short shakeY = 0) =>
        DrawPriority(bus, oam, layer1X, layer1Y, false, shakeX, shakeY);

    /// <summary>Clears all eighteen physical slots and shared observable timers/requests.</summary>
    public void Reset()
    {
        foreach (MotherBrainEnemyProjectileSlot slot in _slots)
            slot.Clear();
        PendingBabyCryCount = 0;
        EarthquakeType = 0;
        EarthquakeTimer = 0;
        SamusInvincibilityTimer = 0;
    }

}
