namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Tourian Baby Metroid's animations, order.</summary>
public enum BabyMetroidSegment
{
    /// <summary>A long wide-open hold, then calm.</summary>
    FinishDraining,
    /// <summary>The calm pulse.</summary>
    Normal,
    /// <summary>The quick draining pulse.</summary>
    LatchedOn,
    /// <summary>
    /// The remorse's calm pulse, at ten updates a pose, which starts the hesitant pulsing before it leaves: played twice
    /// before the pulsing quickens.
    /// </summary>
    Remorse,
    /// <summary>
    /// The remorse's quickening: the random cry, then three pulses that quicken from six updates a pose to two and slow
    /// again to nine.
    /// </summary>
    RemorseQuickening,
}
