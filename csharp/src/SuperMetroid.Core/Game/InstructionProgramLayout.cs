namespace SuperMetroid.Core.Game;

/// <summary>One word of a compiled instruction program: a mechanics value or a presentation slot.</summary>
/// <remarks>
/// Presentation slots are spritemap and similar artwork operands. Installed presentation owns
/// their values, so the program only records where they sit.
/// </remarks>
internal readonly record struct InstructionWord(ushort Value, bool IsPresentation)
{
    /// <summary>An operand slot whose value belongs to installed presentation.</summary>
    internal static InstructionWord Presentation => new(0, true);

    /// <summary>Two packed byte operands (for example X/Y radii) read as one native word.</summary>
    internal static InstructionWord Bytes(byte low, byte high) => new((ushort)(low | high << 8), false);

    public static implicit operator InstructionWord(ushort value) => new(value, false);
}

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

/// <summary>One item of a program in native address order.</summary>
internal readonly struct InstructionItem
{
    private InstructionItem(InstructionItemKind kind, InstructionWord[] words, ushort value)
    {
        Kind = kind;
        Words = words;
        Value = value;
    }

    internal InstructionItemKind Kind { get; }
    internal InstructionWord[] Words { get; }

    /// <summary>Entry or origin address, or the skipped byte count.</summary>
    internal ushort Value { get; }

    /// <summary>An instruction opcode followed by its operands.</summary>
    internal static InstructionItem Op(params InstructionWord[] words)
    {
        if (words.Length == 0) throw new ArgumentException("An instruction needs its opcode word.", nameof(words));
        return new(InstructionItemKind.Words, words, 0);
    }

    /// <summary>
    /// The largest positive frame timer, <c>$7FFF</c>: the pose holds until a callback or other
    /// state moves the instruction pointer, rather than expiring on a chosen cadence.
    /// </summary>
    internal const ushort IndefiniteDuration = 0x7fff;

    /// <summary>A timed frame: its duration followed by its presentation operand.</summary>
    internal static InstructionItem Frame(ushort duration) =>
        new(InstructionItemKind.Words, [duration, InstructionWord.Presentation], 0);

    internal static InstructionItem Entry(ushort address) => new(InstructionItemKind.Entry, [], address);

    internal static InstructionItem Origin(ushort address) => new(InstructionItemKind.Origin, [], address);

    internal static InstructionItem Skip(int bytes)
    {
        if (bytes is <= 0 or > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bytes));
        return new(InstructionItemKind.Skip, [], (ushort)bytes);
    }
}

/// <summary>
/// Native word layout of compiled instruction programs. Addresses follow from item order,
/// so programs are written as instructions rather than address/value tables.
/// </summary>
/// <remarks>
/// Every <see cref="InstructionItem.Entry"/> is checked when the layout is built; a program
/// whose items no longer reach a named entry fails at type initialization.
/// </remarks>
internal sealed class InstructionProgramLayout
{
    private readonly InstructionItem[] items;

    internal InstructionProgramLayout(byte bank, params InstructionItem[] items)
    {
        if (items.Length == 0 || items[0].Kind != InstructionItemKind.Origin)
            throw new ArgumentException("A program layout starts with its origin.", nameof(items));
        Bank = bank;
        this.items = items;
        int address = 0;
        foreach (InstructionItem item in items)
        {
            switch (item.Kind)
            {
                case InstructionItemKind.Origin when item.Value < address:
                    throw new InvalidDataException($"Instruction origin ${bank:X2}:{item.Value:X4} overlaps the preceding words.");
                case InstructionItemKind.Origin:
                    address = item.Value;
                    break;
                case InstructionItemKind.Entry when item.Value != address:
                    throw new InvalidDataException(
                        $"Instruction entry ${bank:X2}:{item.Value:X4} is laid out at ${bank:X2}:{address:X4}.");
                case InstructionItemKind.Skip:
                    address += item.Value;
                    break;
            }
            foreach (InstructionWord word in item.Words)
            {
                if (word.IsPresentation) PresentationSlotCount++;
                else MechanicsWordCount++;
                address += sizeof(ushort);
            }
        }
        EndAddress = address;
    }

    internal byte Bank { get; }
    internal int MechanicsWordCount { get; }
    internal int PresentationSlotCount { get; }

    /// <summary>Exclusive end of the last item.</summary>
    internal int EndAddress { get; }

    /// <summary>The <paramref name="index"/>th mechanics word in address order.</summary>
    internal (ushort Address, ushort Value) MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        foreach ((ushort address, InstructionWord word) in Words())
            if (!word.IsPresentation && index-- == 0)
                return (address, word.Value);
        throw new InvalidOperationException("Instruction mechanics count is inconsistent.");
    }

    /// <summary>Address of the <paramref name="index"/>th presentation slot.</summary>
    internal ushort PresentationSlotAddress(int index)
    {
        if ((uint)index >= PresentationSlotCount) throw new IndexOutOfRangeException();
        foreach ((ushort address, InstructionWord word) in Words())
            if (word.IsPresentation && index-- == 0)
                return address;
        throw new InvalidOperationException("Instruction presentation count is inconsistent.");
    }

    internal bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach ((ushort wordAddress, InstructionWord word) in Words())
        {
            if (wordAddress != address) continue;
            value = word.Value;
            return !word.IsPresentation;
        }
        value = 0;
        return false;
    }

    internal bool IsPresentationWord(ushort address)
    {
        foreach ((ushort wordAddress, InstructionWord word) in Words())
            if (wordAddress == address)
                return word.IsPresentation;
        return false;
    }

    /// <summary>
    /// Counts the timed frames that run from <paramref name="entry"/> and returns the opcode of
    /// the first control instruction after them.
    /// </summary>
    internal (int Frames, ushort Terminator) FramesFrom(ushort entry)
    {
        int index = Array.FindIndex(items, item => item.Kind == InstructionItemKind.Entry && item.Value == entry);
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(entry), entry, "Entry is not part of this layout.");
        int frames = 0;
        for (index++; index < items.Length; index++)
        {
            InstructionItem item = items[index];
            if (item.Kind != InstructionItemKind.Words) continue;
            if (item.Words is [{ IsPresentation: false }, { IsPresentation: true }]) { frames++; continue; }
            return (frames, item.Words[0].Value);
        }
        throw new InvalidDataException($"Instruction entry ${Bank:X2}:{entry:X4} has no terminating instruction.");
    }

    /// <summary>Finds the presentation slot containing either byte at <paramref name="longAddress"/>.</summary>
    internal bool TryGetPresentationWord(int longAddress, out ushort wordAddress)
    {
        wordAddress = 0;
        if (longAddress >> 16 != Bank) return false;
        int bankAddress = longAddress & ushort.MaxValue;
        foreach ((ushort address, InstructionWord word) in Words())
        {
            if (!word.IsPresentation || (bankAddress != address && bankAddress != address + 1)) continue;
            wordAddress = address;
            return true;
        }
        return false;
    }

    /// <summary>True when either byte of a mechanics word lies at <paramref name="longAddress"/>.</summary>
    internal bool IsCompiledMechanicsByte(int longAddress)
    {
        if (longAddress >> 16 != Bank) return false;
        int bankAddress = longAddress & ushort.MaxValue;
        foreach ((ushort wordAddress, InstructionWord word) in Words())
            if (!word.IsPresentation && (bankAddress == wordAddress || bankAddress == wordAddress + 1))
                return true;
        return false;
    }

    /// <summary>True when <paramref name="address"/> is the first byte of any laid-out word.</summary>
    internal bool Owns(ushort address)
    {
        foreach ((ushort wordAddress, _) in Words())
            if (wordAddress == address)
                return true;
        return false;
    }

    private WordWalker Words() => new(items);

    /// <summary>Allocation-free walk of every laid-out word in address order.</summary>
    private struct WordWalker(InstructionItem[] items)
    {
        private int item = -1;
        private int word = -1;
        private int address;

        public readonly WordWalker GetEnumerator() => this;

        public readonly (ushort Address, InstructionWord Word) Current =>
            ((ushort)address, items[item].Words[word]);

        public bool MoveNext()
        {
            if (item >= 0 && word >= 0) address += sizeof(ushort);
            while (true)
            {
                if (item >= 0 && ++word < items[item].Words.Length) return true;
                if (++item >= items.Length) return false;
                word = -1;
                if (items[item].Kind == InstructionItemKind.Origin) address = items[item].Value;
                else if (items[item].Kind == InstructionItemKind.Skip) address += items[item].Value;
            }
        }
    }
}
