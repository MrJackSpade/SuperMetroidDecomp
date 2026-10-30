namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The block a Wave shot's horizontal scan reads when its walk through the room's rows has wrapped past the room's ceiling
/// (#1469). The scan steps two rows' worth of block halves at a time with a single carry; after the carry it lands
/// halfway between two neighbouring blocks, and reads a block made of the second half of the first and the first half of
/// the second. That block's collision family selects the shot's reaction, and a reaction that spawns a room object keeps
/// it as the block to restore, so the straddled block is gameplay-observable.
/// </summary>
/// <remarks>
/// Consulted by the horizontal Wave-shot reaction scan, for each block it reaches at a straddling position. The straddled
/// block takes the first block's two highest block-number bits, its reflections and its collision family as the low eight
/// bits of its block number; the second block's low eight block-number bits give the rest of its block number (two bits),
/// its reflections (two bits) and its collision family (four bits). Evidence: the original scan (<c>$94:A352</c>) adds the
/// room width twice to a byte offset with one carry clear, and reads the unaligned word at an odd offset.
/// </remarks>
[CompatibilityService("a Wave shot's scan past the room's ceiling reads a block straddling two neighbouring blocks")]
public static class StraddledBlockRead
{
    /// <summary>The block-number bits the straddled block takes from each half.</summary>
    private const int HalfBits = 8;

    /// <summary>The block-number bits below the reflections.</summary>
    private const int BlockBits = 10;

    /// <summary>The two mirroring bits above the block number.</summary>
    private const int MirroringMask = 3;

    /// <summary>The block's bits below its collision family.</summary>
    private const int FamilyShift = 12;

    /// <summary>The block straddling <paramref name="first"/> and the block after it, <paramref name="second"/>.</summary>
    /// <param name="first">The block whose second half the read starts in.</param>
    /// <param name="second">The block after it, whose first half the read ends in.</param>
    public static StraddledBlock Of(StraddledBlock first, StraddledBlock second)
    {
        int halfMask = (1 << HalfBits) - 1;
        int straddled = (Pack(first) >> HalfBits) | ((Pack(second) & halfMask) << HalfBits);
        return new StraddledBlock(
            straddled & ((1 << BlockBits) - 1),
            ((straddled >> BlockBits) & MirroringMask) switch
            {
                0 => StraddledBlock.Reflection.None,
                1 => StraddledBlock.Reflection.Horizontal,
                2 => StraddledBlock.Reflection.Vertical,
                _ => StraddledBlock.Reflection.Both,
            },
            straddled >> FamilyShift);
    }

    /// <summary>A block's fields side by side in the order the original read them.</summary>
    private static int Pack(StraddledBlock block) =>
        block.Block |
        ((block.Mirroring switch
        {
            StraddledBlock.Reflection.None => 0,
            StraddledBlock.Reflection.Horizontal => 1,
            StraddledBlock.Reflection.Vertical => 2,
            StraddledBlock.Reflection.Both => MirroringMask,
            _ => throw new ArgumentOutOfRangeException(nameof(block), block.Mirroring, "Undefined mirroring."),
        }) << BlockBits) |
        (block.Family << FamilyShift);
}
