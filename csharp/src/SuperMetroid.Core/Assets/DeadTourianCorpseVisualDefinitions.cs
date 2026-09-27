using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Fixed visual identities selected by the dead Sidehopper, Zoomer, Ripper,
/// and Skree programs. Timing, callbacks, and sleep remain compiled in their
/// corresponding instruction definitions.
/// </summary>
internal static class DeadTourianCorpseVisualDefinitions
{
    /// <summary>The native corpse instruction and OAM bank, $A9.</summary>
    internal const byte Bank = 0xa9;
    internal const int CorpseFrameCount = 8;
    internal const int SidehopperFrameCount = 5;
    internal const int FrameCount = CorpseFrameCount + SidehopperFrameCount;

    private static readonly (ushort Operand, ushort Frame, string Name)[] Entries =
    [
        (0xecf7, 0xed79, "dead_zoomer_corpse_0"),
        (0xecfd, 0xed85, "dead_zoomer_corpse_2"),
        (0xed03, 0xed91, "dead_zoomer_corpse_4"),
        (0xed09, 0xed9d, "dead_ripper_corpse_0"),
        (0xed0f, 0xeda9, "dead_ripper_corpse_2"),
        (0xed15, 0xedb5, "dead_skree_corpse_0"),
        (0xed1b, 0xedcb, "dead_skree_corpse_2"),
        (0xed21, 0xede1, "dead_skree_corpse_4"),
    ];

    private static readonly (ushort Operand, ushort Frame)[] SidehopperSelectors =
    [
        (0xecae, 0xee3c), (0xecb2, 0xee61),
        (0xecb6, 0xee3c), (0xecba, 0xee61),
        (0xecbe, 0xee3c), (0xecc2, 0xee61),
        (0xecc6, 0xee3c), (0xecca, 0xee61),
        (0xece5, 0xee86), (0xeceb, 0xed25),
        (0xecf1, 0xed4f),
    ];

    internal static EnemySpritemapDefinition[] Frames() =>
    [
        .. Entries.Select(entry => new EnemySpritemapDefinition(
            Bank, entry.Frame, entry.Name)),
        new(Bank, 0xee3c, "dead_sidehopper_hop_body"),
        new(Bank, 0xee61, "dead_sidehopper_hop_alt"),
        new(Bank, 0xee86, "dead_sidehopper_idle"),
        new(Bank, 0xed25, "dead_sidehopper_corpse"),
        new(Bank, 0xed4f, "dead_sidehopper_initially_dead"),
    ];

    internal static ushort FrameAt(ushort operandAddress)
    {
        foreach ((ushort operand, ushort frame, _) in Entries)
        {
            if (operand == operandAddress)
                return frame;
        }
        throw new InvalidDataException(
            $"Dead Tourian corpse visual selector $A9:{operandAddress:X4} is not compiled.");
    }

    internal static ushort SidehopperFrameAt(ushort operandAddress)
    {
        foreach ((ushort operand, ushort frame) in SidehopperSelectors)
        {
            if (operand == operandAddress)
                return frame;
        }
        throw new InvalidDataException(
            $"Dead Sidehopper visual selector $A9:{operandAddress:X4} is not compiled.");
    }
}
