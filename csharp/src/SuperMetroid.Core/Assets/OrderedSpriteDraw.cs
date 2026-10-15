namespace SuperMetroid.Core.Assets;

/// <summary>A whole-frame draw with its place among the frame's OAM sprites.</summary>
/// <param name="OamOrdinal">
/// Number of OAM sprites appended before this draw. The draw is in front of OAM sprite <c>OamOrdinal</c> and
/// every later one, and behind the earlier ones, so actors keep their drawing order while they move off OAM.
/// </param>
/// <param name="Draw">The entry.</param>
public readonly record struct OrderedSpriteDraw(int OamOrdinal, SpriteDraw Draw);
