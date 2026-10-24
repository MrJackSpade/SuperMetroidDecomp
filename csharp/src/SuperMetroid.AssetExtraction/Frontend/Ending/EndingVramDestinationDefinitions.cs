namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The VRAM words the ending's post-shot uploads land at, from its upload records at <c>$8B:E45A</c>.</summary>
internal static class EndingVramDestinationDefinitions
{
    /// <summary>The subtitle's VRAM word, from the first record.</summary>
    internal const ushort SubtitleWord = 0x4800;

    /// <summary>The first logo chunk's VRAM word, from the second record; later chunks follow it.</summary>
    internal const ushort FirstLogoTileWord = 0x6000;

    /// <summary>The logo map's VRAM word, from the sixth record.</summary>
    internal const ushort LogoMapWord = 0x5400;
}
