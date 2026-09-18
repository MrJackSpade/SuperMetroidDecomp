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
    /// Returns the cartridge-selected initialization for authored offsets zero, two, and
    /// four. The cartridge clears bit zero, so the paired odd selectors choose the same
    /// records. Values outside the retail domain fail instead of reading adjacent code.
    /// </summary>
    internal static EscapeEtecoonInitialization Initialization(ushort parameter1)
    {
        ushort byteOffset = unchecked((ushort)(parameter1 & 0xfffe));
        int index = byteOffset >> 1;
        if ((uint)index < Initializations.Length)
            return Initializations[index];

        throw new ArgumentOutOfRangeException(
            nameof(parameter1),
            parameter1,
            "Escape Etecoon parameter must select one of the three authored roles.");
    }
}

/// <summary>One escape Etecoon's initial position, behavior, animation, and speed.</summary>
internal readonly record struct EscapeEtecoonInitialization(
    ushort XPosition,
    ushort YPosition,
    EscapeEtecoonPreInstruction PreInstruction,
    ushort InstructionList,
    ushort HorizontalSpeed);
