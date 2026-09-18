using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Mutually exclusive authored roles selected by the escape Etecoon's even parameter-one
/// byte offset. Odd restored values select the same role after the cartridge clears bit zero.
/// </summary>
public enum EscapeEtecoonRole : ushort
{
    LeftWalker = 0,
    RightWalker = 2,
    WaitForEscapeEvent = 4,
}

/// <summary>Fixed initialization definitions for the three escape Etecoon actors.</summary>
internal static class EscapeEtecoonDefinitions
{
    private const int DefinitionBank = 0xb30000;

    /// <summary>Three authored X positions at <c>$B3:E718-$B3:E71D</c>.</summary>
    private const ushort XPositionTable = 0xe718;

    /// <summary>Three authored Y positions at <c>$B3:E71E-$B3:E723</c>.</summary>
    private const ushort YPositionTable = 0xe71e;

    /// <summary>Three authored pre-instruction pointers at <c>$B3:E724-$B3:E729</c>.</summary>
    private const ushort PreInstructionTable = 0xe724;

    /// <summary>Three authored instruction-list pointers at <c>$B3:E72A-$B3:E72F</c>.</summary>
    private const ushort InstructionTable = 0xe72a;

    /// <summary>Three authored signed 8.8 horizontal speeds at <c>$B3:E730-$B3:E735</c>.</summary>
    private const ushort HorizontalSpeedTable = 0xe730;

    /// <summary>
    /// Left walker, right walker, and event-waiting records compiled from the five parallel
    /// tables at <c>$B3:E718-$B3:E735</c>.
    /// </summary>
    private static readonly EscapeEtecoonInitialization[] Initializations =
    [
        new(0x0080, 0x00c8, EscapeEtecoonPreInstruction.WalkAndFall, 0xe556, 0xfe00),
        new(0x00a0, 0x00c8, EscapeEtecoonPreInstruction.WalkAndFall, 0xe582, 0x0280),
        new(0x00e8, 0x00c8, EscapeEtecoonPreInstruction.WaitForEscapeEvent, 0xe5c6, 0x0000),
    ];

    /// <summary>
    /// Returns the cartridge-selected initialization. Authored offsets zero, two, and four
    /// use compiled records. Other restored/debugger values retain the native unchecked
    /// parallel-table reads instead of gaining a host-only bounds rule.
    /// </summary>
    internal static EscapeEtecoonInitialization Initialization(
        ISnesAddressSpace bus,
        ushort parameter1)
    {
        ushort byteOffset = unchecked((ushort)(parameter1 & 0xfffe));
        int index = byteOffset >> 1;
        if ((uint)index < Initializations.Length)
            return Initializations[index];

        return new EscapeEtecoonInitialization(
            ReadWord(bus, XPositionTable, byteOffset),
            ReadWord(bus, YPositionTable, byteOffset),
            (EscapeEtecoonPreInstruction)ReadWord(bus, PreInstructionTable, byteOffset),
            ReadWord(bus, InstructionTable, byteOffset),
            ReadWord(bus, HorizontalSpeedTable, byteOffset));
    }

    private static ushort ReadWord(
        ISnesAddressSpace bus,
        ushort basePointer,
        ushort byteOffset)
    {
        int address = DefinitionBank | unchecked((ushort)(basePointer + byteOffset));
        return (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
    }
}

/// <summary>One escape Etecoon's initial position, behavior, animation, and speed.</summary>
internal readonly record struct EscapeEtecoonInitialization(
    ushort XPosition,
    ushort YPosition,
    EscapeEtecoonPreInstruction PreInstruction,
    ushort InstructionList,
    ushort HorizontalSpeed);
