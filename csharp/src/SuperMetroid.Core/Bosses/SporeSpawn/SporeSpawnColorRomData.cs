namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Authored bank-$A5 RGB5 images selected by Spore Spawn's compiled AI.</summary>
public static class SporeSpawnColorRomData
{
    /// <summary>Sprite palette 7 for spores at $A5:E359, copied to target color 240.</summary>
    public const int SporeSource = 0xa5e359;
    /// <summary>Four sprite-palette-1 health images at $A5:E379..E3F8.</summary>
    public const int HealthSource = 0xa5e379;
    /// <summary>Eight sprite-palette-1 death images at $A5:E3F9..E4F8.</summary>
    public const int DeathSpriteSource = 0xa5e3f9;
    /// <summary>Seven level-graphics palette-4 death images at $A5:E4F9..E5D8.</summary>
    public const int DeathLevelSource = 0xa5e4f9;
    /// <summary>Seven background-graphics palette-7 death images at $A5:E5D9..E6B8.</summary>
    public const int DeathBackgroundSource = 0xa5e5d9;

    /// <summary>Number of RGB555 words in each complete palette image.</summary>
    public const int ColorsPerFrame = 16;
    /// <summary>Number of health-dependent sprite palette images.</summary>
    public const int HealthFrameCount = 4;
    /// <summary>Number of sprite palette images in the death sequence.</summary>
    public const int DeathSpriteFrameCount = 8;
    /// <summary>Number of level and background palette images in the death sequence.</summary>
    public const int DeathSceneFrameCount = 7;
    /// <summary>Byte stride of one native 16-color RGB555 palette image.</summary>
    public const int FrameByteCount = ColorsPerFrame * sizeof(ushort);

    /// <summary>Converts the native instruction's byte offset to an authored frame index.</summary>
    public static int FrameFromByteOffset(ushort offset, int frameCount)
    {
        if (offset % FrameByteCount != 0 || offset / FrameByteCount >= frameCount)
            throw new InvalidDataException(
                $"Spore Spawn palette offset ${offset:X4} is not a catalogued frame.");
        return offset / FrameByteCount;
    }
}
