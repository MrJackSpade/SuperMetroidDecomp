using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the five initial Ceres blast X coordinates against their cartridge words.</summary>
    /// <param name="rom">Address space containing the native explosion placement table.</param>
    private static void VerifyCeresInitialBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc475, 5, i => unchecked((ushort)CeresExplosionDefinitions.InitialExplosion(i).X));

    /// <summary>Checks the five initial Ceres blast Y coordinates against their cartridge words.</summary>
    /// <param name="rom">Address space containing the native explosion placement table.</param>
    private static void VerifyCeresInitialBlastY(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc47f, 5, i => unchecked((ushort)CeresExplosionDefinitions.InitialExplosion(i).Y));

    /// <summary>Checks the five initial Ceres blast delays against their cartridge words.</summary>
    /// <param name="rom">Address space containing the native explosion placement table.</param>
    private static void VerifyCeresInitialBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc46b, 5, i => CeresExplosionDefinitions.InitialExplosion(i).DelayFrames);

    /// <summary>Checks the four final Ceres blast X coordinates against their cartridge words.</summary>
    /// <param name="rom">Address space containing the native explosion placement table.</param>
    private static void VerifyCeresFinalBlastX(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc572, 4, i => unchecked((ushort)CeresExplosionDefinitions.FinalExplosion(i).X));

    /// <summary>Checks the four final Ceres blast delays against their cartridge words.</summary>
    /// <param name="rom">Address space containing the native explosion placement table.</param>
    private static void VerifyCeresFinalBlastDelay(ISnesAddressSpace rom) => VerifyCeresBlastField(rom, 0x8bc56a, 4, i => CeresExplosionDefinitions.FinalExplosion(i).DelayFrames);

    /// <summary>Compares a contiguous sequence of catalog values with native words and rejects unsupported indices.</summary>
    /// <param name="rom">Cartridge address space used to read the expected little-endian words.</param>
    /// <param name="address">Byte address of the first native word in the field's table.</param>
    /// <param name="count">Number of field entries to compare.</param>
    /// <param name="actual">Accessor that returns the compiled field value for a zero-based entry index.</param>
    private static void VerifyCeresBlastField(ISnesAddressSpace rom, int address, int count, Func<int, ushort> actual)
    {
        for (int index = 0; index < count; index++)
        {
            int word = address + index * 2;
            ushort expected = (ushort)(rom.ReadByte(word) | rom.ReadByte(word + 1) << 8);
            AssertEqual(expected, actual(index), "original Ceres explosion field");
        }
        foreach (int invalid in new[] { int.MinValue, -1, count, 256, int.MaxValue })
        {
            try { _ = actual(invalid); }
            catch (ArgumentOutOfRangeException exception)
            {
                AssertEqual("index", exception.ParamName!, "blast placement index parameter");
                continue;
            }
            throw new InvalidOperationException("Unsupported blast index was accepted.");
        }
    }
}
