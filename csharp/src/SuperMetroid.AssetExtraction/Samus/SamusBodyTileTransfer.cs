using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One native seven-byte transfer compiled from palette-indexed PNG tiles.</summary>
public sealed class SamusBodyTileTransfer
{
    /// <summary>Original standalone planar bytes before a definition is bound to its catalog.</summary>
    private readonly byte[]? standalonePlanar;
    /// <summary>Edited in-contour pixel bytes after shared pixels are calculated.</summary>
    internal readonly Dictionary<int, byte>? pixelInputs;
    /// <summary>Edited bits outside each tile's compiled contour mask.</summary>
    internal readonly Dictionary<int, byte>? contourEdits;
    /// <summary>Combined byte count of both native VRAM transfers in this definition.</summary>
    internal int PayloadLength { get; }
    /// <summary>Explicit first-transfer size when the selected frames do not derive it.</summary>
    internal readonly int? firstSizeOverride;
    /// <summary>
    /// First-transfer size calculated once from fixed body pointers and frames at binding,
    /// avoiding a scan of every pose for each later read.
    /// </summary>
    private readonly int? calculatedFirstSize;
    /// <summary>Owning catalog used to reconstruct shared or diagnostic pixel bytes.</summary>
    private readonly SamusBodyCharacterArtwork? body;
    /// <summary>Whether this definition belongs to the upper-body pointer table.</summary>
    private readonly bool upper;
    /// <summary>Native definition-group index for shared pixel reconstruction.</summary>
    private readonly int set;
    /// <summary>Seven-byte record position within the native definition group.</summary>
    private readonly int position;

    /// <summary>The owning catalog; a definition reconstructs shared bytes only once bound to it.</summary>
    private SamusBodyCharacterArtwork BoundCatalog => body ??
        throw new InvalidOperationException("This Samus body transfer is standalone; it is not bound to its catalog.");

    /// <summary>The bound definition's edited in-contour pixels.</summary>
    private Dictionary<int, byte> BoundPixelInputs => pixelInputs ??
        throw new InvalidOperationException("This Samus body transfer is standalone; it has no edited pixel inputs.");

    /// <summary>The bound definition's edited out-of-contour bits.</summary>
    private Dictionary<int, byte> BoundContourEdits => contourEdits ??
        throw new InvalidOperationException("This Samus body transfer is standalone; it has no contour edits.");

    /// <summary>Creates one standalone native body-transfer definition.</summary>
    /// <param name="firstSize">Byte count of the first VRAM transfer.</param>
    /// <param name="secondSize">Byte count of the optional second VRAM transfer.</param>
    /// <param name="planar">Combined SNES planar character bytes.</param>
    public SamusBodyTileTransfer(int firstSize, int secondSize,
        byte[] planar)
    {
        Ensure.NotNull(planar);
        firstSizeOverride = firstSize;
        SecondSize = secondSize;
        standalonePlanar = (byte[])planar.Clone();
        PayloadLength = planar.Length;
    }

    /// <summary>
    /// Binds a standalone definition, separating selected pixels from contour edits and
    /// deriving stock transfer geometry from the catalog's pose/frame selectors.
    /// </summary>
    private SamusBodyTileTransfer(SamusBodyTileTransfer supplied, SamusBodyCharacterArtwork body,
        bool upper, int set, int position, ReadOnlySpan<int> poseFirstFrames, ReadOnlySpan<SamusBodyFrameSelection> frames)
    {
        this.body = body;
        this.upper = upper;
        this.set = set;
        this.position = position;
        PayloadLength = supplied.PayloadLength;
        // Collect the sparse edits first so each dictionary is created at its final size;
        // growing one entry at a time reallocated every tile's tables several times over.
        int[] scratchIndexes = System.Buffers.ArrayPool<int>.Shared.Rent(PayloadLength * 2);
        byte[] scratchValues = System.Buffers.ArrayPool<byte>.Shared.Rent(PayloadLength * 2);
        try
        {
            int pixelCount = 0, contourCount = 0;
            for (int index = 0; index < PayloadLength; index++)
            {
                int value = supplied.ReadPlanarByte(index);
                int mask = SamusBodyPixels.ContourMask(upper, set, position, index);
                int outside = (byte)(value & ~mask);
                if (outside != 0)
                {
                    scratchIndexes[PayloadLength + contourCount] = index;
                    scratchValues[PayloadLength + contourCount++] = (byte)outside;
                }
                value &= mask;
                if (!TryPixelDefault(index, out int pixel) || value != pixel)
                {
                    scratchIndexes[pixelCount] = index;
                    scratchValues[pixelCount++] = (byte)value;
                }
            }
            pixelInputs = new Dictionary<int, byte>(pixelCount);
            for (int entry = 0; entry < pixelCount; entry++)
                pixelInputs.Add(scratchIndexes[entry], scratchValues[entry]);
            contourEdits = new Dictionary<int, byte>(contourCount);
            for (int entry = 0; entry < contourCount; entry++)
                contourEdits.Add(scratchIndexes[PayloadLength + entry], scratchValues[PayloadLength + entry]);
        }
        finally
        {
            System.Buffers.ArrayPool<int>.Shared.Return(scratchIndexes);
            System.Buffers.ArrayPool<byte>.Shared.Return(scratchValues);
        }
        int first = supplied.FirstSize;
        if (SamusBodyTransfers.TryFirstSize(body.Body, new SamusBodyTileSlot(upper, set, position), PayloadLength, poseFirstFrames, frames, out int calculated))
            calculatedFirstSize = calculated;
        firstSizeOverride = calculatedFirstSize == first ? null : first;
    }

    /// <summary>Creates a catalog-bound definition with native transfer size.</summary>
    internal SamusBodyTileTransfer WithTransferGeometry(SamusBodyCharacterArtwork body, bool upper, int set, int position,
        ReadOnlySpan<int> poseFirstFrames, ReadOnlySpan<SamusBodyFrameSelection> frames) =>
        new(this, body, upper, set, position, poseFirstFrames, frames);

    /// <summary>Gets the byte count of the first VRAM transfer, at most one definition's payload.</summary>
    public int FirstSize => firstSizeOverride ?? calculatedFirstSize ??
        throw new InvalidDataException("Installed body composition no longer supplies its transfer row.");
    /// <summary>Gets the byte count of the optional second VRAM transfer: the payload left after the first.</summary>
    public int SecondSize => body is null ? field : PayloadLength - FirstSize;
    /// <summary>Canonical planar snapshot; native padding and shared angle patches calculate.</summary>
    public ReadOnlyMemory<byte> Planar
    {
        get
        {
            if (standalonePlanar is not null) return standalonePlanar;
            var result = new byte[PayloadLength];
            for (int index = 0; index < result.Length; index++) result[index] = ReadPlanarByte(index);
            return result;
        }
    }

    /// <summary>Reconstructs a planar byte from edited pixels, contour bits, or shared defaults.</summary>
    internal byte ReadPlanarByte(int index)
    {
        if ((uint)index >= PayloadLength) throw new ArgumentOutOfRangeException(nameof(index));
        if (standalonePlanar is not null) return standalonePlanar[index];
        if (BoundPixelInputs.ContainsKey(index))
            return (byte)((BoundPixelInputs[index] & SamusBodyPixels.ContourMask(upper, set, position, index)) | BoundContourEdits.GetValueOrDefault(index));
        return TryPixelDefault(index, out int calculated) ? (byte)calculated
            : throw new InvalidDataException("Installed body artwork lost an independent pixel input.");
    }

    /// <summary>Resolves a diagnostic, blank, or shared-source byte before considering edits.</summary>
    private bool TryPixelDefault(int index, out int value)
    {
        if (SamusBodyPixels.TryDiagnosticByte(upper, set, position, index, out value)) return true;
        int tile = index / SamusBodyPixelDefinitions.TileBytes;
        if (SamusBodyPixels.IsBlank(upper, set, position, tile))
        {
            value = 0;
            return true;
        }
        if (SamusBodyPixels.TrySourceByte(upper, set, position, index, out int sourcePosition, out int sourceIndex))
        {
            SamusBodyTileTransfer source = (upper ? BoundCatalog.TopSet(set) : BoundCatalog.BottomSet(set))[sourcePosition];
            if ((uint)sourceIndex < source.PayloadLength)
            {
                value = source.ReadPlanarByte(sourceIndex);
                return true;
            }
        }
        value = 0;
        return false;
    }
}
