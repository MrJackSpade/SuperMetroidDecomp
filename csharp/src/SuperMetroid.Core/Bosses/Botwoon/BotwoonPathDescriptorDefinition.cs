namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One authored Botwoon movement stream, traversal direction, and destination hole.</summary>
/// <param name="FirstSampleIndex">The stream's first movement sample; a reverse stream starts at its closing separator and rewinds two samples.</param>
/// <param name="Direction">Negative to walk the stream backward and negate its samples; zero to walk it forward.</param>
/// <param name="TargetHoleIndex">The destination hole, zero through three.</param>
internal readonly record struct BotwoonPathDescriptorDefinition(
    int FirstSampleIndex,
    short Direction,
    int TargetHoleIndex);
