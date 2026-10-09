namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The five one-block draw lists used by breakable-Grapple-block PLMs. These are
/// complete native level words, not merely tile references: the initial frame
/// installs Grapple collision and the later frames install air collision.
/// </summary>
internal static class RoomPlmGrappleBlockDrawDefinitions
{
    /// <summary>Initial Grapple collision/appearance, $84:A4F9.</summary>
    internal const ushort Grapple = 0xa4f9;
    /// <summary>First breakup appearance, $84:A4FF.</summary>
    internal const ushort BreakFrame0 = 0xa4ff;
    /// <summary>Second breakup appearance, $84:A505.</summary>
    internal const ushort BreakFrame1 = 0xa505;
    /// <summary>Third breakup appearance, $84:A50B.</summary>
    internal const ushort BreakFrame2 = 0xa50b;
    /// <summary>Blank air appearance, $84:A511.</summary>
    internal const ushort Blank = 0xa511;

    /// <summary>Describes one native PLM draw list and the complete level word it installs.</summary>
    /// <param name="Pointer">Bank-$84 address of the draw-list header.</param>
    /// <param name="LevelWord">Tile and collision level word written by that list.</param>
    internal readonly record struct DrawList(ushort Pointer, ushort LevelWord)
    {
        /// <summary>Native one-word row, followed by signed zero X/Y termination.</summary>
        internal const ushort DirectionAndCount = 1;

        /// <summary>Horizontal signed offset from the drawn block to the next row; zero terminates the list.</summary>
        internal const sbyte NextX = 0;

        /// <summary>Vertical signed offset from the drawn block to the next row; zero terminates the list.</summary>
        internal const sbyte NextY = 0;
    }

    /// <summary>Enumerates the five adjacent native draw records in their original publication order.</summary>
    internal static IEnumerable<DrawList> All
    {
        get
        {
            // Five adjacent six-byte records in the original publication order.
            for (int frame = 0; frame < 5; frame++)
            {
                TryGet((ushort)(Grapple + 6 * frame), out DrawList draw);
                yield return draw;
            }
        }
    }

    /// <summary>Resolves a supported Grapple-block draw-list address to its installed level word.</summary>
    /// <param name="pointer">Bank-$84 address of the candidate draw list.</param>
    /// <param name="definition">Receives the draw-list address and its full tile/collision word when found.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> identifies one of the five compiled lists.</returns>
    internal static bool TryGet(ushort pointer, out DrawList definition)
    {
        if (pointer == Grapple)
        {
            definition = new(pointer, 0xe0b7);
            return true;
        }
        if (pointer == Blank)
        {
            definition = new(pointer, 0x00ff);
            return true;
        }
        int relative = pointer - BreakFrame0;
        if ((uint)relative < 3 * 6 && relative % 6 == 0)
        {
            // Three consecutive breakup tiles in six-byte one-block draw records.
            definition = new(pointer, (ushort)(0x0053 + relative / 6));
            return true;
        }
        definition = default;
        return false;
    }
}
