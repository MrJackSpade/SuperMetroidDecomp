namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The eleven frames Mother Brain's glass draws into her room as it breaks.</summary>
internal enum MotherBrainGlassDraw
{
    /// <summary>The intact glass block.</summary>
    Initial,
    /// <summary>First full pane damage frame.</summary>
    PaneDamage1,
    /// <summary>Second full pane damage frame.</summary>
    PaneDamage2,
    /// <summary>The pane giving way below the glass block.</summary>
    PaneTransition,
    /// <summary>First shifted pane damage frame.</summary>
    ShiftedPane1,
    /// <summary>Second shifted pane damage frame.</summary>
    ShiftedPane2,
    /// <summary>The shifted pane emptying.</summary>
    ShiftedPane3,
    /// <summary>First shatter frame.</summary>
    Shatter1,
    /// <summary>Second shatter frame.</summary>
    Shatter2,
    /// <summary>Third shatter frame: the glass's square empty.</summary>
    Shatter3,
    /// <summary>The glass gone, its square empty.</summary>
    Cleared,
}
