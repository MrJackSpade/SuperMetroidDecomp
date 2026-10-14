namespace SuperMetroid.Core.Audio;

/// <summary>Domain-owned transitions over <see cref="ApuPort"/>.</summary>
public static class ApuPorts
{
    /// <summary>
    /// The port that carries zero-based sound library <paramref name="libraryIndex"/>: the cartridge
    /// writes library queue X to <c>$2141+X</c>.
    /// </summary>
    public static ApuPort ForSoundLibrary(int libraryIndex) => libraryIndex switch
    {
        0 => ApuPort.SoundLibrary1,
        1 => ApuPort.SoundLibrary2,
        2 => ApuPort.SoundLibrary3,
        _ => throw new InvalidOperationException($"Sound library {libraryIndex} has no {nameof(ApuPort)}."),
    };
}
