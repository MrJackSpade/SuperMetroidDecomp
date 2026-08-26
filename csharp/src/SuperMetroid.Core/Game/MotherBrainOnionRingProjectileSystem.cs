using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The eighteen bank-$86 enemy-projectile slots as used by Mother Brain's blue rings.
/// </summary>
/// <remarks>
/// This is intentionally a projectile system instead of a timer hidden in the boss actor.
/// Retail code spawns definition <c>$86:CB4B</c> into the highest free projectile slot,
/// then the gameplay loop runs every occupied slot from high to low after all enemy AI.
/// A ring spawned by Mother Brain's head therefore receives its first delayed pre-instruction
/// and its first animation-list frame on that same gameplay frame.
/// </remarks>
public sealed class MotherBrainOnionRingProjectileSystem
{
    /// <summary>Physical enemy-projectile capacity at WRAM <c>$1997-$19B9</c>.</summary>
    public const int SlotCount = 18;

    /// <summary>Projectile definition pointer used by head opcode <c>$A9:9E29</c>.</summary>
    public const ushort ProjectileDefinition = 0xcb4b;

    /// <summary>Initial animation-list pointer stored by definition <c>$86:CB4B</c>.</summary>
    public const ushort InitialInstructionList = 0xc432;

    private const int SignedSineTable = 0xa0b443;
    private const ushort SetXAndYRadiusInstruction = 0x8298;
    private const ushort SleepInstruction = 0x8159;
    private readonly MotherBrainOnionRingProjectileSlot[] _slots =
        Enumerable.Range(0, SlotCount)
            .Select(index => new MotherBrainOnionRingProjectileSlot(index))
            .ToArray();

    /// <summary>Slots in ascending WRAM order; native processing visits them in reverse.</summary>
    public IReadOnlyList<MotherBrainOnionRingProjectileSlot> Slots => _slots;

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

        MotherBrainOnionRingProjectileSlot slot = _slots[slotIndex];
        slot.Clear();
        slot.ProjectileId = ProjectileDefinition;
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
    /// Runs <c>$86:8104-$8160</c>'s blue-ring subset for one gameplay frame.
    /// </summary>
    public MotherBrainOnionRingFrameResult StepFrame(
        ISnesAddressSpace bus,
        MotherBrainRainbowBeamAttackSequence motherBrain,
        BabyMetroidCutsceneState? baby,
        SamusState samus,
        ushort layer1X = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(motherBrain);
        ArgumentNullException.ThrowIfNull(samus);

        if (SamusInvincibilityTimer != 0)
            SamusInvincibilityTimer = unchecked((ushort)(SamusInvincibilityTimer - 1));
        if (EarthquakeTimer != 0)
            EarthquakeTimer = unchecked((ushort)(EarthquakeTimer - 1));

        var events = new List<MotherBrainOnionRingEvent>();

        // EprojRunAll scans physical byte indices `$22,$20,...,$00`. Keeping this order
        // matters when several rings overlap the Baby on the same frame: each live slot
        // applies its own `$50` subtraction before a later slot observes zero health.
        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            MotherBrainOnionRingProjectileSlot slot = _slots[slotIndex];
            if (!slot.IsActive)
                continue;

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
        return new MotherBrainOnionRingFrameResult(activeCount, events.ToArray());
    }

    /// <summary>Consumes all pending Baby cry requests like <c>$A9:C7B7</c>.</summary>
    public ushort ConsumePendingBabyCries()
    {
        ushort count = PendingBabyCryCount;
        PendingBabyCryCount = 0;
        return count;
    }

    private bool RunPreInstruction(
        MotherBrainOnionRingProjectileSlot slot,
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
        MotherBrainOnionRingProjectileSlot slot)
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
                    throw new InvalidDataException($"Blue-ring frame at $86:{pointer:X4} has zero duration.");

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
                        $"Blue-ring instruction $86:{durationOrOpcode:X4} at $86:{pointer:X4} is not translated.");
            }
        }

        throw new InvalidDataException("Blue-ring instruction list did not reach a timed frame within 16 operations.");
    }

    private static void PinToBrain(
        MotherBrainOnionRingProjectileSlot slot,
        MotherBrainRainbowBeamAttackSequence motherBrain)
    {
        slot.XPosition = unchecked((ushort)(motherBrain.BrainXPosition + 0x000a));
        slot.YPosition = unchecked((ushort)(motherBrain.BrainYPosition + 0x0010));
    }

    private static void Deactivate(
        MotherBrainOnionRingProjectileSlot slot,
        bool clearGraphics)
    {
        // Contact explosion `$C410` clears the ID and graphics word but deliberately leaves
        // coordinates/velocity behind for the newly spawned dust projectile and debugger
        // inspection. `$C3C5`'s zero-health double-return clears only the ID.
        slot.ProjectileId = 0;
        if (clearGraphics)
            slot.GraphicsIndex = 0;
    }

    private static void MoveAccordingToVelocity(MotherBrainOnionRingProjectileSlot slot)
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
public sealed class MotherBrainOnionRingProjectileSlot
{
    internal MotherBrainOnionRingProjectileSlot(int index) => Index = index;

    public int Index { get; }
    public ushort ProjectileId { get; internal set; }
    public ushort GraphicsIndex { get; internal set; }
    public ushort DelayTimer { get; internal set; }
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
        GraphicsIndex = 0;
        DelayTimer = 0;
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
public readonly record struct MotherBrainOnionRingFrameResult(
    int ActiveCount,
    IReadOnlyList<MotherBrainOnionRingEvent> Events);
