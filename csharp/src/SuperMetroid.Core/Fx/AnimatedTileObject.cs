namespace SuperMetroid.Core.Fx;

/// <summary>The animated-tile objects rooms, room effects and the Tourian statues animate their characters with.
/// Its values are arbitrary; verification's native view names each member's cartridge word (#1415).
/// </summary>
public enum AnimatedTileObject : ushort
{
    /// <summary>No animated-tile object is loaded.</summary>
    None = 0,
    /// <summary>$87:854C AnimatedTilesObject_TourianStatuePhantoon.</summary>
    TourianStatuePhantoon = 1,
    /// <summary>$87:8552 AnimatedTilesObject_TourianStatueRidley.</summary>
    TourianStatueRidley,
    /// <summary>$87:8558 AnimatedTilesObject_TourianStatueKraid.</summary>
    TourianStatueKraid,
    /// <summary>$87:855E AnimatedTilesObject_TourianStatueDraygon.</summary>
    TourianStatueDraygon,
    /// <summary>$87:824B AnimatedTilesObjects_FX_nothing, the unused area-bit object.</summary>
    Empty,
    /// <summary>$87:8257 AnimatedTilesObjects_FX_horizontalSpikes, common area bit0.</summary>
    HorizontalSpikes,
    /// <summary>$87:8251 AnimatedTilesObjects_FX_verticalSpikes, common area bit1.</summary>
    VerticalSpikes,
    /// <summary>$87:825D AnimatedTilesObjects_FX_crateriaLake, Crateria bit2.</summary>
    CrateriaLake,
    /// <summary>$87:8263 unused Crateria lava object, preserved for area bit3.</summary>
    UnusedCrateriaLava,
    /// <summary>$87:8281 AnimatedTilesObjects_FX_brinstarPlant, Brinstar bit2.</summary>
    BrinstarPlant,
    /// <summary>$87:826F AnimatedTilesObjects_FX_wreckedShipScreen, Wrecked Ship bit4.</summary>
    WreckedShipScreen,
    /// <summary>kAnimtiles_MaridiaSandCeiling, $87:8287, selected by room FX animation bits.</summary>
    MaridiaSandCeiling,
    /// <summary>kAnimtiles_MaridiaSandFalling, $87:828D, selected by room FX animation bits.</summary>
    MaridiaSandFalling,
    /// <summary>Wrecked Ship rightward treadmill object at $87:8275.</summary>
    WreckedShipTreadmillRightwards,
    /// <summary>Wrecked Ship leftward treadmill object at $87:827B.</summary>
    WreckedShipTreadmillLeftwards,

    /// <summary>
    /// <c>kAnimtiles_Lava</c> at $87:82AB, spawned by the lava FX dispatcher at
    /// $88:B279. Its five-frame loop uploads $40 bytes to VRAM word $4280.
    /// </summary>
    Lava,

    /// <summary>
    /// <c>kAnimtiles_Acid</c> at $87:82C9, spawned by the acid FX dispatcher at
    /// $88:B2A1. Its five-frame loop uploads $40 bytes to VRAM word $4280.
    /// </summary>
    Acid,

    /// <summary>
    /// <c>kAnimtiles_Rain</c> at $87:82E7, spawned by the rain FX dispatcher at
    /// $88:C4B9. Its five-frame loop uploads $50 bytes to VRAM word $4280.
    /// </summary>
    Rain,

    /// <summary>
    /// <c>AnimatedTilesObject_Spores</c> at $87:82FD, spawned by FX type $08 at
    /// $88:DA25. Three ten-tick frames upload $30 bytes to VRAM word $4280.
    /// </summary>
    Spores,
}
