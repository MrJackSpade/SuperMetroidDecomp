using System.Reflection;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Test-only access to the legacy CPU-bus oracle on concrete diagnostic address
/// spaces. This file is linked only into verification/debug executables; gameplay
/// Core and asset extractors cannot see this extension.
/// </summary>
internal static class ReferenceBusReadExtensions
{
    /// <summary>Caches diagnostic byte-reader delegates without keeping address spaces alive after their callers release them.</summary>
    private static readonly ConditionalWeakTable<ISnesAddressSpace, Func<int, byte>> Readers = new();

    /// <summary>Reads one byte through the reference reader associated with a diagnostic address space.</summary>
    /// <param name="bus">Address space whose concrete diagnostic reader supplies the byte.</param>
    /// <param name="address">CPU bus address to read.</param>
    /// <returns>The byte returned by the address space's reference reader.</returns>
    internal static byte ReadByte(this ISnesAddressSpace bus, int address) =>
        Readers.GetValue(bus, CreateReader)(address);

    /// <summary>Builds a reference reader using typed mapped reads or the legacy concrete <c>ReadByte(int)</c> method.</summary>
    /// <param name="bus">Concrete diagnostic address space to adapt.</param>
    /// <returns>A delegate that reads bytes from the supplied address space.</returns>
    /// <exception cref="InvalidOperationException">The address space exposes no compatible legacy byte reader.</exception>
    private static Func<int, byte> CreateReader(ISnesAddressSpace bus)
    {
        // The production address space intentionally has no generic read method.
        // Preserve the older verification oracle by selecting its typed source here;
        // this shim is linked only into diagnostic executables, never gameplay.
        if (bus is SuperMetroidAddressSpace mapped)
            return address => ReadMappedByte(mapped, address);

        MethodInfo? method = bus.GetType().GetMethod("ReadByte", [typeof(int)]);
        if (method is null || method.ReturnType != typeof(byte))
            throw new InvalidOperationException(
                $"Diagnostic address space {bus.GetType().FullName} has no reference byte reader.");
        return method.CreateDelegate<Func<int, byte>>(bus);
    }

    /// <summary>Routes a mapped CPU bus read to the typed work RAM, save RAM, or cartridge accessor.</summary>
    /// <param name="bus">Mapped diagnostic address space providing the typed memory reads.</param>
    /// <param name="address">CPU bus address to classify and read.</param>
    /// <returns>The byte from the selected memory region.</returns>
    /// <exception cref="InvalidOperationException">The address is outside the runtime DMA source map.</exception>
    private static byte ReadMappedByte(SuperMetroidAddressSpace bus, int address) =>
        SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) switch
        {
            SnesDmaSourceKind.WorkRam => bus.ReadWorkRamByte(address),
            SnesDmaSourceKind.SaveRam => bus.ReadSaveRamByte(address),
            SnesDmaSourceKind.Cartridge => bus.ReadCartridgeByte(address),
            _ => throw new InvalidOperationException(
                $"CPU read ${address >> 16:X2}:{address & 0xffff:X4} is outside the runtime address map."),
        };
}
