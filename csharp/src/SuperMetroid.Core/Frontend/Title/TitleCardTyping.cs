namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// A title card's text typed as far as <paramref name="Letters"/> of its letters (#1979): what one of the card's typing
/// steps shows. A space counts as a letter and shows nothing.
/// </summary>
/// <param name="Card">The card typed.</param>
/// <param name="Letters">How many of the card's letters are typed, from one to the length of its text.</param>
public readonly record struct TitleCardTyping(TitleCard Card, int Letters);
