using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Composes the Ceres flight's published artwork and caption into the memory image the backends draw (#1358): the Mode 7
/// characters, the blank-filled map with the front or rear view's slice, the OBJ characters and the SPACE COLONY caption.
/// </summary>
internal static class CeresFlightScreenMemory
{
    /// <summary>The image of <paramref name="artwork"/>'s front view, or its rear view once <c>$BE22</c> has queued it.</summary>
    internal static byte[] View(CeresFlightArtworkCatalog artwork, bool rear)
    {
        var vram = new SnesVram();
        // $BCCD writes all $4000 character bytes to $2119, fills every corresponding low
        // byte with tile $8C, then overwrites only one view's $300-byte map.
        var mode7 = new Mode7Vram(vram);
        mode7.LoadCharacters(artwork.FloorCharacters.Span[..CeresFlightArtworkFormat.FloorByteCount]);
        mode7.FillMap(CeresFlightSceneDefinitions.BlankFloorCharacter, AffineFloor.CellCount);
        int slice = rear ? CeresFlightArtworkFormat.MapCellsPerView : 0;
        mode7.LoadMap(artwork.FloorMaps.Span.Slice(slice, CeresFlightArtworkFormat.MapCellsPerView));
        new CeresVram(vram).ObjectCharacters.Load(artwork.ObjectCharacters.Span[..CeresFlightArtworkFormat.ObjectByteCount]);
        return vram.CaptureImage();
    }

    /// <summary>A copy of <paramref name="view"/> with the caption tilemap over it.</summary>
    internal static byte[] WithCaption(ReadOnlySpan<byte> view, ReadOnlySpan<ushort> caption)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, view);
        new CeresVram(vram).SpaceColonyTilemap.Write(caption);
        return vram.CaptureImage();
    }

    /// <summary>The image <paramref name="flight"/> shows: its view with the SPACE COLONY caption over it.</summary>
    internal static byte[] Compose(IntroCeresFlightState flight) =>
        WithCaption(View(flight.Artwork ?? throw new InvalidOperationException("Ceres approach rendering requires installed cinematic artwork."),
            flight.ShowsRearView), BackgroundTileWordBytes.Words(flight.SpaceColonyTilemap));
}
