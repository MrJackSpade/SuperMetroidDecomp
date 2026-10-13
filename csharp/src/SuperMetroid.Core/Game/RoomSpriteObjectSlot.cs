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
    /// <summary>Creates a slot at its fixed position in the 32-entry bank-$B4 pool.</summary>
    internal RoomSpriteObjectSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Zero-based physical pool slot, zero through 31; allocation and update/draw traversal search from the highest slot downward.</summary>
    public int SlotIndex { get; }
    /// <summary>Native word-array byte offset, twice <see cref="SlotIndex"/>, ranging from $00 through $3E.</summary>
    public ushort NativeIndex => unchecked((ushort)(SlotIndex * 2));
    /// <summary>Selector retained from allocation for owner/debugger identity; clearing assigns <see cref="RoomSpriteObjectKind.None"/>.</summary>
    public RoomSpriteObjectKind Kind { get; internal set; } = RoomSpriteObjectKind.None;
    /// <summary>Whether the native instruction-list pointer is nonzero; this alone determines occupancy, even for hidden or update-disabled objects.</summary>
    public bool IsActive => InstructionPointer != 0;
    /// <summary>
    /// Native $7E:F0F8/$7E:F1F8 whole words and their fractional words (Y fraction at $7E:F278)
    /// plus the slot offset: the 16.16 room position, converted to screen coordinates by
    /// subtracting the layer-one camera when drawn. Most sprite objects move only in whole pixels
    /// and leave the fractions zero; Draygon's six burial Evirs consume the exact ROM subspeed
    /// table, so discarding these words visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
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

    /// <summary>Releases occupancy and resets every retained native word before the slot can be reused.</summary>
    internal void Clear()
    {
        Kind = RoomSpriteObjectKind.None;
        Position = FixedPosition<RoomPixels>.Zero;
        GraphicsIndex = 0;
        InstructionPointer = InstructionTimer = SpritemapPointer = DisableFlags = 0;
    }
}
