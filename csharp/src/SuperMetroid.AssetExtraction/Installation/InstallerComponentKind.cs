namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>What an installer component reads and who reads it, which decides what it installs alongside (#1530).</summary>
internal enum InstallerComponentKind
{
    /// <summary>Reads only the cartridge and its own directory; composition components read it.</summary>
    Extraction,

    /// <summary>Reads only the cartridge and its own directory, and no component reads it.</summary>
    Standalone,

    /// <summary>
    /// Reads the extraction components' output once all of them are installed; the picture store reads the sheets it composes.
    /// </summary>
    Composition,

    /// <summary>Writes the picture store from every composition component's sheets once all of them are composed (#1635).</summary>
    PictureStore,
}
