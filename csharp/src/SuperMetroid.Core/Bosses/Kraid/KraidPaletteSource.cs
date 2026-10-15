namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Five immutable RGB5 sources selected by Kraid's palette handlers.</summary>
public enum KraidPaletteSource
{
    /// <summary>$A7:86C7, Palette_KraidRoomBackground: sixteen RGB5 colors for background palette six, restored in the defeated room and used by backdrop fades.</summary>
    RoomBackdrop,
    /// <summary>$A7:AAA6, Kraid initialization's spritePalette3: sixteen rock colors staged in native target OBJ palette three, exposed at global CGRAM palette eleven by the runtime.</summary>
    InitialTarget,
    /// <summary>$A7:B3D3, Palette_Kraid_BG_HurtFlash and eight following health bands: 144 colors selected for background palette seven, also supplying eye-transition targets.</summary>
    Health,
    /// <summary>$A7:B513, Palette_Kraid_Sprite_HurtFlash and eight following health bands: 144 colors selected alongside the background health band for OBJ palette seven.</summary>
    Secondary,
    /// <summary>$A7:B4F3, Palette_Kraid_Death: sixteen colors replacing background palette seven when death retracts the arm before the final fade.</summary>
    DeathArm,
}
