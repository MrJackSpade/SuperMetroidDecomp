using System.Collections.Immutable;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map and menu sprites as the import pipeline reads them (#1409, #1463): the ordered compositions of the
/// map and menu sprites, whose palette inheritance retains live animation, and the menu OBJ characters the menu sprite
/// sheets compose them over.
/// </summary>
public sealed class MapSpriteArtwork : MapSpriteCompositions
{
    /// <summary>Selected map marker, elevator, indicator, and label compositions.</summary>
    private readonly FrameSet frames;

    /// <summary>Shared 4-bpp map object characters loaded into the current menu's OBJ region.</summary>
    internal readonly MapObjectTileArtwork characters;

    /// <summary>Retains validated compositions and the decoded indexed character atlas.</summary>
    /// <param name="frames">The validated compositions.</param>
    /// <param name="characters">The decoded menu OBJ characters.</param>
    private MapSpriteArtwork(FrameSet frames, MapObjectTileArtwork characters)
    {
        this.frames = frames;
        this.characters = characters;
    }

    /// <summary>The shared menu OBJ page the catalog's compositions draw from (#1358).</summary>
    public SpriteArtwork Page => field ??= characters.Page();

    /// <summary>
    /// A named map marker, elevator, label, or indicator as a whole frame over the menu OBJ page (#1392): an area label,
    /// an installed composition, or the native marker geometry, whose parts all take their draw's palette.
    /// </summary>
    internal SpriteFrame Frame(MapSpriteId id)
    {
        string name = MapSpriteNames.Name(id);
        if (GetWorldLabel(id) is { } label)
            return label.ToFrame(name);
        if (GetFrame(id) is { } frame)
            return frame.ToFrame(name, 0);
        return new(name, [.. Enumerable.Range(0, MapMarkerGeometry.PartCount(id)).Select(index =>
        {
            CompiledSpritePart part = MapMarkerGeometry.Part(id, index);
            return part.Part.ToFramePart(0, SpritePartFrames.StandardSizes, inheritPalette: true);
        })]);
    }

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
        MapSpriteId.IndicatorFrame0 => frames.IndicatorFrames[0],
        MapSpriteId.IndicatorFrame1 => frames.IndicatorFrames[1],
        MapSpriteId.IndicatorFrame2 => frames.IndicatorFrames[2],
        MapSpriteId.ElevatorCrateria => frames.ElevatorCrateria,
        MapSpriteId.ElevatorBrinstar => frames.ElevatorBrinstar,
        MapSpriteId.ElevatorNorfair => frames.ElevatorNorfair,
        MapSpriteId.ElevatorWreckedShip => frames.ElevatorWreckedShip,
        MapSpriteId.ElevatorMaridia => frames.ElevatorMaridia,
        MapSpriteId.WorldTitle => frames.WorldTitle,
        MapSpriteId.WorldCrateria or
            MapSpriteId.WorldBrinstar or
            MapSpriteId.WorldNorfair or
            MapSpriteId.WorldWreckedShip or
            MapSpriteId.WorldMaridia or
            MapSpriteId.WorldTourian => throw new KeyNotFoundException($"The given key '{id}' was not present in the dictionary."),
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

    /// <summary>
    /// Retains the 26 named compositions in their drawing roles after JSON validation; the three indicator animation
    /// frames are one list indexed by frame.
    /// </summary>
    [ArchitectureReview("context-bag", "The map screen's compiled sprite compositions by role; every member is immutable artwork data, not a service.")]
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
        ImmutableArray<SpriteComposition?> IndicatorFrames,
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
    /// <param name="json">The compositions document.</param>
    /// <param name="png">The indexed menu OBJ character atlas.</param>
    /// <exception cref="InvalidDataException">The document or the atlas is invalid.</exception>
    public static MapSpriteArtwork Read(Stream json, Stream png)
    {
        FrameSet frames = Compile(json);
        var image = IndexedPng.Read(png, new(MapSpriteFormat.Width, MapSpriteFormat.Height));
        return new(frames, new MapObjectTileArtwork(image));
    }

    /// <summary>Loads all 26 named sprite compositions.</summary>
    /// <exception cref="InvalidDataException">The document is invalid or misses a named composition.</exception>
    private static FrameSet Compile(Stream json)
    {
        MapSpriteDocument document;
        try { document = JsonAssetDocument.Read<MapSpriteDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Map sprite document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid map sprite JSON.", error); }
        if (document.Version != MapSpriteFormat.Version || document.Frames is null || document.Frames.Count != MapSpriteDefinitions.Count)
            throw new InvalidDataException("Map sprite content requires version 1 and all 26 named frames.");
        return new FrameSet(
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
            [Require("Indicator.Frame0", MapSpriteId.IndicatorFrame0), Require("Indicator.Frame1", MapSpriteId.IndicatorFrame1),
             Require("Indicator.Frame2", MapSpriteId.IndicatorFrame2)],
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
    }

    /// <summary>The artwork <paramref name="catalog"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The catalog was not read from installed artwork.</exception>
    public static MapSpriteArtwork Of(MapSpriteCompositions catalog) => catalog as MapSpriteArtwork
        ?? throw new InvalidDataException("The map sprites were not read from installed artwork.");
}
