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
        MethodInfo? method = bus.GetType().GetMethod("ReadByte", [typeof(int)]);
        if (method is null || method.ReturnType != typeof(byte))
            throw new InvalidOperationException(
                $"Diagnostic address space {bus.GetType().FullName} has no reference byte reader.");
        return method.CreateDelegate<Func<int, byte>>(bus);
    }
}
