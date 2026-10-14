namespace SuperMetroid.Core.Assets;

/// <summary>Installed filename and supported schema revision for the boss's bounded editable palette images.</summary>
public static class SporeSpawnColorFormat
{
    /// <summary>Installed editable JSON filename for spore, health, sprite-death, level-death, and background-death colors.</summary>
    public const string FileName = "spore-spawn-colors.json";
    /// <summary>Supported schema revision, requiring the native frame counts and sixteen RGB5 colors per row.</summary>
    public const int Version = 1;
}
