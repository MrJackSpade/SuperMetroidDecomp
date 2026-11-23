namespace Engine.Actors;

/// <summary>
/// A place in one of an enemy's timelines, to pick it up from again: the timeline's name, the step, and how many of
/// that step's cues are already behind it.
/// </summary>
public readonly record struct TimelineMark(string Name, int Step, int CuesPassed);
