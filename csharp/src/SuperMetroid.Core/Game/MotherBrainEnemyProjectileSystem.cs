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
public sealed class MotherBrainEnemyProjectileSystem
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

    private const int SignedSineTable = 0xa0b443;
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
        slot.XVelocity = CalculateVelocityComponent(bus, 0x0450, request.Angle);
        slot.YVelocity = CalculateVelocityComponent(
            bus,
            0x0450,
            unchecked((byte)(request.Angle + 0x40)));
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

        if (SamusInvincibilityTimer != 0)
            SamusInvincibilityTimer = unchecked((ushort)(SamusInvincibilityTimer - 1));
        if (EarthquakeTimer != 0)
            EarthquakeTimer = unchecked((ushort)(EarthquakeTimer - 1));

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
                throw new NotSupportedException(
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

    private void DrawPriority(
        ISnesAddressSpace bus,
        OamBuffer oam,
        ushort layer1X,
        ushort layer1Y,
        bool highPriority,
        short shakeX,
        short shakeY)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        // Both `$8390` and `$83B2` scan physical byte indices `$22,$20,...,$00`. A slot's
        // definition-supplied property bit decides which pass owns it; it can never draw twice.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            MotherBrainEnemyProjectileSlot slot = _slots[slotIndex];
            if (!slot.IsActive || ((slot.Properties & 0x1000) != 0) != highPriority)
                continue;

            ushort screenX = unchecked((ushort)(slot.XPosition - layer1X + shakeX));
            ushort xAdmission = unchecked((ushort)(screenX + 0x0080));
            if ((xAdmission & 0xfe00) != 0)
                continue;

            ushort screenY = unchecked((ushort)(slot.YPosition - layer1Y + shakeY));
            bool originYIsOnScreen = (screenY & 0xff00) == 0;
            if (!originYIsOnScreen)
            {
                ushort yAdmission = unchecked((ushort)(screenY + 0x0080));
                if ((yAdmission & 0xfe00) != 0)
                    continue;
            }

            oam.AddEnemyProjectileSpritemap(
                bus,
                slot.SpritemapPointer,
                screenX,
                screenY,
                slot.GraphicsIndex,
                originYIsOnScreen);
        }
    }

    private static bool RunFiniteTimedInstructionHandler(
        ISnesAddressSpace bus,
        MotherBrainEnemyProjectileSlot slot,
        string definitionName)
    {
        ushort oldTimer = slot.InstructionTimer;
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (oldTimer != 1)
            return false;

        ushort pointer = slot.InstructionPointer;
        for (int operationCount = 0; operationCount < 16; operationCount++)
        {
            ushort durationOrOpcode = ReadWord(bus, 0x860000 | pointer);
            if ((durationOrOpcode & 0x8000) == 0)
            {
                if (durationOrOpcode == 0)
                    throw new InvalidDataException(
                        $"{definitionName} frame at $86:{pointer:X4} has zero duration.");

                slot.InstructionTimer = durationOrOpcode;
                slot.SpritemapPointer = ReadWord(
                    bus,
                    0x860000 | unchecked((ushort)(pointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(pointer + 4));
                return false;
            }

            switch (durationOrOpcode)
            {
                case ClearPreInstruction:
                    // The translated breath already has an inert pre-instruction. Retain
                    // this opcode in the parser so the ROM list, not host setup, owns timing.
                    pointer = unchecked((ushort)(pointer + 2));
                    break;

                case DeleteInstruction:
                    slot.ProjectileId = 0;
                    return true;

                default:
                    throw new NotSupportedException(
                        $"{definitionName} instruction $86:{durationOrOpcode:X4} at " +
                        $"$86:{pointer:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            $"{definitionName} list did not reach a timed frame within 16 operations.");
    }

    private static bool IsOutsideLayerOneWindow(
        MotherBrainEnemyProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y)
    {
        // `$86:E6E0` uses CMP followed by BMI/BPL rather than an unsigned BCC/BCS pair.
        // Express each subtraction as a signed 16-bit result so wraparound at room-space
        // boundaries remains visible instead of becoming a host integer comparison.
        ushort right = unchecked((ushort)(layer1X + 0x0100));
        ushort bottom = unchecked((ushort)(layer1Y + 0x0100));
        return unchecked((short)(slot.XPosition - layer1X)) < 0 ||
               unchecked((short)(slot.XPosition - right)) >= 0 ||
               unchecked((short)(slot.YPosition - layer1Y)) < 0 ||
               unchecked((short)(slot.YPosition - bottom)) >= 0;
    }

    /// <summary>
    /// Executes `$86:E4FE` and the shared instruction interpreter for one misc-dust slot.
    /// Kept as one helper because projectiles born from another projectile's pre-instruction
    /// can legitimately reach this sequence in the middle of the descending slot pass.
    /// </summary>
    private static void RunMiscDustForCurrentPass(
        ISnesAddressSpace bus,
        MotherBrainEnemyProjectileSlot slot,
        ushort layer1X,
        ushort layer1Y)
    {
        // `$86:E4FE` removes room-coordinate dust as soon as its ORIGIN leaves the strict
        // 256x256 layer-1 window. Individual spritemap pieces still use the separate
        // bank-$8D edge-wrap rules when an admitted origin straddles a boundary.
        if (IsOutsideLayerOneWindow(slot, layer1X, layer1Y))
        {
            slot.ProjectileId = 0;
            return;
        }

        RunFiniteTimedInstructionHandler(bus, slot, "misc dust/explosion");
    }

    private static bool RunBombPreInstruction(
        MotherBrainEnemyProjectileSlot slot,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        SamusBombProjectileSystem? samusBombs,
        out MotherBrainBombEvent? bombEvent)
    {
        bombEvent = null;

        // `$86:C1BF-$C203` scans Samus's five bomb slots from low to high. Only the normal-
        // bomb family with variable/timer zero is an explosion capable of destroying this
        // enemy projectile; generic projectile damage and non-exploding bombs do not count.
        if (samusBombs is not null && samusBombs.BombCounter != 0)
        {
            foreach (SamusBombProjectileSlot samusBomb in samusBombs.Slots)
            {
                if ((samusBomb.Type & 0x0f00) != SamusBombProjectileSystem.NormalBombType ||
                    samusBomb.BombTimer != 0 ||
                    !StrictAxisOverlap(
                        slot.XPosition,
                        slot.YPosition,
                        slot.XRadius,
                        slot.YRadius,
                        samusBomb.XPosition,
                        samusBomb.YPosition,
                        samusBomb.XRadius,
                        samusBomb.YRadius))
                {
                    continue;
                }

                motherBrain.RegisterBombDeletion();
                slot.XVelocity = 0;
                slot.YVelocity = 0;
                slot.ProjectileId = 0;
                bombEvent = new MotherBrainBombEvent(
                    slot.Index,
                    MotherBrainBombEventKind.DestroyedBySamusBomb,
                    slot.XPosition,
                    slot.YPosition,
                    slot.BounceTableOffset,
                    AfterburnCount: null,
                    DustParameter: 0x0009,
                    EnemyDropRequested: true,
                    QueuedSoundLibraryThree: null);
                return true;
            }
        }

        ushort acceleration;
        if (slot.BounceTableOffset == 0)
        {
            // Before the first floor impact only, horizontal velocity loses `$0002` of
            // absolute magnitude per call. The signed BPL clamp is reproduced explicitly.
            bool movingLeft = (slot.XVelocity & 0x8000) != 0;
            ushort magnitude = movingLeft
                ? unchecked((ushort)-slot.XVelocity)
                : slot.XVelocity;
            ushort slowedMagnitude = unchecked((ushort)(magnitude - 0x0002));
            if ((slowedMagnitude & 0x8000) != 0)
                slowedMagnitude = 0;
            slot.XVelocity = movingLeft
                ? unchecked((ushort)-slowedMagnitude)
                : slowedMagnitude;
            acceleration = 0x0007;
        }
        else
        {
            int accelerationIndex = slot.BounceTableOffset >> 1;
            if ((uint)accelerationIndex >= (uint)BombYAccelerations.Length)
            {
                throw new InvalidDataException(
                    $"Mother Brain bomb bounce-table offset ${slot.BounceTableOffset:X4} is outside $C550-$C563.");
            }

            acceleration = BombYAccelerations[accelerationIndex];
            if (acceleration == 0)
            {
                // Natural expiry publishes three independent effects. The first uses the
                // preserved LOW X-subposition byte, not the wider host SpawnParameter word.
                motherBrain.RegisterBombDeletion();
                slot.XVelocity = 0;
                slot.YVelocity = 0;
                slot.ProjectileId = 0;
                bombEvent = new MotherBrainBombEvent(
                    slot.Index,
                    MotherBrainBombEventKind.Expired,
                    slot.XPosition,
                    slot.YPosition,
                    slot.BounceTableOffset,
                    AfterburnCount: unchecked((byte)slot.XSubposition),
                    DustParameter: 0x0003,
                    EnemyDropRequested: false,
                    QueuedSoundLibraryThree: 0x0013);
                return true;
            }
        }

        if (!MoveBomb(slot, acceleration))
            return false;

        // Var1 is a byte offset into the word table, hence two increments per bounce.
        slot.BounceTableOffset = unchecked((ushort)(slot.BounceTableOffset + 2));
        bombEvent = new MotherBrainBombEvent(
            slot.Index,
            MotherBrainBombEventKind.Bounced,
            slot.XPosition,
            slot.YPosition,
            slot.BounceTableOffset,
            AfterburnCount: null,
            DustParameter: 0,
            EnemyDropRequested: false,
            QueuedSoundLibraryThree: null);
        return false;
    }

    private static bool MoveBomb(MotherBrainEnemyProjectileSlot slot, ushort acceleration)
    {
        slot.YVelocity = unchecked((ushort)(slot.YVelocity + acceleration));
        MoveAccordingToVelocity(slot);

        // These are CMP/BMI pairs, so test the sign of the wrapped subtraction rather than
        // applying an unsigned host comparison. Crossing screen X `$F0` reflects velocity
        // without clamping X, exactly as `$86:C5CC-C5DB` does.
        if (unchecked((short)(slot.XPosition - 0x00f0)) >= 0)
            slot.XVelocity = unchecked((ushort)-slot.XVelocity);

        if (unchecked((short)(slot.YPosition - 0x00d0)) < 0)
            return false;

        slot.YPosition = 0x00d0;
        slot.XVelocity = (slot.XVelocity & 0x8000) != 0
            ? unchecked((ushort)-slot.BounceHorizontalSpeed)
            : slot.BounceHorizontalSpeed;
        slot.YVelocity = 0xfe00;
        return true;
    }

    private static void RunLoopingInstructionHandler(
        ISnesAddressSpace bus,
        MotherBrainEnemyProjectileSlot slot,
        string projectileName)
    {
        ushort oldTimer = slot.InstructionTimer;
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (oldTimer != 1)
            return;

        ushort pointer = slot.InstructionPointer;
        for (int operationCount = 0; operationCount < 16; operationCount++)
        {
            ushort durationOrOpcode = ReadWord(bus, 0x860000 | pointer);
            if ((durationOrOpcode & 0x8000) == 0)
            {
                if (durationOrOpcode == 0)
                    throw new InvalidDataException(
                        $"{projectileName} frame at $86:{pointer:X4} has zero duration.");

                slot.InstructionTimer = durationOrOpcode;
                slot.SpritemapPointer = ReadWord(
                    bus,
                    0x860000 | unchecked((ushort)(pointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(pointer + 4));
                return;
            }

            if (durationOrOpcode != GotoInstruction)
            {
                throw new NotSupportedException(
                    $"{projectileName} instruction $86:{durationOrOpcode:X4} at " +
                    $"$86:{pointer:X4} is not translated.");
            }

            pointer = ReadWord(bus, 0x860000 | unchecked((ushort)(pointer + 2)));
        }

        throw new InvalidDataException(
            $"{projectileName} instruction list did not reach a timed frame within 16 operations.");
    }

    private static bool RunEscapeDoorParticlePreInstruction(
        MotherBrainEnemyProjectileSlot slot,
        out MotherBrainEscapeDoorParticleDustRequest? dustRequest)
    {
        dustRequest = null;

        // `$C9D2` removes `$10` (1/16 pixel per frame) from the absolute X velocity and
        // restores the original sign. The BPL clamp is a signed 16-bit test, so preserve
        // wrapping arithmetic instead of using floating point or Math.Max on host ints.
        bool movingLeft = (slot.XVelocity & 0x8000) != 0;
        ushort magnitude = movingLeft
            ? unchecked((ushort)-slot.XVelocity)
            : slot.XVelocity;
        ushort slowedMagnitude = unchecked((ushort)(magnitude - 0x0010));
        if ((slowedMagnitude & 0x8000) != 0)
            slowedMagnitude = 0;
        slot.XVelocity = movingLeft
            ? unchecked((ushort)-slowedMagnitude)
            : slowedMagnitude;

        // Gravity is +$20 in native 8.8 units. The common mover consumes the HIGH
        // subposition byte and signed velocity high byte exactly as it does for blue rings.
        slot.YVelocity = unchecked((ushort)(slot.YVelocity + 0x0020));
        MoveAccordingToVelocity(slot);

        // Var0 starts at `$20`, is decremented after motion, and deletes only when the
        // result is negative. Therefore every fragment moves 33 times: `$1F..0,$FFFF`.
        slot.Lifetime = unchecked((ushort)(slot.Lifetime - 1));
        if ((slot.Lifetime & 0x8000) == 0)
            return false;

        slot.ProjectileId = 0;
        slot.YPosition = unchecked((ushort)(slot.YPosition - 4));
        dustRequest = new MotherBrainEscapeDoorParticleDustRequest(
            slot.Index,
            slot.XPosition,
            slot.YPosition,
            ProjectileParameter: 0x0009);
        return true;
    }

    private static void RunEscapeDoorParticleInstructionHandler(
        ISnesAddressSpace bus,
        MotherBrainEnemyProjectileSlot slot)
    {
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (slot.InstructionTimer != 0)
            return;

        ushort pointer = slot.InstructionPointer;
        for (int operationCount = 0; operationCount < 16; operationCount++)
        {
            ushort durationOrOpcode = ReadWord(bus, 0x860000 | pointer);
            if ((durationOrOpcode & 0x8000) == 0)
            {
                if (durationOrOpcode == 0)
                {
                    throw new InvalidDataException(
                        $"Escape-door particle frame at $86:{pointer:X4} has zero duration.");
                }

                slot.InstructionTimer = durationOrOpcode;
                slot.SpritemapPointer = ReadWord(
                    bus,
                    0x860000 | unchecked((ushort)(pointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(pointer + 4));
                return;
            }

            if (durationOrOpcode != GotoInstruction)
            {
                throw new NotSupportedException(
                    $"Escape-door particle instruction $86:{durationOrOpcode:X4} at " +
                    $"$86:{pointer:X4} is not translated.");
            }

            // `$86:81AB` receives Y already advanced past the opcode and replaces it with
            // the following word. The target `$CA22` immediately yields frame zero during
            // this same interpreter call; there is no blank animation frame at the loop.
            pointer = ReadWord(bus, 0x860000 | unchecked((ushort)(pointer + 2)));
        }

        throw new InvalidDataException(
            "Escape-door particle instruction list did not reach a timed frame within 16 operations.");
    }

    /// <summary>Consumes all pending Baby cry requests like <c>$A9:C7B7</c>.</summary>
    public ushort ConsumePendingBabyCries()
    {
        ushort count = PendingBabyCryCount;
        PendingBabyCryCount = 0;
        return count;
    }

    private bool RunPreInstruction(
        MotherBrainEnemyProjectileSlot slot,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        BabyMetroidCutsceneState? baby,
        SamusState samus,
        ushort layer1X,
        out MotherBrainOnionRingCollisionKind collision,
        out BabyMetroidOnionRingHitResult babyHit)
    {
        collision = MotherBrainOnionRingCollisionKind.None;
        babyHit = default;

        if (slot.DelayTimer != 0)
        {
            // `$C335` decrements before re-pinning, so delay values 8..1 produce eight
            // stationary/following calls. Motion begins only when zero is visible on entry.
            slot.DelayTimer = unchecked((ushort)(slot.DelayTimer - 1));
            PinToBrain(slot, motherBrain);
            return false;
        }

        MoveAccordingToVelocity(slot);

        // `$C3A9` runs before Samus and room collision only while the shared Baby-enemy
        // index names a live slot. During the death animation the slot is still registered,
        // so zero health deletes every later ring immediately. `$CD02` eventually clears
        // that index while deleting the actor; a retained host object must not keep acting
        // like the now-free slot or phase-three rings would disappear before testing Samus.
        if (baby is { IsDeleted: false })
        {
            if (baby.Health == 0)
            {
                Deactivate(slot, clearGraphics: false);
                collision = MotherBrainOnionRingCollisionKind.DeletedAfterBabyDeath;
                return true;
            }

            if (StrictAxisOverlap(
                slot.XPosition,
                slot.YPosition,
                slot.XRadius,
                slot.YRadius,
                baby.XPosition,
                baby.YPosition,
                BabyMetroidCutsceneState.XHitboxRadius,
                BabyMetroidCutsceneState.YHitboxRadius))
            {
                PendingBabyCryCount = unchecked((ushort)(PendingBabyCryCount + 1));
                babyHit = baby.ApplyMotherBrainOnionRingHit();
                collision = MotherBrainOnionRingCollisionKind.BabyMetroid;
                Deactivate(slot, clearGraphics: true);
                return true;
            }
        }

        // The pre-instruction's Samus collision path ignores the generic projectile
        // invincibility gate and explicitly replaces the timer with `$60` after damage.
        if (StrictAxisOverlap(
            slot.XPosition,
            slot.YPosition,
            slot.XRadius,
            slot.YRadius,
            samus.XPosition,
            samus.YPosition,
            samus.Kinematics.XRadius,
            samus.Kinematics.YRadius))
        {
            ushort healthBefore = samus.Health;
            ushort damage = DivideDamageBySuit(0x0050, samus.EquippedItems);
            samus.Health = samus.Health < damage
                ? (ushort)0
                : unchecked((ushort)(samus.Health - damage));
            // Reuse the small health-transition carrier that the pre-instruction already
            // returns to its caller. The public event names the fields generically because
            // this branch's target is Samus, not the now-absent Baby slot.
            babyHit = new BabyMetroidOnionRingHitResult(
                Applied: true,
                HealthBefore: healthBefore,
                HealthAfter: samus.Health,
                FlashTimer: 0);
            SamusInvincibilityTimer = 0x0060;
            samus.KnockbackTimer = 5;
            samus.KnockbackXDirection = unchecked((short)(samus.XPosition - slot.XPosition)) >= 0
                ? (ushort)1
                : (ushort)0;
            collision = MotherBrainOnionRingCollisionKind.Samus;
            Deactivate(slot, clearGraphics: true);
            return true;
        }

        // `$C3C9` uses signed comparisons for the left bounds, literal room Y `$20-$D7`,
        // and a camera-relative right edge at `$F8`. This is not level-tile collision.
        short signedY = unchecked((short)slot.YPosition);
        short signedX = unchecked((short)slot.XPosition);
        short screenX = unchecked((short)(slot.XPosition - layer1X));
        if (signedY < 0x20 || slot.YPosition >= 0x00d8 || signedX < 0 || screenX < 0 || screenX >= 0x00f8)
        {
            EarthquakeType = 5;
            EarthquakeTimer = 10;
            collision = MotherBrainOnionRingCollisionKind.RoomBoundary;
            Deactivate(slot, clearGraphics: true);
            return true;
        }

        return false;
    }

    private static void RunInstructionHandler(
        ISnesAddressSpace bus,
        MotherBrainEnemyProjectileSlot slot)
    {
        ushort oldTimer = slot.InstructionTimer;
        slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
        if (oldTimer != 1)
            return;

        ushort pointer = slot.InstructionPointer;
        for (int operationCount = 0; operationCount < 16; operationCount++)
        {
            ushort durationOrOpcode = ReadWord(bus, 0x860000 | pointer);
            if ((durationOrOpcode & 0x8000) == 0)
            {
                if (durationOrOpcode == 0)
                    throw new InvalidDataException(
                        $"Mother Brain projectile frame at $86:{pointer:X4} has zero duration.");

                slot.InstructionTimer = durationOrOpcode;
                slot.SpritemapPointer = ReadWord(
                    bus,
                    0x860000 | unchecked((ushort)(pointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(pointer + 4));
                return;
            }

            switch (durationOrOpcode)
            {
                case SetXAndYRadiusInstruction:
                    // `$8298` reads the two one-byte arguments together as the packed
                    // low-X/high-Y radius word, then advances over both bytes.
                    slot.XRadius = bus.ReadByte(0x860000 | unchecked((ushort)(pointer + 2)));
                    slot.YRadius = bus.ReadByte(0x860000 | unchecked((ushort)(pointer + 3)));
                    pointer = unchecked((ushort)(pointer + 4));
                    break;

                case SleepInstruction:
                    // Sleep points back to itself and returns before loading a duration.
                    // The just-decremented timer remains zero; on the following call it
                    // underflows and will never again equal one without an external reset.
                    slot.InstructionPointer = pointer;
                    return;

                default:
                    throw new NotSupportedException(
                        $"Mother Brain projectile instruction $86:{durationOrOpcode:X4} at " +
                        $"$86:{pointer:X4} is not translated.");
            }
        }

        throw new InvalidDataException(
            "Mother Brain projectile instruction list did not reach a timed frame within 16 operations.");
    }

    private static void PinToBrain(
        MotherBrainEnemyProjectileSlot slot,
        MotherBrainRainbowBeamAttackSequence motherBrain)
    {
        slot.XPosition = unchecked((ushort)(motherBrain.BrainXPosition + 0x000a));
        slot.YPosition = unchecked((ushort)(motherBrain.BrainYPosition + 0x0010));
    }

    private static void Deactivate(
        MotherBrainEnemyProjectileSlot slot,
        bool clearGraphics)
    {
        // Contact explosion `$C410` clears the ID and graphics word but deliberately leaves
        // coordinates/velocity behind for the newly spawned dust projectile and debugger
        // inspection. `$C3C5`'s zero-health double-return clears only the ID.
        slot.ProjectileId = 0;
        if (clearGraphics)
            slot.GraphicsIndex = 0;
    }

    private static void MoveAccordingToVelocity(MotherBrainEnemyProjectileSlot slot)
    {
        slot.XPosition = AddNativeEightEightVelocity(
            slot.XPosition,
            slot.XSubposition,
            slot.XVelocity,
            out ushort xSubposition);
        slot.XSubposition = xSubposition;
        slot.YPosition = AddNativeEightEightVelocity(
            slot.YPosition,
            slot.YSubposition,
            slot.YVelocity,
            out ushort ySubposition);
        slot.YSubposition = ySubposition;
    }

    private static ushort AddNativeEightEightVelocity(
        ushort position,
        ushort subposition,
        ushort velocity,
        out ushort newSubposition)
    {
        // The common enemy-projectile mover forms a signed 16.16 delta from the 8.8
        // velocity and adds it to the split whole/subpixel position. The low byte of the
        // subposition remains below the translated precision and is preserved.
        int fractionalSum = (subposition >> 8) + (velocity & 0x00ff);
        newSubposition = unchecked((ushort)(
            ((byte)fractionalSum << 8) | (subposition & 0x00ff)));
        int wholeDelta = unchecked((sbyte)(velocity >> 8)) + (fractionalSum > 0xff ? 1 : 0);
        return unchecked((ushort)(position + wholeDelta));
    }

    private static ushort CalculateVelocityComponent(
        ISnesAddressSpace bus,
        ushort speed,
        byte sineIndex)
    {
        int address = SignedSineTable + sineIndex * 2;
        short sine = unchecked((short)ReadWord(bus, address));
        uint product = unchecked((uint)(speed * Math.Abs((int)sine)));
        ushort magnitude = unchecked((ushort)(product >> 8));
        return sine < 0 ? unchecked((ushort)-magnitude) : magnitude;
    }

    private static bool StrictAxisOverlap(
        ushort firstX,
        ushort firstY,
        ushort firstXRadius,
        ushort firstYRadius,
        ushort secondX,
        ushort secondY,
        ushort secondXRadius,
        ushort secondYRadius) =>
        Math.Abs(unchecked((short)(firstX - secondX))) < firstXRadius + secondXRadius &&
        Math.Abs(unchecked((short)(firstY - secondY))) < firstYRadius + secondYRadius;

    private static ushort DivideDamageBySuit(ushort damage, ushort equippedItems) =>
        (equippedItems & SamusLiquidPhysicsState.GravitySuitItem) != 0
            ? (ushort)(damage >> 2)
            : (equippedItems & 0x0001) != 0
                ? (ushort)(damage >> 1)
                : damage;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}

/// <summary>One semantic view over an enemy-projectile WRAM slot.</summary>
public sealed class MotherBrainEnemyProjectileSlot
{
    internal MotherBrainEnemyProjectileSlot(int index) => Index = index;

    public int Index { get; }
    public ushort ProjectileId { get; internal set; }

    /// <summary>
    /// Definition property word copied from bank $86. Bit $1000 selects the native
    /// high-priority draw pass; the remaining bits are retained so later translations do
    /// not have to reconstruct definition state from the host projectile type.
    /// </summary>
    public ushort Properties { get; internal set; }

    public ushort GraphicsIndex { get; internal set; }

    /// <summary>Initialization parameter retained for inspecting table-selected fragments.</summary>
    public ushort SpawnParameter { get; internal set; }

    /// <summary>Blue-ring head-follow delay; zero for the other translated definitions.</summary>
    public ushort DelayTimer { get; internal set; }

    /// <summary>Door-fragment Var0 countdown; unused by blue rings, bombs, and the subtitle.</summary>
    public ushort Lifetime { get; internal set; }

    /// <summary>
    /// Mother Brain bomb Var0: the absolute horizontal speed restored at each floor bounce.
    /// </summary>
    public ushort BounceHorizontalSpeed { get; internal set; }

    /// <summary>
    /// Mother Brain bomb Var1: an even byte offset into `$86:C550`'s acceleration table.
    /// Zero has the special pre-first-bounce friction path; `$12` selects the terminating zero.
    /// </summary>
    public ushort BounceTableOffset { get; internal set; }
    public byte Angle { get; internal set; }
    public ushort XPosition { get; internal set; }
    public ushort XSubposition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort YSubposition { get; internal set; }
    public ushort XVelocity { get; internal set; }
    public ushort YVelocity { get; internal set; }
    public ushort XRadius { get; internal set; }
    public ushort YRadius { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }

    public bool IsActive => ProjectileId != 0;

    internal void Clear()
    {
        ProjectileId = 0;
        Properties = 0;
        GraphicsIndex = 0;
        SpawnParameter = 0;
        DelayTimer = 0;
        Lifetime = 0;
        BounceHorizontalSpeed = 0;
        BounceTableOffset = 0;
        Angle = 0;
        XPosition = 0;
        XSubposition = 0;
        YPosition = 0;
        YSubposition = 0;
        XVelocity = 0;
        YVelocity = 0;
        XRadius = 0;
        YRadius = 0;
        InstructionPointer = 0;
        InstructionTimer = 0;
        SpritemapPointer = 0;
    }
}

/// <summary>Collision/deletion reason produced by one ring pre-instruction.</summary>
public enum MotherBrainOnionRingCollisionKind
{
    None,
    BabyMetroid,
    Samus,
    RoomBoundary,
    DeletedAfterBabyDeath,
}

/// <summary>Debugger witness for one ring collision.</summary>
public readonly record struct MotherBrainOnionRingEvent(
    int SlotIndex,
    MotherBrainOnionRingCollisionKind Collision,
    ushort XPosition,
    ushort YPosition,
    ushort TargetHealthBefore,
    ushort TargetHealthAfter);

/// <summary>Aggregate result of one native enemy-projectile pass.</summary>
public readonly record struct MotherBrainEnemyProjectileFrameResult(
    int ActiveCount,
    IReadOnlyList<MotherBrainOnionRingEvent> Events,
    IReadOnlyList<MotherBrainEscapeDoorParticleDustRequest> EscapeDoorDustRequests,
    IReadOnlyList<MotherBrainBombEvent> BombEvents);

/// <summary>Observable transition emitted by `$86:C4C8-C604`'s bomb pre-instruction.</summary>
public enum MotherBrainBombEventKind
{
    Bounced,
    DestroyedBySamusBomb,
    Expired,
}

/// <summary>
/// One debugger-visible Mother Brain bomb bounce or deletion and its external spawn/sound
/// requests. A normal movement call intentionally emits no event; its exact state remains on
/// the projectile slot for stepping and watch windows.
/// </summary>
public readonly record struct MotherBrainBombEvent(
    int SlotIndex,
    MotherBrainBombEventKind Kind,
    ushort XPosition,
    ushort YPosition,
    ushort BounceTableOffset,
    ushort? AfterburnCount,
    ushort DustParameter,
    bool EnemyDropRequested,
    ushort? QueuedSoundLibraryThree);

/// <summary>
/// Final parameter-nine misc-dust spawn produced when one `$86:CB21` fragment expires.
/// </summary>
public readonly record struct MotherBrainEscapeDoorParticleDustRequest(
    int SourceSlotIndex,
    ushort XPosition,
    ushort YPosition,
    ushort ProjectileParameter);
