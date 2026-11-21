using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>Schema, dimensions, filenames, and validation limits for editable title graphics.</summary>
public static class TitleGraphicsFormat
{
    /// <summary>
    /// Current title-graphics document schema version; version three keys sprite frames by composition name, version four
    /// keys each card's frames by its card and the letters it has typed (#1979).
    /// </summary>
    public const int Version = 4;
    /// <summary>Number of whole title sprite frames, one per <see cref="TitleSprite"/>.</summary>
    public const int SpriteFrameCount = 2;
    /// <summary>Maximum OBJ parts accepted in one title sprite frame.</summary>
    public const int MaximumSpriteParts = 128;
    /// <summary>Width of the title OBJ character sheet in eight-pixel tiles.</summary>
    public const int ObjectTileColumns = 32;
    /// <summary>Height of the title OBJ character sheet in eight-pixel tiles.</summary>
    public const int ObjectTileRows = 16;
    /// <summary>Import-data filename of the title floor characters' PNG.</summary>
    public const string FloorTilesFile = "title-mode7-tiles.png";
    /// <summary>Import-data filename of the title floor map JSON document.</summary>
    public const string FloorMapFile = "title-mode7-map.json";
    /// <summary>Import-data filename of the title sprite characters' PNG.</summary>
    public const string ObjectTilesFile = "title-object-tiles.png";
    /// <summary>Import-data filename of the Baby Metroid characters' PNG.</summary>
    public const string BabyTilesFile = "title-baby-tiles.png";
    /// <summary>Width in pixels of the title Mode 7 character image.</summary>
    public const int FloorWidth = 128;
    /// <summary>Height in pixels of the title Mode 7 character image.</summary>
    public const int FloorHeight = 128;
    /// <summary>Width in cells of the title Mode 7 map.</summary>
    public const int MapWidth = 64;
    /// <summary>Height in cells of the title Mode 7 map.</summary>
    public const int MapHeight = 64;
    /// <summary>Width in pixels of the title OBJ character image.</summary>
    public const int ObjectWidth = 256;
    /// <summary>Height in pixels of the title OBJ character image.</summary>
    public const int ObjectHeight = 128;
    /// <summary>Width in pixels of the baby Metroid Mode 7 character image.</summary>
    public const int BabyWidth = 32;
    /// <summary>Height in pixels of the baby Metroid Mode 7 character image.</summary>
    public const int BabyHeight = 32;
    /// <summary>The title sprites' 512 four-bit characters.</summary>
    public const int ObjectCharacterByteCount = 512 * 32;
    /// <summary>Every <see cref="TitleSprite"/>, each exactly once: the logo and the copyright, which the import's title artwork must provide.</summary>
    public static readonly IReadOnlyList<TitleSprite> Sprites =
    [
        TitleSprite.SuperMetroidLogo, TitleSprite.NintendoCopyright,
    ];
    /// <summary>
    /// Every typing step of the four cards, each exactly once, in typing order (#1979): the import's title artwork must
    /// provide each as the whole of what it shows.
    /// </summary>
    public static readonly IReadOnlyList<TitleCardTyping> Typings =
        [.. TitleCards.All.SelectMany(TitleCards.Typings)];
}
