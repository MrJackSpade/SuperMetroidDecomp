namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's sprite colour sets (#1405): eight sets of sixteen. A room load fills the common sprite colours and the enemy
/// projectiles', the room's graphics set loads each enemy's colours into the set it gives that enemy, and enemy effects
/// change them. A sprite shows the set its graphics set gives it, or one its AI or a hit flash selects. The colours Samus
/// holds are hers, not the sets'.
/// </summary>
public sealed class SpriteColourSets : IGameplayColourOwner, ISpriteColourSets
{
    /// <summary>The number of sets.</summary>
    public const int SetCount = 8;

    /// <summary>The colours in one set.</summary>
    public const int SetColours = 16;

    /// <summary>The gameplay position of the first set's first colour, for writers that still address colours by position.</summary>
    private const int FirstPosition = 128;

    /// <summary>The sets' colours, set after set.</summary>
    private readonly Bgr555[] colours = new Bgr555[SetCount * SetColours];

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
    public bool Holds(int position) => position >= FirstPosition && position < FirstPosition + colours.Length;

    /// <inheritdoc />
    public Bgr555 At(int position) => colours[Check(position)];

    /// <inheritdoc />
    public void Set(int position, Bgr555 colour) => colours[Check(position)] = colour;

    /// <summary>The index of a held gameplay position among the sets' colours.</summary>
    private int Check(int position) => Holds(position)
        ? position - FirstPosition
        : throw new ArgumentOutOfRangeException(nameof(position), position, "The sprite colour sets hold no colour there.");
}
