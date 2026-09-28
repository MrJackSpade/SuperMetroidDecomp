namespace SuperMetroid.Core.Assets;

/// <summary>Cartridge source ranges for the two editable Ceres escape tile sheets.</summary>
internal static class CeresEscapeTileRomData
{
    /// <summary>$B7:DA00, the nine-tile-row escape warning-text character page.</summary>
    internal const int WarningTextSource = 0xb7da00;

    /// <summary>$B7:DA00-$B7:E2FF, the complete warning-text source length.</summary>
    internal const int WarningTextByteCount = 0x0900;

    /// <summary>$B0:BA00, the Ceres escape door character page.</summary>
    internal const int DoorSource = 0xb0ba00;

    /// <summary>$B0:BA00-$B0:BFFF, the complete door-character source length.</summary>
    internal const int DoorByteCount = 0x0600;
}
