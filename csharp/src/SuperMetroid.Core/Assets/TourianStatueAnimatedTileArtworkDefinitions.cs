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

    /// <summary>
    /// Resolves the nine timed-frame operands of each stock $87:83AC..854B program.
    /// The first five positions traverse three contiguous pictures forward and back
    /// (min(index, 4-index)); the next two replay pictures one and two during release.
    /// Eye-glow and soul waits share the boss-specific released picture. The native
    /// Kraid/Draygon "Other" labels are reversed; selection follows original operands.
    /// Only exact operand identities are accepted, with no wrapping or extrapolation.
    /// </summary>
    public static int SourceAddress(TourianStatueAnimatedTileProgramDefinition definition,
        ushort operandPointer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        (int first, int width, int released) = definition.ObjectPointer switch
        {
            AnimatedTileObjectPointers.TourianStatuePhantoon => (0x9364, 0x80, 0x97e4),
            AnimatedTileObjectPointers.TourianStatueRidley => (0x94e4, 0x40, 0x9864),
            AnimatedTileObjectPointers.TourianStatueKraid => (0x9724, 0x40, 0x98a4),
            AnimatedTileObjectPointers.TourianStatueDraygon => (0x95a4, 0x80, 0x98e4),
            _ => throw new InvalidDataException($"Unknown Tourian statue $87:{definition.ObjectPointer:X4}."),
        };
        for (int index = 0; index < definition.SourceOperandPointers.Count; index++)
            if (definition.SourceOperandPointers[index] == operandPointer)
            {
                int source = index >= 7 ? released
                    : first + width * (index <= 4 ? Math.Min(index, 4 - index) : index - 4);
                return RoomFxRomData.Banks.AnimatedTiles | source;
            }
        throw new InvalidDataException(
            $"Tourian statue $87:{definition.ObjectPointer:X4} has no frame operand $87:{operandPointer:X4}.");
    }
}
