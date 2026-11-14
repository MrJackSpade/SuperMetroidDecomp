using System.Collections.Frozen;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Decodes the sound driver's library number, as the extracted sound catalog and the driver's ports number the
/// libraries, into the library it names, and encodes a library back to that number (#1553). This is the only place the
/// game's sound libraries meet their cartridge numbering.
/// </summary>
[EncodingOwner("library numbers of the sound driver's three sound libraries")]
internal static class SoundLibraryNumbers
{
    /// <summary>Each library's number.</summary>
    private static readonly FrozenDictionary<SoundEffectLibrary, int> Numbers = new Dictionary<SoundEffectLibrary, int>
    {
        [SoundEffectLibrary.Samus] = 1,
        [SoundEffectLibrary.Enemies] = 2,
        [SoundEffectLibrary.Room] = 3,
    }.ToFrozenDictionary();

    /// <summary>Each number's library.</summary>
    private static readonly FrozenDictionary<int, SoundEffectLibrary> Libraries = Numbers.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The library sound library <paramref name="number"/> is.</summary>
    /// <exception cref="InvalidDataException">No library has the number.</exception>
    internal static SoundEffectLibrary Decode(int number) => Libraries.TryGetValue(number, out SoundEffectLibrary library)
        ? library
        : throw new InvalidDataException($"Unknown sound library {number}.");

    /// <summary>The number the sound driver knows <paramref name="library"/> by.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is not a sound library.</exception>
    internal static int Number(SoundEffectLibrary library) => Numbers.TryGetValue(library, out int number)
        ? number
        : throw new ArgumentOutOfRangeException(nameof(library), library, "Unknown sound-effect library.");

    /// <summary>
    /// Where the sound driver, and the game's handler state beside it, keeps <paramref name="library"/>'s state: zero-based,
    /// one below its number, which is also the order the driver serves the libraries in (#1755).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="library"/> is not a sound library.</exception>
    internal static int DriverIndex(SoundEffectLibrary library) => Number(library) - 1;

    /// <summary>The library whose state the sound driver keeps at zero-based <paramref name="index"/>.</summary>
    /// <exception cref="InvalidDataException">No library is kept there.</exception>
    internal static SoundEffectLibrary FromDriverIndex(int index) => Decode(index + 1);
}
