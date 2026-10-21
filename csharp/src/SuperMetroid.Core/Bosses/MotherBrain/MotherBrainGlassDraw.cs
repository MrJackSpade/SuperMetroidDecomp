namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The eleven Mother Brain glass draw lists..9846.</summary>
internal enum MotherBrainGlassDraw
{
    /// <summary>Initial single glass block.</summary>
    Initial,
    /// <summary>First full pane damage frame.</summary>
    PaneDamage1,
    /// <summary>Second full pane damage frame.</summary>
    PaneDamage2,
    /// <summary>Three-block pane transition.</summary>
    PaneTransition,
    /// <summary>First shifted pane damage frame.</summary>
    ShiftedPane1,
    /// <summary>Second shifted pane damage frame.</summary>
    ShiftedPane2,
    /// <summary>Reduced shifted pane frame.</summary>
    ShiftedPane3,
    /// <summary>First four-run shatter frame.</summary>
    Shatter1,
    /// <summary>Second four-run shatter frame.</summary>
    Shatter2,
    /// <summary>Third four-run shatter frame.</summary>
    Shatter3,
    /// <summary>Four-run empty-glass frame.</summary>
    Cleared,
}
