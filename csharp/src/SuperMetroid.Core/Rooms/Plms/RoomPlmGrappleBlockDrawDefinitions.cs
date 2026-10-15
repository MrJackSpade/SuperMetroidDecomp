namespace SuperMetroid.Core.Rooms.Plms;

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

    /// <summary>A single Grapple-block PLM draw record carrying both its native pointer and installed level word.</summary>
    internal readonly record struct DrawList(ushort Pointer, ushort LevelWord)
    {
        /// <summary>Native one-word row, followed by signed zero X/Y termination.</summary>
        internal const ushort DirectionAndCount = 1;
        /// <summary>The one-block record leaves the next draw position in its current column.</summary>
        internal const sbyte NextX = 0;
        /// <summary>The one-block record leaves the next draw position in its current row.</summary>
        internal const sbyte NextY = 0;
    }

    /// <summary>Enumerates the initial Grapple block, three breakup frames, and blank air in native order.</summary>
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

    /// <summary>The five draw-list records, valued by bank-$84 pointer.</summary>
    private enum DrawRecord : ushort
    {
        /// <summary>$84:A4F9, initial Grapple collision/appearance.</summary>
        Grapple = RoomPlmGrappleBlockDrawDefinitions.Grapple,
        /// <summary>$84:A4FF, first breakup appearance.</summary>
        BreakFrame0 = RoomPlmGrappleBlockDrawDefinitions.BreakFrame0,
        /// <summary>$84:A505, second breakup appearance.</summary>
        BreakFrame1 = RoomPlmGrappleBlockDrawDefinitions.BreakFrame1,
        /// <summary>$84:A50B, third breakup appearance.</summary>
        BreakFrame2 = RoomPlmGrappleBlockDrawDefinitions.BreakFrame2,
        /// <summary>$84:A511, blank air appearance.</summary>
        Blank = RoomPlmGrappleBlockDrawDefinitions.Blank,
    }

    /// <summary>Resolves only the five compiled draw-list pointers; unknown pointers produce a default record.</summary>
    internal static bool TryGet(ushort pointer, out DrawList definition)
    {
        if (!Enum.IsDefined((DrawRecord)pointer))
        {
            definition = default;
            return false;
        }
        ushort levelWord = (DrawRecord)pointer switch
        {
            DrawRecord.Grapple => 0xe0b7,
            // Three consecutive breakup tiles in six-byte one-block draw records.
            DrawRecord.BreakFrame0 => 0x0053,
            DrawRecord.BreakFrame1 => 0x0054,
            DrawRecord.BreakFrame2 => 0x0055,
            DrawRecord.Blank => 0x00ff,
            var record => throw new InvalidOperationException($"Undefined {nameof(DrawRecord)} {(int)record}."),
        };
        definition = new(pointer, levelWord);
        return true;
    }
}
