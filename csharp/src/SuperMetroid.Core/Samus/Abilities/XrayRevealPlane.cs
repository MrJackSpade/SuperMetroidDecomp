using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The X-ray reveal BG2 shows, drawn from block pictures (#1392): every shown cell of the reveal as the block it draws and
/// how it lays the block's picture out (#1431), over the pictures the room's blocks show now. A block copied from beyond
/// the room lies below or right of anything BG2's scroll shows, and draws nothing.
/// </summary>
public sealed class XrayRevealPlane
{

    /// <summary>The blocks, row by row, <see cref="XrayRevealLayout.MetatileColumns"/> to a row; null beyond the room.</summary>
    private readonly RevealedBlock?[] blocks;

    /// <summary>Keeps the blocks and the pictures they draw from.</summary>
    /// <exception cref="ArgumentException">The blocks do not fill the reveal's rows and columns.</exception>
    /// <exception cref="InvalidDataException">A block draws a definition the pictures do not hold.</exception>
    public XrayRevealPlane(ReadOnlySpan<RevealedBlock?> blocks, RoomBlockPictureTable pictures)
    {
        Ensure.LengthEqual(blocks, XrayRevealLayout.Cells);
        Pictures = Ensure.NotNull(pictures);
        foreach (RevealedBlock? shown in blocks)
        {
            if (shown is { } block && pictures.Picture(block.Definition) is null)
                throw new InvalidDataException($"The X-ray reveal draws block definition {block.Definition}, which was not resolved.");
        }
        this.blocks = blocks.ToArray();
    }

    /// <summary>The pictures the blocks draw from.</summary>
    public RoomBlockPictureTable Pictures { get; }

    /// <summary>The blocks, row by row; null where a block was copied from beyond the room.</summary>
    public ReadOnlySpan<RevealedBlock?> Blocks => blocks;
}
