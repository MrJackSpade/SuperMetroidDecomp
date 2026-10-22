namespace SuperMetroid.Core.Hardware;

/// <summary>What shows over runs of background character VRAM words (#1392): the artwork a picture was coloured over.</summary>
public interface IBgCharacterArtwork
{
    /// <summary>
    /// The artwork that shows over <paramref name="wordCount"/> VRAM words from <paramref name="firstWord"/>, and how far
    /// into it they begin.
    /// </summary>
    /// <exception cref="InvalidDataException">Nothing shows over the words, or what shows newest covers only some.</exception>
    BgArtworkShown Showing(int firstWord, int wordCount);
}
