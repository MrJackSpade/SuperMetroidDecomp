namespace SuperMetroid.AssetExtraction.Memory;

/// <summary>Reads the console's work memory, which only checks do.</summary>
public interface IWorkMemory
{
    /// <summary>The byte at <paramref name="address"/>.</summary>
    /// <param name="address">The address read.</param>
    byte ReadWorkByte(int address);
}
