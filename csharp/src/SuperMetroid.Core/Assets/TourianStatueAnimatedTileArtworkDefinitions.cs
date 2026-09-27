using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Stock bank-$87 frame selections for the four Tourian entrance statues. These
/// are presentation identities, separate from the compiled animation bytecode.
/// </summary>
public static class TourianStatueAnimatedTileArtworkDefinitions
{
    /// <summary>First byte of the contiguous statue animation characters at $87:9364.</summary>
    public const int FirstSource = 0x879364;
    /// <summary>Last byte beyond the statue animation characters at $87:9964.</summary>
    public const int SourceEnd = 0x879964;
    /// <summary>Complete editable 2-bpp character strip, including shared frames.</summary>
    public const int TransferByteCount = SourceEnd - FirstSource;

    // Each row follows the nine source operands of the corresponding $87 header.
    // Repeated pointers are intentional: native programs reuse existing pictures.
    private static readonly ushort[][] Sources =
    [
        [0x9364, 0x93e4, 0x9464, 0x93e4, 0x9364, 0x93e4, 0x9464, 0x97e4, 0x97e4],
        [0x94e4, 0x9524, 0x9564, 0x9524, 0x94e4, 0x9524, 0x9564, 0x9864, 0x9864],
        [0x9724, 0x9764, 0x97a4, 0x9764, 0x9724, 0x9764, 0x97a4, 0x98a4, 0x98a4],
        [0x95a4, 0x9624, 0x96a4, 0x9624, 0x95a4, 0x9624, 0x96a4, 0x98e4, 0x98e4],
    ];

    /// <summary>Resolves one source operand in the stock statue animation program.</summary>
    public static int SourceAddress(TourianStatueAnimatedTileProgramDefinition definition,
        ushort operandPointer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        int statue = -1;
        for (int index = 0; index < TourianStatueAnimatedTileMechanicsDefinitions.All.Count; index++)
            if (TourianStatueAnimatedTileMechanicsDefinitions.All[index].ObjectPointer ==
                definition.ObjectPointer)
            {
                statue = index;
                break;
            }
        if (statue < 0)
            throw new InvalidDataException($"Unknown Tourian statue $87:{definition.ObjectPointer:X4}.");
        for (int index = 0; index < definition.SourceOperandPointers.Count; index++)
            if (definition.SourceOperandPointers[index] == operandPointer)
                return RoomFxRomData.Banks.AnimatedTiles | Sources[statue][index];
        throw new InvalidDataException(
            $"Tourian statue $87:{definition.ObjectPointer:X4} has no frame operand $87:{operandPointer:X4}.");
    }
}
