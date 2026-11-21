using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The place the sound driver keeps each library's state at (#2604): test code reading the driver's memory finds the
/// libraries' bytes side by side in queue order, the first library's first, and names each library by its place there.
/// </summary>
internal static class SoundDriverLibraryPlaces
{
    /// <summary>The number the driver's first library has; each later place holds the next number's.</summary>
    private const int FirstLibraryNumber = 1;

    /// <summary>The library whose state the driver keeps at zero-based <paramref name="place"/>.</summary>
    /// <exception cref="InvalidDataException">No library is kept there.</exception>
    internal static SoundEffectLibrary At(int place) => SoundLibraryNumbers.Decode(place + FirstLibraryNumber);
}
