using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The characters a frontend screen's page names by tile number (#1467), all of one depth: runs of an artwork's characters
/// placed at consecutive tile numbers, a later run showing over an earlier one. A tile number no run places shows nothing,
/// as on the screen it was cut from.
/// </summary>
internal sealed class ScreenCharacters
{
    /// <summary>Each placed run, earliest first.</summary>
    private readonly List<(int FirstTile, ReadOnlyMemory<byte> Characters)> runs = [];

    /// <summary>Starts with no characters placed.</summary>
    /// <param name="depth">The depth of every character placed.</param>
    internal ScreenCharacters(TileInkDepth depth) => Depth = depth;

    /// <summary>The depth of every character placed.</summary>
    internal TileInkDepth Depth { get; }

    /// <summary>The bytes of one character.</summary>
    internal int CharacterBytes => ScreenTileSheet.CharacterWordsOf(Depth) * sizeof(ushort);

    /// <summary>Places <paramref name="characters"/>, whole characters, at consecutive tile numbers from <paramref name="firstTile"/>.</summary>
    /// <exception cref="InvalidDataException">The bytes are not whole characters of the depth.</exception>
    internal ScreenCharacters Place(int firstTile, ReadOnlyMemory<byte> characters)
    {
        Ensure.AtLeastZero(firstTile);
        if (characters.IsEmpty || characters.Length % CharacterBytes != 0)
            throw new InvalidDataException($"{characters.Length} bytes are not whole {Depth} characters.");
        runs.Add((firstTile, characters));
        return this;
    }

    /// <summary>The bytes of the character tile number <paramref name="tile"/> shows; false where no run places one.</summary>
    internal bool TryCharacter(int tile, out ReadOnlySpan<byte> character)
    {
        for (int run = runs.Count - 1; run >= 0; run--)
        {
            (int first, ReadOnlyMemory<byte> characters) = runs[run];
            int index = tile - first;
            if (index >= 0 && index < characters.Length / CharacterBytes)
            {
                character = characters.Span.Slice(index * CharacterBytes, CharacterBytes);
                return true;
            }
        }
        character = default;
        return false;
    }
}
