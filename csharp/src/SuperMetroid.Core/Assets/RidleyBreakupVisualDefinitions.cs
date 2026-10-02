namespace SuperMetroid.Core.Assets;

/// <summary>Body pieces used as ordinary sprites by Ridley's breakup actors.</summary>
internal static class RidleyBreakupVisualDefinitions
{
    /// <summary>$A6:ED29/EF25, left/right leg spritemaps used by the breakup lists.</summary>
    internal static readonly EnemySpritemapDefinition[] Legs =
        [new(0xa6, 0xed29, "ridley_breakup_legs_left"), new(0xa6, 0xef25, "ridley_breakup_legs_right")];
    /// <summary>$A6:ECDC/EED8, left/right torso spritemaps used by the breakup lists.</summary>
    internal static readonly EnemySpritemapDefinition[] Torso =
        [new(0xa6, 0xecdc, "ridley_breakup_torso_left"), new(0xa6, 0xeed8, "ridley_breakup_torso_right")];
    /// <summary>$A6:ED95/EF91, left/right open head and neck spritemaps.</summary>
    internal static readonly EnemySpritemapDefinition[] Head =
        [new(0xa6, 0xed95, "ridley_breakup_head_left"), new(0xa6, 0xef91, "ridley_breakup_head_right")];
    /// <summary>$A6:ED8E/EF8A, left/right claw spritemaps used by the breakup lists.</summary>
    internal static readonly EnemySpritemapDefinition[] Claw =
        [new(0xa6, 0xed8e, "ridley_breakup_claw_left"), new(0xa6, 0xef8a, "ridley_breakup_claw_right")];
}
