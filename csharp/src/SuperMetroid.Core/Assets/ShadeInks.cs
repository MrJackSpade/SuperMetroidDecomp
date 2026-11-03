using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// What one installed shade shows (#1512): the picture's inks it recolours and the colour each shows under it. Inks it
/// leaves out keep what lies under the shade.
/// </summary>
public sealed record ShadeInks
{
    /// <summary>Keeps a shade's recoloured inks.</summary>
    /// <param name="inks">The inks recoloured, each once, in rising order.</param>
    /// <param name="colours">The colour each ink shows, in the same order.</param>
    /// <exception cref="ArgumentException">The lists differ in length, or the inks repeat or are out of order.</exception>
    public ShadeInks(ImmutableArray<int> inks, ImmutableArray<RgbColor> colours)
    {
        if (inks.Length != colours.Length)
            throw new ArgumentException($"A shade recolours {inks.Length} inks with {colours.Length} colours.", nameof(colours));
        for (int index = 1; index < inks.Length; index++)
        {
            if (inks[index] <= inks[index - 1])
                throw new ArgumentException($"A shade's inks are not in rising order at {inks[index]}.", nameof(inks));
        }
        Inks = inks;
        Colours = colours;
    }

    /// <summary>The inks recoloured, in rising order.</summary>
    public ImmutableArray<int> Inks { get; }

    /// <summary>The colour each ink shows.</summary>
    public ImmutableArray<RgbColor> Colours { get; }

    /// <summary>Lays the shade over <paramref name="inks"/>, one colour per ink of its picture.</summary>
    /// <exception cref="InvalidDataException">The shade recolours an ink the picture does not have.</exception>
    public void Over(Span<RgbColor> inks)
    {
        for (int index = 0; index < Inks.Length; index++)
        {
            int ink = Inks[index];
            if (ink >= inks.Length)
                throw new InvalidDataException($"A shade recolours ink {ink} of a picture with {inks.Length}.");
            inks[ink] = Colours[index];
        }
    }
}
