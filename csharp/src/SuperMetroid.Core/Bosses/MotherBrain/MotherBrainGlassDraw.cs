namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The eleven frames Mother Brain's glass draws into her room as it breaks (#2211), named for what each shows.</summary>
internal enum MotherBrainGlassDraw
{
    /// <summary>The intact glass block.</summary>
    Initial,
    /// <summary>The pane cracked from its right edge.</summary>
    PaneCracked,
    /// <summary>The pane's cracks reaching across it.</summary>
    PaneCrackedThrough,
    /// <summary>The pane giving way below the glass block.</summary>
    PaneTransition,
    /// <summary>The cracked pane shifted to the glass's left side, whole.</summary>
    PaneShifted,
    /// <summary>The shifted pane crumbling from its top.</summary>
    PaneShiftedCrumbling,
    /// <summary>The shifted pane emptying.</summary>
    PaneShiftedEmptying,
    /// <summary>The glass's rim buckling inward.</summary>
    RimBuckling,
    /// <summary>The glass's rim bursting, the tubes behind it showing.</summary>
    RimBursting,
    /// <summary>The rim gone: only the tubes behind the glass's square remain.</summary>
    RimGone,
    /// <summary>The glass gone, its square empty.</summary>
    Cleared,
}
