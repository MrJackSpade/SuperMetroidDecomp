using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The bank-$B4 room sprite-object pool: 32 instruction-driven decorative actors (dust, splashes, shells)
/// that enemies and projectiles spawn, step and draw independently of enemy and projectile slots.
/// </summary>
internal sealed class RoomSpriteObjectPool
{
    private readonly EnemyRoomBinding _room;

    internal RoomSpriteObjectPool(EnemyRoomBinding room)
    {
        _room = room;
    }
    internal const int RoomSpriteObjectSlotCount = 32;
    private readonly RoomSpriteObjectSlot[] _roomSpriteObjects =
        Enumerable.Range(0, RoomSpriteObjectSlotCount)
            .Select(index => new RoomSpriteObjectSlot(index))
            .ToArray();

    /// <summary>All 32 physical slots, including inactive ones.</summary>
    internal IReadOnlyList<RoomSpriteObjectSlot> Slots => _roomSpriteObjects;

    /// <summary>Clears every slot when a room loads.</summary>
    internal void ResetForRoom()
    {
        foreach (RoomSpriteObjectSlot slot in _roomSpriteObjects)
            slot.Clear();
    }

    /// <summary>Ports <c>CreateSpriteAtPos</c> at <c>$B4:BC26</c>.</summary>
    internal RoomSpriteObjectSlot? SpawnRoomSpriteObject(
        Position16<RoomPixels> position,
        RoomSpriteObjectKind kind,
        ushort graphicsIndex)
    {
        // Native allocation searches indexes $3E,$3C,...,$00. A saturated pool drops the
        // request and returns $FFFF; callers must not grow an unbounded host collection.
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (slot.IsActive)
                continue;

            slot.Clear();
            slot.Kind = kind;
            slot.Position = slot.Position.WithWhole(position);
            slot.GraphicsIndex = graphicsIndex;
            slot.InstructionPointer = RoomSpriteObjectDefinitions.InstructionPointer(kind);
            LoadRoomSpriteObjectFrame(slot);
            return slot;
        }

        return null;
    }

    /// <summary>Ports the non-frozen body of <c>HandleSpriteObjects</c> at $B4:BC82.</summary>
    internal void StepRoomSpriteObjects()
    {
        // The hardware walks descending native indexes. No current opcode spawns another
        // object, but retaining the order keeps pool behavior stable for later families.
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (!slot.IsActive || (slot.DisableFlags & 1) != 0)
                continue;

            if (EnemyMath.IsNegative16(slot.InstructionTimer))
            {
                ProcessRoomSpriteObjectOpcode(slot);
                continue;
            }

            ushort oldTimer = slot.InstructionTimer;
            slot.InstructionTimer = unchecked((ushort)(oldTimer - 1));
            if (oldTimer != 1)
                continue;

            slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
            ushort durationOrOpcode =
                RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                    slot.InstructionPointer);
            if (EnemyMath.IsNegative16(durationOrOpcode))
            {
                slot.InstructionTimer = durationOrOpcode;
                ProcessRoomSpriteObjectOpcode(slot);
            }
            else
            {
                LoadRoomSpriteObjectFrame(slot);
            }
        }
    }

    private static void ProcessRoomSpriteObjectOpcode(RoomSpriteObjectSlot slot)
    {
        RoomSpriteObjectInstruction instruction = ClosedNativeWords.Decode<RoomSpriteObjectInstruction>(
            slot.InstructionTimer, "room sprite-object instruction");
        switch (instruction)
        {
            case RoomSpriteObjectInstruction.RepeatLast:
                // Repeat-last backs up to the timed record that preceded the opcode and
                // pins its timer at $7FFF. Its already selected spritemap remains visible.
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer - 4));
                slot.InstructionTimer = 0x7fff;
                return;

            case RoomSpriteObjectInstruction.Terminate:
                slot.Clear();
                return;

            case RoomSpriteObjectInstruction.Goto:
                slot.InstructionPointer =
                    RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                // `$B4:BD12` installs the destination's first word as the timer and returns.
                // Usually that word is a duration, but preserving an opcode destination lets
                // the ordinary next-frame dispatcher handle chained control records exactly
                // as cartridge data specifies instead of assuming every Goto targets art.
                slot.InstructionTimer =
                    RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                        slot.InstructionPointer);
                if (!EnemyMath.IsNegative16(slot.InstructionTimer))
                    LoadRoomSpriteObjectFrame(slot);
                return;

            default:
                throw new InvalidOperationException(
                    $"Undefined sprite-object instruction {instruction} at $B4:{slot.InstructionPointer:X4}.");
        }
    }

    internal static void LoadRoomSpriteObjectFrame(RoomSpriteObjectSlot slot)
    {
        ushort duration = RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
            slot.InstructionPointer);
        if (duration == 0 || EnemyMath.IsNegative16(duration))
        {
            throw new InvalidDataException(
                $"Sprite object {slot.Kind} expected a timed frame at " +
                $"$B4:{slot.InstructionPointer:X4}, found ${duration:X4}.");
        }

        slot.InstructionTimer = duration;
        ushort operand = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.SpritemapPointer = RoomSpriteObjectVisualDefinitions.FrameAt(operand);
    }

    /// <summary>Ports <c>DrawSpriteObjects</c> at <c>$B4:BD32</c>.</summary>
    internal void DrawRoomSpriteObjects(OamBuffer oam, Position16<RoomPixels> camera)
    {
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (!slot.IsActive)
                continue;

            Position16<ScreenPixels> screen = slot.Position.Whole.ToScreen(camera);
            // The 65816 uses signed branch tests on `(x + 16)`, `(x - 272)`, `y`, and
            // `(y - 272)`. Unsigned host comparisons incorrectly discarded partially visible
            // objects whose wrapped screen coordinate represented a small negative offset.
            if (unchecked((short)(screen.X + 16)) < 0 ||
                unchecked((short)(screen.X - 0x0110)) >= 0 ||
                unchecked((short)screen.Y) < 0 ||
                unchecked((short)(screen.Y - 0x0110)) >= 0)
            {
                continue;
            }

            ushort paletteBits = new SnesObjAttributeWord(slot.GraphicsIndex).PaletteBits;
            ushort baseTileIndex = unchecked((ushort)
                new SnesObjAttributeWord(slot.GraphicsIndex).TileNumber);
            var spritemaps = _room.Artwork?.Spritemaps ?? throw new InvalidOperationException(
                "Room sprite objects require installed sprite artwork.");
            if (!spritemaps.TryGetDisplay(
                    RoomSpriteObjectVisualDefinitions.Bank, slot.SpritemapPointer,
                    out EnemySpritemapParts installed))
            {
                throw new InvalidDataException(
                    $"Installed room sprite-object artwork lacks " +
                    $"$B4:{slot.SpritemapPointer:X4} for {slot.Kind}.");
            }
            oam.AddEnemySpritemap(installed, screen.ToOam(),
                paletteBits, baseTileIndex);
        }
    }
}
