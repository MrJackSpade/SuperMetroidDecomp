namespace SuperMetroid.Core;

/// <summary>A note the game shows; checks also make one with a prefix.</summary>
public sealed class Note
{
    /// <summary>The note's text.</summary>
    public string Text { get; }

    /// <summary>Creates a note of <paramref name="text"/>.</summary>
    /// <param name="text">The note's text.</param>
    public Note(string text)
    {
        Text = text;
    }
}
