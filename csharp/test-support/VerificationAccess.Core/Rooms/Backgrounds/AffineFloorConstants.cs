using System.Collections.Immutable;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Cartridge values and helpers of <see cref="AffineFloor"/> that only verification reads.</summary>
internal static class AffineFloorConstants
{
    /// <summary>The floor a frontend scene's memory image <paramref name="image"/> maps, showing <paramref name="tiles"/>.</summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    [PpuLayoutBoundary("capture: Mode 7's tile map, the low byte of each of the first 16384 VRAM words")]
    public static AffineFloor Capture(ReadOnlySpan<byte> image, Mode7TilePictures tiles)
    {
        Ensure.LengthEqual(image, SnesPpuLayout.VramByteCount);
        var cells = ImmutableArray.CreateBuilder<AffineFloorCell>(AffineFloor.CellCount);
        for (int cell = 0; cell < AffineFloor.CellCount; cell++)
            cells.Add(new AffineFloorCell(image[cell * sizeof(ushort)]));
        return new AffineFloor(cells.MoveToImmutable(), tiles);
    }
}
