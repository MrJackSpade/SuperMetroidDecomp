namespace SuperMetroid.Core.Compatibility;

/// <summary>A floor update residue verification writes byte by byte, to stage what Samus's delay over-reads see.</summary>
internal sealed class RecordedFloorUpdateResidue : IFloorUpdateResidue
{
    /// <summary>The staged bytes.</summary>
    private readonly byte[] bytes = new byte[FloorUpdateResidue.ByteCount];

    /// <summary>Stages <paramref name="value"/> at residue byte <paramref name="index"/>.</summary>
    internal void Write(int index, byte value) => bytes[index] = value;

    /// <inheritdoc/>
    public byte ByteAt(int index) => (uint)index < bytes.Length ? bytes[index]
        : throw new InvalidDataException($"Byte {index} lies outside the {bytes.Length}-byte floor update residue.");
}
