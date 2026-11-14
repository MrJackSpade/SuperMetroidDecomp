using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The X-ray reveal the background shows, drawn from block pictures (#1392): every revealed block as the block it draws
/// and how it lays the block's picture out (#1431), placed where the room's own block lies in the foreground (#1514), over
/// the pictures the room's blocks show now. A block beyond the room draws nothing.
/// </summary>
public sealed class XrayRevealPlane
{

    /// <summary>The blocks, row by row, <see cref="XrayRevealArea.Columns"/> to a row; null beyond the room.</summary>
    private readonly RevealedBlock?[] blocks;

    /// <summary>Keeps where the reveal lies, its blocks and the pictures they draw from.</summary>
    /// <param name="origin">The foreground pixel the first block's top-left corner lies on.</param>
    /// <param name="blocks">The blocks, row by row, <see cref="XrayRevealArea.Columns"/> to a row.</param>
    /// <param name="pictures">The pictures the blocks draw from.</param>
    /// <exception cref="ArgumentException">The blocks do not fill the reveal's rows and columns.</exception>
    /// <exception cref="InvalidDataException">A block draws a picture the pictures do not hold.</exception>
    public XrayRevealPlane(Position16<RoomPixels> origin, ReadOnlySpan<RevealedBlock?> blocks, RoomBlockPictureTable pictures)
    {
        Ensure.LengthEqual(blocks, XrayRevealArea.BlockCount);
        Origin = origin;
        Pictures = Ensure.NotNull(pictures);
        foreach (RevealedBlock? shown in blocks)
        {
            if (shown is { } block && pictures.Picture(block.Picture) is null)
                throw new InvalidDataException($"The X-ray reveal draws {block.Picture}, which was not resolved.");
        }
        this.blocks = blocks.ToArray();
    }

    /// <summary>The foreground pixel the first block's top-left corner lies on; the blocks follow it 16 pixels apart.</summary>
    public Position16<RoomPixels> Origin { get; }

    /// <summary>The pictures the blocks draw from.</summary>
    public RoomBlockPictureTable Pictures { get; }

    /// <summary>The blocks, row by row; null where a block lies beyond the room.</summary>
    public ReadOnlySpan<RevealedBlock?> Blocks => blocks;
}
