namespace SuperMetroid.Core.Programs;

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
    /// <summary>Immutable native-order item sequence from which word addresses are calculated.</summary>
    internal readonly InstructionItem[] items;
    /// <summary>Address index built once on first lookup instead of walking items on every instruction frame.</summary>
    /// <remarks>Lookups run per instruction and per byte; the layout is immutable, so its words are laid out once and
    /// indexed by address instead of walked on every query.</remarks>
    internal readonly Lazy<WordIndex> index;

    /// <summary>Checks the declared origin and named entries while counting mechanics and presentation words.</summary>
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
                // A matching entry and a plain word run only advance through their words.
                case InstructionItemKind.Entry:
                case InstructionItemKind.Words:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined instruction item kind {item.Kind}.");
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

    /// <summary>Native bank containing this compiled instruction layout.</summary>
    internal byte Bank { get; }
    /// <summary>Number of control, duration, and other nonvisual words owned by the layout.</summary>
    internal int MechanicsWordCount { get; }
    /// <summary>Number of visual operand slots, including compiled visual identities.</summary>
    internal int PresentationSlotCount { get; }

    /// <summary>Exclusive end of the last item.</summary>
    internal int EndAddress { get; }

    /// <summary>Address of the <paramref name="index"/>th presentation slot.</summary>
    internal ushort PresentationSlotAddress(int index)
    {
        if ((uint)index >= PresentationSlotCount) throw new IndexOutOfRangeException();
        return this.index.Value.Words[this.index.Value.Presentation[index]].Address;
    }

    /// <summary>Every laid-out word in address order, its address index, and its two orderings.</summary>
    internal sealed class WordIndex
    {
        /// <summary>All owned native words in address order.</summary>
        internal readonly (ushort Address, InstructionWord Word)[] Words;
        /// <summary>Positions of control and artwork words within the ordered word array.</summary>
        internal readonly int[] Mechanics, Presentation;
        /// <summary>Maps an exact native word address to its ordered position.</summary>
        internal readonly Dictionary<ushort, int> byAddress = [];

        /// <summary>Indexes one walk of the immutable program layout by address and word kind.</summary>
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
    }

    /// <summary>Allocation-free walk of every laid-out word in address order.</summary>
    internal struct WordWalker(InstructionItem[] items)
    {
        /// <summary>Current item cursor, beginning before the declared origin.</summary>
        private int item = -1;
        /// <summary>Current word within an item, reset at each origin or gap.</summary>
        private int word = -1;
        /// <summary>Native address of the current word, advanced across owned and skipped bytes.</summary>
        private int address;

        /// <summary>Uses this mutable walker directly to avoid iterator allocation.</summary>
        public readonly WordWalker GetEnumerator() => this;

        /// <summary>Native address and value/kind of the current laid-out word.</summary>
        public readonly (ushort Address, InstructionWord Word) Current =>
            ((ushort)address, items[item].Words[word]);

        /// <summary>Advances to the next owned word, honoring origins and unowned gaps.</summary>
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
