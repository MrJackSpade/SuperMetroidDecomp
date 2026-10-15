namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>A large menu font letter's two menu atlas cells.</summary>
/// <param name="Upper">The cell holding the letter's top half.</param>
/// <param name="Lower">The cell holding the letter's bottom half.</param>
internal readonly record struct MenuLargeFontLetter(int Upper, int Lower);
