namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One path choice: the authored path Botwoon walks, which way, where it starts and the hole it reaches.</summary>
/// <param name="Path">The authored path, an index into <see cref="BotwoonPathDefinitions.Paths"/>.</param>
/// <param name="Traversal">Which way the path is walked.</param>
/// <param name="FirstStep">The step the walk starts at.</param>
/// <param name="TargetHoleIndex">The destination hole, zero through three.</param>
internal readonly record struct BotwoonPathDescriptorDefinition(
    int Path,
    BotwoonPathTraversal Traversal,
    int FirstStep,
    int TargetHoleIndex);
