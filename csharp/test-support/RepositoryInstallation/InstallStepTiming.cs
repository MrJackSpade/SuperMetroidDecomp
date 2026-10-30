using System.Diagnostics;
using System.Globalization;

namespace SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>
/// Breaks the repository installation's extraction down by step (#1466): each progress report the installer makes as it
/// starts a step closes the step before it with a time and allocation line, so a slow import shows which step is slow
/// instead of one fixture total. Reports arrive on the installing thread, in order.
/// </summary>
internal sealed class InstallStepTiming : IProgress<string>
{
    /// <summary>The time the step being timed has taken.</summary>
    private readonly Stopwatch clock = new();

    /// <summary>The step being timed, as the installer reported it, or null between steps.</summary>
    private string? step;

    /// <summary>The bytes the process had allocated when the step began.</summary>
    private long allocatedAtStart;

    /// <summary>Closes the step before <paramref name="value"/> and starts timing <paramref name="value"/>.</summary>
    public void Report(string value)
    {
        Finish();
        step = value;
        allocatedAtStart = GC.GetTotalAllocatedBytes();
        clock.Restart();
    }

    /// <summary>Closes the step being timed, if any.</summary>
    internal void Finish()
    {
        if (step is null)
            return;
        double megabytes = (GC.GetTotalAllocatedBytes() - allocatedAtStart) / (1024.0 * 1024.0);
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
            $"TIME     Install step {step.TrimEnd('.', '…')}: {clock.Elapsed.TotalSeconds:F2} s, {megabytes:F1} MB allocated"));
        step = null;
    }
}
