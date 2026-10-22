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
    public const int ManifestVersion = 2;

    /// <summary>
    /// The enemies whose characters change while they draw, as their bosses and corpses upload tiles at run time; they
    /// draw from those characters until they draw from pre-coloured parts, and have no sheets.
    /// </summary>
    public static IReadOnlySet<EnemyDefinitionId> DrawnFromCharacters { get; } = new HashSet<EnemyDefinitionId>
    {
        EnemyDefinitionId.CorpseSidehopper, EnemyDefinitionId.CorpseTorizo,
        EnemyDefinitionId.ShipBottomEntrance, EnemyDefinitionId.ShipTop,
        EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue,
        EnemyDefinitionId.DraygonArms, EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail,
        EnemyDefinitionId.MotherBrainBody, EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.MotherBrainTubes,
        EnemyDefinitionId.Ridley, EnemyDefinitionId.RidleyCeres, EnemyDefinitionId.RidleyExplosion,
        EnemyDefinitionId.BombTorizo, EnemyDefinitionId.GoldenTorizo,
    };

    /// <summary>The sheet frame showing an ordinary enemy frame.</summary>
    public static string FrameName(EnemyFrameId frame) => frame.ToString();

    /// <summary>The sheet frame showing component <paramref name="component"/> of an extended enemy frame.</summary>
    public static string ComponentFrameName(EnemyFrameId frame, int component) => $"{frame}.{component}";

    /// <summary>The spawned-object sheet frame showing an enemy projectile frame.</summary>
    internal static string ProjectileFrameName(Projectiles.EnemyProjectileFrameId frame) => $"Projectile.{frame}";

    /// <summary>The spawned-object sheet frame showing a room sprite-object frame by the enemy frame it shows.</summary>
    internal static string SpriteObjectFrameName(EnemyFrameId frame) => $"SpriteObject.{frame}";
}
