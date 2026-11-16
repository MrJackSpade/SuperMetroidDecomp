namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>The ending's text.</summary>
internal sealed class EndingText
{
    /// <summary>Holds <paramref name="text"/>.</summary>
    /// <param name="text">The text.</param>
    internal EndingText(string text) => Text = text;

    /// <summary>The text, which production reads.</summary>
    internal string Text { get; }
}