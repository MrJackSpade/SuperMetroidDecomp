namespace Engine.Shared;

/// <summary>One step of a <see cref="FrameTimeline"/>.</summary>
/// <param name="Picture">The picture shown during the step, as a path within the assets tree; empty for none.</param>
/// <param name="Updates">How many updates the step lasts.</param>
/// <param name="Cues">The names of the cues reached as the step begins.</param>
public sealed record TimelineStep(string Picture, int Updates, IReadOnlyList<string> Cues);
