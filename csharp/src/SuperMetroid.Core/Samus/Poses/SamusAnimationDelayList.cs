namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The delay program a Samus pose animates by.</summary>
/// <param name="Segment">The program's place among the compiled delay programs, or null for an invalid pose's program.</param>
/// <param name="ResidueStart">Where an invalid pose's program starts in the floor update residue; zero for a compiled program.</param>
public readonly record struct SamusAnimationDelayList(int? Segment, int ResidueStart);
