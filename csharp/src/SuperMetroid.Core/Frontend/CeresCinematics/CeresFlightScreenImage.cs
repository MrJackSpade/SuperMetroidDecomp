using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Composes the Ceres flight's published artwork and caption into the memory image the backends draw (#1358): the Mode 7
/// characters, the blank-filled map with the front or rear view's slice, the OBJ characters and the SPACE COLONY caption.
/// </summary>
[PpuLayoutBoundary("codec: the Ceres flight's Mode 7 view, OBJ characters and caption at their native words")]
internal static class CeresFlightScreenImage
{
    /// <summary>The image of <paramref name="artwork"/>'s front view, or its rear view once <c>$BE22</c> has queued it.</summary>
    internal static byte[] View(CeresFlightArtworkCatalog artwork, bool rear)
    {
        var vram = new SnesVram();
        // $BCCD writes all $4000 character bytes to $2119, fills every corresponding low
        // byte with tile $8C, then overwrites only one view's $300-byte map.
        var mode7 = new Mode7Vram(vram);
        mode7.LoadCharacters(artwork.Mode7Characters.Span[..CeresFlightRomData.Vram.Mode7CharacterByteCount]);
        mode7.FillMap(CeresFlightRomData.Vram.Mode7BlankMapTile, CeresFlightRomData.Vram.Mode7MapFillWordCount);
        int slice = rear ? CeresFlightRomData.Vram.Mode7MapSliceByteCount : 0;
        mode7.LoadMap(artwork.Mode7Maps.Span.Slice(slice, CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        new CeresVram(vram).ObjectCharacters.Load(artwork.ObjectCharacters.Span[..CeresFlightRomData.Vram.ObjectCharacterByteCount]);
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
}
