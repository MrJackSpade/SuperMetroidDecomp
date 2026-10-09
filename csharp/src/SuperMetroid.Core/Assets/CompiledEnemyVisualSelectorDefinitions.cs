using SuperMetroid.Core.Game;

// Bank files contain remaining literal selectors. Calculated families are dispatched
// here and merged into the public enumeration without materializing a lookup cache.
namespace SuperMetroid.Core.Assets;

/// <summary>One immutable cartridge visual-pointer operand and its selected target.</summary>
/// <param name="Address">Combined bank and operand address used as the lookup key.</param>
/// <param name="Pointer">Selected spritemap or frame pointer supplied for that operand.</param>
internal readonly record struct CompiledEnemyVisualSelector(int Address, ushort Pointer);

/// <summary>
/// Sparse fixed visual selectors from compiled instruction catalogs. These are
/// engine definitions, not editable art, callback code, or a reconstructed ROM.
/// A selected target still needs its own renderer and presentation asset.
/// </summary>
internal static partial class CompiledEnemyVisualSelectors
{
    /// <summary>Sorted sparse literal selectors searched after calculated selector families have been handled.</summary>
    internal static readonly CompiledEnemyVisualSelector[] Entries =
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

    /// <summary>Identifies instruction operands whose visual pointers are derived by a family-specific resolver.</summary>
    /// <param name="address">Combined bank and operand address to classify.</param>
    /// <returns><see langword="true"/> when a calculated selector family owns the address.</returns>
    internal static bool IsCalculatedSelector(int address) => (address >> 16) switch
    {
        0x86 => BotwoonProjectileInstructionProgramDefinitions.IsPresentationWord((ushort)address) || BombTorizoStatueInstructionProgramDefinitions.IsPresentationWord((ushort)address) || BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord((ushort)address) || AlcoonFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address) || FuneNamiheFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa2 => ShutterVisualDefinitions.IsPresentationWord((ushort)address) || BoyonInstructionProgramDefinitions.IsPresentationWord((ushort)address) || VerticalShutterInstructionProgramDefinitions.IsKamerPresentationWord((ushort)address),
        0xa3 => ElevatorInstructionProgramDefinitions.IsPresentationWord((ushort)address) || PlatformInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa6 => BoulderInstructionProgramDefinitions.IsPresentationWord((ushort)address) ||
            FakeKraidInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa7 => KraidNailInstructionProgramDefinitions.IsPresentationWord((ushort)address) || PhantoonInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa8 => BlueBrinstarFaceBlockInstructionProgramDefinitions.IsPresentationWord((ushort)address) || BeetomInstructionProgramDefinitions.IsPresentationWord((ushort)address) || FuneNamiheInstructionProgramDefinitions.IsPresentationWord((ushort)address) || AlcoonInstructionProgramDefinitions.IsPresentationWord((ushort)address) ||
            AtomicInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xb3 => BotwoonInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        _ => false,
    };
    /// <summary>Resolves a visual operand through calculated families first, then the sparse literal selector table.</summary>
    /// <param name="bank">Bank byte containing the instruction operand.</param>
    /// <param name="operandAddress">Bank-local address of the visual-pointer word.</param>
    /// <param name="pointer">Receives the selected frame pointer, or zero when no selector is compiled for the operand.</param>
    /// <returns><see langword="true"/> when a calculated or literal selector supplies a target.</returns>
    internal static bool TryGet(byte bank, ushort operandAddress, out ushort pointer)
    {
        if (bank == 0xa2 && operandAddress == PolypInstructionProgramDefinitions.PresentationWord)
        {
            pointer = PolypInstructionProgramDefinitions.FrameAt(operandAddress);
            return true;
        }
        if (bank == 0x86 && operandAddress == PolypRockInstructionProgramDefinitions.PresentationWord)
        {
            pointer = PolypRockInstructionProgramDefinitions.FrameAt(operandAddress);
            return true;
        }
        int key = (bank << 16) | operandAddress;
        if (IsCalculatedSelector(key))
        {
            pointer = bank switch
            {
                0x86 => AlcoonFireballInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(operandAddress)
                    : BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemyProjectileSpritemapDefinitions.BombTorizoDroolFrameAt(operandAddress)
                    : BombTorizoStatueInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemyProjectileSpritemapDefinitions.BombTorizoStatueFrameAt(operandAddress)
                    : BotwoonProjectileInstructionProgramDefinitions.IsPresentationWord(operandAddress) ? EnemyProjectileSpritemapDefinitions.BotwoonProjectileFrameAt(operandAddress) : EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(operandAddress),
                0xa2 => ShutterVisualDefinitions.IsPresentationWord(operandAddress) ? ShutterVisualDefinitions.PointerAt(operandAddress)
                    : VerticalShutterInstructionProgramDefinitions.IsKamerPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.KamerPlatformFrameAt(operandAddress) : EnemySpritemapDefinitions.BoyonFrameAt(operandAddress),
                0xa3 => PlatformInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? TripperKamerVisualDefinitions.FrameAt(operandAddress) : EnemySpritemapDefinitions.ElevatorFrameAt(operandAddress),
                0xa6 => BoulderInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.BoulderFrameAt(operandAddress)
                    : KraidVisualDefinitions.FrameAt(RoomEnemySystem.FakeKraidDefinition, operandAddress),
                0xa7 => PhantoonInstructionProgramDefinitions.IsPresentationWord(operandAddress) ? PhantoonInstructionProgramDefinitions.FrameAt(operandAddress)
                    : KraidVisualDefinitions.FrameAt(RoomEnemySystem.KraidGoodNailDefinition, operandAddress),
                0xb3 => BotwoonVisualDefinitions.FrameAt(operandAddress),
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
