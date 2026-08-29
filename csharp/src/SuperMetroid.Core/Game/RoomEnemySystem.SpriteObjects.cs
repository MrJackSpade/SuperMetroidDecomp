using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge object number passed to <c>CreateSpriteAtPos</c>. These are table indexes into
/// <c>$B4:BDA8</c>, not invented managed identifiers, so a debugger watch can be compared
/// directly with the caller's native argument.
/// </summary>
public enum RoomSpriteObjectKind : ushort
{
    None = 0xffff,
    SporeSpawnDyingExplosion = 0x0003,
    EnemyProjectileDud = 0x0006,
    NinjaPirateLandingDust = 0x000a,
    BotwoonSmallExplosion = 0x0009,
    BotwoonLargeExplosion = 0x001d,
    DustCloud = 0x0015,
    CrocomireAcidSmoke = DustCloud,
    NuclearWaffleBody = 0x002b,
    NuclearWaffleTurnClockwise = 0x002c,
    NuclearWaffleTurnCounterClockwise = 0x002d,
    NuclearWaffleTurnOverlay = 0x002e,
    FallingSparkTrail = 0x0030,
    MetroidOuterBodyA = 0x0032,
    MetroidOuterBodyB = 0x0034,
    YappingMawRootVariantZero = 0x0038,
    YappingMawRootVariantOne = 0x0039,
}

/// <summary>
/// One of the 32 physical bank-$B4 sprite-object slots. The retail pool is shared by enemy
/// bodies, attack afterimages, explosions, and room effects; modeling it once preserves its
/// descending allocation order and finite-capacity behavior for every translated family.
/// </summary>
public sealed class RoomSpriteObjectSlot
{
    internal RoomSpriteObjectSlot(int slotIndex) => SlotIndex = slotIndex;

    public int SlotIndex { get; }
    public ushort NativeIndex => unchecked((ushort)(SlotIndex * 2));
    public RoomSpriteObjectKind Kind { get; internal set; } = RoomSpriteObjectKind.None;
    public bool IsActive => InstructionPointer != 0;
    public ushort XPosition { get; internal set; }
    public ushort YPosition { get; internal set; }
    public ushort GraphicsIndex { get; internal set; }
    public ushort InstructionPointer { get; internal set; }
    public ushort InstructionTimer { get; internal set; }
    public ushort SpritemapPointer { get; internal set; }
    public ushort DisableFlags { get; internal set; }

    internal void Clear()
    {
        Kind = RoomSpriteObjectKind.None;
        XPosition = YPosition = GraphicsIndex = 0;
        InstructionPointer = InstructionTimer = SpritemapPointer = DisableFlags = 0;
    }
}

/// <summary>
/// Shared translation of <c>CreateSpriteAtPos</c>, <c>HandleSpriteObjects</c>, and
/// <c>DrawSpriteObjects</c> from bank $B4. Enemy families own when objects are spawned or
/// repositioned; this pool owns only allocation, cartridge bytecode, lifetime, and drawing.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const int RoomSpriteObjectSlotCount = 32;
    private const int RoomSpriteObjectInstructionTable = 0xb4bda8;
    private const ushort SpriteObjectRepeatLastInstruction = 0xbcf0;
    private const ushort SpriteObjectTerminateInstruction = 0xbd07;
    private const ushort SpriteObjectGotoInstruction = 0xbd12;

    private readonly RoomSpriteObjectSlot[] _roomSpriteObjects =
        Enumerable.Range(0, RoomSpriteObjectSlotCount)
            .Select(index => new RoomSpriteObjectSlot(index))
            .ToArray();

    /// <summary>All 32 physical bank-$B4 slots, including inactive ones.</summary>
    public IReadOnlyList<RoomSpriteObjectSlot> RoomSpriteObjects => _roomSpriteObjects;

    /// <summary>
    /// Compatibility/debugger view of Spark's object-$30 trail entries inside the now-shared
    /// native pool. Other object kinds no longer steal an impossible second set of 32 slots.
    /// </summary>
    public IReadOnlyList<RoomSpriteObjectSlot> FallingSparkTrails =>
        _roomSpriteObjects
            .Where(slot => slot.Kind == RoomSpriteObjectKind.FallingSparkTrail)
            .ToArray();

    public int ActiveFallingSparkTrailCount =>
        _roomSpriteObjects.Count(slot =>
            slot.IsActive && slot.Kind == RoomSpriteObjectKind.FallingSparkTrail);

    /// <summary>Ports <c>CreateSpriteAtPos</c> at <c>$B4:BC26</c>.</summary>
    private RoomSpriteObjectSlot? SpawnRoomSpriteObject(
        ushort x,
        ushort y,
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
            slot.XPosition = x;
            slot.YPosition = y;
            slot.GraphicsIndex = graphicsIndex;
            slot.InstructionPointer = ReadWord(
                _bus!,
                RoomSpriteObjectInstructionTable + (ushort)kind * 2);
            LoadRoomSpriteObjectFrame(slot);
            return slot;
        }

        return null;
    }

    /// <summary>Ports the non-frozen body of <c>HandleSpriteObjects</c> at $B4:BC82.</summary>
    private void StepRoomSpriteObjects()
    {
        // The hardware walks descending native indexes. No current opcode spawns another
        // object, but retaining the order keeps pool behavior stable for later families.
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (!slot.IsActive || (slot.DisableFlags & 1) != 0)
                continue;

            if (IsNegative16(slot.InstructionTimer))
            {
                ProcessRoomSpriteObjectOpcode(slot);
                continue;
            }

            ushort oldTimer = slot.InstructionTimer;
            slot.InstructionTimer = unchecked((ushort)(oldTimer - 1));
            if (oldTimer != 1)
                continue;

            slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
            ushort durationOrOpcode = ReadWord(
                _bus!,
                0xb40000 | slot.InstructionPointer);
            if (IsNegative16(durationOrOpcode))
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

    private void ProcessRoomSpriteObjectOpcode(RoomSpriteObjectSlot slot)
    {
        switch (slot.InstructionTimer)
        {
            case SpriteObjectRepeatLastInstruction:
                // Repeat-last backs up to the timed record that preceded the opcode and
                // pins its timer at $7FFF. Its already selected spritemap remains visible.
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer - 4));
                slot.InstructionTimer = 0x7fff;
                return;

            case SpriteObjectTerminateInstruction:
                slot.Clear();
                return;

            case SpriteObjectGotoInstruction:
                slot.InstructionPointer = ReadWord(
                    _bus!,
                    0xb40000 | unchecked((ushort)(slot.InstructionPointer + 2)));
                // `$B4:BD12` installs the destination's first word as the timer and returns.
                // Usually that word is a duration, but preserving an opcode destination lets
                // the ordinary next-frame dispatcher handle chained control records exactly
                // as cartridge data specifies instead of assuming every Goto targets art.
                slot.InstructionTimer = ReadWord(
                    _bus!,
                    0xb40000 | slot.InstructionPointer);
                if (!IsNegative16(slot.InstructionTimer))
                    LoadRoomSpriteObjectFrame(slot);
                return;

            default:
                throw new NotSupportedException(
                    $"Sprite-object instruction $B4:{slot.InstructionTimer:X4} at " +
                    $"$B4:{slot.InstructionPointer:X4} is not translated.");
        }
    }

    private void LoadRoomSpriteObjectFrame(RoomSpriteObjectSlot slot)
    {
        ushort duration = ReadWord(_bus!, 0xb40000 | slot.InstructionPointer);
        if (duration == 0 || IsNegative16(duration))
        {
            throw new InvalidDataException(
                $"Sprite object {slot.Kind} expected a timed frame at " +
                $"$B4:{slot.InstructionPointer:X4}, found ${duration:X4}.");
        }

        slot.InstructionTimer = duration;
        slot.SpritemapPointer = ReadWord(
            _bus!,
            0xb40000 | unchecked((ushort)(slot.InstructionPointer + 2)));
    }

    /// <summary>Ports <c>DrawSpriteObjects</c> at <c>$B4:BD32</c>.</summary>
    private void DrawRoomSpriteObjects(OamBuffer oam, ushort cameraX, ushort cameraY)
    {
        for (int index = _roomSpriteObjects.Length - 1; index >= 0; index--)
        {
            RoomSpriteObjectSlot slot = _roomSpriteObjects[index];
            if (!slot.IsActive)
                continue;

            ushort screenX = unchecked((ushort)(slot.XPosition - cameraX));
            ushort screenY = unchecked((ushort)(slot.YPosition - cameraY));
            // The 65816 uses signed branch tests on `(x + 16)`, `(x - 272)`, `y`, and
            // `(y - 272)`. Unsigned host comparisons incorrectly discarded partially visible
            // objects whose wrapped screen coordinate represented a small negative offset.
            if (unchecked((short)(screenX + 16)) < 0 ||
                unchecked((short)(screenX - 0x0110)) >= 0 ||
                unchecked((short)screenY) < 0 ||
                unchecked((short)(screenY - 0x0110)) >= 0)
            {
                continue;
            }

            oam.AddEnemySpritemap(
                _bus!,
                bank: 0xb4,
                slot.SpritemapPointer,
                screenX,
                screenY,
                paletteBits: unchecked((ushort)(slot.GraphicsIndex & 0x0e00)),
                baseTileIndex: unchecked((ushort)(slot.GraphicsIndex & 0x01ff)));
        }
    }
}
