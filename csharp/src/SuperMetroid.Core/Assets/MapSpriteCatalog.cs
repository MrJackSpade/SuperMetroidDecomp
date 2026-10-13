using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Assets;

/// <summary>Indexed artwork and ordered compositions for map sprites; palette inheritance retains live animation.</summary>
public sealed class MapSpriteCatalog
{
    /// <summary>Selected map marker, elevator, indicator, and label compositions.</summary>
    private readonly FrameSet frames;
    /// <summary>Shared 4-bpp map object characters loaded into the current menu's OBJ region.</summary>
    internal readonly MapObjectTileArtwork characters;
    /// <summary>Retains validated compositions and the decoded indexed character atlas.</summary>
    private MapSpriteCatalog(FrameSet frames, MapObjectTileArtwork characters) { this.frames = frames; this.characters = characters; }
    /// <summary>Draws a named map marker, elevator, label, or indicator at a screen-pixel anchor.</summary>
    public void Draw(MapSpriteId id, OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
    {
        var label = GetWorldLabel(id);
        if (label is not null) { label.Draw(oam, origin, paletteBits); return; }
        var frame = GetFrame(id);
        if (frame is not null) { frame.DrawOnScreen(oam, origin, paletteBits); return; }
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        for (int index = 0; index < MapMarkerGeometry.PartCount(id); index++)
        {
            var part = MapMarkerGeometry.Part(id, index);
            oam.AddOnScreenSpritePart(part.X, part.Y, part.Attributes.WithPaletteBits(paletteBits), origin);
        }
    }
    /// <summary>Loads the compiled shared 4-bpp map-object characters into the screen's OBJ character region.</summary>
    public void LoadArtworkTo(VramRegion destination) => characters.LoadTo(destination);
    /// <summary>Selects an installed composition by its named native drawing role.</summary>
    internal SpriteComposition? GetFrame(MapSpriteId id) => id switch
    {
        MapSpriteId.ArrowRight => frames.ArrowRight,
        MapSpriteId.ArrowLeft => frames.ArrowLeft,
        MapSpriteId.ArrowUp => frames.ArrowUp,
        MapSpriteId.ArrowDown => frames.ArrowDown,
        MapSpriteId.MarkerBoss => frames.MarkerBoss,
        MapSpriteId.StationEnergy => frames.StationEnergy,
        MapSpriteId.StationMissile => frames.StationMissile,
        MapSpriteId.StationMap => frames.StationMap,
        MapSpriteId.MarkerDefeatedBoss => frames.MarkerDefeatedBoss,
        MapSpriteId.MarkerGunship => frames.MarkerGunship,
        MapSpriteId.IndicatorBacking => frames.IndicatorBacking,
        MapSpriteId.IndicatorFrame0 => frames.IndicatorFrame0,
        MapSpriteId.IndicatorFrame1 => frames.IndicatorFrame1,
        MapSpriteId.IndicatorFrame2 => frames.IndicatorFrame2,
        MapSpriteId.ElevatorCrateria => frames.ElevatorCrateria,
        MapSpriteId.ElevatorBrinstar => frames.ElevatorBrinstar,
        MapSpriteId.ElevatorNorfair => frames.ElevatorNorfair,
        MapSpriteId.ElevatorWreckedShip => frames.ElevatorWreckedShip,
        MapSpriteId.ElevatorMaridia => frames.ElevatorMaridia,
        MapSpriteId.WorldTitle => frames.WorldTitle,
        _ => throw new KeyNotFoundException($"The given key '{id}' was not present in the dictionary."),
    };

    /// <summary>Selects the six world-map label compositions, leaving ordinary markers to the frame path.</summary>
    internal WorldMapLabelComposition? GetWorldLabel(MapSpriteId id) => id switch
    {
        MapSpriteId.WorldCrateria => frames.WorldCrateria,
        MapSpriteId.WorldBrinstar => frames.WorldBrinstar,
        MapSpriteId.WorldNorfair => frames.WorldNorfair,
        MapSpriteId.WorldWreckedShip => frames.WorldWreckedShip,
        MapSpriteId.WorldMaridia => frames.WorldMaridia,
        MapSpriteId.WorldTourian => frames.WorldTourian,
        MapSpriteId.ArrowRight or MapSpriteId.ArrowLeft or MapSpriteId.ArrowUp or MapSpriteId.ArrowDown or
            MapSpriteId.MarkerBoss or MapSpriteId.StationEnergy or MapSpriteId.StationMissile or
            MapSpriteId.StationMap or MapSpriteId.MarkerDefeatedBoss or MapSpriteId.MarkerGunship or
            MapSpriteId.IndicatorBacking or MapSpriteId.IndicatorFrame0 or MapSpriteId.IndicatorFrame1 or
            MapSpriteId.IndicatorFrame2 or MapSpriteId.ElevatorCrateria or MapSpriteId.ElevatorBrinstar or
            MapSpriteId.ElevatorNorfair or MapSpriteId.ElevatorWreckedShip or MapSpriteId.ElevatorMaridia or
            MapSpriteId.WorldTitle => null,
        _ => throw new InvalidOperationException($"Undefined MapSpriteId {id}."),
    };

    /// <summary>Retains the 26 named compositions in their drawing roles after JSON validation.</summary>
    private sealed record FrameSet(
        SpriteComposition? ArrowRight,
        SpriteComposition? ArrowLeft,
        SpriteComposition? ArrowUp,
        SpriteComposition? ArrowDown,
        SpriteComposition? MarkerBoss,
        SpriteComposition? StationEnergy,
        SpriteComposition? StationMissile,
        SpriteComposition? StationMap,
        SpriteComposition? MarkerDefeatedBoss,
        SpriteComposition? MarkerGunship,
        SpriteComposition? IndicatorBacking,
        SpriteComposition? IndicatorFrame0,
        SpriteComposition? IndicatorFrame1,
        SpriteComposition? IndicatorFrame2,
        SpriteComposition? ElevatorCrateria,
        SpriteComposition? ElevatorBrinstar,
        SpriteComposition? ElevatorNorfair,
        SpriteComposition? ElevatorWreckedShip,
        SpriteComposition? ElevatorMaridia,
        SpriteComposition? WorldTitle,
        WorldMapLabelComposition WorldCrateria,
        WorldMapLabelComposition WorldBrinstar,
        WorldMapLabelComposition WorldNorfair,
        WorldMapLabelComposition WorldWreckedShip,
        WorldMapLabelComposition WorldMaridia,
        WorldMapLabelComposition WorldTourian);

    /// <summary>Loads all 26 named sprite compositions and their indexed character atlas.</summary>
    public static MapSpriteCatalog Load(Stream json, Stream png)
    {
        MapSpriteDocument document;
        try { document = JsonAssetDocument.Read<MapSpriteDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map sprite document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map sprite JSON.", error); }
        if (document.Version != MapSpriteFormat.Version || document.Frames is null || document.Frames.Count != MapSpriteDefinitions.Count)
            throw new InvalidDataException("Map sprite content requires version 1 and all 26 named frames.");
        var frames = new FrameSet(
            Require("Arrow.Right", MapSpriteId.ArrowRight),
            Require("Arrow.Left", MapSpriteId.ArrowLeft),
            Require("Arrow.Up", MapSpriteId.ArrowUp),
            Require("Arrow.Down", MapSpriteId.ArrowDown),
            Require("Marker.Boss", MapSpriteId.MarkerBoss),
            Require("Station.Energy", MapSpriteId.StationEnergy),
            Require("Station.Missile", MapSpriteId.StationMissile),
            Require("Station.Map", MapSpriteId.StationMap),
            Require("Marker.DefeatedBoss", MapSpriteId.MarkerDefeatedBoss),
            Require("Marker.Gunship", MapSpriteId.MarkerGunship),
            Require("Indicator.Backing", MapSpriteId.IndicatorBacking),
            Require("Indicator.Frame0", MapSpriteId.IndicatorFrame0),
            Require("Indicator.Frame1", MapSpriteId.IndicatorFrame1),
            Require("Indicator.Frame2", MapSpriteId.IndicatorFrame2),
            Require("Elevator.Crateria", MapSpriteId.ElevatorCrateria),
            Require("Elevator.Brinstar", MapSpriteId.ElevatorBrinstar),
            Require("Elevator.Norfair", MapSpriteId.ElevatorNorfair),
            Require("Elevator.WreckedShip", MapSpriteId.ElevatorWreckedShip),
            Require("Elevator.Maridia", MapSpriteId.ElevatorMaridia),
            Require("World.Title", MapSpriteId.WorldTitle),
            RequireWorld("World.Crateria", MapSpriteId.WorldCrateria),
            RequireWorld("World.Brinstar", MapSpriteId.WorldBrinstar),
            RequireWorld("World.Norfair", MapSpriteId.WorldNorfair),
            RequireWorld("World.WreckedShip", MapSpriteId.WorldWreckedShip),
            RequireWorld("World.Maridia", MapSpriteId.WorldMaridia),
            RequireWorld("World.Tourian", MapSpriteId.WorldTourian));
        SpriteComposition? Require(string name, MapSpriteId id)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > MapSpriteFormat.MaximumParts)
                throw new InvalidDataException($"Map sprite {name} requires an ordered array of at most 128 parts.");
            return MapMarkerGeometry.Matches(id, parts) ? null : MenuSpriteCompiler.Compile(parts, name);
        }
        WorldMapLabelComposition RequireWorld(string name, MapSpriteId id)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > MapSpriteFormat.MaximumParts)
                throw new InvalidDataException($"Map sprite {name} requires an ordered array of at most 128 parts.");
            return WorldMapLabelComposition.Compile(id, parts, name);
        }
        var image = IndexedPng.Read(png, new(MapSpriteFormat.Width, MapSpriteFormat.Height));
        return new(frames, new MapObjectTileArtwork(image));
    }
}
/// <summary>Defines every named map-screen sprite composition.</summary>
public sealed record MapSpriteDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the exact 26-frame dictionary of ordered OBJ parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
/// <summary>Defines the map sprite document, atlas geometry, and native transfer sizes.</summary>
public static class MapSpriteFormat
{
    /// <summary>Supported map sprite schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON composition file for map markers and world labels.</summary>
    public const string JsonFile = "map-sprites.json";
    /// <summary>Indexed PNG file for shared map object characters.</summary>
    public const string PngFile = "map-objects.png";
    /// <summary>Sixteen eight-pixel character columns in the map object atlas.</summary>
    public const int TileColumns = 16;
    /// <summary>Sixteen eight-pixel character rows in the map object atlas.</summary>
    public const int TileRows = 16;
    /// <summary>Atlas width derived from its character columns.</summary>
    public const int Width = TileColumns * 8;
    /// <summary>Atlas height derived from its character rows.</summary>
    public const int Height = TileRows * 8;
    /// <summary>Largest supported number of ordered OBJ parts in one composition.</summary>
    public const int MaximumParts = 128;
    /// <summary>$B6:C000 shared menu object characters, transferred to the active menu's OBJ base.</summary>
    public const int SourceAddress = 0xb6c000;
    /// <summary>Compiled byte count of the 128-by-128-pixel 4-bpp atlas.</summary>
    public const int ByteCount = Width * Height / 2;
    /// <summary>File-select OBSEL=$03 places shared menu OBJ characters at VRAM byte $C000.</summary>
    public const int FileSelectDestination = 0xc000;
    /// <summary>Pause OBSEL=$01 places the same shared characters at VRAM byte $4000.</summary>
    public const int PauseDestination = 0x4000;
}
