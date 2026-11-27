namespace Engine.Shared;

/// <summary>One step of a <see cref="FrameTimeline"/>.</summary>
/// <param name="Picture">The picture shown during the step, as a path within the assets tree; empty for none.</param>
/// <param name="Updates">How many updates the step lasts.</param>
/// <param name="Cues">The names of the cues reached as the step begins.</param>
public sealed record TimelineStep(string Picture, int Updates, IReadOnlyList<string> Cues)
{
    /// <summary>
    /// Where the picture's top-left corner lies from the place of the thing shown, in whole pixels across and down,
    /// beyond any offset the thing has of its own: the timeline's, and the step's own added to it where it has one.
    /// </summary>
    public (int X, int Y) Offset { get; init; }

    /// <summary>
    /// Where the picture lies against the tiles of the room ("lies": "in_front_of_tiles" or "behind_room"), for
    /// something launched or left that shows it: the step's own word, or its timeline's where the step has none.
    /// </summary>
    public LiesAgainstTiles Lies { get; init; }
}
