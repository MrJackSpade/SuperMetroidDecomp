namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Which run of Kraid's part spritemaps a <see cref="KraidPartFrame"/> indexes.</summary>
public enum KraidPartFrameKind
{
    /// <summary>The foot's standing pose before the rise ends.</summary>
    FootStanding,
    /// <summary>The 34 foot poses.</summary>
    Foot,
    /// <summary>The 22 arm poses.</summary>
    Arm,
    /// <summary>The two belly-lint poses: initial and after growth.</summary>
    Lint,
    /// <summary>The eight fingernail poses.</summary>
    Nail,
}
