namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>What one of the Boulder's rolls is (<see cref="BoulderRollDefinitions.Rolls"/>).</summary>
/// <param name="Direction">The way the Boulder rolls, which its trigger window and speed face.</param>
/// <param name="Segment">The animation segment its roll starts at.</param>
/// <param name="BreaksOnImpact">Whether the Boulder breaks on its first impact rather than bouncing on.</param>
[IndependentFlag("whether a roll breaks on impact is a fact of that roll, independent of its direction and segment")]
internal sealed record BoulderRollDefinition(HorizontalDirection Direction, BoulderSegment Segment, bool BreaksOnImpact);
