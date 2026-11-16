using SuperMetroid.AssetExtraction.Memory;

namespace SuperMetroid.Verification.Memory;

/// <summary>Memory a check recorded.</summary>
/// <param name="bytes">The recorded bytes.</param>
internal sealed class RecordedMemory(byte[] bytes) : IMemoryBus
{
    /// <inheritdoc />
    public byte ReadWorkByte(int address) => bytes[address];

    /// <summary>The first recorded byte, read through the work memory contract.</summary>
    internal byte Head => First(this);

    /// <summary>The first byte of <paramref name="memory"/>.</summary>
    /// <param name="memory">The memory read.</param>
    internal static byte First(IWorkMemory memory) => memory.ReadWorkByte(0);
}