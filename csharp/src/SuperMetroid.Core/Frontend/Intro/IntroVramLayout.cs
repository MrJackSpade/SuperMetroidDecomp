namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening cinematic's VRAM allocation (#630), the literal VMADD values of
/// <c>$8B:A469-$A529</c>. The intro takes over the whole store.
/// </summary>
[PpuLayoutBoundary("catalog: opening cinematic VRAM")]
public static class IntroVramLayout
{
    /// <summary>VMADD $0000-$3FFF: the decompressed BG1/BG2 scene characters.</summary>
    public static readonly PpuLayoutCell BackgroundCharacters = new(nameof(BackgroundCharacters), PpuLayoutRegion.Vram,
        0x0000, 0x4000, "4-bpp BG characters", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4000-$447F: the first font, BG3's 2-bpp narration characters.</summary>
    public static readonly PpuLayoutCell FontOne = new(nameof(FontOne), PpuLayoutRegion.Vram,
        IntroCinematicRomData.Layers.FontCharacterBaseWord, 0x0480, "2-bpp BG3 characters", PpuMemoryLayout.Vram);

    /// <summary>
    /// VMADD $4180-$447F: the Japanese glyph area <c>BlankOut_JapanText_Tiles</c> overwrites with one
    /// repeated blank character before the first illustrated page.
    /// </summary>
    public static readonly PpuLayoutCell JapaneseBlankCharacters = new(nameof(JapaneseBlankCharacters), PpuLayoutRegion.Vram,
        0x4180, 0x0300, "2-bpp BG3 characters", FontOne);

    /// <summary>The narration font character number at which <see cref="JapaneseBlankCharacters"/> begins ($30: 2-bpp characters are eight words).</summary>
    public static int JapaneseBlankFirstCharacter => (JapaneseBlankCharacters.Offset - FontOne.Offset) / TwoBppCharacterWords;

    /// <summary>Eight VRAM words per 2-bpp narration glyph when converting a word offset to a character index.</summary>
    private const int TwoBppCharacterWords = 8;

    /// <summary>VMADD $4800-$4BFF: the Samus portrait BG tilemap.</summary>
    public static readonly PpuLayoutCell PortraitTilemap = new(nameof(PortraitTilemap), PpuLayoutRegion.Vram,
        IntroCinematicRomData.Layers.PortraitTilemapWord, 0x0400, "BG tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $4C00-$4FFF: BG3's narration tilemap, rewritten from the <c>$7E:3000</c> staging map.</summary>
    public static readonly PpuLayoutCell NarrationTilemap = new(nameof(NarrationTilemap), PpuLayoutRegion.Vram,
        IntroCinematicRomData.Layers.NarrationTilemapWord, IntroCinematicRomData.Layers.TextTilemapWordCount,
        "BG3 tilemap words (32x32)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $5000-$5FFF: the four scene BG tilemap pages (scene BG1, scene BG2, scientists and spare).</summary>
    public static readonly PpuLayoutCell BackgroundPages = new(nameof(BackgroundPages), PpuLayoutRegion.Vram,
        IntroCinematicRomData.Layers.SceneBg1TilemapWord, 0x1000, "BG tilemap words (four 32x32 pages)", PpuMemoryLayout.Vram);

    /// <summary>VMADD $6000-$6FFF: the intro-specific OBJ characters from <c>$9A:D200</c>.</summary>
    public static readonly PpuLayoutCell IntroObjectCharacters = new(nameof(IntroObjectCharacters), PpuLayoutRegion.Vram,
        0x6000, 0x1000, "4-bpp OBJ characters", PpuMemoryLayout.Vram);

    /// <summary>VMADD $6E00-$7FFF: the decompressed cinematic OBJ characters, overwriting the intro sheet's tail.</summary>
    public static readonly PpuLayoutCell CinematicObjectCharacters = new(nameof(CinematicObjectCharacters), PpuLayoutRegion.Vram,
        0x6e00, 0x1200, "4-bpp OBJ characters", PpuMemoryLayout.Vram);
}
