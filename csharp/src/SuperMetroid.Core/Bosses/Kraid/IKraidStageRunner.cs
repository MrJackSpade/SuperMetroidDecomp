namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Runs one update of the stage Kraid's body is in (#2554): each stage hands itself to the method for its case
/// (<see cref="KraidStageRun.RunWith"/>), so every stage has its phase run and none is left to a default.
/// </summary>
internal interface IKraidStageRunner
{
    /// <summary>Runs his rise through the floor.</summary>
    /// <param name="rising">The rise and the phase of it he is in.</param>
    void Run(KraidStageRun.Rising rising);

    /// <summary>Runs his fight.</summary>
    /// <param name="fight">The fight and the phase of it he is in.</param>
    void Run(KraidStageRun.Fighting fight);

    /// <summary>Runs his growth into his second phase.</summary>
    /// <param name="growth">The growth and the phase of it he is in.</param>
    void Run(KraidStageRun.Growing growth);

    /// <summary>Runs his death up to his sinking.</summary>
    /// <param name="dying">The death and the phase of it he is in.</param>
    void Run(KraidStageRun.Dying dying);

    /// <summary>Runs the sunk body's last phases.</summary>
    /// <param name="sunk">The sunk body and the phase of it he is in.</param>
    void Run(KraidStageRun.Sunk sunk);
}
