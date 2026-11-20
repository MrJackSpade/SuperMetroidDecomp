namespace SuperMetroid.AssetExtraction.Frontend.FileSelect;

/// <summary>Cartridge values and helpers of <see cref="FileSelectCursorPositions"/> that only verification reads.</summary>
internal static class FileSelectCursorPositionsConstants
{
    /// <summary>Slots A-C followed by Copy, Clear and Exit.</summary>
    internal const int MainSelectionCount = 6;
    /// <summary>Slots A-C followed by Exit.</summary>
    internal const int DataSelectionCount = 4;
}
