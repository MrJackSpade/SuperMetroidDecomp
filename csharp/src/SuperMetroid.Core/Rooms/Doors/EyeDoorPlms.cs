using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Eye-door PLMs (Tourian and Kraid lair): eye projectile spawning, hit reaction and conversion to a blue door.</summary>
internal sealed class EyeDoorPlms
{
    /// <summary>Supplies Samus, persistence bits, and enemy projectile spawning to eye-door bytecode.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Queues rejected-shot and launched-projectile sounds.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds eye-door PLM handling to room services and sound requests.</summary>
    internal EyeDoorPlms(PlmRoomBindings bindings, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _sounds = sounds;
    }

    /// <summary>Recognizes the eye, middle, and bottom headers for either door orientation.</summary>
    internal static bool IsEyeDoorHeader(PlmHeaderId header) => header is
        PlmHeaderId.EyeDoorEyeFacingRight or
        PlmHeaderId.EyeDoorFacingRight or
        PlmHeaderId.EyeDoorBottomFacingRight or
        PlmHeaderId.EyeDoorEyeFacingLeft or
        PlmHeaderId.EyeDoorFacingLeft or
        PlmHeaderId.EyeDoorBottomFacingLeft;

    /// <summary>Initializes a door component and its closed collision blocks unless its persistent bit is set.</summary>
    internal void SetupEyeDoorSlot(
        RoomLevelData level,
        PlmSlot slot)
    {
        (EyeDoorComponent component, EyeDoorOrientation orientation) = slot.HeaderPointer switch
        {
            PlmHeaderId.EyeDoorEyeFacingRight =>
                (EyeDoorComponent.Eye, EyeDoorOrientation.Right),
            PlmHeaderId.EyeDoorFacingRight =>
                (EyeDoorComponent.Door, EyeDoorOrientation.Right),
            PlmHeaderId.EyeDoorBottomFacingRight =>
                (EyeDoorComponent.Bottom, EyeDoorOrientation.Right),
            PlmHeaderId.EyeDoorEyeFacingLeft =>
                (EyeDoorComponent.Eye, EyeDoorOrientation.Left),
            PlmHeaderId.EyeDoorFacingLeft =>
                (EyeDoorComponent.Door, EyeDoorOrientation.Left),
            PlmHeaderId.EyeDoorBottomFacingLeft =>
                (EyeDoorComponent.Bottom, EyeDoorOrientation.Left),
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.HeaderPointer, "Not an eye-door PLM header."),
        };
        slot.EyeDoor = new EyeDoorPlmState(component);
        slot.StartAnimation(new EyeDoorPlmAnimation(orientation, component,
            component == EyeDoorComponent.Eye ? EyeDoorPlmProgram.EyeStart : EyeDoorPlmProgram.ComponentStart));

        Bank80SystemState system = _bindings.System ??
            throw new InvalidOperationException("Eye-door setup has no persistence owner.");
        if (unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasOpenedDoorBit(slot.RoomArgument))
        {
            // Setup routines deliberately leave already-open terrain untouched. Their first
            // bytecode instruction takes the persistent branch and either deletes this
            // passive component or constructs the ordinary blue-door cap from the eye.
            return;
        }

        if (component == EyeDoorComponent.Eye)
        {
            RoomPlmSystem.WritePlmCollisionTypeAndBts(level, slot.BlockIndex, EyeDoorPlmRomData.EyeCollisionWord);
            RoomPlmSystem.WritePlmCollisionTypeAndBts(
                level,
                checked(slot.BlockIndex + level.Size.X),
                EyeDoorPlmRomData.EyeExtensionWord);
            return;
        }

        RoomPlmSystem.WritePlmCollisionTypeAndBts(level, slot.BlockIndex, EyeDoorPlmRomData.ClosedComponentWord);
    }

    /// <summary>Wakes opened components or consumes a pending missile hit before instruction dispatch.</summary>
    internal void RunEyeDoorPreInstruction(PlmSlot slot)
    {
        if (slot.EyeDoor is null || slot.PreInstruction == 0)
            return;

        Bank80SystemState system = _bindings.System ??
            throw new InvalidOperationException("A live eye door has no persistence owner.");
        EyeDoorPreInstruction preInstruction =
            ClosedNativeWords.Decode<EyeDoorPreInstruction>(slot.PreInstruction, "eye-door PLM pre-instruction");
        switch (preInstruction)
        {
            case EyeDoorPreInstruction.WakeWhenDoorBitSet:
                if (unchecked((short)slot.RoomArgument) >= 0 &&
                    system.HasOpenedDoorBit(slot.RoomArgument))
                {
                    slot.PreInstruction = 0;
                    slot.WakeAtLink();
                }
                return;

            case EyeDoorPreInstruction.MissileHit:
                if (!slot.EyeDoor.HasPendingHit)
                    return;

                SamusProjectileFamily family = new SamusProjectileTypeWord(slot.LoopTimer).Family;
                slot.LoopTimer = 0;
                slot.EyeDoor.HasPendingHit = false;
                if (family == SamusProjectileFamily.SuperMissile)
                {
                    // $BD50 seeds the counter so the next increment meets the threshold.
                    slot.EyeDoor.HitCounter = EyeDoorPlmProgramDefinitions.SuperMissileHitCounter;
                }
                else if (family != SamusProjectileFamily.Missile)
                {
                    _sounds._soundRequests.Add(_sounds.CreateSoundRequest(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            EyeDoorPlmRomData.RejectedShotSound),
                        MaximumQueued: 6));
                    return;
                }

                slot.WakeAtLink();
                return;

            default:
                throw new InvalidOperationException($"Undefined EyeDoorPreInstruction {preInstruction}.");
        }
    }

    /// <summary><c>$84:8D41</c>: whether Samus stands within <paramref name="columns"/> blocks across and <paramref name="rows"/> blocks up or down of the PLM.</summary>
    internal bool IsSamusNear(RoomLevelData level, PlmSlot slot, byte columns, byte rows)
    {
        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException("Eye-door proximity requires Samus.");
        int plmX = slot.BlockIndex % level.Size.X;
        int plmY = slot.BlockIndex / level.Size.X;
        int deltaX = Math.Abs((samus.Position.X.Whole >> 4) - plmX);
        int deltaY = Math.Abs((samus.Position.Y.Whole >> 4) - plmY);
        return deltaX <= columns && deltaY <= rows;
    }

    /// <summary><c>$84:D77A</c>: fires an eye-door projectile and queues its sound.</summary>
    internal void Shoot(PlmSlot slot, ushort parameter)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Projectile, parameter);
        _sounds._soundRequests.Add(_sounds.CreateSoundRequest(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EyeDoorPlmRomData.ProjectileSound),
            MaximumQueued: 6));
    }

    /// <summary><c>$84:D790</c>: the eye sweats.</summary>
    internal void SpawnSweat(PlmSlot slot, ushort parameter) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Sweat, parameter);

    /// <summary><c>$84:D79F</c>: two randomly placed smoke puffs.</summary>
    internal void SpawnTwoSmoke(PlmSlot slot)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Smoke, EyeDoorPlmRomData.RandomizedSmokeParameter);
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Smoke, EyeDoorPlmRomData.RandomizedSmokeParameter);
    }

    /// <summary><c>$84:D7B6</c>: one centred smoke puff.</summary>
    internal void SpawnSmoke(PlmSlot slot) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Smoke, EyeDoorPlmRomData.CenteredSmokeParameter);

    /// <summary><c>$84:8A91</c>: counts a hit on the eye; true when it opens the door.</summary>
    internal static bool CountEyeHit(PlmSlot slot, DoorPlms doors)
    {
        EyeDoorPlmState eye = slot.EyeDoor
            ?? throw new InvalidOperationException("An eye's hit reached a slot without its eye-door state.");
        eye.HitCounter = unchecked((byte)(eye.HitCounter + 1));
        if (eye.HitCounter < EyeDoorPlmProgramDefinitions.HitThreshold)
            return false;
        doors.MarkDoorOpened(slot);
        return true;
    }

    /// <summary>
    /// <c>$84:D7C3</c> (right) / <c>$84:D7DA</c> (left): moves the eye PLM up one block and writes the blue cap facing
    /// <paramref name="orientation"/> with its extension rows.
    /// </summary>
    internal static void ConvertEyeToBlueDoor(RoomLevelData level, PlmSlot slot, EyeDoorOrientation orientation)
    {
        RoomBlockBehavior facing = orientation switch
        {
            EyeDoorOrientation.Left => RoomBlockBehaviorValues.BlueDoorFacingLeft,
            EyeDoorOrientation.Right => RoomBlockBehaviorValues.BlueDoorFacingRight,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        slot.BlockIndex = checked(slot.BlockIndex - level.Size.X);
        RoomPlmSystem.WritePlmCollisionTypeAndBts(
            level,
            slot.BlockIndex,
            unchecked((ushort)(0xc000 | facing.Value)));
        for (int row = 1; row <= EyeDoorPlmRomData.BlueDoorExtensionCount; row++)
        {
            RoomPlmSystem.WritePlmCollisionTypeAndBts(
                level,
                checked(slot.BlockIndex + row * level.Size.X),
                unchecked((ushort)(0xd000 | unchecked((byte)-row))));
        }
    }

    /// <summary>Sends an eye-door projectile request with the source block and room argument to enemy services.</summary>
    private void SpawnEyeDoorProjectile(
        PlmSlot slot,
        EyeDoorProjectileDefinition definitionPointer,
        ushort parameter)
    {
        IEyeDoorProjectileSpawner enemies = _bindings.Enemies ??
            throw new InvalidOperationException("Eye-door bytecode has no bank-$86 spawn owner.");
        enemies.SpawnEyeDoorProjectile(new EyeDoorProjectileRequest(
            definitionPointer,
            parameter,
            slot.BlockIndex,
            slot.RoomArgument));
    }

    /// <summary>Per-component hit state retained across eye-door instruction frames.</summary>
    internal sealed class EyeDoorPlmState(EyeDoorComponent component)
    {
        /// <summary>Identifies whether this slot is the eye, middle, or bottom component.</summary>
        public EyeDoorComponent Component { get; } = component;
        /// <summary>Missile-hit count used by the door's threshold instruction.</summary>
        public byte HitCounter { get; set; }
        /// <summary>Whether a projectile collision awaits the eye's missile pre-instruction.</summary>
        public bool HasPendingHit { get; set; }
    }
}
