namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The installed pre-coloured enemy sheets (#1392): one sheet per enemy as a room graphics set loads it, with the frames
/// its behaviour shows, coloured in the palette its row holds once the set has loaded. Identical sheets are installed
/// once; the manifest names each set's sheet for each of its enemies.
/// </summary>
public static class EnemySheetDefinitions
{
    /// <summary>The installation directory holding the sheets and their manifest.</summary>
    public const string DirectoryName = "enemy-sheets";

    /// <summary>The manifest naming each graphics set's sheet for each of its enemies.</summary>
    public const string ManifestFileName = "enemy-sheets.json";

    /// <summary>The manifest revision this build reads.</summary>
    public const int ManifestVersion = 6;

    /// <summary>The spawned-object sheet frame showing a frame an encounter draws itself (#1392).</summary>
    public static string EncounterFrameName(EnemyFrameId frame) => $"Encounter.{frame}";

    /// <summary>The sheet frame showing an ordinary enemy frame.</summary>
    public static string FrameName(EnemyFrameId frame) => frame.ToString();

    /// <summary>The sheet frame showing component <paramref name="component"/> of an extended enemy frame.</summary>
    public static string ComponentFrameName(EnemyFrameId frame, int component) => $"{frame}.{component}";

    /// <summary>The spawned-object sheet frame showing an enemy projectile frame.</summary>
    internal static string ProjectileFrameName(Projectiles.EnemyProjectileFrameId frame) => $"Projectile.{frame}";

    /// <summary>The spawned-object sheet frame showing a room sprite-object frame by the enemy frame it shows.</summary>
    internal static string SpriteObjectFrameName(EnemyFrameId frame) => $"SpriteObject.{frame}";
}
