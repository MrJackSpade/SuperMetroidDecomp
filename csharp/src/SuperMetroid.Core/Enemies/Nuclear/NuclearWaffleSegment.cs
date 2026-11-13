namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>
/// Puromi/Nuclear Waffle's body animation as a typed animation (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="NuclearWaffleAnimations"/>.
/// </summary>
internal enum NuclearWaffleSegment
{
    /// <summary>The body loop animation.</summary>
    BodyLoop,
}
