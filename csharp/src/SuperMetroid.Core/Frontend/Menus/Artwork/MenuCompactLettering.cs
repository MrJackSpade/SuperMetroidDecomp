namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>Finds a compact-lettering label (<see cref="MenuCompactLetteringDefinitions"/>) by the text it spells.</summary>
internal static class MenuCompactLettering
{
    /// <summary>The label strip spelling <paramref name="text"/>.</summary>
    internal static MenuLetteringStrip Strip(string text) =>
        MenuCompactLetteringDefinitions.Strips.FirstOrDefault(strip => strip.Text == text) ??
        throw new ArgumentOutOfRangeException(nameof(text), text, "No compact-lettering label spells this text.");
}
