namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Chooses a Nuclear Waffle's sweep.</summary>
internal static class NuclearWaffleSweeps
{
    /// <summary>The sweep, with its endpoints, link spacing and turn thresholds, that runs in <paramref name="direction"/>.</summary>
    internal static NuclearWaffleSweepDefinition Sweep(NuclearWaffleSweepDirection direction) => direction switch
    {
        NuclearWaffleSweepDirection.Backward => NuclearWaffleDefinitions.BackwardSweep,
        NuclearWaffleSweepDirection.Forward => NuclearWaffleDefinitions.ForwardSweep,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined Nuclear Waffle sweep direction."),
    };
}
