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
    public void Draw(ushort id, OamBuffer oam, ushort x, ushort y, ushort paletteBits)
    {
        var label = GetWorldLabel(id);
        if (label is not null) { label.Draw(oam, x, y, paletteBits); return; }
        var frame = GetFrame(id);
        if (frame is not null) { frame.DrawOnScreen(oam, x, y, paletteBits); return; }
        _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
        for (int index = 0; index < MapMarkerGeometry.PartCount(id); index++)
        {
            var part = MapMarkerGeometry.Part(id, index);
            oam.AddOnScreenSpritePart(part.X, part.Y, part.Attributes.WithPaletteBits(paletteBits), x, y);
        }
    }
    internal bool StoresComposition(ushort id) => GetWorldLabel(id)?.StoresComposition ?? (GetFrame(id) is not null);
    internal int StoredLabelHorizontalCount(ushort id) => GetWorldLabel(id)?.StoredHorizontalCount ?? 0;
    public void LoadArtworkTo(SnesVram vram, int destinationByte) => characters.LoadTo(vram, destinationByte);
    internal int StoredReservePixelCount => characters.StoredReservePixelCount;
    internal int StoredArtworkByteCount => characters.StoredOtherByteCount;
    /// <summary>Selects an installed composition by its named native drawing role.</summary>
    private SpriteComposition? GetFrame(ushort id) => id switch
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
        _ => throw new KeyNotFoundException($"The given key '{id}' was not present in the dictionary."),
    };

    private WorldMapLabelComposition? GetWorldLabel(ushort id) => id switch
    {
        MapSpriteDefinitions.WorldCrateria => frames.WorldCrateria,
        MapSpriteDefinitions.WorldBrinstar => frames.WorldBrinstar,
        MapSpriteDefinitions.WorldNorfair => frames.WorldNorfair,
        MapSpriteDefinitions.WorldWreckedShip => frames.WorldWreckedShip,
        MapSpriteDefinitions.WorldMaridia => frames.WorldMaridia,
        MapSpriteDefinitions.WorldTourian => frames.WorldTourian,
        _ => null,
    };

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

    public static MapSpriteCatalog Load(Stream json, Stream png)
    {
        MapSpriteDocument document;
        try { document = JsonAssetDocument.Read<MapSpriteDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Map sprite document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map sprite JSON.", error); }
        if (document.Version != MapSpriteFormat.Version || document.Frames is null || document.Frames.Count != MapSpriteDefinitions.Count)
            throw new InvalidDataException("Map sprite content requires version 1 and all 26 named frames.");
        var frames = new FrameSet(
            Require("Arrow.Right", MapSpriteDefinitions.ArrowRight),
            Require("Arrow.Left", MapSpriteDefinitions.ArrowLeft),
            Require("Arrow.Up", MapSpriteDefinitions.ArrowUp),
            Require("Arrow.Down", MapSpriteDefinitions.ArrowDown),
            Require("Marker.Boss", MapSpriteDefinitions.MarkerBoss),
            Require("Station.Energy", MapSpriteDefinitions.StationEnergy),
            Require("Station.Missile", MapSpriteDefinitions.StationMissile),
            Require("Station.Map", MapSpriteDefinitions.StationMap),
            Require("Marker.DefeatedBoss", MapSpriteDefinitions.MarkerDefeatedBoss),
            Require("Marker.Gunship", MapSpriteDefinitions.MarkerGunship),
            Require("Indicator.Backing", MapSpriteDefinitions.IndicatorBacking),
            Require("Indicator.Frame0", MapSpriteDefinitions.IndicatorFrame0),
            Require("Indicator.Frame1", MapSpriteDefinitions.IndicatorFrame1),
            Require("Indicator.Frame2", MapSpriteDefinitions.IndicatorFrame2),
            Require("Elevator.Crateria", MapSpriteDefinitions.ElevatorCrateria),
            Require("Elevator.Brinstar", MapSpriteDefinitions.ElevatorBrinstar),
            Require("Elevator.Norfair", MapSpriteDefinitions.ElevatorNorfair),
            Require("Elevator.WreckedShip", MapSpriteDefinitions.ElevatorWreckedShip),
            Require("Elevator.Maridia", MapSpriteDefinitions.ElevatorMaridia),
            Require("World.Title", MapSpriteDefinitions.WorldTitle),
            RequireWorld("World.Crateria", MapSpriteDefinitions.WorldCrateria),
            RequireWorld("World.Brinstar", MapSpriteDefinitions.WorldBrinstar),
            RequireWorld("World.Norfair", MapSpriteDefinitions.WorldNorfair),
            RequireWorld("World.WreckedShip", MapSpriteDefinitions.WorldWreckedShip),
            RequireWorld("World.Maridia", MapSpriteDefinitions.WorldMaridia),
            RequireWorld("World.Tourian", MapSpriteDefinitions.WorldTourian));
        SpriteComposition? Require(string name, ushort id)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > MapSpriteFormat.MaximumParts)
                throw new InvalidDataException($"Map sprite {name} requires an ordered array of at most 128 parts.");
            return MapMarkerGeometry.Matches(id, parts) ? null : MenuSpriteCompiler.Compile(parts, name);
        }
        WorldMapLabelComposition RequireWorld(string name, ushort id)
        {
            if (!document.Frames.TryGetValue(name, out var parts) || parts is null || parts.Length > MapSpriteFormat.MaximumParts)
                throw new InvalidDataException($"Map sprite {name} requires an ordered array of at most 128 parts.");
            return WorldMapLabelComposition.Compile(id, parts, name);
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
