namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// How far a Yapping Maw's tongue has curled (#2555): angle units, with a fraction of a unit carried from update to update.
/// Each update the curl moves by the shared quadratic curve's speed at its place, the speed's pixels counting as the
/// curl's angle units, out to the largest curl and back; its whole units choose how its links bend.
/// </summary>
public readonly partial record struct YappingMawCurl
{
    /// <summary>Keeps the curl in angle units, fraction included, at the precision the curve's speeds add in.</summary>
    internal YappingMawCurl(SignedSixteenSixteen units) => Units = units;

    /// <summary>The curl in angle units, fraction included.</summary>
    internal SignedSixteenSixteen Units { get; }

    /// <summary>No curl at all: the tongue drawn in.</summary>
    public static YappingMawCurl None => default;

    /// <summary>The curl of <paramref name="units"/> whole angle units, with no fraction.</summary>
    public static YappingMawCurl FromWholeUnits(int units) => new(SignedSixteenSixteen.FromWholePixels(units));

    /// <summary>The curl's whole angle units, its fraction dropped toward fewer units.</summary>
    public int WholeUnits => Units.WholePixels;

    /// <summary>Whether the curl has passed back below none: its whole units are negative.</summary>
    public bool IsNegative => Units.IsNegative;

    /// <summary>
    /// The curl moved by one update of the curve's <paramref name="speed"/>, its pixels counting as angle units, the
    /// fraction carrying into the whole units.
    /// </summary>
    public YappingMawCurl MovedBy(SignedSixteenSixteen speed) => new(Units + speed);
}
