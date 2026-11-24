namespace Engine.Core;

/// <summary>
/// What follows a question answered yes before play goes on: a sound, a wait, and a second message, as a save that
/// is asked about, heard and then told of. The answer is given to whoever asked once the second message is dismissed.
/// </summary>
/// <param name="Sound">The sound heard as the question goes away, by name; empty for none.</param>
/// <param name="WaitUpdates">How many updates pass between the question going away and the message being brought up.</param>
/// <param name="Message">The message that follows, by name.</param>
/// <param name="HoldUpdates">How many updates the message stays up before it can be dismissed.</param>
/// <param name="BeforeOpeningUpdates">
/// For a game whose messages show through a window, how many updates nothing shows before the message's window
/// begins to open, in place of the style's own.
/// </param>
public sealed record MessageAfterYes(string Sound, int WaitUpdates, string Message, int HoldUpdates, int BeforeOpeningUpdates);
