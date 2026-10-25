namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The shapes of Draygon's colour sets: the introduction, the body, the sprite, the white flash and the health bands.</summary>
public static class DraygonColorDefinitions
{
    /// <summary>
    /// The colours of the introduction's transfer: all of the first sprite palette and the first nine colours of the
    /// second, a partial introductory sprite transfer rather than the full body palette.
    /// </summary>
    public const int IntroCount = 25;

    /// <summary>The colours of the body's background palette, including its first slot.</summary>
    public const int BackgroundCount = 16;

    /// <summary>The colours of the normal sprite palette, including its transparent first slot.</summary>
    public const int SpriteCount = 16;

    /// <summary>The colours of the white flash shared by the body and sprite hurt transfers.</summary>
    public const int WhiteFlashCount = 16;

    /// <summary>The health-dependent colour rows, in descending-health order.</summary>
    public const int HealthBandCount = 8;

    /// <summary>The colours in each health row, replacing only the body's damage-sensitive colours.</summary>
    public const int HealthBandColorCount = 4;
}
