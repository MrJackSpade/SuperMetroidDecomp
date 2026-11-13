using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Composes the Ceres flight's published artwork and caption into the memory image the backends draw (#1358): the Mode 7
/// characters, the blank-filled map with the front or rear view's slice, the OBJ characters and the SPACE COLONY caption.
/// </summary>
internal static class CeresFlightScreenMemory
{
    /// <summary>The image of <paramref name="artwork"/>'s front view, or its rear view once <c>$BE22</c> has queued it.</summary>
    [WholeImageFixture("the Ceres flight's memory image")]
    internal static byte[] View(CeresFlightCharacters artwork, bool rear)
    {
        var vram = new SnesVram();
        // $BCCD writes all $4000 character bytes to $2119, fills every corresponding low
        // byte with tile $8C, then overwrites only one view's $300-byte map.
        var mode7 = new Mode7Vram(vram);
        mode7.LoadCharacters(artwork.FloorCharacters.Span[..CeresFlightArtworkFileFormat.FloorByteCount]);
        mode7.FillMap(CeresFloorShotDefinitions.BlankTile, FloorSheetComposer.CellCount);
        int slice = rear ? CeresFlightArtworkFileFormat.MapCellsPerView : 0;
        mode7.LoadMap(artwork.FloorMaps.Span.Slice(slice, CeresFlightArtworkFileFormat.MapCellsPerView));
        new CeresVram(vram).ObjectCharacters.Load(artwork.ObjectCharacters.Span[..CeresFlightArtworkFileFormat.ObjectByteCount]);
        return vram.CaptureImage();
    }

    /// <summary>A copy of <paramref name="view"/> with the caption tilemap over it.</summary>
    [WholeImageFixture("the Ceres flight's memory image")]
    internal static byte[] WithCaption(ReadOnlySpan<byte> view, ReadOnlySpan<ushort> caption)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, view);
        new CeresVram(vram).SpaceColonyTilemap.Write(caption);
        return vram.CaptureImage();
    }

    /// <summary>The image <paramref name="flight"/> shows: its view with the SPACE COLONY caption over it.</summary>
    [WholeImageFixture("the Ceres flight's memory image")]
    internal static byte[] Compose(IntroCeresFlightState flight) =>
        WithCaption(View((flight.Artwork ?? throw new InvalidOperationException("Ceres approach rendering requires installed cinematic artwork.")).ImportSource,
            flight.ShowsRearView), BackgroundTileWordBytes.Words(CaptionTiles(flight)));

    /// <summary>
    /// The caption tilemap the cartridge's caption list leaves for <paramref name="flight"/>'s state (#1467): the letters
    /// typed so far on their row and, once drawn, the katakana caption below, each as the import pictures it.
    /// </summary>
    internal static BackgroundTile[] CaptionTiles(IntroCeresFlightState flight)
    {
        const int cell = ScreenGeometry.BackgroundTileSizePixels;
        var tiles = new BackgroundTile[TilemapScreenGeometry.CellCount];
        for (int letter = 0; letter < flight.CaptionLettersShown; letter++)
            tiles[SpaceColonyCaptionDefinitions.Top / cell * TilemapScreenGeometry.WidthInTiles + SpaceColonyCaption.Left(letter) / cell] =
                SpaceColonyCaptionTileDefinitions.Glyphs[CeresCinematicPictureSheetComposer.CaptionCharacter(letter)];
        if (flight.JapaneseCaptionShown)
        {
            ReadOnlySpan<BackgroundTile> japanese = SpaceColonyCaptionTileDefinitions.JapaneseTiles;
            for (int index = 0; index < japanese.Length; index++)
                tiles[(SpaceColonyCaptionDefinitions.JapaneseTop / cell + index / CeresCinematicPictureSheetComposer.JapaneseWidth) * TilemapScreenGeometry.WidthInTiles +
                    SpaceColonyCaptionDefinitions.JapaneseLeft / cell + index % CeresCinematicPictureSheetComposer.JapaneseWidth] = japanese[index];
        }
        return tiles;
    }
}
