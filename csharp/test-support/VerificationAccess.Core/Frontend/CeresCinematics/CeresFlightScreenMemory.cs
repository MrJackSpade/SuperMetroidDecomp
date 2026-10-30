using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Composes the Ceres flight's published artwork and caption into the memory image the backends draw (#1358): the Mode 7
/// characters, the blank-filled map with the front or rear view's slice, the OBJ characters and the SPACE COLONY caption.
/// </summary>
internal static class CeresFlightScreenMemory
{
    /// <summary>The image of <paramref name="artwork"/>'s front view, or its rear view once <c>$BE22</c> has queued it.</summary>
    [WholeImageFixture("the Ceres flight's memory image")]
    internal static byte[] View(CeresFlightArtwork artwork, bool rear)
    {
        var vram = new SnesVram();
        // $BCCD writes all $4000 character bytes to $2119, fills every corresponding low
        // byte with tile $8C, then overwrites only one view's $300-byte map.
        var mode7 = new Mode7Vram(vram);
        mode7.LoadCharacters(artwork.FloorCharacters.Span[..CeresFlightArtworkFormat.FloorByteCount]);
        mode7.FillMap(CeresFloorShotDefinitions.BlankTile, FloorSheetComposer.CellCount);
        int slice = rear ? CeresFlightArtworkFormat.MapCellsPerView : 0;
        mode7.LoadMap(CeresFlightCharacters.Of(artwork).FloorMaps.Span.Slice(slice, CeresFlightArtworkFormat.MapCellsPerView));
        new CeresVram(vram).ObjectCharacters.Load(artwork.ObjectCharacters.Span[..CeresFlightArtworkFormat.ObjectByteCount]);
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
        WithCaption(View(flight.Artwork ?? throw new InvalidOperationException("Ceres approach rendering requires installed cinematic artwork."),
            flight.ShowsRearView), BackgroundTileWordBytes.Words(CaptionTiles(flight)));

    /// <summary>
    /// The caption tilemap the cartridge's caption list leaves for <paramref name="flight"/>'s state (#1467): the letters
    /// typed so far on their row and, once drawn, the katakana caption below, each as the import pictures it.
    /// </summary>
    internal static BackgroundTile[] CaptionTiles(IntroCeresFlightState flight)
    {
        var tiles = new BackgroundTile[CeresCinematicPictures.PlaneCells * CeresCinematicPictures.PlaneCells];
        for (int letter = 0; letter < flight.CaptionLettersShown; letter++)
            tiles[SpaceColonyCaptionDefinitions.Row * CeresCinematicPictures.PlaneCells + SpaceColonyCaption.Column(letter)] =
                SpaceColonyCaptionTileDefinitions.Glyphs[SpaceColonyCaption.Character(letter)];
        if (flight.JapaneseCaptionShown)
        {
            ReadOnlySpan<BackgroundTile> japanese = SpaceColonyCaptionTileDefinitions.JapaneseTiles;
            for (int index = 0; index < japanese.Length; index++)
                tiles[(SpaceColonyCaptionDefinitions.JapaneseRow + index / SpaceColonyCaptionDefinitions.JapaneseWidth) * CeresCinematicPictures.PlaneCells +
                    SpaceColonyCaptionDefinitions.JapaneseColumn + index % SpaceColonyCaptionDefinitions.JapaneseWidth] = japanese[index];
        }
        return tiles;
    }
}
