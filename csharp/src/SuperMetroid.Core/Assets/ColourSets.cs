namespace SuperMetroid.Core.Assets;

/// <summary>
/// Eight colour sets of sixteen that one half of gameplay's colours keeps (#1405): the backgrounds' or the sprites'. A
/// writer reaches a set, a run of sets, or all of them; the router reaches them by position for the writers that still
/// address colours that way.
/// </summary>
public abstract class ColourSets : IGameplayColourOwner
{
    /// <summary>The number of sets.</summary>
    public const int SetCount = 8;

    /// <summary>The colours in one set.</summary>
    public const int SetColours = 16;

    /// <summary>The gameplay position of the first set's first colour.</summary>
    private readonly int firstPosition;

    /// <summary>The sets' colours, set after set.</summary>
    private readonly RgbColor[] colours = new RgbColor[SetCount * SetColours];

    /// <summary>Sets whose first colour sits at gameplay position <paramref name="firstPosition"/>.</summary>
    private protected ColourSets(int firstPosition) => this.firstPosition = firstPosition;

    /// <summary>Every set, as one run.</summary>
    public ColourRun All => new(colours, 0, colours.Length);

    /// <inheritdoc />
    public IReadOnlyList<ColourRun> Stores => [All];

    /// <summary>Set <paramref name="set"/>, zero through seven.</summary>
    public ColourRun Set(int set) => new(colours, Ensure.BetweenInclusive(0, SetCount - 1, set) * SetColours, SetColours);

    /// <summary>The <paramref name="count"/> sets from <paramref name="first"/>, as one run.</summary>
    public ColourRun Sets(int first, int count)
    {
        Ensure.BetweenInclusive(0, SetCount - 1, first);
        Ensure.BetweenInclusive(1, SetCount - first, count);
        return new ColourRun(colours, first * SetColours, count * SetColours);
    }

    /// <inheritdoc />
    public bool Holds(int position) => position >= firstPosition && position < firstPosition + colours.Length;

    /// <inheritdoc />
    public RgbColor At(int position) => colours[Check(position)];

    /// <inheritdoc />
    public void Set(int position, RgbColor colour) => colours[Check(position)] = colour;

    /// <summary>The index of a held gameplay position among the sets' colours.</summary>
    private int Check(int position) => Holds(position)
        ? position - firstPosition
        : throw new ArgumentOutOfRangeException(nameof(position), position, "These colour sets hold no colour there.");
}
