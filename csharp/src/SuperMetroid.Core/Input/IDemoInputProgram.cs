namespace SuperMetroid.Core.Input;

/// <summary>
/// The owner of a demo-input object (#633): it supplies the object's compiled instruction list and
/// interprets the object's private pre-instructions and bank-$91 routines. An owner without private
/// routines says so explicitly; the interpreter then accepts only the shared return-only
/// pre-instructions and rejects every private opcode.
/// </summary>
public interface IDemoInputProgram
{
    /// <summary>Reads one word of the owner's bounded compiled instruction list.</summary>
    ushort ReadInstructionWord(ushort pointer);

    /// <summary>
    /// Runs the owner's pre-instruction at <paramref name="pointer"/>. Returns false when the owner
    /// has no private pre-instructions, leaving the interpreter to validate the shared ones.
    /// </summary>
    bool TryRunPreInstruction(DemoInputState demo, ushort pointer);

    /// <summary>
    /// Executes the private bank-$91 routine <paramref name="opcode"/> whose operands start at
    /// <paramref name="cursor"/>, or returns <see cref="DemoInputInstructionResult.NotHandled"/>.
    /// </summary>
    DemoInputInstructionResult RunSpecialInstruction(DemoInputState demo, ushort opcode, ushort cursor);
}
