namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>
    /// Rebinds host policy after an interactive debugger-state load. Cartridge state,
    /// resource totals, and the retained frame remain intact unless a tester grant is newly enabled. Exact replay must omit
    /// this override unless the recording explicitly supplies different host options.
    /// </summary>
    public void ApplyHostOptions(SuperMetroidGameOptions hostOptions)
    {
        ArgumentNullException.ThrowIfNull(hostOptions);
        runtime?.ApplyHostOptions(hostOptions);
        gameOptions = hostOptions;
    }
}
