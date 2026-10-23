namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Schema, dimensions, filenames, and validation limits for editable title graphics.</summary>
public static class TitleGraphicsFormat
{
    /// <summary>Current title-graphics document schema version; version three keys sprite frames by composition name.</summary>
    public const int Version = 3;
    /// <summary>Number of title sprite frames, one per <see cref="TitleSprite"/>.</summary>
    public const int SpriteFrameCount = 31;
    /// <summary>Maximum OBJ parts accepted in one title sprite frame.</summary>
    public const int MaximumSpriteParts = 128;
    /// <summary>Width of the title OBJ character sheet in eight-pixel tiles.</summary>
    public const int ObjectTileColumns = 32;
    /// <summary>Height of the title OBJ character sheet in eight-pixel tiles.</summary>
    public const int ObjectTileRows = 16;
    /// <summary>Installed filename of the title Mode 7 character PNG.</summary>
    public const string FloorTilesFile = "title-mode7-tiles.png";
    /// <summary>Installed filename of the title Mode 7 map JSON document.</summary>
    public const string FloorMapFile = "title-mode7-map.json";
    /// <summary>Installed filename of the title OBJ character PNG.</summary>
    public const string ObjectTilesFile = "title-object-tiles.png";
    /// <summary>Installed filename of the baby Metroid Mode 7 character PNG.</summary>
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
}
