using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Colours one transfer writes across several runs, in order (#1464): a boss's opening colours that fill one sprite colour
/// store and run on into the next, or the two stores the Chozo statues colour. Colour <c>n</c> of the sequence is the
/// colour that many places after the first run's first, counting through each run before the next.
/// </summary>
public readonly struct ColourRunSequence
{
    /// <summary>Takes <paramref name="runs"/>, in the order their colours are counted.</summary>
    public ColourRunSequence(params ReadOnlySpan<ColourRun> runs) => Runs = [.. runs];

    /// <summary>The runs, in the order their colours are counted.</summary>
    public ImmutableArray<ColourRun> Runs { get; }

    /// <summary>The number of colours across every run.</summary>
    public int Count => Runs.Sum(run => run.Count);

    /// <summary>Changes colour <paramref name="colour"/> of the sequence.</summary>
    public void Write(int colour, RgbColor value)
    {
        ColourRun run = RunHolding(ref colour);
        run.Write(colour, value);
    }

    /// <summary>Changes the sequence's colours to <paramref name="source"/>, which holds exactly as many, run after run.</summary>
    public void CopyFrom(ReadOnlySpan<RgbColor> source)
    {
        Ensure.LengthEqual(source, Count);
        foreach (ColourRun run in Runs)
        {
            run.CopyFrom(source[..run.Count]);
            source = source[run.Count..];
        }
    }

    /// <summary>
    /// Changes the copy <paramref name="image"/> holds of the sequence's colours to <paramref name="source"/>, which holds
    /// exactly as many, run after run.
    /// </summary>
    public void CopyInto(ColourImage image, ReadOnlySpan<RgbColor> source)
    {
        Ensure.NotNull(image);
        Ensure.LengthEqual(source, Count);
        foreach (ColourRun run in Runs)
        {
            source[..run.Count].CopyTo(image.Of(run));
            source = source[run.Count..];
        }
    }

    /// <summary>The run holding the sequence's colour <paramref name="colour"/>, which becomes that colour within the run.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colour is not one of the sequence's.</exception>
    private ColourRun RunHolding(ref int colour)
    {
        Ensure.BetweenInclusive(0, Count - 1, colour);
        foreach (ColourRun run in Runs)
        {
            if (colour < run.Count)
                return run;
            colour -= run.Count;
        }
        throw new ArgumentOutOfRangeException(nameof(colour), colour, "The sequence holds no such colour.");
    }
}
