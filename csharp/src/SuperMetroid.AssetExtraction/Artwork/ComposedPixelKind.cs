namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// How a pixel the import composed against the cartridge's colours shows once installed (#1633), as its composed palette
/// index decides: the import's knowledge while it converts a sheet, never installed beside the pixel.
/// </summary>
internal enum ComposedPixelKind
{
    /// <summary>Clear, keeping no colour of its own: installed as clear black.</summary>
    Transparent,

    /// <summary>Clear, keeping a colour of its own, which a layer drawn opaque shows: installed as that colour under alpha zero.</summary>
    Clear,

    /// <summary>Opaque.</summary>
    Opaque,

    /// <summary>Opaque, and blending with what lies behind it where the sheet is drawn as sprites.</summary>
    Blending,
}
