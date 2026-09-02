using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored door-cap reactions owned by bank $84's PLM pool.</summary>
public sealed partial class RoomPlmSystem
{
    private const ushort YellowDoorFacingLeftHeader = 0xc85a;
    private const ushort YellowDoorFacingRightHeader = 0xc860;
    private const ushort YellowDoorFacingUpHeader = 0xc866;
    private const ushort YellowDoorFacingDownHeader = 0xc86c;
    private const ushort GreenDoorFacingLeftHeader = 0xc872;
    private const ushort GreenDoorFacingRightHeader = 0xc878;
    private const ushort GreenDoorFacingUpHeader = 0xc87e;
    private const ushort GreenDoorFacingDownHeader = 0xc884;
    private const ushort RedDoorFacingLeftHeader = 0xc88a;
    private const ushort RedDoorFacingRightHeader = 0xc890;
    private const ushort RedDoorFacingUpHeader = 0xc896;
    private const ushort RedDoorFacingDownHeader = 0xc89c;

    private const byte BlueDoorFacingLeftBts = 0x40;
    private const byte BlueDoorFacingRightBts = 0x41;
    private const byte BlueDoorFacingUpBts = 0x42;
    private const byte BlueDoorFacingDownBts = 0x43;

    private Bank80SystemState? _coloredDoorSystem;

    /// <summary>Every resident colored-door actor in the shared native PLM pool.</summary>
    public IReadOnlyList<ColoredDoorPlmSnapshot> ColoredDoors => _slots
        .Where(slot => slot.Active && slot.ColoredDoor is not null)
        .Select(slot => new ColoredDoorPlmSnapshot(
            slot.HeaderPointer,
            slot.BlockIndex,
            slot.RoomArgument,
            slot.ColoredDoor!.Color,
            slot.ColoredDoor.Orientation,
            slot.ColoredDoor.Phase,
            slot.ColoredDoor.HitCounter))
        .ToArray();

    /// <summary>
    /// Publishes the projectile word observed by a resident type-$C/BTS-$44 door. The
    /// resident actor, not the collision table, decides whether that family is accepted.
    /// </summary>
    public bool TryNotifyColoredDoorHit(int blockIndex, ushort projectileType)
    {
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.ColoredDoor is null)
                continue;

            // Once the threshold branch has selected the opening list, native clears the
            // pre-instruction pointer. Later projectiles therefore cannot enqueue another
            // family check or restart the animation while the cap is disappearing.
            if (slot.ColoredDoor.Phase == ColoredDoorPhase.Opening)
                return false;

            slot.ColoredDoor.PendingProjectileType = projectileType;
            slot.ColoredDoor.HasPendingHit = true;
            return true;
        }
        // Keep the established public name for projectile/bomb callers, but route the
        // same generic shot-trigger collision to grey doors. In native code BTS $44 finds
        // the resident PLM by block index; it does not distinguish the door's color here.
        return TryNotifyGreyDoorHit(blockIndex, projectileType);
    }

    private static void ApplyColoredDoorSetup(RoomLevelData level, int blockIndex)
    {
        // Setup `$84:C7B1` preserves the visual twelve bits, installs shootable-solid
        // collision, and selects BTS $44 so subsequent projectiles find this resident PLM.
        ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetForegroundEntry(blockIndex, unchecked((ushort)((originalWord & 0x0fff) | 0xc000)));
        level.SetBehavior(blockIndex, 0x44);
    }

    /// <summary>
    /// Executes the colored-door header setup against the slot already allocated by the
    /// sequential room-population loader. Allocation deliberately does not live here:
    /// native <c>Spawn_Room_PLM</c> chooses the physical ID before calling this routine.
    /// </summary>
    private void SetupColoredDoorSlot(
        ISnesAddressSpace bus,
        RoomLevelData level,
        Bank80SystemState system,
        PlmSlot slot,
        ColoredDoorColor color,
        ColoredDoorOrientation orientation)
    {
        ApplyColoredDoorSetup(level, slot.BlockIndex);

        ushort initialList = slot.InstructionPointer;
        ushort closedBlueList = ReadBank84Word(bus, unchecked((ushort)(initialList + 2)));
        ushort hitList = ReadBank84Word(bus, unchecked((ushort)(initialList + 6)));
        byte hitThreshold = bus.ReadByte(0x840000 | unchecked((ushort)(hitList + 2)));
        ushort openingList = ReadBank84Word(bus, unchecked((ushort)(hitList + 3)));
        ushort coloredClosedDraw = ReadBank84Word(bus, unchecked((ushort)(initialList + 14)));
        bool wasOpened = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasOpenedDoorBit(slot.RoomArgument);

        slot.ColoredDoor = new ColoredDoorPlmState(
            color,
            orientation,
            closedBlueList,
            hitList,
            openingList,
            coloredClosedDraw,
            hitThreshold,
            wasOpened ? ColoredDoorPhase.ConvertToBlue : ColoredDoorPhase.Waiting);
    }

    private bool TryStepColoredDoor(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        ColoredDoorPlmState? door = slot.ColoredDoor;
        if (door is null)
            return false;

        if (door.Phase == ColoredDoorPhase.ConvertToBlue)
        {
            // The initial Goto_if_room_argument_door_is_set branch selects a short list:
            // PLM_BTS_Y, one timed closed-blue draw, then delete. Execute its observable
            // setup/draw atomically here while retaining the cartridge pointers and art.
            byte blueBts = unchecked((byte)(BlueDoorFacingLeftBts + (byte)door.Orientation));
            ushort drawPointer = ReadBank84Word(
                bus,
                unchecked((ushort)(door.ClosedBlueList + 5)));
            level.SetBehavior(slot.BlockIndex, blueBts);
            DrawRomInstruction(
                bus,
                level,
                streamer,
                slot.BlockIndex,
                drawPointer,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset);
            slot.Active = false;
            slot.ColoredDoor = null;
            return true;
        }

        if (!door.InitialDrawCompleted)
        {
            DrawRomInstruction(
                bus,
                level,
                streamer,
                slot.BlockIndex,
                door.ColoredClosedDraw,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset);
            door.InitialDrawCompleted = true;
        }

        if (!door.HasPendingHit)
            return door.Phase == ColoredDoorPhase.Waiting;

        ushort projectileFamily = unchecked((ushort)(door.PendingProjectileType & 0x0f00));
        door.HasPendingHit = false;
        bool accepted = door.Color switch
        {
            ColoredDoorColor.Yellow => projectileFamily == 0x0300,
            ColoredDoorColor.Green => projectileFamily == 0x0200,
            ColoredDoorColor.Red => projectileFamily is 0x0100 or 0x0200,
            _ => throw new InvalidDataException($"Unknown colored-door family {door.Color}."),
        };
        if (!accepted)
        {
            // Each native pre-instruction queues library-two sound $57 for a rejected
            // projectile. A flash already in progress is not interrupted by that dud.
            _soundRequests.Add(new PlmSoundRequest(2, 0x57, MaximumQueued: 6));
            return door.Phase == ColoredDoorPhase.Waiting;
        }

        // Red-door setup writes $77 before the shared INC instruction when struck by a
        // Super Missile. Unsigned wrap therefore guarantees the very next increment meets
        // the five-hit threshold, exactly reproducing the cartridge's one-super behavior.
        if (door.Color == ColoredDoorColor.Red && projectileFamily == 0x0200)
            door.HitCounter = 0x77;
        door.HitCounter = unchecked((byte)(door.HitCounter + 1));

        if (door.HitCounter >= door.HitThreshold)
        {
            if (unchecked((short)slot.RoomArgument) >= 0)
            {
                (_coloredDoorSystem ?? throw new InvalidOperationException(
                    "A resident colored door has no persistence owner."))
                    .SetOpenedDoorBit(slot.RoomArgument);
                slot.RoomArgument |= 0x8000;
            }

            door.Phase = ColoredDoorPhase.Opening;
            slot.PreInstruction = 0;
            slot.InstructionPointer = door.OpeningList;
            slot.InstructionTimer = 1;
            return false;
        }

        // The five bytes after INC/threshold/open-target are the first operation in the
        // cartridge's nonfatal blue/colored flash list. Later Goto returns to Sleep, where
        // the common interpreter hands the actor back to Waiting.
        door.Phase = ColoredDoorPhase.Flashing;
        slot.InstructionPointer = unchecked((ushort)(door.HitList + 5));
        slot.InstructionTimer = 1;
        return false;
    }

    private static bool IsColoredDoorHeader(ushort header) => header is
        YellowDoorFacingLeftHeader or YellowDoorFacingRightHeader or
        YellowDoorFacingUpHeader or YellowDoorFacingDownHeader or
        GreenDoorFacingLeftHeader or GreenDoorFacingRightHeader or
        GreenDoorFacingUpHeader or GreenDoorFacingDownHeader or
        RedDoorFacingLeftHeader or RedDoorFacingRightHeader or
        RedDoorFacingUpHeader or RedDoorFacingDownHeader;

    private static bool TryIdentifyColoredDoor(
        ushort header,
        out ColoredDoorColor color,
        out ColoredDoorOrientation orientation)
    {
        ushort firstHeader;
        if (header is >= YellowDoorFacingLeftHeader and <= YellowDoorFacingDownHeader)
        {
            color = ColoredDoorColor.Yellow;
            firstHeader = YellowDoorFacingLeftHeader;
        }
        else if (header is >= GreenDoorFacingLeftHeader and <= GreenDoorFacingDownHeader)
        {
            color = ColoredDoorColor.Green;
            firstHeader = GreenDoorFacingLeftHeader;
        }
        else if (header is >= RedDoorFacingLeftHeader and <= RedDoorFacingDownHeader)
        {
            color = ColoredDoorColor.Red;
            firstHeader = RedDoorFacingLeftHeader;
        }
        else
        {
            color = default;
            orientation = default;
            return false;
        }

        int byteOffset = header - firstHeader;
        if (byteOffset % 6 != 0 || byteOffset > 18)
        {
            orientation = default;
            return false;
        }
        orientation = (ColoredDoorOrientation)(byteOffset / 6);
        return true;
    }

    /// <summary>
    /// Spawns the blue-door entry selected by shootable BTS <c>$40..$43</c> at
    /// <c>$94:9F26-$9F2C</c> and applies setup <c>$84:C7BB</c>.
    /// </summary>
    /// <remarks>
    /// The cap's top/left origin begins as a type-$C shootable solid. Setup changes only
    /// that origin to type $8, preserving its twelve-bit tile index; the ROM instruction
    /// list then animates all four cap blocks and ends on the ordinary air-door frame.
    /// A power bomb is the one rejected projectile family. Exhausting all forty PLM slots
    /// silently loses the request, matching <c>Spawn_PLM_to_CurrentBlockIndex</c>.
    /// </remarks>
    public bool TrySpawnBlueDoorOpening(
        RoomLevelData level,
        int blockIndex,
        byte behavior,
        ushort projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior is < BlueDoorFacingLeftBts or > BlueDoorFacingDownBts)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                behavior,
                "Blue-door shootable BTS must be $40 through $43.");
        }

        // Setup_BlueDoor masks the native projectile word with $0F00. A power-bomb
        // collision therefore deletes the just-allocated PLM without touching the cap.
        if ((projectileType & 0x0f00) == 0x0300)
            return false;

        ushort instructionPointer = behavior switch
        {
            BlueDoorFacingLeftBts => 0xc489,
            BlueDoorFacingRightBts => 0xc4ba,
            BlueDoorFacingUpBts => 0xc4eb,
            BlueDoorFacingDownBts => 0xc51c,
            _ => throw new InvalidOperationException(
                "Validated blue-door BTS escaped its four-way instruction table."),
        };
        ushort headerPointer = unchecked((ushort)(0xc8a2 +
            ((behavior - BlueDoorFacingLeftBts) * 6)));

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = headerPointer;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = instructionPointer;
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            slot.RoomArgument = 0;
            slot.LoopTimer = 0;
            slot.Item = null;

            // `$84:C7D3-$C7DD` is a direct LevelData write, not a PLM draw. The first
            // animated draw occurs on this actor's next handler pass and owns the VRAM
            // update, while collision observes the type-$8 origin immediately.
            ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
            level.SetForegroundEntry(blockIndex, (ushort)((originalWord & 0x0fff) | 0x8000));
            return true;
        }

        return false;
    }
}

/// <summary>The three projectile-gated door families in header order.</summary>
public enum ColoredDoorColor : byte
{
    Yellow,
    Green,
    Red,
}

/// <summary>Door-cap orientation shared by colored and blue BTS tables.</summary>
public enum ColoredDoorOrientation : byte
{
    Left,
    Right,
    Up,
    Down,
}

/// <summary>Debugger-visible phase of one resident colored-door actor.</summary>
public enum ColoredDoorPhase : byte
{
    Waiting,
    Flashing,
    Opening,
    ConvertToBlue,
}

/// <summary>Stable debugger view over a resident colored-door PLM slot.</summary>
public readonly record struct ColoredDoorPlmSnapshot(
    ushort Header,
    int BlockIndex,
    ushort RoomArgument,
    ColoredDoorColor Color,
    ColoredDoorOrientation Orientation,
    ColoredDoorPhase Phase,
    byte HitCounter);
