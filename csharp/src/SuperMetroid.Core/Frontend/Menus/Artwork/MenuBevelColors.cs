namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>The face, highlight and shadow colours of a beveled menu square.</summary>
/// <param name="Face">The face.</param>
/// <param name="Highlight">The highlight.</param>
/// <param name="Shadow">The shadow.</param>
internal readonly record struct MenuBevelColors(int Face, int Highlight, int Shadow);
