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
    private static readonly ConditionalWeakTable<ISnesAddressSpace, Func<int, byte>> Readers = new();

    internal static byte ReadByte(this ISnesAddressSpace bus, int address) =>
        Readers.GetValue(bus, CreateReader)(address);

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
