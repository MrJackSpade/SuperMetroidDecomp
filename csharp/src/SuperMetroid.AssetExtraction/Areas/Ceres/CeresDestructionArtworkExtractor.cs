using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Exports the remaining Ceres map slices and Zebes reveal transfers.</summary>
internal static class CeresDestructionArtworkExtractor
{
    /// <summary>Exports the three Ceres destruction map slices, Zebes reveal
    /// artwork, sprite frames, and actor placements, then checks decoded transfer
    /// bytes and pinned native coordinates before publishing the six asset files.</summary>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        byte[] allCeresMaps = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.CeresTilemaps,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        int firstDestructionByte = 2 * CeresFlightArtworkFormat.MapCellsPerView;
        if (allCeresMaps.Length < firstDestructionByte + CeresDestructionArtworkFormat.MapByteCount)
            throw new InvalidDataException("Ceres destruction lacks its three native map slices.");
        var views = new int[CeresDestructionArtworkFormat.ViewCount][];
        for (int view = 0; view < views.Length; view++)
            views[view] = allCeresMaps.AsSpan(firstDestructionByte +
                    view * CeresDestructionArtworkFormat.CellsPerView,
                    CeresDestructionArtworkFormat.CellsPerView)
                .SelectToArray(tile => (int)tile);
        using var mapJson = new MemoryStream();
        CeresDestructionArtwork.WriteMap(mapJson, new CeresDestructionMapDocument
        {
            Version = CeresDestructionArtworkFormat.Version,
            Width = CeresDestructionArtworkFormat.MapWidth,
            Height = CeresDestructionArtworkFormat.MapHeight,
            Views = views,
        });

        byte[] zebesMap = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.ZebesTilemap,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        if (zebesMap.Length < CeresDestructionArtworkFormat.ZebesMapByteCount)
            throw new InvalidDataException("Zebes reveal tilemap is shorter than its native transfer.");
        byte[] zebesMapJson = RoomBackgroundTilemapExtractor.Encode(
            zebesMap.AsSpan(0, CeresDestructionArtworkFormat.ZebesMapByteCount));

        byte[] zebesCharacters = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.ZebesCharacters,
            maximumOutputBytes: CeresDestructionArtworkFormat.ZebesCharacterByteCount);
        if (zebesCharacters.Length != CeresDestructionArtworkFormat.ZebesCharacterByteCount)
            throw new InvalidDataException("Zebes reveal character stream has the wrong transfer length.");
        byte[] pixels = SnesGraphics.DecodePlanarTiles(zebesCharacters, 4,
            IntroCinematicArtworkFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] zebesPng = png.ToArray();

        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CeresDestructionSpriteFrameDefinition definition in
            CeresDestructionSpriteDefinitions.Frames)
            frames.Add(definition.Name, IntroCinematicSpriteFrameExtractor.Extract(
                bus, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        using var spritesJson = new MemoryStream();
        CeresDestructionSpritePresentation.Write(spritesJson,
            new CeresDestructionSpriteDocument
            {
                Version = CeresDestructionSpriteFormat.Version,
                Frames = frames,
            });
        byte[] spritesFile = spritesJson.ToArray();

        // These six X/Y operands are presentation data. Read the pinned cartridge
        // directly, then verify that the existing translated actor definitions
        // have not drifted from the source before publishing an editable layout.
        var placements = new CeresRevealActorPlacement[
            CeresDestructionActorDefinitions.ZebesActorCount];
        for (int index = 0; index < placements.Length; index++)
        {
            var source = CeresCinematicSourceDefinitions.ZebesPlacements[index];
            int x = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                CeresCinematicSourceDefinitions.PlacementBank | source.FirstOperand);
            int y = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                CeresCinematicSourceDefinitions.PlacementBank | source.SecondOperand);
            CeresDestructionActorDefinition translatedActor =
                CeresDestructionActors.ZebesActor(index);
            if (x != translatedActor.Position.X || y != translatedActor.Position.Y)
                throw new InvalidDataException(
                    $"Ceres reveal actor {source.Id} placement differs from pinned cartridge operands.");
            placements[index] = new CeresRevealActorPlacement
            {
                Id = source.Id, X = x, Y = y,
            };
        }
        using var actorsJson = new MemoryStream();
        CeresRevealActorPlacements.Write(actorsJson, new CeresRevealActorLayoutDocument
        {
            Version = CeresRevealActorLayoutFormat.Version,
            Actors = placements,
        });
        byte[] actorsFile = actorsJson.ToArray();

        // The three initial actor positions are the bounded initializer results
        // already checked against the pinned cartridge by the native actor verifier.
        // Export only presentation coordinates; their movement and callbacks stay in code.
        var destructionPlacements = new CeresDestructionActorPlacement[
            CeresDestructionActorDefinitions.InitialActorCount];
        for (int index = 0; index < destructionPlacements.Length; index++)
        {
            CeresDestructionActorDefinition actor =
                CeresDestructionActors.InitialActor(index);
            destructionPlacements[index] = new CeresDestructionActorPlacement
            {
                Id = CeresDestructionActors.InitialPlacementId(index),
                X = actor.Position.X,
                Y = actor.Position.Y,
            };
        }
        using var destructionActorsJson = new MemoryStream();
        CeresDestructionActorPlacements.Write(destructionActorsJson,
            new CeresDestructionActorLayoutDocument
            {
                Version = CeresDestructionActorLayoutFormat.Version,
                Actors = destructionPlacements,
            });
        byte[] destructionActorsFile = destructionActorsJson.ToArray();

        CeresDestructionArtwork compiled = CeresDestructionArtwork.Load(
            new MemoryStream(mapJson.ToArray(), writable: false),
            new MemoryStream(zebesMapJson, writable: false),
            RoomCharacterAtlasPng.Read(new MemoryStream(zebesPng, writable: false), CeresDestructionArtworkFormat.ZebesCharacterByteCount),
            new MemoryStream(actorsFile, writable: false),
            new MemoryStream(destructionActorsFile, writable: false));
        _ = CeresDestructionSpritePresentation.Load(new MemoryStream(spritesFile, writable: false));
        if (!compiled.CeresMaps.Span.SequenceEqual(allCeresMaps.AsSpan(firstDestructionByte,
                CeresDestructionArtworkFormat.MapByteCount)) ||
            !BackgroundTileWords.ToBytes(compiled.ZebesMap.Tiles.Span).SequenceEqual(zebesMap.AsSpan(0,
                CeresDestructionArtworkFormat.ZebesMapByteCount)) ||
            !compiled.ZebesCharacters.Characters.Span.SequenceEqual(zebesCharacters))
            throw new InvalidDataException("Ceres destruction PNG/JSON export changed native bytes.");
        return new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CeresDestructionArtworkFormat.CeresMapFileName] = mapJson.ToArray(),
            [CeresDestructionArtworkFormat.ZebesMapFileName] = zebesMapJson,
            [CeresDestructionArtworkFormat.ZebesCharacterFileName] = zebesPng,
            [CeresDestructionSpriteFormat.FileName] = spritesFile,
            [CeresRevealActorLayoutFormat.FileName] = actorsFile,
            [CeresDestructionActorLayoutFormat.FileName] = destructionActorsFile,
        };
    }
}
