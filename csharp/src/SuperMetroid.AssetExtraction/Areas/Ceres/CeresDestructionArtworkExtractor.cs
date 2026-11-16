using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Exports the remaining Ceres map slices and Zebes reveal transfers.</summary>
internal static class CeresDestructionArtworkExtractor
{
    /// <summary>Exports the three Ceres destruction map slices, Zebes reveal
    /// artwork, sprite frames, and actor placements, checking the cartridge's reveal
    /// placement operands against the translated actor definitions.</summary>
    public static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        byte[] allCeresMaps = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.CeresTilemaps,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        int firstDestructionByte = 2 * CeresFlightArtworkFileFormat.MapCellsPerView;
        if (allCeresMaps.Length < firstDestructionByte + CeresDestructionMapFormat.MapByteCount)
            throw new InvalidDataException("Ceres destruction lacks its three native map slices.");
        var views = new int[CeresDestructionMapFormat.ViewCount][];
        for (int view = 0; view < views.Length; view++)
            views[view] = allCeresMaps.AsSpan(firstDestructionByte +
                    view * CeresDestructionMapFormat.CellsPerView,
                    CeresDestructionMapFormat.CellsPerView)
                .SelectToArray(tile => (int)tile);
        using var mapJson = new MemoryStream();
        CeresDestructionMaps.Write(mapJson, new CeresDestructionMapDocument
        {
            Version = CeresDestructionMapFormat.Version,
            Width = CeresDestructionMapFormat.MapWidth,
            Height = CeresDestructionMapFormat.MapHeight,
            Views = views,
        });

        byte[] zebesMap = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.ZebesTilemap,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        if (zebesMap.Length < ZebesRevealArtworkFormat.MapByteCount)
            throw new InvalidDataException("Zebes reveal tilemap is shorter than its native transfer.");
        byte[] zebesMapJson = RoomBackgroundTilemapExtractor.Encode(
            zebesMap.AsSpan(0, ZebesRevealArtworkFormat.MapByteCount));

        byte[] zebesCharacters = RomDataReader.Decompress(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.ZebesCharacters,
            maximumOutputBytes: ZebesRevealArtworkFormat.CharacterByteCount);
        if (zebesCharacters.Length != ZebesRevealArtworkFormat.CharacterByteCount)
            throw new InvalidDataException("Zebes reveal character stream has the wrong transfer length.");
        byte[] pixels = SnesGraphics.DecodePlanarTiles(zebesCharacters, 4,
            IntroCinematicArtworkFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        byte[] zebesPng = png.ToArray();

        var frames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal);
        foreach (CeresDestructionSpriteFrameDefinition definition in
            CeresDestructionSpriteFrameDefinitions.Frames)
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

        // These seven X/Y operands are presentation data, read from the pinned cartridge into an editable layout; the game
        // keeps no copy of them (#1615).
        CeresRevealActorPlacement[] placements =
        [
            .. CeresCinematicSourceDefinitions.ZebesPlacements.Select(source => new CeresRevealActorPlacement
            {
                Role = source.Role,
                X = CoordinateAxis.AsSigned(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.FirstOperand)),
                Y = CoordinateAxis.AsSigned(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.SecondOperand)),
            }),
        ];
        using var actorsJson = new MemoryStream();
        CeresRevealActorLayoutFile.Write(actorsJson, new CeresRevealActorLayoutDocument
        {
            Version = CeresRevealActorLayoutFormat.Version,
            Actors = placements,
        });
        byte[] actorsFile = actorsJson.ToArray();

        // The three initial actors' starts are read from their initializers' immediates; only presentation coordinates are
        // exported, and their movement and callbacks stay in code.
        CeresDestructionActorPlacement[] destructionPlacements =
        [
            .. CeresCinematicSourceDefinitions.DestructionPlacements.Select(source => new CeresDestructionActorPlacement
            {
                Role = source.Role,
                X = CoordinateAxis.AsSigned(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.FirstOperand)),
                Y = CoordinateAxis.AsSigned(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.SecondOperand)),
            }),
        ];
        using var destructionActorsJson = new MemoryStream();
        CeresDestructionActorLayoutFile.Write(destructionActorsJson,
            new CeresDestructionActorLayoutDocument
            {
                Version = CeresDestructionActorLayoutFormat.Version,
                Actors = destructionPlacements,
            });
        byte[] destructionActorsFile = destructionActorsJson.ToArray();

        return new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CeresDestructionMapFormat.FileName] = mapJson.ToArray(),
            [ZebesRevealArtworkFormat.MapFileName] = zebesMapJson,
            [ZebesRevealArtworkFormat.CharacterFileName] = zebesPng,
            [CeresDestructionSpriteFormat.FileName] = spritesFile,
            [CeresRevealActorLayoutFormat.FileName] = actorsFile,
            [CeresDestructionActorLayoutFormat.FileName] = destructionActorsFile,
        };
    }
}
