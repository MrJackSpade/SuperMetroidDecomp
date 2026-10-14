namespace SuperMetroid.Core.Game;

/// <summary>One word of a compiled instruction program: a mechanics value or a presentation slot.</summary>
/// <remarks>
/// Presentation slots are spritemap and similar artwork operands. Usually installed presentation
/// owns their values, so the program only records where they sit. A few programs (Mother Brain)
/// compile a fixed visual identity into the slot instead; <see cref="Visual"/> records it.
/// </remarks>
internal readonly record struct InstructionWord(ushort Value, bool IsPresentation, bool IsCompiledVisual = false)
{
    /// <summary>An operand slot whose value belongs to installed presentation.</summary>
    internal static InstructionWord Presentation => new(0, true);

    /// <summary>A presentation slot holding a compiled visual identity (a spritemap pointer).</summary>
    internal static InstructionWord Visual(ushort identity) => new(identity, true, true);

    /// <summary>Two packed byte operands (for example X/Y radii) read as one native word.</summary>
    internal static InstructionWord Bytes(byte low, byte high) => new((ushort)(SnesLittleEndian.Word(low, high)), false);

    /// <summary>Treats a literal native word as control data rather than an artwork operand.</summary>
    public static implicit operator InstructionWord(ushort value) => new(value, false);
}
