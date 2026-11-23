namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>The way a nuclear waffle sweeps around its orbit (#1519); the two sweeps mirror each other.</summary>
internal enum NuclearWaffleSweepDirection
{
    /// <summary>From the sweep's far end back to its near end (<see cref="NuclearWaffleDefinitions.BackwardSweep"/>), its links trailing at larger angles.</summary>
    Backward,

    /// <summary>From the sweep's near end forward to its far end (<see cref="NuclearWaffleDefinitions.ForwardSweep"/>), its links trailing at smaller angles.</summary>
    Forward,
}
