namespace SuperMetroid.Core.Frontend.Title;

/// <summary>One title card's typing (#1979): its text, typed one letter per step, and how its typing is timed and ends.</summary>
/// <param name="Card">The card's identity.</param>
/// <param name="Text">The card's text; each typing step types one more of its letters, a space among them.</param>
/// <param name="OpeningFrames">Updates the card shows nothing before its first letter; zero for none.</param>
/// <param name="HoldFrames">Updates the card holds its whole text before it ends.</param>
/// <param name="Ending">The cue the card runs as it ends, before it removes itself.</param>
public sealed record TitleCardDefinition(TitleCard Card, string Text, int OpeningFrames, int HoldFrames, TitleCardCue Ending);
