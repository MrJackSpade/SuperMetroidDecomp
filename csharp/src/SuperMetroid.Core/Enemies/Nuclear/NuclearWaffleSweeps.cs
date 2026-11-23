namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Chooses a Nuclear Waffle's sweep.</summary>
internal static partial class NuclearWaffleSweeps
{
    /// <summary>The sweep of each direction.</summary>
    private static IdentityTable<NuclearWaffleSweepDirection, NuclearWaffleSweepDefinition> SweepByDirection => field ??= new(new Dictionary<NuclearWaffleSweepDirection, NuclearWaffleSweepDefinition>
    {
        [NuclearWaffleSweepDirection.Backward] = NuclearWaffleDefinitions.BackwardSweep,
        [NuclearWaffleSweepDirection.Forward] = NuclearWaffleDefinitions.ForwardSweep,
    });

    /// <summary>The sweep, with its endpoints, link spacing and turn thresholds, that runs in <paramref name="direction"/>.</summary>
    internal static NuclearWaffleSweepDefinition Sweep(NuclearWaffleSweepDirection direction) => SweepByDirection[direction];
}
