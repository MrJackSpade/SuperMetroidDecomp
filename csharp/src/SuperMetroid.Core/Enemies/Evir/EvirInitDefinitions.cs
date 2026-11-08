namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Data of an Evir body's initialization.</summary>
public static class EvirInitDefinitions
{
    /// <summary>
    /// The first bobbing half-cycle of a body, by bobbing speed. The game's initialization looks the body's own
    /// direction-change period up with the position its speed is stored at in place of the body's own position, so the
    /// value it halves belongs to another enemy, fixed by the speed alone. Every Evir in the game bobs at half or three
    /// quarters of a pixel per update: the slower lands on the second enemy's direction-change period, and the faster on
    /// the third enemy's palette. The read happens while the room's population is loading, so an enemy later in the
    /// list than the body still holds the empty record the room load left.
    /// </summary>
    public static IReadOnlyList<EvirFirstHalfCycleSource> FirstHalfCycleSources { get; } =
    [
        new(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(8), LenderPlace: 1, EvirHalfCycleSourceField.DirectionChangePeriod),
        new(Speed: SignedSixteenSixteen.FromSixteenthsOfPixel(12), LenderPlace: 2, EvirHalfCycleSourceField.SpitColours),
    ];

    /// <summary>
    /// The first half-cycle of a body that reads an Evir spit's colours (#1512). The game reads the spit's palette
    /// selection, the row its room's graphics set loads Evir's colours into, kept nine bits up, so halving its upper
    /// eight bits gives the row. Both retail rooms holding Evirs load them into the first row the graphics set fills, so
    /// the half-cycle is one; the row itself is the import's.
    /// </summary>
    public const int SpitColoursHalfCycle = 1;
}
