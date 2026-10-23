namespace SuperMetroid.Core.Assets;

/// <summary>
/// A copy of some runs of owners' colours (#1405): a fade's target, or colours set aside to restore. Any run within one
/// of the copied runs finds its colours in the copy.
/// </summary>
public sealed class ColourImage
{
    /// <summary>The runs copied, which <see cref="Restore"/> writes back.</summary>
    private readonly ColourRun[] runs;

    /// <summary>Each run's copied colours.</summary>
    private readonly Bgr555[][] copies;

    /// <summary>Copies <paramref name="runs"/>' colours as they are now.</summary>
    public ColourImage(params ColourRun[] runs)
    {
        this.runs = [.. Ensure.NotNull(runs)];
        copies = [.. this.runs.Select(run => run.Colors.ToArray())];
    }

    /// <summary>The number of runs copied.</summary>
    internal int RunCount => runs.Length;

    /// <summary>Copied run <paramref name="index"/>, live.</summary>
    internal ColourRun Run(int index) => runs[index];

    /// <summary>The copy of run <paramref name="index"/>'s colours.</summary>
    internal ReadOnlySpan<Bgr555> Copy(int index) => copies[index];

    /// <summary>The copy of <paramref name="run"/>'s colours, which lie within one of the copied runs.</summary>
    public Span<Bgr555> Of(ColourRun run)
    {
        for (int index = 0; index < runs.Length; index++)
        {
            if (runs[index].Contains(run))
                return copies[index].AsSpan(run.First - runs[index].First, run.Count);
        }
        throw new ArgumentException("The image holds no copy of these colours.", nameof(run));
    }

    /// <summary>Changes every copied colour to <paramref name="colour"/>.</summary>
    public void Fill(Bgr555 colour)
    {
        foreach (Bgr555[] copy in copies)
            copy.AsSpan().Fill(colour);
    }

    /// <summary>Writes the copied colours back to the runs.</summary>
    public void Restore()
    {
        for (int index = 0; index < runs.Length; index++)
            runs[index].CopyFrom(copies[index]);
    }
}
