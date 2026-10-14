namespace SuperMetroid.Core.Rooms;

/// <summary>The eleven bank-$84 Mother Brain glass draw lists, $84:9717..9846.</summary>
internal enum MotherBrainGlassDraw : ushort
{
    /// <summary>Initial single glass block at $84:9717.</summary>
    Initial = 0x9717,
    /// <summary>First full pane damage frame at $84:971D.</summary>
    PaneDamage1 = 0x971d,
    /// <summary>Second full pane damage frame at $84:9731.</summary>
    PaneDamage2 = 0x9731,
    /// <summary>Three-block pane transition at $84:9745.</summary>
    PaneTransition = 0x9745,
    /// <summary>First shifted pane damage frame at $84:974F.</summary>
    ShiftedPane1 = 0x974f,
    /// <summary>Second shifted pane damage frame at $84:9769.</summary>
    ShiftedPane2 = 0x9769,
    /// <summary>Reduced shifted pane frame at $84:9781.</summary>
    ShiftedPane3 = 0x9781,
    /// <summary>First four-run shatter frame at $84:978F.</summary>
    Shatter1 = 0x978f,
    /// <summary>Second four-run shatter frame at $84:97B7.</summary>
    Shatter2 = 0x97b7,
    /// <summary>Third four-run shatter frame at $84:97E7.</summary>
    Shatter3 = 0x97e7,
    /// <summary>Four-run empty-glass frame at $84:9817.</summary>
    Cleared = 0x9817,
}
