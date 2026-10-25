namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Where the cartridge writes Spore Spawn's colour images in its colour memory: verification's native view of the colour sets.</summary>
internal static class SporeSpawnColorDestinations
{
    /// <summary>First CGRAM color index for the independent spore palette.</summary>
    public const int SporeDestination = 240;
    /// <summary>First CGRAM color index for Spore Spawn's body palette.</summary>
    public const int SpriteDestination = 144;
    /// <summary>First CGRAM color index for the death sequence's level-graphics row.</summary>
    public const int LevelDestination = 64;
    /// <summary>First CGRAM color index for the death sequence's background-graphics row.</summary>
    public const int BackgroundDestination = 112;
    /// <summary>Returns the CGRAM start of a selected death-sequence palette row.</summary>
    public static int DeathDestination(SporeSpawnDeathPaletteLayer layer) => layer switch
    {
        SporeSpawnDeathPaletteLayer.Sprite => SpriteDestination,
        SporeSpawnDeathPaletteLayer.Level => LevelDestination,
        SporeSpawnDeathPaletteLayer.Background => BackgroundDestination,
        _ => throw new ArgumentOutOfRangeException(nameof(layer)),
    };
}
