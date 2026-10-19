namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Typed view of the title screen's composed memory image (#630, #1358).</summary>
[PpuLayoutBoundary("rendering view: title screen VRAM")]
public readonly struct TitleVram(SnesVram vram)
{
    /// <summary>The title OBJ characters.</summary>
    public VramRegion ObjectCharacters => new(vram, TitleVramLayout.ObjectCharacters);

    /// <summary>Writes one Baby Metroid character page through the Mode 7 high-byte port, keeping the map bytes.</summary>
    public void LoadBabyMetroidPage(ReadOnlySpan<byte> pixels)
    {
        Ensure.LengthEqual(pixels, TitleVramLayout.BabyMetroidCharacters.Count);
        new Mode7Vram(vram).LoadCharacters(pixels, TitleVramLayout.BabyMetroidCharacters.Offset);
    }
}
