namespace SuperMetroid.Core.Hardware;

/// <summary>What shows over runs of a layer's background characters (#1392, #1406): the artwork a picture was coloured over.</summary>
public interface IBgCharacterArtwork
{
    /// <summary>
    /// The artwork that shows over <paramref name="characterCount"/> characters from <paramref name="firstCharacter"/>,
    /// and how far into it they begin.
    /// </summary>
    /// <exception cref="InvalidDataException">Nothing shows over the characters, or what shows newest covers only some.</exception>
    BgArtworkShown Showing(int firstCharacter, int characterCount);
}
