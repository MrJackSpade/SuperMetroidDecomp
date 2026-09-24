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

    internal readonly record struct DrawList(ushort Pointer, ushort LevelWord)
    {
        /// <summary>Native one-word row, followed by signed zero X/Y termination.</summary>
        internal const ushort DirectionAndCount = 1;
        internal const sbyte NextX = 0;
        internal const sbyte NextY = 0;
    }

    private static readonly DrawList[] Lists =
    [
        new(Grapple, 0xe0b7),
        new(BreakFrame0, 0x0053),
        new(BreakFrame1, 0x0054),
        new(BreakFrame2, 0x0055),
        new(Blank, 0x00ff),
    ];

    internal static IEnumerable<DrawList> All => Lists;

    internal static bool TryGet(ushort pointer, out DrawList definition)
    {
        foreach (DrawList candidate in Lists)
        {
            if (candidate.Pointer != pointer)
                continue;
            definition = candidate;
            return true;
        }

        definition = default;
        return false;
    }
}
