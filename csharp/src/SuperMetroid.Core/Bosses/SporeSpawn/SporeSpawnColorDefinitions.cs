namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The shape of Spore Spawn's colour images: the spore set, the health stages and the death sequence.</summary>
public static class SporeSpawnColorDefinitions
{
    /// <summary>The colours in each Spore Spawn colour image, including the transparent first colour.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The health-stage body images, from healthy to nearly dead.</summary>
    public const int HealthFrameCount = 4;

    /// <summary>The body images of the death sequence.</summary>
    public const int DeathSpriteFrameCount = 8;

    /// <summary>The room foreground and background images of the death sequence.</summary>
    public const int DeathSceneFrameCount = 7;
}
