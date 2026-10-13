using SuperMetroid.Core;
using SuperMetroid.Core.Input;

/// <summary>A demo-input owner whose list uses only the shared instructions and pre-instructions (#633).</summary>
internal sealed class DemoInputListProgram : IDemoInputProgram
{
    /// <summary>Reads words from the owning fixture's bounded compiled demo list.</summary>
    private readonly Func<ushort, ushort> readInstructionWord;

    /// <summary>Wraps the owner's bounded compiled list reader.</summary>
    public DemoInputListProgram(Func<ushort, ushort> readInstructionWord) =>
        this.readInstructionWord = Ensure.NotNull(readInstructionWord);

    /// <inheritdoc/>
    public ushort ReadInstructionWord(ushort pointer) => readInstructionWord(pointer);

    /// <inheritdoc/>
    public bool TryRunPreInstruction(DemoInputState demo, ushort pointer) => false;

    /// <inheritdoc/>
    public DemoInputInstructionResult RunSpecialInstruction(DemoInputState demo, ushort opcode, ushort cursor) =>
        DemoInputInstructionResult.NotHandled(cursor);
}
