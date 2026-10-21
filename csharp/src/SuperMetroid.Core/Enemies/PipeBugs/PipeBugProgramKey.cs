
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The strength and animation selector that choose a pipe bug's program.</summary>
/// <param name="Strong">The strong.</param>
/// <param name="Selector">The selector.</param>
internal readonly record struct PipeBugProgramKey(bool Strong, PipeBugAnimationSelector Selector);
