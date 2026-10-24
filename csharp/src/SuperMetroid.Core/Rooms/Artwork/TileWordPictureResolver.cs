using SuperMetroid.Core.Assets;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what tile words written over a room's BG1 or BG2 show (#1392): each word's picture in the room's tile word
/// sheet over what the word's character shows, Crocomire's melt among it, pictured at each of its states.
/// </summary>
public sealed class TileWordPictureResolver(TileWordSheet sheet)
{
    /// <summary>The words last resolved: a body tilemap's array or an overlay.</summary>
    private object? source;

    /// <summary>What each word's character showed when last resolved.</summary>
    private BgArtworkShown[] shown = [];

    /// <summary>The pictures last resolved, null where a word is unwritten.</summary>
    private ImmutableArray<BgTilePicture?> pictures;

    /// <summary>What each character shows this resolution, by character.</summary>
    private readonly Dictionary<int, BgArtworkShown> characters = [];

    /// <summary>The sheet.</summary>
    public TileWordSheet Sheet { get; } = Ensure.NotNull(sheet);

    /// <summary>The picture of each of a boss body <paramref name="tilemap"/>'s words over what the room's <paramref name="characters"/> show now.</summary>
    /// <exception cref="InvalidDataException">The sheet pictures a word over nothing its character shows.</exception>
    public ImmutableArray<BgTilePicture?> Resolve(ImmutableArray<ushort> tilemap, RoomCharacterSource characters)
    {
        ushort[] words = ImmutableCollectionsMarshal.AsArray(tilemap) ?? throw new ArgumentException("A default tilemap has no words.", nameof(tilemap));
        return ResolveWords(words, words.Length, index => words[index], Bg12Artwork(characters), characters.Animated);
    }

    /// <summary>The picture of each word written in <paramref name="overlay"/> over what the room's <paramref name="characters"/> show now.</summary>
    /// <exception cref="InvalidDataException">The sheet pictures a word over nothing its character shows.</exception>
    public ImmutableArray<BgTilePicture?> Resolve(RoomTilemapOverlay overlay, RoomCharacterSource characters)
    {
        ImmutableArray<int> words = Ensure.NotNull(overlay).Words;
        return ResolveWords(overlay, words.Length, index => words[index], Bg12Artwork(characters), characters.Animated);
    }

    /// <summary>What shows over the characters BG1 and BG2's words name: <paramref name="characters"/>'.</summary>
    /// <exception cref="InvalidOperationException">The sheet pictures another layer's words.</exception>
    private IBgCharacterArtwork Bg12Artwork(RoomCharacterSource characters) => Sheet.Layer == TileWordLayer.Bg12 ? characters.Artwork
        : throw new InvalidOperationException($"{Sheet.Name} pictures no {TileWordLayer.Bg12} words.");

    /// <summary>
    /// The picture of each of <paramref name="count"/> words <paramref name="wordAt"/> gives, <see cref="RoomTilemapOverlay.Unwritten"/>
    /// for none, re-resolving only the words whose characters show something else since <paramref name="words"/> was last resolved.
    /// The layer that owns the words gives them, as the BG3 plane's do (Layer3PictureResolution). A character an animation
    /// paints shows the frame <paramref name="animated"/> says it selected (#1406); any other shows what is bound over it.
    /// </summary>
    internal ImmutableArray<BgTilePicture?> ResolveWords(object words, int count, Func<int, int> wordAt, IBgCharacterArtwork bindings,
        AnimatedCharacterView animated)
    {
        int characterWords = TileWordSheet.CharacterWordsOf(Sheet.Layer);
        Ensure.NotNull(bindings);
        bool fresh = !ReferenceEquals(words, source);
        if (fresh)
        {
            source = words;
            shown = new BgArtworkShown[count];
        }
        characters.Clear();
        BgTilePicture?[]? next = null;
        for (int index = 0; index < count; index++)
        {
            int written = wordAt(index);
            if (written == RoomTilemapOverlay.Unwritten)
                continue;
            var word = new SnesBgTilemapWord((ushort)written);
            if (!characters.TryGetValue(word.CharacterIndex, out BgArtworkShown now))
            {
                if (!animated.TryShowing(word.CharacterIndex, characterWords, out now))
                    now = bindings.Showing(word.CharacterIndex, 1);
                characters.Add(word.CharacterIndex, now);
            }
            if (!fresh && now == shown[index])
                continue;
            shown[index] = now;
            next ??= fresh ? new BgTilePicture?[count] : [.. pictures];
            next[index] = Sheet.TryPicture(new BgTileArtwork(word, now), out BgTilePicture picture) ? picture
                : throw new InvalidDataException($"{Sheet.Name} pictures no tile ${word.Raw:X4} over {now.Artwork.Name} at word {now.WordOffset}.");
        }
        if (next is not null)
            pictures = [.. next];
        else if (fresh)
            pictures = [.. new BgTilePicture?[count]];
        return pictures;
    }
}
