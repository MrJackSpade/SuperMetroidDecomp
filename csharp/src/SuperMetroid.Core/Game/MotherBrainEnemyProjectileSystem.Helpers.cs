using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Enemy-projectile drawing, instruction dispatch, motion, collision, and damage helpers.
/// </summary>
public sealed partial class MotherBrainEnemyProjectileSystem
{
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
                    throw new InvalidDataException(
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
                if (samusBomb.PackedType.Family != SamusProjectileFamily.Bomb ||
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
                throw new InvalidDataException(
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
                throw new InvalidDataException(
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
                    slot.XRadius = bus.ReadByte(
                        (int)new SnesAddress(0x86, unchecked((ushort)(pointer + 2))));
                    slot.YRadius = bus.ReadByte(
                        (int)new SnesAddress(0x86, unchecked((ushort)(pointer + 3))));
                    pointer = unchecked((ushort)(pointer + 4));
                    break;

                case SleepInstruction:
                    // Sleep points back to itself and returns before loading a duration.
                    // The just-decremented timer remains zero; on the following call it
                    // underflows and will never again equal one without an external reset.
                    slot.InstructionPointer = pointer;
                    return;

                default:
                    throw new InvalidDataException(
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
        ushort speed,
        byte sineIndex)
    {
        return EnemyTrigonometryTables.MultiplySignedSine(speed, sineIndex);
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
        equippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? (ushort)(damage >> 2)
            : (equippedItems & 0x0001) != 0
                ? (ushort)(damage >> 1)
                : damage;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
