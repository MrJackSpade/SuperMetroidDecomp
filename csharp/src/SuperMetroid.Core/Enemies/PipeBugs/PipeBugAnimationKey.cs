
namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The strength and animation that choose a pipe bug's segment.</summary>
/// <param name="Strong">Whether the bug is the strong Zebbo rather than the ordinary Zeb.</param>
/// <param name="Selector">The rising or shooting animation and its facing.</param>
internal readonly record struct PipeBugAnimationKey(bool Strong, PipeBugAnimationSelector Selector);
