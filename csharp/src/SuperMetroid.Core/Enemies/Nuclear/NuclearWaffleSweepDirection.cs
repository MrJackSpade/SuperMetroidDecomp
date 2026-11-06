namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>The way a nuclear waffle sweeps around its orbit (#1519); the two sweeps mirror each other.</summary>
internal enum NuclearWaffleSweepDirection
{
    /// <summary>From angle 400 back to angle 240, its links trailing at larger angles.</summary>
    Backward,

    /// <summary>From angle 240 forward to angle 400, its links trailing at smaller angles.</summary>
    Forward,
}
