namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Which of Mother Brain's contact regions currently hurt Samus. The game tests each part's flag independently
/// and composes them (the brain alone during the glass phase, all three from the ascent onward, none after death),
/// so the set is composable flags. One codec converts the set to its snapshot
/// value.
/// </summary>
[Flags]
public enum MotherBrainHitboxParts
{
    /// <summary>No region touches Samus.</summary>
    None = 0,

    /// <summary>The body's contact rectangles, tested first.</summary>
    Body = 1 << 0,

    /// <summary>The brain's contact rectangles, tested second.</summary>
    Brain = 1 << 1,

    /// <summary>The three middle neck joints' rectangles, tested last.</summary>
    Neck = 1 << 2,
}
