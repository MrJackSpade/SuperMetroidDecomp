namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Puromi/Nuclear Waffle's body animation as a typed animation (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="NuclearWaffleAnimations"/>.
/// </summary>
internal enum NuclearWaffleSegment
{
    /// <summary>Its body turning through its eight angles in a tumbling order, repeating.</summary>
    BodyLoop,
}
