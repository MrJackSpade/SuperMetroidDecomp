namespace SuperMetroid.AssetExtraction.Bosses.Kraid;

/// <summary>Five immutable RGB5 sources selected by Kraid's palette handlers.</summary>
public enum KraidPaletteSource
{
    /// <summary>Sixteen RGB5 colors for background palette six, restored in the defeated room and used by backdrop fades.</summary>
    RoomBackdrop,
    /// <summary>Kraid initialization's spritePalette3: sixteen rock colors staged in target sprite palette three, exposed at global palette eleven by the runtime.</summary>
    InitialTarget,
    /// <summary>Palette_Kraid_BG_HurtFlash and eight following health bands: 144 colors selected for background palette seven, also supplying eye-transition targets.</summary>
    Health,
    /// <summary>Palette_Kraid_Sprite_HurtFlash and eight following health bands: 144 colors selected alongside the background health band for OBJ palette seven.</summary>
    Secondary,
    /// <summary>Sixteen colors replacing background palette seven when death retracts the arm before the final fade.</summary>
    DeathArm,
}
