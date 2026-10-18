namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Which of Mother Brain's contact regions currently hurt Samus. The cartridge tests each part's bit independently
/// and composes them (the brain alone during the glass phase, all three from the ascent onward, none after death),
/// so the set is composable flags. <c>MotherBrainHitboxPartsCodec</c> is the one conversion to the snapshot
/// word.
/// </summary>
[Flags]
public enum MotherBrainHitboxParts : ushort
{
    /// <summary>No region touches Samus.</summary>
    None = 0,

    /// <summary>The body's contact rectangles, tested first.</summary>
    Body = 1,

    /// <summary>The brain's contact rectangles, tested second.</summary>
    Brain = 2,

    /// <summary>The three middle neck joints' rectangles, tested last.</summary>
    Neck = 4,
}
