using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Eye-door PLMs (Tourian and Kraid lair): eye projectile spawning, hit reaction and conversion to a blue door.</summary>
internal sealed class EyeDoorPlms
{
    private readonly PlmRoomBindings _bindings;
    private readonly PlmSoundQueue _sounds;

    internal EyeDoorPlms(PlmRoomBindings bindings, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _sounds = sounds;
    }

    internal static bool IsEyeDoorHeader(PlmHeaderId header) => header is
        PlmHeaderId.EyeDoorEyeFacingRight or
        PlmHeaderId.EyeDoorFacingRight or
        PlmHeaderId.EyeDoorBottomFacingRight or
        PlmHeaderId.EyeDoorEyeFacingLeft or
        PlmHeaderId.EyeDoorFacingLeft or
        PlmHeaderId.EyeDoorBottomFacingLeft;

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
                    slot.InstructionPointer = slot.LinkInstruction;
                    slot.InstructionTimer = 1;
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
                    // $BD50 writes $77 so the next unsigned increment meets threshold three.
                    slot.EyeDoor.HitCounter = 0x77;
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

                slot.InstructionPointer = slot.LinkInstruction;
                slot.InstructionTimer = 1;
                return;

            default:
                throw new InvalidOperationException($"Undefined EyeDoorPreInstruction {preInstruction}.");
        }
    }

    internal bool TryExecuteEyeDoorInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        PlmSlot slot,
        RoomPlmInstruction instruction)
    {
        EyeDoorPlmState? state = slot.EyeDoor;
        if (state is null)
            return false;

        if (instruction is not (
            RoomPlmInstruction.GotoIfDoorBitSet or
            RoomPlmInstruction.LinkInstruction or
            RoomPlmInstruction.GotoIfSamusNear or
            RoomPlmInstruction.IncrementDoorHitCounterAndGoto or
            RoomPlmInstruction.ShootEyeDoorProjectile or
            RoomPlmInstruction.SpawnEyeDoorSweat or
            RoomPlmInstruction.SpawnTwoEyeDoorSmoke or
            RoomPlmInstruction.SpawnEyeDoorSmoke or
            RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight or
            RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft))
            return false;

        switch (instruction)
        {
            case RoomPlmInstruction.GotoIfDoorBitSet:
            {
                ushort destination = RoomPlmSystem.ReadProgramWord(
                    bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                bool opened = unchecked((short)slot.RoomArgument) >= 0 &&
                    (_bindings.System ?? throw new InvalidOperationException(
                        "Eye-door branch has no persistence owner."))
                    .HasOpenedDoorBit(slot.RoomArgument);
                slot.InstructionPointer = opened
                    ? destination
                    : unchecked((ushort)(slot.InstructionPointer + 4));
                return true;
            }

            case RoomPlmInstruction.LinkInstruction:
                slot.LinkInstruction = RoomPlmSystem.ReadProgramWord(
                    bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                return true;

            case RoomPlmInstruction.GotoIfSamusNear:
            {
                SamusState? samus = _bindings.Samus?.Current;
                if (samus is null)
                    throw new InvalidOperationException("Eye-door proximity bytecode requires Samus.");
                int plmX = slot.BlockIndex % level.Size.X;
                int plmY = slot.BlockIndex / level.Size.X;
                byte maxX = RoomPlmSystem.ReadProgramByte(bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                byte maxY = RoomPlmSystem.ReadProgramByte(bus, unchecked((ushort)(slot.InstructionPointer + 3)));
                ushort destination = RoomPlmSystem.ReadProgramWord(
                    bus, unchecked((ushort)(slot.InstructionPointer + 4)));
                int deltaX = Math.Abs((samus.Position.X.Whole >> 4) - plmX);
                int deltaY = Math.Abs((samus.Position.Y.Whole >> 4) - plmY);
                slot.InstructionPointer = deltaX <= maxX && deltaY <= maxY
                    ? destination
                    : unchecked((ushort)(slot.InstructionPointer + 6));
                return true;
            }

            case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
            {
                byte threshold = RoomPlmSystem.ReadProgramByte(
                    bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                ushort destination = RoomPlmSystem.ReadProgramWord(
                    bus, unchecked((ushort)(slot.InstructionPointer + 3)));
                state.HitCounter = unchecked((byte)(state.HitCounter + 1));
                if (state.HitCounter < threshold)
                {
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 5));
                    return true;
                }

                if (unchecked((short)slot.RoomArgument) >= 0)
                {
                    Bank80SystemState system = _bindings.System ??
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

            case RoomPlmInstruction.ShootEyeDoorProjectile:
                SpawnEyeDoorProjectile(
                    bus,
                    slot,
                    EyeDoorProjectileDefinition.Projectile,
                    hasParameter: true);
                _sounds._soundRequests.Add(_sounds.CreateSoundRequest(
                    SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        EyeDoorPlmRomData.ProjectileSound),
                    MaximumQueued: 6));
                return true;

            case RoomPlmInstruction.SpawnEyeDoorSweat:
                SpawnEyeDoorProjectile(
                    bus,
                    slot,
                    EyeDoorProjectileDefinition.Sweat,
                    hasParameter: true);
                return true;

            case RoomPlmInstruction.SpawnTwoEyeDoorSmoke:
                SpawnEyeDoorProjectile(
                    slot,
                    EyeDoorProjectileDefinition.Smoke,
                    EyeDoorPlmRomData.RandomizedSmokeParameter);
                SpawnEyeDoorProjectile(
                    slot,
                    EyeDoorProjectileDefinition.Smoke,
                    EyeDoorPlmRomData.RandomizedSmokeParameter);
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                return true;

            case RoomPlmInstruction.SpawnEyeDoorSmoke:
                SpawnEyeDoorProjectile(
                    slot,
                    EyeDoorProjectileDefinition.Smoke,
                    EyeDoorPlmRomData.CenteredSmokeParameter);
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                return true;

            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight:
                ConvertEyeToBlueDoor(level, slot, RoomBlockBehaviorValues.BlueDoorFacingRight);
                return true;

            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft:
                ConvertEyeToBlueDoor(level, slot, RoomBlockBehaviorValues.BlueDoorFacingLeft);
                return true;

            default:
                throw new InvalidOperationException($"Unhandled RoomPlmInstruction {instruction}.");
        }
    }

    private void SpawnEyeDoorProjectile(
        ISnesAddressSpace bus,
        PlmSlot slot,
        EyeDoorProjectileDefinition definitionPointer,
        bool hasParameter)
    {
        ushort parameter = hasParameter
            ? RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(slot.InstructionPointer + 2)))
            : (ushort)0;
        SpawnEyeDoorProjectile(slot, definitionPointer, parameter);
        slot.InstructionPointer = unchecked((ushort)(
            slot.InstructionPointer + (hasParameter ? 4 : 2)));
    }

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

    private static void ConvertEyeToBlueDoor(
        RoomLevelData level,
        PlmSlot slot,
        RoomBlockBehavior facing)
    {
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
        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
    }

    internal sealed class EyeDoorPlmState(EyeDoorComponent component)
    {
        public EyeDoorComponent Component { get; } = component;
        public byte HitCounter { get; set; }
        public bool HasPendingHit { get; set; }
    }
}
