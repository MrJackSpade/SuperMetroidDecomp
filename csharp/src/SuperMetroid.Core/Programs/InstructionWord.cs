namespace SuperMetroid.Core.Programs;

/// <summary>One word of a compiled instruction program: a mechanics value or a presentation slot.</summary>
/// <remarks>
/// Presentation slots are spritemap and similar artwork operands. Usually installed presentation
/// owns their values, so the program only records where they sit. A few programs (Mother Brain)
/// compile a fixed visual identity into the slot instead; <c>Visual</c> records it.
/// </remarks>
internal readonly record struct InstructionWord(ushort Value, bool IsPresentation)
{
    /// <summary>An operand slot whose value belongs to installed presentation.</summary>
    internal static InstructionWord Presentation => new(0, true);

    /// <summary>Treats a literal native word as control data rather than an artwork operand.</summary>
    public static implicit operator InstructionWord(ushort value) => new(value, false);
}
