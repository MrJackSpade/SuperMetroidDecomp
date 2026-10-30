using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The characters a frontend screen's page names by tile number (#1467), all of one depth: runs of an artwork's characters
/// placed at consecutive tile numbers, a later run showing over an earlier one. A tile number no run places shows nothing,
/// as on the screen it was cut from.
/// </summary>
internal sealed class ScreenCharacters
{
    /// <summary>Each placed run, earliest first.</summary>
    private readonly List<ScreenCharacterRun> runs = [];

    /// <summary>Starts with no characters placed.</summary>
    /// <param name="depth">The depth of every character placed.</param>
    internal ScreenCharacters(TileInkDepth depth) => Depth = depth;

    /// <summary>The depth of every character placed.</summary>
    internal TileInkDepth Depth { get; }

    /// <summary>The bytes of one character.</summary>
    internal int CharacterBytes => Depth == TileInkDepth.SixteenColours ? RoomTileCanvas.CharacterBytes : RoomTileCanvas.TwoBppCharacterBytes;

    /// <summary>How a sheet of pictures painted from these characters is coloured.</summary>
    internal SpriteImageColours Colours => Depth == TileInkDepth.SixteenColours ? SpriteImageColours.Background : SpriteImageColours.TwoBppBackground;

    /// <summary>Places <paramref name="characters"/>, whole characters, at consecutive tile numbers from <paramref name="firstTile"/>.</summary>
    /// <exception cref="InvalidDataException">The bytes are not whole characters of the depth.</exception>
    internal ScreenCharacters Place(int firstTile, ReadOnlyMemory<byte> characters)
    {
        Ensure.AtLeastZero(firstTile);
        if (characters.IsEmpty || characters.Length % CharacterBytes != 0)
            throw new InvalidDataException($"{characters.Length} bytes are not whole {Depth} characters.");
        runs.Add(new ScreenCharacterRun(firstTile, characters));
        return this;
    }

    /// <summary>The bytes of the character tile number <paramref name="tile"/> shows; false where no run places one.</summary>
    internal bool TryCharacter(int tile, out ReadOnlySpan<byte> character)
    {
        for (int run = runs.Count - 1; run >= 0; run--)
        {
            ScreenCharacterRun placed = runs[run];
            int index = tile - placed.FirstTile;
            if (index >= 0 && index < placed.Characters.Length / CharacterBytes)
            {
                character = placed.Characters.Span.Slice(index * CharacterBytes, CharacterBytes);
                return true;
            }
        }
        character = default;
        return false;
    }
}
