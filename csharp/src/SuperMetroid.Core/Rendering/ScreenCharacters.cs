namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a frontend screen's background characters show (#1392): each installed artwork it loads, as the pre-coloured sheet
/// of its characters, at the VRAM words it loads them to; a later load shows over an earlier one.
/// </summary>
public sealed class ScreenCharacters
{
    /// <summary>The loads, in order.</summary>
    private readonly List<Load> loads = [];

    /// <summary>The pictures each layer reads, by its character base and depth, until the loads change.</summary>
    private readonly Dictionary<LayerCharacters, ScreenLayerPictures> layers = [];

    /// <summary>Records <paramref name="characters"/> characters of <paramref name="sheet"/>'s artwork from <paramref name="firstCharacter"/>, loaded at VRAM word <paramref name="firstWord"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds fewer characters.</exception>
    public ScreenCharacters Show(ScreenTileSheet sheet, int firstWord, int firstCharacter, int characters)
    {
        Ensure.NotNull(sheet);
        Ensure.BetweenInclusive(0, sheet.Characters - 1, firstCharacter);
        Ensure.BetweenInclusive(1, sheet.Characters - firstCharacter, characters);
        loads.Add(new Load(sheet, VramWordRange.Require(firstWord), firstCharacter, characters, 1));
        layers.Clear();
        return this;
    }

    /// <summary>Records <paramref name="copies"/> copies of <paramref name="sheet"/>'s <paramref name="character"/>, loaded one after another from VRAM word <paramref name="firstWord"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no such character.</exception>
    public ScreenCharacters ShowRepeated(ScreenTileSheet sheet, int firstWord, int character, int copies)
    {
        Ensure.NotNull(sheet);
        Ensure.BetweenInclusive(0, sheet.Characters - 1, character);
        Ensure.BetweenInclusive(1, int.MaxValue, copies);
        loads.Add(new Load(sheet, VramWordRange.Require(firstWord), character, copies, 0));
        layers.Clear();
        return this;
    }

    /// <summary>What a layer of <paramref name="depth"/> characters from VRAM word <paramref name="characterBase"/> shows.</summary>
    [PpuLayoutBoundary("codec: the VRAM words a layer's character indices name, wrapping as VRAM addresses do")]
    public ScreenLayerPictures Pictures(int characterBase, BgBitDepth depth)
    {
        var layer = new LayerCharacters(characterBase, depth);
        if (layers.TryGetValue(layer, out ScreenLayerPictures? known))
            return known;
        int words = ScreenTileSheet.CharacterWordsOf(depth);
        // A character's address wraps past VRAM's last word.
        const int vramWords = SnesPpuLayout.VramByteCount / sizeof(ushort);
        var tiles = new ScreenTilePicture?[ScreenLayerPictures.CharacterIndices];
        for (int index = 0; index < tiles.Length; index++)
            tiles[index] = Showing((characterBase + index * words) % vramWords, depth);
        var pictures = new ScreenLayerPictures([.. tiles], depth);
        layers.Add(layer, pictures);
        return pictures;
    }

    /// <summary>The character the newest load over VRAM word <paramref name="word"/> shows there as a <paramref name="depth"/> character, if one does.</summary>
    private ScreenTilePicture? Showing(int word, BgBitDepth depth)
    {
        for (int index = loads.Count - 1; index >= 0; index--)
        {
            Load load = loads[index];
            int words = ScreenTileSheet.CharacterWordsOf(load.Sheet.Depth);
            int offset = word - load.FirstWord;
            if (offset < 0 || offset >= load.Characters * words)
                continue;
            // A character read at another depth, or from part way into one, shows bytes the artwork's sheet does not picture.
            return load.Sheet.Depth == depth && offset % words == 0
                ? new ScreenTilePicture(load.Sheet, load.FirstCharacter + offset / words * load.Step) : null;
        }
        return null;
    }

    /// <summary>The characters a layer reads: its character base and their depth.</summary>
    /// <param name="CharacterBase">The VRAM word of character zero.</param>
    /// <param name="Depth">The characters' depth.</param>
    private readonly record struct LayerCharacters(int CharacterBase, BgBitDepth Depth);

    /// <summary>One load.</summary>
    /// <param name="Sheet">The artwork's sheet.</param>
    /// <param name="FirstWord">The VRAM word its first character loads at.</param>
    /// <param name="FirstCharacter">The first character loaded.</param>
    /// <param name="Characters">The characters loaded.</param>
    /// <param name="Step">How far each loaded character moves through the artwork: one for consecutive characters, zero for copies of one.</param>
    private sealed record Load(ScreenTileSheet Sheet, int FirstWord, int FirstCharacter, int Characters, int Step);
}
