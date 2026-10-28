using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Cartridge values and helpers of <see cref="AffineFloor"/> that only verification reads.</summary>
internal static class AffineFloorConstants
{
    /// <summary>The floor a frontend scene's memory image <paramref name="image"/> maps, showing <paramref name="tiles"/>.</summary>
    /// <exception cref="ArgumentException">The image is not a whole memory image.</exception>
    public static AffineFloor Capture(ReadOnlySpan<byte> image, FloorTilePictures tiles)
    {
        Ensure.LengthEqual(image, SnesVram.ByteCount);
        var cells = ImmutableArray.CreateBuilder<AffineFloorCell>(AffineFloor.CellCount);
        for (int cell = 0; cell < AffineFloor.CellCount; cell++)
            cells.Add(new AffineFloorCell(image[cell * sizeof(ushort)]));
        return new AffineFloor(cells.MoveToImmutable(), tiles);
    }
}
