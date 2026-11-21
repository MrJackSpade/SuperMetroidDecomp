namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// One of the ending's texts typed as far as <paramref name="Letters"/> of its letters (#2195): what a typed text's actor
/// shows, as a title card's typing does (<see cref="Title.TitleCardTyping"/>). Its spaces are no letters: each is typed in
/// the pause after the letter before it.
/// </summary>
/// <param name="Text">The text typed.</param>
/// <param name="Letters">How many of the text's letters are typed, from one to all of them.</param>
public readonly partial record struct EndingTextTyping(EndingTypedText Text, int Letters);
