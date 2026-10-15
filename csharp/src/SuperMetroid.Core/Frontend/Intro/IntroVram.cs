namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Typed view of the opening cinematic's VRAM (#630).</summary>
[PpuLayoutBoundary("rendering view: opening cinematic VRAM")]
public readonly struct IntroVram(SnesVram vram)
{
    /// <summary>Scene characters.</summary>
    public VramRegion BackgroundCharacters => new(vram, IntroVramLayout.BackgroundCharacters);

    /// <summary>The narration font.</summary>
    public VramRegion FontOne => new(vram, IntroVramLayout.FontOne);

    /// <summary>The blanked Japanese glyph area.</summary>
    public VramRegion JapaneseBlankCharacters => new(vram, IntroVramLayout.JapaneseBlankCharacters);

    /// <summary>The Samus portrait tilemap.</summary>
    public VramRegion PortraitTilemap => new(vram, IntroVramLayout.PortraitTilemap);

    /// <summary>The narration tilemap.</summary>
    public VramRegion NarrationTilemap => new(vram, IntroVramLayout.NarrationTilemap);

    /// <summary>The scene tilemap pages.</summary>
    public VramRegion BackgroundPages => new(vram, IntroVramLayout.BackgroundPages);

    /// <summary>Intro OBJ characters.</summary>
    public VramRegion IntroObjectCharacters => new(vram, IntroVramLayout.IntroObjectCharacters);

    /// <summary>Cinematic OBJ characters.</summary>
    public VramRegion CinematicObjectCharacters => new(vram, IntroVramLayout.CinematicObjectCharacters);
}
