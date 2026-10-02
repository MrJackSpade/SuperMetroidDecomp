using SuperMetroid.Core.Game;

// Bank files contain remaining literal selectors. Calculated families are dispatched
// here and merged into the public enumeration without materializing a lookup cache.
namespace SuperMetroid.Core.Assets;

/// <summary>One immutable cartridge visual-pointer operand and its selected target.</summary>
internal readonly record struct CompiledEnemyVisualSelector(int Address, ushort Pointer);

/// <summary>
/// Sparse fixed visual selectors from compiled instruction catalogs. These are
/// engine definitions, not editable art, callback code, or a reconstructed ROM.
/// A selected target still needs its own renderer and presentation asset.
/// </summary>
internal static partial class CompiledEnemyVisualSelectors
{
    private static readonly CompiledEnemyVisualSelector[] Entries =
    [
        .. Bank86,
        .. BankA2,
        .. BankA3,
        .. BankA4,
        .. BankA5,
        .. BankA6,
        .. BankA7,
        .. BankA8,
        .. BankA9,
        .. BankAA,
        .. BankB2,
        .. BankB3,
        .. BankB4,
    ];

    private static int CalculatedCount => BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount + BlueBrinstarFaceBlockInstructionProgramDefinitions.PresentationWordCount + BeetomInstructionProgramDefinitions.PresentationWordCount + PlatformInstructionProgramDefinitions.PresentationWordCount + ElevatorInstructionProgramDefinitions.PresentationWordCount + GrowingShutterInstructionProgramDefinitions.PresentationWordCount + HorizontalShutterInstructionProgramDefinitions.PresentationWordCount + (VerticalShutterInstructionProgramDefinitions.PresentationWordCount - 1) + AlcoonFireballInstructionProgramDefinitions.PresentationWordCount + FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount + BoyonInstructionProgramDefinitions.PresentationWordCount +
        BoulderInstructionProgramDefinitions.PresentationWordCount +
        FakeKraidInstructionProgramDefinitions.PresentationWordCount +
        KraidNailInstructionProgramDefinitions.PresentationWordCount +
        FuneNamiheInstructionProgramDefinitions.PresentationWordCount + AlcoonInstructionProgramDefinitions.PresentationWordCount + AtomicInstructionProgramDefinitions.PresentationWordCount;
    internal static int Count => Entries.Length + CalculatedCount;

    internal static CompiledEnemyVisualSelector At(int index)
    {
        if ((uint)index >= Count) throw new IndexOutOfRangeException();
        // Calculated entries are ordered by native bank/address. Their merged rank
        // is their own ordinal plus the number of literal entries before them.
        int low = 0, high = CalculatedCount;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            CompiledEnemyVisualSelector candidate = CalculatedAt(middle);
            int literalLow = 0, literalHigh = Entries.Length;
            while (literalLow < literalHigh)
            {
                int literalMiddle = literalLow + (literalHigh - literalLow) / 2;
                if (Entries[literalMiddle].Address < candidate.Address) literalLow = literalMiddle + 1;
                else literalHigh = literalMiddle;
            }
            int rank = literalLow + middle;
            if (rank == index) return candidate;
            if (rank < index) low = middle + 1;
            else high = middle;
        }
        return Entries[index - low];
    }

    private static CompiledEnemyVisualSelector CalculatedAt(int index)
    {
        if (index < AlcoonFireballInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(operand));
        }
        index -= AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
        if (index < BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.BombTorizoDroolFrameAt(operand));
        }
        index -= BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount;
        if (index < FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(operand));
        }
        index -= FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount;
        if (index < BoyonInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = BoyonInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa20000 | operand, EnemySpritemapDefinitions.BoyonFrameAt(operand));
        }
        index -= BoyonInstructionProgramDefinitions.PresentationWordCount;
        int growingCount = GrowingShutterInstructionProgramDefinitions.PresentationWordCount;
        if (index < growingCount + HorizontalShutterInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = index < growingCount
                ? GrowingShutterInstructionProgramDefinitions.PresentationWordAddress(index)
                : HorizontalShutterInstructionProgramDefinitions.PresentationWordAddress(index - growingCount);
            return new(0xa20000 | operand, ShutterVisualDefinitions.PointerAt(operand));
        }
        index -= growingCount + HorizontalShutterInstructionProgramDefinitions.PresentationWordCount;
        int kamerCount = VerticalShutterInstructionProgramDefinitions.PresentationWordCount - 1;
        if (index < kamerCount)
        {
            ushort operand = VerticalShutterInstructionProgramDefinitions.PresentationWordAddress(index + 1);
            return new(0xa20000 | operand, EnemySpritemapDefinitions.KamerPlatformFrameAt(operand));
        }
        index -= kamerCount;
        if (index < ElevatorInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = ElevatorInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa30000 | operand, EnemySpritemapDefinitions.ElevatorFrameAt(operand));
        }
        index -= ElevatorInstructionProgramDefinitions.PresentationWordCount;
        if (index < PlatformInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = PlatformInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa30000 | operand, TripperKamerVisualDefinitions.FrameAt(operand));
        }
        index -= PlatformInstructionProgramDefinitions.PresentationWordCount;
        if (index < BoulderInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = BoulderInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa60000 | operand, EnemySpritemapDefinitions.BoulderFrameAt(operand));
        }
        index -= BoulderInstructionProgramDefinitions.PresentationWordCount;
        if (index < FakeKraidInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = FakeKraidInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa60000 | operand, KraidVisualDefinitions.FrameAt(RoomEnemySystem.FakeKraidDefinition, operand));
        }
        index -= FakeKraidInstructionProgramDefinitions.PresentationWordCount;
        if (index < KraidNailInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = KraidNailInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa70000 | operand, KraidVisualDefinitions.FrameAt(RoomEnemySystem.KraidGoodNailDefinition, operand));
        }
        index -= KraidNailInstructionProgramDefinitions.PresentationWordCount;
        if (index < FuneNamiheInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = FuneNamiheInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.FuneNamiheFrameAt(operand));
        }
        index -= FuneNamiheInstructionProgramDefinitions.PresentationWordCount;
        if (index < BeetomInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = BeetomInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.BeetomFrameAt(operand));
        }
        index -= BeetomInstructionProgramDefinitions.PresentationWordCount;
        if (index < AlcoonInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = AlcoonInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AlcoonFrameAt(operand));
        }
        index -= AlcoonInstructionProgramDefinitions.PresentationWordCount;
        if (index < AtomicInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = AtomicInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AtomicFrameAt(operand));
        }
        index -= AtomicInstructionProgramDefinitions.PresentationWordCount;
        ushort faceOperand = BlueBrinstarFaceBlockInstructionProgramDefinitions.PresentationWordAddress(index);
        return new(0xa80000 | faceOperand, BlueBrinstarFaceBlockVisualDefinitions.FrameAt(faceOperand));
    }

    internal static bool IsCalculatedSelector(int address) => (address >> 16) switch
    {
        0x86 => BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord((ushort)address) || AlcoonFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address) || FuneNamiheFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa2 => ShutterVisualDefinitions.IsPresentationWord((ushort)address) || BoyonInstructionProgramDefinitions.IsPresentationWord((ushort)address) || VerticalShutterInstructionProgramDefinitions.IsKamerPresentationWord((ushort)address),
        0xa3 => ElevatorInstructionProgramDefinitions.IsPresentationWord((ushort)address) || PlatformInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa6 => BoulderInstructionProgramDefinitions.IsPresentationWord((ushort)address) ||
            FakeKraidInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa7 => KraidNailInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa8 => BlueBrinstarFaceBlockInstructionProgramDefinitions.IsPresentationWord((ushort)address) || BeetomInstructionProgramDefinitions.IsPresentationWord((ushort)address) || FuneNamiheInstructionProgramDefinitions.IsPresentationWord((ushort)address) || AlcoonInstructionProgramDefinitions.IsPresentationWord((ushort)address) ||
            AtomicInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        _ => false,
    };
    internal static bool TryGet(byte bank, ushort operandAddress, out ushort pointer)
    {
        int key = (bank << 16) | operandAddress;
        if (IsCalculatedSelector(key))
        {
            pointer = bank switch
            {
                0x86 => AlcoonFireballInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(operandAddress)
                    : BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemyProjectileSpritemapDefinitions.BombTorizoDroolFrameAt(operandAddress)
                    : EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(operandAddress),
                0xa2 => ShutterVisualDefinitions.IsPresentationWord(operandAddress) ? ShutterVisualDefinitions.PointerAt(operandAddress)
                    : VerticalShutterInstructionProgramDefinitions.IsKamerPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.KamerPlatformFrameAt(operandAddress) : EnemySpritemapDefinitions.BoyonFrameAt(operandAddress),
                0xa3 => PlatformInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? TripperKamerVisualDefinitions.FrameAt(operandAddress) : EnemySpritemapDefinitions.ElevatorFrameAt(operandAddress),
                0xa6 => BoulderInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.BoulderFrameAt(operandAddress)
                    : KraidVisualDefinitions.FrameAt(RoomEnemySystem.FakeKraidDefinition, operandAddress),
                0xa7 => KraidVisualDefinitions.FrameAt(RoomEnemySystem.KraidGoodNailDefinition, operandAddress),
                _ => FuneNamiheInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.FuneNamiheFrameAt(operandAddress)
                    : AlcoonInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.AlcoonFrameAt(operandAddress)
                    : BeetomInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.BeetomFrameAt(operandAddress)
                    : BlueBrinstarFaceBlockInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? BlueBrinstarFaceBlockVisualDefinitions.FrameAt(operandAddress)
                    : EnemySpritemapDefinitions.AtomicFrameAt(operandAddress),
            };
            return true;
        }
        int low = 0;
        int high = Entries.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CompiledEnemyVisualSelector entry = Entries[middle];
            if (entry.Address == key)
            {
                pointer = entry.Pointer;
                return true;
            }
            if (entry.Address < key)
                low = middle + 1;
            else
                high = middle - 1;
        }
        pointer = 0;
        return false;
    }
}
