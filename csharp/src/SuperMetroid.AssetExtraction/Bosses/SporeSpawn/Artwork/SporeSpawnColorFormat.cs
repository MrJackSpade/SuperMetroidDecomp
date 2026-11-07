namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>Installed filename and supported schema revision for the boss's bounded editable palette images.</summary>
public static class SporeSpawnColorFormat
{
    /// <summary>Installed editable JSON filename for spore, health, sprite-death, level-death, and background-death colors.</summary>
    public const string FileName = "spore-spawn-colors.json";
    /// <summary>Supported schema revision, requiring the frame counts and sixteen RGB5 colors per row.</summary>
    public const int Version = 1;

    /// <summary>The colours in each Spore Spawn colour image, including the transparent first colour.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The body images of the death sequence.</summary>
    public const int DeathSpriteFrameCount = 8;

    /// <summary>The room foreground and background images of the death sequence.</summary>
    public const int DeathSceneFrameCount = 7;
}
