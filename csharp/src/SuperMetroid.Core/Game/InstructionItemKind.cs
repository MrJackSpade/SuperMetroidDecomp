namespace SuperMetroid.Core.Game;

/// <summary>How an <see cref="InstructionItem"/> participates in the layout.</summary>
internal enum InstructionItemKind : byte
{
    /// <summary>Words of one instruction or timed frame.</summary>
    Words,
    /// <summary>Requires the layout to be at the named native entry point.</summary>
    Entry,
    /// <summary>Continues the layout at an address; the gap belongs to another owner.</summary>
    Origin,
    /// <summary>Bytes the program does not own, such as a packed sound byte operand.</summary>
    Skip,
}
