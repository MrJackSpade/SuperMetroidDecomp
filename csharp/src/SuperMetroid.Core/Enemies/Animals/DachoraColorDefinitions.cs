namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The shape of Dachora's colour sets: one default set, and the four-image speed and stored-shine cycles.</summary>
public static class DachoraColorDefinitions
{
    /// <summary>The colours in each Dachora colour set, including the transparent first colour.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The ordered images in each speed or stored-shine colour cycle.</summary>
    public const int AnimatedFrameCount = 4;
}
