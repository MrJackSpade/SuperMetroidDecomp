namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A5 Draygon visual palette sources and native CGRAM transfer geometry.</summary>
public static class DraygonColorRomData
{
    /// <summary>
    /// Body initializer at $A5:8687 copies 25 words from $A5:A217: all of sprite palette
    /// one and the first nine words of sprite palette two, into target colors 144..168.
    /// Native <c>Palette_Draygon_Sprite1</c> continues into <c>Palette_Draygon_Sprite2</c>;
    /// this is a partial introductory OBJ transfer, not the full BG body palette.
    /// </summary>
    public const int IntroSource = 0xa5a217;
    /// <summary>Twenty-five sixteen-bit SNES color words in the initializer's contiguous OBJ transfer; this count is in colors, not bytes.</summary>
    public const int IntroCount = 25;
    /// <summary>Zero-based CGRAM color index 144 (<c>$90</c>), the first color of OBJ palette one; the 25-color introductory transfer ends at index 168.</summary>
    public const int IntroDestination = 144;

    /// <summary>Normal BG2 body palette <c>Palette_Draygon_BG12_5</c> at <c>$A5:A277</c>, copied by hurt AI <c>$A5:954D</c> before overlaying the selected health-band colors.</summary>
    public const int BackgroundSource = 0xa5a277;
    /// <summary>Sixteen color words in the complete BG body palette, including its first palette slot.</summary>
    public const int BackgroundCount = 16;
    /// <summary>Zero-based CGRAM color index 80 (<c>$50</c>), the beginning of BG palette five; normal and white-flash transfers replace indices 80..95.</summary>
    public const int BackgroundDestination = 80;

    /// <summary>Normal <c>Palette_Draygon_Sprite7</c> at <c>$A5:A1F7</c>, copied by hurt AI <c>$A5:954D</c> separately from the BG body colors.</summary>
    public const int SpriteSource = 0xa5a1f7;
    /// <summary>Sixteen color words in the complete normal OBJ palette, including the transparent first slot.</summary>
    public const int SpriteCount = 16;
    /// <summary>Zero-based CGRAM color index 240 (<c>$F0</c>), the beginning of OBJ palette seven; normal and white-flash transfers replace indices 240..255.</summary>
    public const int SpriteDestination = 240;

    /// <summary><c>Palette_Draygon_WhiteFlash</c> at <c>$A5:A297</c>, copied to both the BG2 body and OBJ palette-seven destinations during the hurt flash.</summary>
    public const int WhiteFlashSource = 0xa5a297;
    /// <summary>Sixteen color words in the flash image shared by the complete BG and OBJ hurt-palette transfers.</summary>
    public const int WhiteFlashCount = 16;

    /// <summary>Native <c>DraygonHealthBasedPaletteTable</c> at <c>$A5:96AF</c>: eight consecutive four-color rows in descending-health order; the separate thresholds remain compiled gameplay data.</summary>
    public const int HealthBandsSource = 0xa596af;
    /// <summary>Eight ordered health-dependent color rows; native table-index words are even selectors 0..14, not direct row numbers.</summary>
    public const int HealthBandCount = 8;
    /// <summary>Four sixteen-bit color words per health row, occupying eight bytes and replacing only the body's damage-sensitive pens.</summary>
    public const int HealthBandColorCount = 4;
    /// <summary>Zero-based CGRAM color index 89 (<c>$59</c>), BG palette-five pen nine; a health-band update replaces only indices 89..92.</summary>
    public const int HealthDestination = 89;
}
