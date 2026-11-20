namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>What the Baby Metroid's approach does along an axis on which it stands exactly on its target (#2557).</summary>
internal enum BabyMetroidOnTarget
{
    /// <summary>The Baby has arrived along the axis, its speed kept: the vertical approach.</summary>
    Arrives,

    /// <summary>The Baby still accelerates towards the lesser side: the horizontal approach, which leaves it leftward.</summary>
    KeepsAccelerating,
}
