namespace SuperMetroid.Core.Game;

/// <summary>One of the three independent palette rows advanced by Spore Spawn's death script.</summary>
public enum SporeSpawnDeathPaletteLayer
{
    /// <summary>Spore Spawn's sprite-palette row at CGRAM 144.</summary>
    Sprite,
    /// <summary>The level-graphics palette row at CGRAM 64.</summary>
    Level,
    /// <summary>The background-graphics palette row at CGRAM 112.</summary>
    Background,
}
