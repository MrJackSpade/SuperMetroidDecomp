namespace SuperMetroid.Core.Input;

/// <summary>Control-flow result from one explicitly translated demo-list instruction.</summary>
/// <param name="Handled">Whether the callback recognizes the bank-$91 routine pointer.</param>
/// <param name="NextInstructionPointer">Cursor after all operands consumed by that routine.</param>
/// <param name="TerminateProcessing">Whether the generic handler must return immediately.</param>
public readonly record struct DemoInputInstructionResult(
    bool Handled,
    ushort NextInstructionPointer,
    bool TerminateProcessing)
{
    /// <summary>Reports that a callback does not own the encountered routine.</summary>
    public static DemoInputInstructionResult NotHandled(ushort argumentPointer) =>
        new(false, argumentPointer, false);

    /// <summary>Continues decoding at the supplied cursor after a handled instruction.</summary>
    public static DemoInputInstructionResult ContinueAt(ushort nextInstructionPointer) =>
        new(true, nextInstructionPointer, false);
}
