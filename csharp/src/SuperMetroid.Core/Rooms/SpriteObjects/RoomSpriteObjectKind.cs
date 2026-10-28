namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>
/// Cartridge object number passed to <c>CreateSpriteAtPos</c>. These are table indexes into
/// <c>$B4:BDA8</c>, not invented managed identifiers, so a debugger watch can be compared
/// directly with the caller's native argument.
/// </summary>
public enum RoomSpriteObjectKind
{
    /// <summary>Managed $FFFF sentinel for a cleared slot; not a valid selector in the native $B4:BDA8 dispatch table.</summary>
    None = 0xffff,
    /// <summary>Selector $03, <c>$B4:BEA4 InstList_SpriteObject_3_SmallExplosion</c>: finite small explosion used by Spore Spawn, Draygon, and escape effects.</summary>
    SporeSpawnDyingExplosion = 0x0003,
    /// <summary>Alias of selector $03's small-explosion program, as the generic enemy-death explosions spawn it.</summary>
    SmallExplosion = SporeSpawnDyingExplosion,
    /// <summary>Selector $0C, <c>$B4:BF56 InstList_SpriteObject_C_Smoke</c>: finite smoke the generic enemy-death explosions scatter.</summary>
    Smoke = 0x000c,
    /// <summary>Selector $06, <c>$B4:BEEA InstList_SpriteObject_6_DudShot</c>: finite impact flash for shots rejected by enemy collision handlers.</summary>
    EnemyProjectileDud = 0x0006,
    /// <summary>Selector $0A, <c>$B4:BF32 InstList_SpriteObject_A_SpacePirateLandingDustCloud</c>: finite dust animation spawned at a Ninja Pirate's landing.</summary>
    NinjaPirateLandingDust = 0x000a,
    /// <summary>Selector $09, <c>$B4:BF1C InstList_SpriteObject_9_SmallDudShot</c>: small finite flash used as one of Botwoon's death-explosion variants.</summary>
    BotwoonSmallExplosion = 0x0009,
    /// <summary>Selector $1D, <c>$B4:BF74 InstList_SpriteObject_1D_BigExplosion</c>: large finite explosion used by Botwoon's death sequence.</summary>
    BotwoonLargeExplosion = 0x001d,
    /// <summary>Selector $12, <c>$B4:C014 InstList_SpriteObject_12_ShortBigDustCloud</c>: finite cloud among the escape's explosions.</summary>
    ShortBigDustCloud = 0x0012,
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
