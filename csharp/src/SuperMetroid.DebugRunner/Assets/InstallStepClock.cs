using System.Diagnostics;

namespace SuperMetroid.DebugRunner.Assets;

/// <summary>
/// Prints each installation step as it starts, with the time since the installation began (#1530): steps run alongside
/// one another, so a step's own duration is not the gap to the next report.
/// </summary>
internal sealed class InstallStepClock : IProgress<string>
{
    /// <summary>Times the whole installation.</summary>
    private readonly Stopwatch total = Stopwatch.StartNew();

    /// <inheritdoc />
    public void Report(string value) => Console.WriteLine($"{total.Elapsed.TotalSeconds,7:F1} s  {value}");

    /// <summary>Prints the whole installation's time.</summary>
    internal void Finish() => Console.WriteLine($"{total.Elapsed.TotalSeconds,7:F1} s  total");
}
