namespace SuperMetroid.Core.Game;

/// <summary>One word of a compiled instruction program: a mechanics value or a presentation slot.</summary>
/// <remarks>
/// Presentation slots are spritemap and similar artwork operands. Usually installed presentation
/// owns their values, so the program only records where they sit. A few programs (Mother Brain)
/// compile a fixed visual identity into the slot instead; <see cref="Visual"/> records it.
/// </remarks>
/// <param name="Value">Word value, meaningful for mechanics words and compiled visual slots.</param>
/// <param name="IsPresentation">Whether installed presentation supplies this operand.</param>
/// <param name="IsCompiledVisual">Whether a presentation word carries a compiled visual identity.</param>
internal readonly record struct InstructionWord(ushort Value, bool IsPresentation, bool IsCompiledVisual = false)
{
    /// <summary>An operand slot whose value belongs to installed presentation.</summary>
    internal static InstructionWord Presentation => new(0, true);

    /// <summary>A presentation slot holding a compiled visual identity (a spritemap pointer).</summary>
    internal static InstructionWord Visual(ushort identity) => new(identity, true, true);

    /// <summary>Two packed byte operands (for example X/Y radii) read as one native word.</summary>
    internal static InstructionWord Bytes(byte low, byte high) => new((ushort)(low | high << 8), false);

    /// <summary>Wraps a literal instruction word as mechanics data.</summary>
    /// <param name="value">Native word value to place in the program.</param>
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

    /// <summary>Describes whether this item emits words or changes the layout cursor.</summary>
    internal InstructionItemKind Kind { get; }
    /// <summary>Instruction words emitted by a word-bearing item; empty for layout markers.</summary>
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

    /// <summary>Declares an expected address and validates that preceding items reach it.</summary>
    /// <param name="address">Required native address at this point in the layout.</param>
    internal static InstructionItem Entry(ushort address) => new(InstructionItemKind.Entry, [], address);

    /// <summary>Moves the layout cursor forward to a later native address.</summary>
    /// <param name="address">New bank-local origin; it may not overlap preceding words.</param>
    internal static InstructionItem Origin(ushort address) => new(InstructionItemKind.Origin, [], address);

    /// <summary>Reserves bytes owned by another part of the program without decoding them here.</summary>
    /// <param name="bytes">Positive number of bytes to advance the layout cursor.</param>
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
    /// <summary>Immutable item sequence from which native addresses and word counts are derived.</summary>
    private readonly InstructionItem[] items;
    // Lookups run per instruction and per byte; the layout is immutable, so its words are laid
    // out once and indexed by address instead of walked on every query.
    internal readonly Lazy<WordIndex> index;

    /// <summary>Builds the address index and validates each declared entry against the preceding layout.</summary>
    /// <param name="bank">Native bank shared by the program's addresses.</param>
    /// <param name="items">Ordered instructions, entries, origins, and skipped ranges.</param>
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

    /// <summary>Native bank containing this instruction program.</summary>
    internal byte Bank { get; }
    /// <summary>Number of laid-out words whose values are mechanics data.</summary>
    internal int MechanicsWordCount { get; }
    /// <summary>Number of presentation operand slots in the layout.</summary>
    internal int PresentationSlotCount { get; }

    /// <summary>Exclusive end of the last item.</summary>
    internal int EndAddress { get; }

    /// <summary>Address of the <paramref name="index"/>th presentation slot.</summary>
    internal ushort PresentationSlotAddress(int index)
    {
        if ((uint)index >= PresentationSlotCount) throw new IndexOutOfRangeException();
        return this.index.Value.Words[this.index.Value.Presentation[index]].Address;
    }

    /// <summary>Looks up a word only when its slot is owned by mechanics rather than presentation.</summary>
    /// <param name="address">Native address to query.</param>
    /// <param name="value">Receives the word when the address contains mechanics data.</param>
    /// <returns>True only for a laid-out mechanics word.</returns>
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

    /// <summary>Reports whether an address belongs to an installed-presentation operand slot.</summary>
    /// <param name="address">Native address to query.</param>
    /// <returns>True when the laid-out word is a presentation operand.</returns>
    internal bool IsPresentationWord(ushort address) =>
        index.Value.TryGet(address, out InstructionWord word) && word.IsPresentation;

    /// <summary>True when <paramref name="address"/> is the first byte of any laid-out word.</summary>
    internal bool Owns(ushort address) => index.Value.TryGet(address, out _);

    /// <summary>Every laid-out word in address order, its address index, and its two orderings.</summary>
    internal sealed class WordIndex
    {
        /// <summary>All laid-out words paired with their native addresses in ascending order.</summary>
        internal readonly (ushort Address, InstructionWord Word)[] Words;
        /// <summary>Indexes into <see cref="Words"/> partitioning mechanics-owned values from presentation operands.</summary>
        internal readonly int[] Mechanics, Presentation;
        /// <summary>Maps each owned native address to its position in <see cref="Words"/>.</summary>
        private readonly Dictionary<ushort, int> byAddress = [];

        /// <summary>Materializes address-ordered words and their mechanics/presentation index lists.</summary>
        /// <param name="walker">Allocation-free traversal of the compiled layout.</param>
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

        /// <summary>Finds the compiled word beginning at an exact native address.</summary>
        /// <param name="address">Address to locate.</param>
        /// <param name="word">Receives the word when the address is owned by this program.</param>
        /// <returns>True when an instruction word begins at the requested address.</returns>
        internal bool TryGet(ushort address, out InstructionWord word)
        {
            bool found = byAddress.TryGetValue(address, out int position);
            word = found ? Words[position].Word : default;
            return found;
        }
    }

    /// <summary>Allocation-free walk of every laid-out word in address order.</summary>
    /// <param name="items">Program items whose words and address shifts are traversed.</param>
    internal struct WordWalker(InstructionItem[] items)
    {
        /// <summary>Current item position, initially before the first layout item.</summary>
        private int item = -1;
        /// <summary>Current word within the item, initially before its first word.</summary>
        private int word = -1;
        /// <summary>Address cursor updated by word widths, origins, and skipped byte ranges.</summary>
        private int address;

        /// <summary>Returns a copy of this value-type walker for foreach enumeration.</summary>
        public readonly WordWalker GetEnumerator() => this;

        /// <summary>Address and instruction word selected by the current cursor.</summary>
        public readonly (ushort Address, InstructionWord Word) Current =>
            ((ushort)address, items[item].Words[word]);

        /// <summary>Advances to the next laid-out word, applying origin and skip markers along the way.</summary>
        /// <returns>True when another word is available.</returns>
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
