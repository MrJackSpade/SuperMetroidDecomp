namespace SuperMetroid.Core.Programs;

/// <summary>One item of a program in native address order.</summary>
internal readonly struct InstructionItem
{
    /// <summary>Captures a word run, named entry, address origin, or unowned byte gap.</summary>
    private InstructionItem(InstructionItemKind kind, InstructionWord[] words, ushort value)
    {
        Kind = kind;
        Words = words;
        Value = value;
    }

    /// <summary>Determines whether this item contributes words or only changes the layout cursor.</summary>
    internal InstructionItemKind Kind { get; }
    /// <summary>Ordered words owned by this item; empty for entries, origins, and gaps.</summary>
    internal InstructionWord[] Words { get; }

    /// <summary>Entry or origin address, or the skipped byte count.</summary>
    internal ushort Value { get; }

    /// <summary>An instruction opcode followed by its operands.</summary>
    internal static InstructionItem Op(params InstructionWord[] words)
    {
        if (words.Length == 0) throw new ArgumentException("An instruction needs its opcode word.", nameof(words));
        return new(InstructionItemKind.Words, words, 0);
    }

    /// <summary>A timed frame: its duration followed by its presentation operand.</summary>
    internal static InstructionItem Frame(ushort duration) =>
        new(InstructionItemKind.Words, [duration, InstructionWord.Presentation], 0);

    /// <summary>Asserts that preceding items land exactly at a native named entry point.</summary>
    internal static InstructionItem Entry(ushort address) => new(InstructionItemKind.Entry, [], address);

    /// <summary>Resumes layout at a later native address after bytes owned by another program.</summary>
    internal static InstructionItem Origin(ushort address) => new(InstructionItemKind.Origin, [], address);
}
