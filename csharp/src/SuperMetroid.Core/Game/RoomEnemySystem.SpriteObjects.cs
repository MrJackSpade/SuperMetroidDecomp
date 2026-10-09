using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge object number passed to <c>CreateSpriteAtPos</c>. These are table indexes into
/// <c>$B4:BDA8</c>, not invented managed identifiers, so a debugger watch can be compared
/// directly with the caller's native argument.
/// </summary>
public enum RoomSpriteObjectKind : ushort
{
    /// <summary>Managed $FFFF sentinel for a cleared slot; not a valid selector in the native $B4:BDA8 dispatch table.</summary>
    None = 0xffff,
    /// <summary>Selector $03, <c>$B4:BEA4 InstList_SpriteObject_3_SmallExplosion</c>: finite small explosion used by Spore Spawn, Draygon, and escape effects.</summary>
    SporeSpawnDyingExplosion = 0x0003,
    /// <summary>Selector $06, <c>$B4:BEEA InstList_SpriteObject_6_DudShot</c>: finite impact flash for shots rejected by enemy collision handlers.</summary>
    EnemyProjectileDud = 0x0006,
    /// <summary>Selector $0A, <c>$B4:BF32 InstList_SpriteObject_A_SpacePirateLandingDustCloud</c>: finite dust animation spawned at a Ninja Pirate's landing.</summary>
    NinjaPirateLandingDust = 0x000a,
    /// <summary>Selector $09, <c>$B4:BF1C InstList_SpriteObject_9_SmallDudShot</c>: small finite flash used as one of Botwoon's death-explosion variants.</summary>
    BotwoonSmallExplosion = 0x0009,
    /// <summary>Selector $1D, <c>$B4:BF74 InstList_SpriteObject_1D_BigExplosion</c>: large finite explosion used by Botwoon's death sequence.</summary>
    BotwoonLargeExplosion = 0x001d,
    /// <summary>Selector $15, <c>$B4:C05E InstList_SpriteObject_15_BigDustCloud</c>: finite cloud shared by impacts, burial smoke, and acid effects.</summary>
    DustCloud = 0x0015,
    /// <summary>Alias of selector $15's big-dust-cloud program, used for Crocomire's acid smoke rather than a distinct native instruction list.</summary>
    CrocomireAcidSmoke = DustCloud,
    /// <summary>Selector $18, <c>$B4:C10C InstList_SpriteObject_18_ShortDraygonBreathBubbles</c>: finite breath bubbles emitted during Draygon's flight.</summary>
    DraygonBreathBubble = 0x0018,
    /// <summary>Selector $2B, <c>$B4:C30A InstList_SpriteObject_2B_PuromiBody</c>: looping Nuclear Waffle body-segment art, positioned by the owning enemy.</summary>
    NuclearWaffleBody = 0x002b,
    /// <summary>Selector $2C, <c>$B4:C33E InstList_SpriteObject_2C_PuromiRightExplosion</c>: finite right-turn burst selected for Nuclear Waffle turn variant zero.</summary>
    NuclearWaffleTurnClockwise = 0x002c,
    /// <summary>Selector $2D, <c>$B4:C35C InstList_SpriteObject_2D_PuromiLeftExplosion</c>: finite left-turn burst selected for the other Nuclear Waffle turn variant.</summary>
    NuclearWaffleTurnCounterClockwise = 0x002d,
    /// <summary>Selector $2E, <c>$B4:C37A InstList_SpriteObject_2E_PuromiSplash</c>: finite splash overlay spawned alongside the directional Nuclear Waffle turn burst.</summary>
    NuclearWaffleTurnOverlay = 0x002e,
    /// <summary>Selector $30, <c>$B4:C390 InstList_SpriteObject_30_FallingSparkTrail</c>: finite afterimage left behind by a falling spark.</summary>
    FallingSparkTrail = 0x0030,
    /// <summary>Selector $32, <c>$B4:C3BA InstList_SpriteObject_32_MetroidElectricity</c>: looping electrical outer-body art, allocated and repositioned by the Metroid owner.</summary>
    MetroidOuterBodyA = 0x0032,
    /// <summary>Selector $34, <c>$B4:C4B6 InstList_SpriteObject_34_MetroidShell</c>: looping shell layer paired with the Metroid's electrical sprite object.</summary>
    MetroidOuterBodyB = 0x0034,
    /// <summary>Selector $38, <c>$B4:C5D8 InstList_SpriteObject_38_YappingMawBaseFacingDown</c>: held root art placed eight pixels above a parameter-two-zero Yapping Maw.</summary>
    YappingMawRootVariantZero = 0x0038,
    /// <summary>Selector $39, <c>$B4:C5DE InstList_SpriteObject_39_YappingMawBaseFacingUp</c>: held root art placed eight pixels below a nonzero-parameter-two Yapping Maw.</summary>
    YappingMawRootVariantOne = 0x0039,
    /// <summary>Selector $3B, <c>$B4:C608 InstList_SpriteObject_3B_EvirFacingLeft</c>: looping left-facing Evir art used for both Draygon's opening dance and burial.</summary>
    DraygonIntroEvir = 0x003b,
    /// <summary>Selector $3C, <c>$B4:C61C InstList_SpriteObject_3C_EvirFacingRight</c>: looping right-facing Evir art moved by Draygon's burial sequence.</summary>
    DraygonDeathEvirFacingRight = 0x003c,
    /// <summary>Selector $3D, <c>$B4:BE24 InstList_SpriteObject_3D_DraygonFoamingAtTheMouth</c>: finite mouth-foam animation emitted during the carry spiral and fatal drift.</summary>
    DraygonSpiralFoam = 0x003d,
}

/// <summary>
/// One of the 32 physical bank-$B4 sprite-object slots. The retail pool is shared by enemy
/// bodies, attack afterimages, explosions, and room effects; modeling it once preserves its
/// descending allocation order and finite-capacity behavior for every translated family.
/// </summary>
public sealed class RoomSpriteObjectSlot
{
    /// <summary>Creates a slot with its fixed position in the shared 32-entry pool.</summary>
    /// <param name="slotIndex">Zero-based pool index retained for native word-array addressing.</param>
    internal RoomSpriteObjectSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Zero-based physical pool slot, zero through 31; allocation and update/draw traversal search from the highest slot downward.</summary>
    public int SlotIndex { get; }
    /// <summary>Native word-array byte offset, twice <see cref="SlotIndex"/>, ranging from $00 through $3E.</summary>
    public ushort NativeIndex => unchecked((ushort)(SlotIndex * 2));
    /// <summary>Selector retained from allocation for owner/debugger identity; clearing assigns <see cref="RoomSpriteObjectKind.None"/>.</summary>
    public RoomSpriteObjectKind Kind { get; internal set; } = RoomSpriteObjectKind.None;
    /// <summary>Whether the native instruction-list pointer is nonzero; this alone determines occupancy, even for hidden or update-disabled objects.</summary>
    public bool IsActive => InstructionPointer != 0;
    /// <summary>Native $7E:F0F8 plus the slot offset: whole-pixel room X, converted to screen X by subtracting layer-one camera X when drawn.</summary>
    public ushort XPosition { get; internal set; }
    /// <summary>Native $7E:F1F8 plus the slot offset: whole-pixel room Y, converted to screen Y by subtracting layer-one camera Y when drawn.</summary>
    public ushort YPosition { get; internal set; }
    /// <summary>
    /// Fractional halves of the native 16.16 world coordinates. Most sprite objects move
    /// only in whole pixels and leave these zero; Draygon's six burial Evirs consume the
    /// exact ROM subspeed table, so discarding these words visibly changes their fan-in.
    /// </summary>
    public ushort XSubposition { get; internal set; }
    /// <summary>Native $7E:F278 plus the slot offset: fractional low word paired with <see cref="YPosition"/> for 16.16 room coordinates; drawing uses only the whole word.</summary>
    public ushort YSubposition { get; internal set; }
    /// <summary>Native $7E:F078 plus the slot offset: packed OBJ attribute base; drawing extracts palette bits $0E00 and base tile number $01FF.</summary>
    public ushort GraphicsIndex { get; internal set; }
    /// <summary>Native $7E:EF78 plus the slot offset: bank-$B4 program cursor; timed records occupy four bytes, and zero marks a free slot.</summary>
    public ushort InstructionPointer { get; internal set; }
    /// <summary>Native $7E:EFF8 plus the slot offset: frame-duration countdown or high-bit-set opcode address; repeat-last installs $7FFF while preserving the visible frame.</summary>
    public ushort InstructionTimer { get; internal set; }
    /// <summary>Bank-$B4 visual-frame pointer resolved from the current timed record's second word; drawing combines its tile offsets with <see cref="GraphicsIndex"/>.</summary>
    public ushort SpritemapPointer { get; internal set; }
    /// <summary>Native $7E:F2F8 plus the slot offset: bit zero suppresses instruction updates but leaves active-object drawing and slot occupancy intact.</summary>
    public ushort DisableFlags { get; internal set; }

    /// <summary>Resets the slot's object identity, position, graphics, instruction state, and flags.</summary>
    internal void Clear()
    {
        Kind = RoomSpriteObjectKind.None;
        XPosition = YPosition = XSubposition = YSubposition = GraphicsIndex = 0;
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
    /// <summary>Number of physical sprite-object entries in the native bank-$B4 pool.</summary>
    private const int RoomSpriteObjectSlotCount = 32;
    /// <summary>Fixed sprite-object slots shared by enemy bodies, effects, and room objects.</summary>
    private readonly RoomSpriteObjectSlot[] _roomSpriteObjects =
        Enumerable.Range(0, RoomSpriteObjectSlotCount)
            .Select(index => new RoomSpriteObjectSlot(index))
            .ToArray();

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
            slot.InstructionPointer = RoomSpriteObjectDefinitions.InstructionPointer(kind);
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
            ushort durationOrOpcode =
                RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
                    slot.InstructionPointer);
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

    /// <summary>Executes the repeat-last, terminate, or goto opcode stored in a slot's instruction timer.</summary>
    /// <param name="slot">Active object whose current program record is an opcode.</param>
    /// <exception cref="InvalidDataException">The slot contains an opcode that has no translated implementation.</exception>
    private static void ProcessRoomSpriteObjectOpcode(RoomSpriteObjectSlot slot)
    {
        switch (slot.InstructionTimer)
        {
            case RoomSpriteObjectInstructionProgramDefinitions.RepeatLast:
                // Repeat-last backs up to the timed record that preceded the opcode and
                // pins its timer at $7FFF. Its already selected spritemap remains visible.
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer - 4));
                slot.InstructionTimer = 0x7fff;
                return;

            case RoomSpriteObjectInstructionProgramDefinitions.Terminate:
                slot.Clear();
                return;

            case RoomSpriteObjectInstructionProgramDefinitions.Goto:
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
                if (!IsNegative16(slot.InstructionTimer))
                    LoadRoomSpriteObjectFrame(slot);
                return;

            default:
                throw new InvalidDataException(
                    $"Sprite-object instruction $B4:{slot.InstructionTimer:X4} at " +
                    $"$B4:{slot.InstructionPointer:X4} is not translated.");
        }
    }

    /// <summary>Loads a timed frame's duration and resolves its visual spritemap operand.</summary>
    /// <param name="slot">Object slot positioned at a timed instruction record.</param>
    /// <exception cref="InvalidDataException">The record does not contain a positive timed-frame duration.</exception>
    private static void LoadRoomSpriteObjectFrame(RoomSpriteObjectSlot slot)
    {
        ushort duration = RoomSpriteObjectInstructionProgramDefinitions.ReadMechanicsWord(
            slot.InstructionPointer);
        if (duration == 0 || IsNegative16(duration))
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

            ushort paletteBits = new SnesObjAttributeWord(slot.GraphicsIndex).PaletteBits;
            ushort baseTileIndex = unchecked((ushort)
                new SnesObjAttributeWord(slot.GraphicsIndex).TileNumber);
            var spritemaps = TileArtwork?.Spritemaps ?? throw new InvalidOperationException(
                "Room sprite objects require installed sprite artwork.");
            if (!spritemaps.TryGetDisplay(
                    RoomSpriteObjectVisualDefinitions.Bank, slot.SpritemapPointer,
                    out EnemySpritemapParts installed))
            {
                throw new InvalidDataException(
                    $"Installed room sprite-object artwork lacks " +
                    $"$B4:{slot.SpritemapPointer:X4} for {slot.Kind}.");
            }
            oam.AddEnemySpritemap(installed, screenX, screenY,
                paletteBits, baseTileIndex);
        }
    }
}
