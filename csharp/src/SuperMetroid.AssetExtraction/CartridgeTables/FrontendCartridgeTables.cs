namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where the cartridge stores the front-end tables the title and map extractors read.</summary>
internal static class FrontendCartridgeTables
{
    /// <summary><c>$82:C80B</c>, bank-$82 save-point map-coordinate-list pointers per area.</summary>
    internal const int SavePointMapPointers = 0x82c80b;

    /// <summary>$8C:BC5D, TitleSequenceHDMATables: sixteen pointers indexed by zoom bits 4..7.</summary>
    internal const int FixedColorPointers = 0x8cbc5d;
}
