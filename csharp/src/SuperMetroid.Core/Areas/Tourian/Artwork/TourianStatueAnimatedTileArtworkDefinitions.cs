using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>
/// Stock bank-$87 artwork windows for the four Tourian entrance statues' pictures. These are presentation
/// identities, separate from the statues' typed programs.
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
    /// The artwork window a statue picture transfers. Each statue's three oscillation pictures are contiguous windows
    /// of its own width; the released picture is a separate window. The native Kraid/Draygon "Other" labels are
    /// reversed; selection follows the original operands, which the ROM oracle compares.
    /// </summary>
    public static int SourceAddress(TourianStatueAnimatedTileProgramDefinition definition,
        TourianStatueTilePicture picture)
    {
        Ensure.NotNull(definition);
        (int first, int width, int released) = definition.ObjectPointer switch
        {
            AnimatedTileObject.TourianStatuePhantoon => (0x9364, 0x80, 0x97e4),
            AnimatedTileObject.TourianStatueRidley => (0x94e4, 0x40, 0x9864),
            AnimatedTileObject.TourianStatueKraid => (0x9724, 0x40, 0x98a4),
            AnimatedTileObject.TourianStatueDraygon => (0x95a4, 0x80, 0x98e4),
            _ => throw new InvalidDataException($"Unknown Tourian statue {definition.ObjectPointer}."),
        };
        int source = picture switch
        {
            TourianStatueTilePicture.OscillationFirst => first,
            TourianStatueTilePicture.OscillationSecond => first + width,
            TourianStatueTilePicture.OscillationThird => first + 2 * width,
            TourianStatueTilePicture.Released => released,
            _ => throw new InvalidDataException($"Undefined {nameof(TourianStatueTilePicture)} {picture}."),
        };
        return RoomFxRomData.Banks.AnimatedTiles | source;
    }
}
