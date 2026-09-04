namespace SuperMetroid.Core.Frontend;

/// <summary>Stable identifiers and geometry for the human-readable game-save envelope.</summary>
public static class GameSaveJsonFormat
{
    /// <summary>First explicitly versioned JSON save schema.</summary>
    public const int SchemaVersion = 1;

    /// <summary>Bytes shown on each offset-labelled preservation page.</summary>
    public const int PreservationPageByteCount = 0x0100;

    /// <summary>Extension replacing the emulator-oriented <c>.srm</c> host file.</summary>
    public const string FileExtension = ".save.json";
}
