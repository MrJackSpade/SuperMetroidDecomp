namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>The installed escape-warning program document as the import reads and writes it (#1502).</summary>
public static class EscapeTypewriterDocumentDefinitions
{
    /// <summary>The supported editable escape-typewriter JSON schema version.</summary>
    public const int Version = 2;

    /// <summary>The embedded editable escape-warning program asset file name.</summary>
    public const string FileName = "escape-typewriter.json";

    /// <summary>Maximum authored warning length, in character cells.</summary>
    public const int MaximumLineLength = 32;
}
