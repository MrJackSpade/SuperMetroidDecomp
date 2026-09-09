using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored, mirrored eye-door PLM family.</summary>
public sealed partial class RoomPlmSystem
{
    private Bank80SystemState? _eyeDoorSystem;
    private Func<SamusState?>? _eyeDoorSamus;
    private Action<EyeDoorProjectileRequest>? _spawnEyeDoorProjectile;

    /// <summary>Debugger-visible projections of every live eye-door component.</summary>
    public IReadOnlyList<EyeDoorPlmSnapshot> EyeDoors => _slots
        .Where(slot => slot.Active && slot.EyeDoor is not null)
        .Select(slot => new EyeDoorPlmSnapshot(
            slot.HeaderPointer,
            slot.BlockIndex,
            slot.RoomArgument,
            slot.EyeDoor!.Component,
            slot.EyeDoor.Orientation,
            slot.EyeDoor.HitCounter,
            slot.InstructionPointer,
            slot.PreInstruction))
        .ToArray();

    private static bool IsEyeDoorHeader(ushort header) => header is
        RoomPlmHeaders.EyeDoorEyeFacingRight or
        RoomPlmHeaders.EyeDoorFacingRight or
        RoomPlmHeaders.EyeDoorBottomFacingRight or
        RoomPlmHeaders.EyeDoorEyeFacingLeft or
        RoomPlmHeaders.EyeDoorFacingLeft or
        RoomPlmHeaders.EyeDoorBottomFacingLeft;

    private void SetupEyeDoorSlot(
        RoomLevelData level,
        PlmSlot slot)
    {
        (EyeDoorComponent component, EyeDoorOrientation orientation) = slot.HeaderPointer switch
        {
            RoomPlmHeaders.EyeDoorEyeFacingRight =>
                (EyeDoorComponent.Eye, EyeDoorOrientation.Right),
            RoomPlmHeaders.EyeDoorFacingRight =>
                (EyeDoorComponent.Door, EyeDoorOrientation.Right),
            RoomPlmHeaders.EyeDoorBottomFacingRight =>
                (EyeDoorComponent.Bottom, EyeDoorOrientation.Right),
            RoomPlmHeaders.EyeDoorEyeFacingLeft =>
                (EyeDoorComponent.Eye, EyeDoorOrientation.Left),
            RoomPlmHeaders.EyeDoorFacingLeft =>
                (EyeDoorComponent.Door, EyeDoorOrientation.Left),
            RoomPlmHeaders.EyeDoorBottomFacingLeft =>
                (EyeDoorComponent.Bottom, EyeDoorOrientation.Left),
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.HeaderPointer, "Not an eye-door PLM header."),
        };
        slot.EyeDoor = new EyeDoorPlmState(component, orientation);

        Bank80SystemState system = _eyeDoorSystem ??
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
            WritePlmCollisionTypeAndBts(level, slot.BlockIndex, EyeDoorPlmRomData.EyeCollisionWord);
            WritePlmCollisionTypeAndBts(
                level,
                checked(slot.BlockIndex + level.WidthInBlocks),
                EyeDoorPlmRomData.EyeExtensionWord);
            return;
        }

        WritePlmCollisionTypeAndBts(level, slot.BlockIndex, EyeDoorPlmRomData.ClosedComponentWord);
    }

    /// <summary>
    /// Publishes the projectile word to the resident eye controller at a type-$C/BTS-$44
    /// block. The following PLM pre-instruction performs the missile-family filtering.
    /// </summary>
    private bool TryNotifyEyeDoorHit(
        int blockIndex,
        SamusProjectileTypeWord projectileType)
    {
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex ||
                slot.EyeDoor?.Component != EyeDoorComponent.Eye)
            {
                continue;
            }

            // PLM_Timers is a single pending word. A later collision before the handler
            // pass replaces it rather than accumulating a host-side queue.
            slot.LoopTimer = projectileType.Raw;
            slot.EyeDoor.HasPendingHit = true;
            return true;
        }
        return false;
    }

    private void RunEyeDoorPreInstruction(PlmSlot slot)
    {
        if (slot.EyeDoor is null || slot.PreInstruction == 0)
            return;

        Bank80SystemState system = _eyeDoorSystem ??
            throw new InvalidOperationException("A live eye door has no persistence owner.");
        switch (slot.PreInstruction)
        {
            case EyeDoorPlmRomData.WakeWhenDoorBitSetPreInstruction:
                if (unchecked((short)slot.RoomArgument) >= 0 &&
                    system.HasOpenedDoorBit(slot.RoomArgument))
                {
                    slot.PreInstruction = 0;
                    slot.InstructionPointer = slot.LinkInstruction;
                    slot.InstructionTimer = 1;
                }
                return;

            case EyeDoorPlmRomData.MissileHitPreInstruction:
                if (!slot.EyeDoor.HasPendingHit)
                    return;

                SamusProjectileFamily family = new SamusProjectileTypeWord(slot.LoopTimer).Family;
                slot.LoopTimer = 0;
                slot.EyeDoor.HasPendingHit = false;
                if (family == SamusProjectileFamily.SuperMissile)
                {
                    // $BD50 writes $77 so the next unsigned increment meets threshold three.
                    slot.EyeDoor.HitCounter = 0x77;
                }
                else if (family != SamusProjectileFamily.Missile)
                {
                    _soundRequests.Add(new PlmSoundRequest(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            EyeDoorPlmRomData.RejectedShotSound),
                        MaximumQueued: 6));
                    return;
                }

                slot.InstructionPointer = slot.LinkInstruction;
                slot.InstructionTimer = 1;
                return;

            default:
                throw new InvalidDataException(
                    $"Eye-door PLM reached untranslated pre-instruction $84:{slot.PreInstruction:X4}.");
        }
    }

    private bool TryExecuteEyeDoorInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        PlmSlot slot,
        ushort instruction)
    {
        EyeDoorPlmState? state = slot.EyeDoor;
        if (state is null)
            return false;

        switch (instruction)
        {
            case RoomPlmInstructionCodes.GotoIfDoorBitSet:
            {
                ushort destination = ReadBank84Word(
                    bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                bool opened = unchecked((short)slot.RoomArgument) >= 0 &&
                    (_eyeDoorSystem ?? throw new InvalidOperationException(
                        "Eye-door branch has no persistence owner."))
                    .HasOpenedDoorBit(slot.RoomArgument);
                slot.InstructionPointer = opened
                    ? destination
                    : unchecked((ushort)(slot.InstructionPointer + 4));
                return true;
            }

            case RoomPlmInstructionCodes.LinkInstruction:
                slot.LinkInstruction = ReadBank84Word(
                    bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                return true;

            case RoomPlmInstructionCodes.GotoIfSamusNear:
            {
                SamusState? samus = _eyeDoorSamus?.Invoke();
                if (samus is null)
                    throw new InvalidOperationException("Eye-door proximity bytecode requires Samus.");
                int plmX = slot.BlockIndex % level.WidthInBlocks;
                int plmY = slot.BlockIndex / level.WidthInBlocks;
                byte maxX = bus.ReadByte(Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                byte maxY = bus.ReadByte(Bank84(unchecked((ushort)(slot.InstructionPointer + 3))));
                ushort destination = ReadBank84Word(
                    bus, unchecked((ushort)(slot.InstructionPointer + 4)));
                int deltaX = Math.Abs((samus.XPosition >> 4) - plmX);
                int deltaY = Math.Abs((samus.YPosition >> 4) - plmY);
                slot.InstructionPointer = deltaX <= maxX && deltaY <= maxY
                    ? destination
                    : unchecked((ushort)(slot.InstructionPointer + 6));
                return true;
            }

            case RoomPlmInstructionCodes.IncrementDoorHitCounterAndGoto:
            {
                byte threshold = bus.ReadByte(
                    Bank84(unchecked((ushort)(slot.InstructionPointer + 2))));
                ushort destination = ReadBank84Word(
                    bus, unchecked((ushort)(slot.InstructionPointer + 3)));
                state.HitCounter = unchecked((byte)(state.HitCounter + 1));
                if (state.HitCounter < threshold)
                {
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 5));
                    return true;
                }

                if (unchecked((short)slot.RoomArgument) >= 0)
                {
                    Bank80SystemState system = _eyeDoorSystem ??
                        throw new InvalidOperationException("Eye-door hit counter has no persistence owner.");
                    int bitIndex = slot.RoomArgument;
                    system.SetOpenedDoorBit(bitIndex);
                    slot.RoomArgument = unchecked((ushort)(
                        0x8000 | system.GetOpenedDoorByteRaw(bitIndex >> 3)));
                }
                slot.PreInstruction = 0;
                slot.InstructionPointer = destination;
                return true;
            }

            case RoomPlmInstructionCodes.ShootEyeDoorProjectile:
                SpawnEyeDoorProjectile(
                    bus,
                    slot,
                    EyeDoorEnemyProjectileRomData.ProjectileDefinition,
                    hasParameter: true);
                _soundRequests.Add(new PlmSoundRequest(
                    SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        EyeDoorPlmRomData.ProjectileSound),
                    MaximumQueued: 6));
                return true;

            case RoomPlmInstructionCodes.SpawnEyeDoorSweat:
                SpawnEyeDoorProjectile(
                    bus,
                    slot,
                    EyeDoorEnemyProjectileRomData.SweatDefinition,
                    hasParameter: true);
                return true;

            case RoomPlmInstructionCodes.SpawnTwoEyeDoorSmoke:
                SpawnEyeDoorProjectile(
                    slot,
                    EyeDoorEnemyProjectileRomData.SmokeDefinition,
                    EyeDoorPlmRomData.RandomizedSmokeParameter);
                SpawnEyeDoorProjectile(
                    slot,
                    EyeDoorEnemyProjectileRomData.SmokeDefinition,
                    EyeDoorPlmRomData.RandomizedSmokeParameter);
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                return true;

            case RoomPlmInstructionCodes.SpawnEyeDoorSmoke:
                SpawnEyeDoorProjectile(
                    slot,
                    EyeDoorEnemyProjectileRomData.SmokeDefinition,
                    EyeDoorPlmRomData.CenteredSmokeParameter);
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                return true;

            case RoomPlmInstructionCodes.MoveUpAndMakeBlueDoorFacingRight:
                ConvertEyeToBlueDoor(level, slot, RoomBlockBehaviorValues.BlueDoorFacingRight);
                return true;

            case RoomPlmInstructionCodes.MoveUpAndMakeBlueDoorFacingLeft:
                ConvertEyeToBlueDoor(level, slot, RoomBlockBehaviorValues.BlueDoorFacingLeft);
                return true;

            default:
                return false;
        }
    }

    private void SpawnEyeDoorProjectile(
        ISnesAddressSpace bus,
        PlmSlot slot,
        ushort definitionPointer,
        bool hasParameter)
    {
        ushort parameter = hasParameter
            ? ReadBank84Word(bus, unchecked((ushort)(slot.InstructionPointer + 2)))
            : (ushort)0;
        SpawnEyeDoorProjectile(slot, definitionPointer, parameter);
        slot.InstructionPointer = unchecked((ushort)(
            slot.InstructionPointer + (hasParameter ? 4 : 2)));
    }

    private void SpawnEyeDoorProjectile(
        PlmSlot slot,
        ushort definitionPointer,
        ushort parameter)
    {
        Action<EyeDoorProjectileRequest> spawn = _spawnEyeDoorProjectile ??
            throw new InvalidOperationException("Eye-door bytecode has no bank-$86 spawn owner.");
        spawn(new EyeDoorProjectileRequest(
            definitionPointer,
            parameter,
            slot.BlockIndex,
            slot.RoomArgument));
    }

    private static void ConvertEyeToBlueDoor(
        RoomLevelData level,
        PlmSlot slot,
        RoomBlockBehavior facing)
    {
        slot.BlockIndex = checked(slot.BlockIndex - level.WidthInBlocks);
        WritePlmCollisionTypeAndBts(
            level,
            slot.BlockIndex,
            unchecked((ushort)(0xc000 | facing.Value)));
        for (int row = 1; row <= EyeDoorPlmRomData.BlueDoorExtensionCount; row++)
        {
            WritePlmCollisionTypeAndBts(
                level,
                checked(slot.BlockIndex + row * level.WidthInBlocks),
                unchecked((ushort)(0xd000 | unchecked((byte)-row))));
        }
        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
    }

    private void ResetEyeDoorState()
    {
        _eyeDoorSystem = null;
        _eyeDoorSamus = null;
        _spawnEyeDoorProjectile = null;
    }

    private sealed class EyeDoorPlmState(
        EyeDoorComponent component,
        EyeDoorOrientation orientation)
    {
        public EyeDoorComponent Component { get; } = component;
        public EyeDoorOrientation Orientation { get; } = orientation;
        public byte HitCounter { get; set; }
        public bool HasPendingHit { get; set; }
    }
}

/// <summary>Stable debugger view over one of the three physical eye-door PLMs.</summary>
public readonly record struct EyeDoorPlmSnapshot(
    ushort Header,
    int BlockIndex,
    ushort RoomArgument,
    EyeDoorComponent Component,
    EyeDoorOrientation Orientation,
    byte HitCounter,
    ushort InstructionPointer,
    ushort PreInstruction);
