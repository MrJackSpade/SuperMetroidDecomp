namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A place along the shared quadratic speed table (<see cref="EnemyQuadraticSpeeds"/>), counted in 256ths of a row: its
/// whole rows select the row whose speed applies, and its fraction carries a slower climb or fall toward the next row.
/// The enemies that accelerate through the table (the boulders, the Ki-Hunter's detached wing, the large Maridia snail,
/// Botwoon's falling head, the Puyo, and the whole-row movers: the Beetom's, Choot's and hoppers' hops, the platforms,
/// the yellow Pipe Bug's arc, the Polyp's rock and the Yapping Maw's curl) step such a place each update instead of a
/// speed; a step is a place's difference in the same 256ths. Places only grow, shrink and compare; nothing wraps, because each mover caps or floors its place well inside
/// the table, and a place past the table's last row or below its first fails when its row is read.
/// </summary>
public readonly record struct EnemyQuadraticSpeedIndex : IComparable<EnemyQuadraticSpeedIndex>
{
    /// <summary>Keeps the place's reading in 256ths of a row.</summary>
    private EnemyQuadraticSpeedIndex(int twoHundredFiftySixthsOfRow) => InTwoHundredFiftySixthsOfRow = twoHundredFiftySixthsOfRow;

    /// <summary>The place in 256ths of a row, below zero only for a mover that keeps a place it took past the first row.</summary>
    [IntegerQuantity("place along the quadratic speed table in 256ths of a row")]
    public int InTwoHundredFiftySixthsOfRow { get; }

    /// <summary>The first row with no fraction: the place an accelerating mover starts from.</summary>
    public static EnemyQuadraticSpeedIndex Start => default;

    /// <summary>The step of one whole row, by which the whole-row movers move along the table each update.</summary>
    public static EnemyQuadraticSpeedIndex OneRow => FromRows(1);

    /// <summary>The place <paramref name="rows"/> whole rows along the table, or the step of that many rows.</summary>
    public static EnemyQuadraticSpeedIndex FromRows(int rows) => new(EightEight.FromWhole(rows));

    /// <summary>The place <paramref name="twoHundredFiftySixths"/> 256ths of a row along the table, or the step of that many.</summary>
    public static EnemyQuadraticSpeedIndex FromTwoHundredFiftySixthsOfRow(int twoHundredFiftySixths) => new(twoHundredFiftySixths);

    /// <summary>The whole rows of the place, its fraction dropped toward negative infinity.</summary>
    [IntegerQuantity("whole rows of a quadratic speed table place")]
    public int WholeRows => EightEight.WholeOf(InTwoHundredFiftySixthsOfRow);

    /// <summary>The row whose speed applies at this place: the row of its whole rows.</summary>
    /// <exception cref="InvalidDataException">The place lies past the table's last row or below its first.</exception>
    public EnemyQuadraticSpeedRow Row => EnemyQuadraticSpeeds.Row(WholeRows);

    /// <summary>Whether the place lies below the table's first row.</summary>
    public bool IsNegative => InTwoHundredFiftySixthsOfRow < 0;

    /// <summary>The place moved on by <paramref name="step"/>.</summary>
    public static EnemyQuadraticSpeedIndex operator +(EnemyQuadraticSpeedIndex place, EnemyQuadraticSpeedIndex step) =>
        new(checked(place.InTwoHundredFiftySixthsOfRow + step.InTwoHundredFiftySixthsOfRow));

    /// <summary>The place moved back by <paramref name="step"/>.</summary>
    public static EnemyQuadraticSpeedIndex operator -(EnemyQuadraticSpeedIndex place, EnemyQuadraticSpeedIndex step) =>
        new(checked(place.InTwoHundredFiftySixthsOfRow - step.InTwoHundredFiftySixthsOfRow));

    /// <summary>The earlier of two places.</summary>
    public static EnemyQuadraticSpeedIndex Min(EnemyQuadraticSpeedIndex left, EnemyQuadraticSpeedIndex right) =>
        left <= right ? left : right;

    /// <summary>Orders places by how far along the table they lie.</summary>
    public int CompareTo(EnemyQuadraticSpeedIndex other) => InTwoHundredFiftySixthsOfRow.CompareTo(other.InTwoHundredFiftySixthsOfRow);

    /// <summary>Whether <paramref name="left"/> lies earlier along the table.</summary>
    public static bool operator <(EnemyQuadraticSpeedIndex left, EnemyQuadraticSpeedIndex right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> lies further along the table.</summary>
    public static bool operator >(EnemyQuadraticSpeedIndex left, EnemyQuadraticSpeedIndex right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> lies no further along the table.</summary>
    public static bool operator <=(EnemyQuadraticSpeedIndex left, EnemyQuadraticSpeedIndex right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> lies no earlier along the table.</summary>
    public static bool operator >=(EnemyQuadraticSpeedIndex left, EnemyQuadraticSpeedIndex right) => left.CompareTo(right) >= 0;
}
