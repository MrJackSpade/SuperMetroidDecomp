using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Indexed artwork and ordered compositions for map sprites; palette inheritance retains live animation.</summary>
public sealed class MapSpriteCatalog
{
    private readonly FrameSet frames;
    private readonly MapObjectTileArtwork characters;
    private MapSpriteCatalog(FrameSet frames, MapObjectTileArtwork characters) { this.frames = frames; this.characters = characters; }
    public void Draw(ushort id, OamBuffer oam, ushort x, ushort y, ushort paletteBits) => GetFrame(id).DrawOnScreen(oam, x, y, paletteBits);
    public void LoadArtworkTo(SnesVram vram, int destinationByte) => characters.LoadTo(vram, destinationByte);
    internal int StoredReservePixelCount => characters.StoredReservePixelCount;
    internal int StoredArtworkByteCount => characters.StoredOtherByteCount;
    /// <summary>Selects an installed composition by its named native drawing role.</summary>
    private SpriteComposition GetFrame(ushort id) => id switch
    {
        MapSpriteDefinitions.ArrowRight => frames.ArrowRight,
        MapSpriteDefinitions.ArrowLeft => frames.ArrowLeft,
        MapSpriteDefinitions.ArrowUp => frames.ArrowUp,
        MapSpriteDefinitions.ArrowDown => frames.ArrowDown,
        MapSpriteDefinitions.MarkerBoss => frames.MarkerBoss,
        MapSpriteDefinitions.StationEnergy => frames.StationEnergy,
        MapSpriteDefinitions.StationMissile => frames.StationMissile,
        MapSpriteDefinitions.StationMap => frames.StationMap,
        MapSpriteDefinitions.MarkerDefeatedBoss => frames.MarkerDefeatedBoss,
        MapSpriteDefinitions.MarkerGunship => frames.MarkerGunship,
        MapSpriteDefinitions.IndicatorBacking => frames.IndicatorBacking,
        MapSpriteDefinitions.IndicatorFrame0 => frames.IndicatorFrame0,
        MapSpriteDefinitions.IndicatorFrame1 => frames.IndicatorFrame1,
        MapSpriteDefinitions.IndicatorFrame2 => frames.IndicatorFrame2,
        MapSpriteDefinitions.ElevatorCrateria => frames.ElevatorCrateria,
        MapSpriteDefinitions.ElevatorBrinstar => frames.ElevatorBrinstar,
        MapSpriteDefinitions.ElevatorNorfair => frames.ElevatorNorfair,
        MapSpriteDefinitions.ElevatorWreckedShip => frames.ElevatorWreckedShip,
        MapSpriteDefinitions.ElevatorMaridia => frames.ElevatorMaridia,
        MapSpriteDefinitions.WorldTitle => frames.WorldTitle,
        MapSpriteDefinitions.WorldCrateria => frames.WorldCrateria,
        MapSpriteDefinitions.WorldBrinstar => frames.WorldBrinstar,
        MapSpriteDefinitions.WorldNorfair => frames.WorldNorfair,
        MapSpriteDefinitions.WorldWreckedShip => frames.WorldWreckedShip,
        MapSpriteDefinitions.WorldMaridia => frames.WorldMaridia,
        MapSpriteDefinitions.WorldTourian => frames.WorldTourian,
        _ => throw new KeyNotFoundException($"The given key '{id}' was not present in the dictionary."),
    };

    private sealed record FrameSet(
        SpriteComposition ArrowRight,
        SpriteComposition ArrowLeft,
        SpriteComposition ArrowUp,
        SpriteComposition ArrowDown,
        SpriteComposition MarkerBoss,
        SpriteComposition StationEnergy,
        SpriteComposition StationMissile,
        SpriteComposition StationMap,
        SpriteComposition MarkerDefeatedBoss,
        SpriteComposition MarkerGunship,
        SpriteComposition IndicatorBacking,
        SpriteComposition IndicatorFrame0,
        SpriteComposition IndicatorFrame1,
        SpriteComposition IndicatorFrame2,
        SpriteComposition ElevatorCrateria,
        SpriteComposition ElevatorBrinstar,
        SpriteComposition ElevatorNorfair,
        SpriteComposition ElevatorWreckedShip,
        SpriteComposition ElevatorMaridia,
        SpriteComposition WorldTitle,
        SpriteComposition WorldCrateria,
        SpriteComposition WorldBrinstar,
        SpriteComposition WorldNorfair,
        SpriteComposition WorldWreckedShip,
        SpriteComposition WorldMaridia,
        SpriteComposition WorldTourian);

    public static MapSpriteCatalog Load(Stream json, Stream png)
    {
        MapSpriteDocument document;
        try { document = JsonAssetDocument.Read<MapSpriteDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map sprite document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map sprite JSON.", error); }
        if (document.Version != MapSpriteFormat.Version || document.Frames is null || document.Frames.Count != MapSpriteDefinitions.Count)
            throw new InvalidDataException("Map sprite content requires version 1 and all 26 named frames.");
        var frames = new FrameSet(
            Require("Arrow.Right"),
            Require("Arrow.Left"),
            Require("Arrow.Up"),
            Require("Arrow.Down"),
            Require("Marker.Boss"),
            Require("Station.Energy"),
            Require("Station.Missile"),
            Require("Station.Map"),
            Require("Marker.DefeatedBoss"),
            Require("Marker.Gunship"),
            Require("Indicator.Backing"),
            Require("Indicator.Frame0"),
            Require("Indicator.Frame1"),
            Require("Indicator.Frame2"),
            Require("Elevator.Crateria"),
            Require("Elevator.Brinstar"),
            Require("Elevator.Norfair"),
            Require("Elevator.WreckedShip"),
            Require("Elevator.Maridia"),
            Require("World.Title"),
            Require("World.Crateria"),
            Require("World.Brinstar"),
            Require("World.Norfair"),
            Require("World.WreckedShip"),
            Require("World.Maridia"),
            Require("World.Tourian"));
        SpriteComposition Require(string name)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > MapSpriteFormat.MaximumParts)
                throw new InvalidDataException($"Map sprite {name} requires an ordered array of at most 128 parts.");
            return MenuSpriteCompiler.Compile(parts, name);
        }
        var image = IndexedPng.Read(png, MapSpriteFormat.Width, MapSpriteFormat.Height);
        return new(frames, new MapObjectTileArtwork(image));
    }
}
public sealed record MapSpriteDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
public static class MapSpriteFormat
{
    public const int Version = 1;
    public const string JsonFile = "map-sprites.json", PngFile = "map-objects.png";
    public const int TileColumns = 16, TileRows = 16;
    public const int Width = TileColumns * 8, Height = TileRows * 8;
    public const int MaximumParts = 128;
    /// <summary>$B6:C000 shared menu object characters, transferred to the active menu's OBJ base.</summary>
    public const int SourceAddress = 0xb6c000;
    public const int ByteCount = Width * Height / 2;
    /// <summary>File-select OBSEL=$03 places shared menu OBJ characters at VRAM byte $C000.</summary>
    public const int FileSelectDestination = 0xc000;
    /// <summary>Pause OBSEL=$01 places the same shared characters at VRAM byte $4000.</summary>
    public const int PauseDestination = 0x4000;
}
