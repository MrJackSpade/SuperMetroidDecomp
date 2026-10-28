using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.SpriteObjects;

/// <summary>An independent reader of the cartridge's bank-$B4 sprite-object selector table and instruction lists.</summary>
internal static class RoomSpriteObjectNativeLists
{
    /// <summary>The native selector table <c>$B4:BDA8</c>.</summary>
    internal const int SelectorTable = 0xb4bda8;

    /// <summary>The cartridge's 62 object selectors.</summary>
    internal const int SelectorCount = 62;

    /// <summary><c>Instruction_SpriteObject_GoBack4Bytes</c> at $B4:BCF0.</summary>
    internal const ushort GoBack4Bytes = 0xbcf0;

    /// <summary><c>Instruction_SpriteObject_Delete</c> at $B4:BD07.</summary>
    internal const ushort Delete = 0xbd07;

    /// <summary><c>Instruction_SpriteObject_GotoParameter</c> at $B4:BD12.</summary>
    internal const ushort GotoParameter = 0xbd12;

    /// <summary>The bank-$B4 word at <paramref name="address"/>.</summary>
    internal static ushort Word(ISnesAddressSpace rom, int address) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWord(0xb40000 | (address & 0xffff));

    /// <summary>The list address selector <paramref name="selector"/> starts.</summary>
    internal static ushort Entry(ISnesAddressSpace rom, int selector) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWord(SelectorTable + selector * sizeof(ushort));

    /// <summary>Every spritemap the lists reachable from the 62 selectors show, following gotos.</summary>
    internal static IEnumerable<ushort> SpritemapPointers(ISnesAddressSpace rom)
    {
        var seen = new HashSet<ushort>();
        var work = new Stack<ushort>(Enumerable.Range(0, SelectorCount).Select(selector => Entry(rom, selector)));
        while (work.Count != 0)
        {
            ushort pointer = work.Pop();
            while (seen.Add(pointer))
            {
                ushort word = Word(rom, pointer);
                if ((word & 0x8000) == 0)
                {
                    yield return Word(rom, pointer + 2);
                    pointer = unchecked((ushort)(pointer + 4));
                    continue;
                }
                if (word == GotoParameter)
                    work.Push(Word(rom, pointer + 2));
                break;
            }
        }
    }

    /// <summary>The native sprite-object handler run on one list from the cartridge, as $B4:BC82 runs it.</summary>
    internal sealed class Reference(ISnesAddressSpace rom, ushort entry)
    {
        /// <summary>The native instruction pointer, zero once deleted.</summary>
        internal ushort Pointer { get; private set; } = entry;

        /// <summary>The native timer word.</summary>
        internal ushort Timer { get; private set; } = Word(rom, entry);

        /// <summary>The spritemap of the record shown, zero once deleted.</summary>
        internal ushort Spritemap { get; private set; } = Word(rom, entry + 2);

        /// <summary>Whether the object is live.</summary>
        internal bool IsActive => Pointer != 0;

        /// <summary>Runs one handler call.</summary>
        internal void Step()
        {
            if (!IsActive)
                return;
            if ((Timer & 0x8000) != 0)
            {
                Opcode();
                return;
            }
            ushort old = Timer;
            Timer = unchecked((ushort)(old - 1));
            if (old != 1)
                return;
            Pointer = unchecked((ushort)(Pointer + 4));
            Timer = Word(rom, Pointer);
            if ((Timer & 0x8000) != 0)
                Opcode();
            else
                Spritemap = Word(rom, Pointer + 2);
        }

        /// <summary>Runs the opcode the timer holds.</summary>
        private void Opcode()
        {
            switch (Timer)
            {
                case GoBack4Bytes:
                    Pointer = unchecked((ushort)(Pointer - 4));
                    Timer = 0x7fff;
                    return;
                case Delete:
                    Pointer = Timer = Spritemap = 0;
                    return;
                case GotoParameter:
                    Pointer = Word(rom, Pointer + 2);
                    Timer = Word(rom, Pointer);
                    if ((Timer & 0x8000) == 0)
                        Spritemap = Word(rom, Pointer + 2);
                    return;
                default:
                    throw new InvalidDataException($"Unknown sprite-object opcode ${Timer:X4} at $B4:{Pointer:X4}.");
            }
        }
    }
}
