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

    /// <summary>A timed frame whose visual operand is a compiled identity.</summary>
    internal static InstructionItem Frame(ushort duration, ushort visual) =>
        new(InstructionItemKind.Words, [duration, InstructionWord.Visual(visual)], 0);

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
    // Lookups run per instruction and per byte; the layout is immutable, so its words are laid
    // out once and indexed by address instead of walked on every query.
    private readonly Lazy<WordIndex> index;

    internal InstructionProgramLayout(byte bank, params InstructionItem[] items)
    {
        if (items.Length == 0 || items[0].Kind != InstructionItemKind.Origin)
            throw new ArgumentException("A program layout starts with its origin.", nameof(items));
        Bank = bank;
        this.items = items;
        index = new(() => new WordIndex(new WordWalker(items)));
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
        var (address, word) = this.index.Value.Words[this.index.Value.Mechanics[index]];
        return (address, word.Value);
    }

    /// <summary>Address of the <paramref name="index"/>th presentation slot.</summary>
    internal ushort PresentationSlotAddress(int index)
    {
        if ((uint)index >= PresentationSlotCount) throw new IndexOutOfRangeException();
        return this.index.Value.Words[this.index.Value.Presentation[index]].Address;
    }

    internal bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (index.Value.TryGet(address, out InstructionWord word))
        {
            value = word.Value;
            return !word.IsPresentation;
        }
        value = 0;
        return false;
    }

    /// <summary>
    /// Reads any compiled word: a mechanics value or a compiled visual identity. Installed
    /// presentation slots have no compiled value and read as absent.
    /// </summary>
    internal bool TryReadWord(ushort address, out ushort value)
    {
        if (index.Value.TryGet(address, out InstructionWord word))
        {
            value = word.Value;
            return !word.IsPresentation || word.IsCompiledVisual;
        }
        value = 0;
        return false;
    }

    internal bool IsPresentationWord(ushort address) =>
        index.Value.TryGet(address, out InstructionWord word) && word.IsPresentation;

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
        // Words never overlap, so the byte belongs to the word starting one byte before it or at it.
        if (TryWordContaining(bankAddress, out ushort start, out InstructionWord word) && word.IsPresentation)
        {
            wordAddress = start;
            return true;
        }
        return false;
    }

    /// <summary>The word containing <paramref name="bankAddress"/>, checking the earlier start first.</summary>
    private bool TryWordContaining(int bankAddress, out ushort start, out InstructionWord word)
    {
        if (bankAddress > 0 && index.Value.TryGet((ushort)(bankAddress - 1), out word))
        {
            start = (ushort)(bankAddress - 1);
            return true;
        }
        if (bankAddress <= ushort.MaxValue && index.Value.TryGet((ushort)bankAddress, out word))
        {
            start = (ushort)bankAddress;
            return true;
        }
        start = 0;
        word = default;
        return false;
    }

    /// <summary>True when either byte of a mechanics word lies at <paramref name="longAddress"/>.</summary>
    internal bool IsCompiledMechanicsByte(int longAddress)
    {
        if (longAddress >> 16 != Bank) return false;
        int bankAddress = longAddress & ushort.MaxValue;
        return TryWordContaining(bankAddress, out _, out InstructionWord word) && !word.IsPresentation;
    }

    /// <summary>True when <paramref name="address"/> is the first byte of any laid-out word.</summary>
    internal bool Owns(ushort address) => index.Value.TryGet(address, out _);

    /// <summary>Every laid-out word in address order, its address index, and its two orderings.</summary>
    private sealed class WordIndex
    {
        internal readonly (ushort Address, InstructionWord Word)[] Words;
        internal readonly int[] Mechanics, Presentation;
        private readonly Dictionary<ushort, int> byAddress = [];

        internal WordIndex(WordWalker walker)
        {
            var words = new List<(ushort, InstructionWord)>();
            var mechanics = new List<int>();
            var presentation = new List<int>();
            foreach ((ushort address, InstructionWord word) in walker)
            {
                (word.IsPresentation ? presentation : mechanics).Add(words.Count);
                byAddress.TryAdd(address, words.Count);
                words.Add((address, word));
            }
            Words = [.. words];
            Mechanics = [.. mechanics];
            Presentation = [.. presentation];
        }

        internal bool TryGet(ushort address, out InstructionWord word)
        {
            bool found = byAddress.TryGetValue(address, out int position);
            word = found ? Words[position].Word : default;
            return found;
        }
    }

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
