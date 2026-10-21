namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Fixed visual identities selected by the dead Sidehopper, Zoomer, Ripper,
/// and Skree programs. Timing, callbacks, and sleep remain compiled in their
/// corresponding instruction definitions.
/// </summary>
internal static class DeadTourianCorpseVisualDefinitions
{
    /// <summary>The native corpse instruction and OAM bank, $A9.</summary>
    internal const byte Bank = 0xa9;

    /// <summary>Distinct dead Zoomer, Ripper, and Skree frame identities in bank $A9.</summary>
    private static readonly DeadTourianCorpseFrameName[] Entries =
    [
        new DeadTourianCorpseFrameName(0xed79, "dead_zoomer_corpse_0"),
        new DeadTourianCorpseFrameName(0xed85, "dead_zoomer_corpse_2"),
        new DeadTourianCorpseFrameName(0xed91, "dead_zoomer_corpse_4"),
        new DeadTourianCorpseFrameName(0xed9d, "dead_ripper_corpse_0"),
        new DeadTourianCorpseFrameName(0xeda9, "dead_ripper_corpse_2"),
        new DeadTourianCorpseFrameName(0xedb5, "dead_skree_corpse_0"),
        new DeadTourianCorpseFrameName(0xedcb, "dead_skree_corpse_2"),
        new DeadTourianCorpseFrameName(0xede1, "dead_skree_corpse_4"),
    ];

    /// <summary>Provides each installed corpse spritemap once for the four dead Tourian enemy families.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames() =>
    [
        .. Entries.Select(entry => new NativeEnemySpritemapDefinition(
            Bank, checked((ushort)entry.Frame), entry.Name)),
        new(Bank, 0xee3c, "dead_sidehopper_hop_body"),
        new(Bank, 0xee61, "dead_sidehopper_hop_alt"),
        new(Bank, 0xee86, "dead_sidehopper_idle"),
        new(Bank, 0xed25, "dead_sidehopper_corpse"),
        new(Bank, 0xed4f, "dead_sidehopper_initially_dead"),
    ];
}
