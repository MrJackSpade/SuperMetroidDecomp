namespace SuperMetroid.AssetExtraction.Cartridge;

/// <summary>The ending's text.</summary>
internal sealed class EndingText
{
    /// <summary>Holds <paramref name="text"/>.</summary>
    /// <param name="text">The text.</param>
    internal EndingText(string text) => Text = text;

    /// <summary>The text, which production reads.</summary>
    internal string Text { get; }

    /// <summary>The text as the typewriter types it, which only checks read.</summary>
    internal string Typewriter() => Text.ToLowerInvariant();
}