namespace Engine.Content;

/// <summary>How far the making of a game's assets has come (<see cref="AssetsMaker.Make"/>), for the setup screen to show.</summary>
/// <param name="Doing">What is being done, in words to show.</param>
/// <param name="Done">How much of the whole making is done, from 0 to 1: the bar's length.</param>
/// <param name="Detail">A line that changes as often as work is done: a count of what has been read or written.</param>
public sealed record MakingProgress(string Doing, float Done, string Detail);
