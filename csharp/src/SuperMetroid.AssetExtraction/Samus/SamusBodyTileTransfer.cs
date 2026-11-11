using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>One native seven-byte transfer compiled from palette-indexed PNG tiles, held as installed.</summary>
public sealed class SamusBodyTileTransfer
{
    /// <summary>The installed planar character bytes of both transfers.</summary>
    private readonly byte[] planar;

    /// <summary>Holds one installed body-transfer definition.</summary>
    /// <param name="firstSize">Byte count of the first VRAM transfer.</param>
    /// <param name="secondSize">Byte count of the optional second VRAM transfer.</param>
    /// <param name="planar">Combined SNES planar character bytes.</param>
    public SamusBodyTileTransfer(int firstSize, int secondSize,
        byte[] planar)
    {
        Ensure.NotNull(planar);
        FirstSize = firstSize;
        SecondSize = secondSize;
        this.planar = (byte[])planar.Clone();
    }

    /// <summary>Combined byte count of both native VRAM transfers in this definition.</summary>
    internal int PayloadLength => planar.Length;
    /// <summary>Gets the installed byte count of the first VRAM transfer.</summary>
    public int FirstSize { get; }
    /// <summary>Gets the installed byte count of the optional second VRAM transfer.</summary>
    public int SecondSize { get; }
    /// <summary>The installed planar characters.</summary>
    public ReadOnlyMemory<byte> Planar => planar;
}
