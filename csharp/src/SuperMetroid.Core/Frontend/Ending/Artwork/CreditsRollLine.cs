namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>One staff line on the credits roll (#1467, #1601): its role, its picture and where it lies on the roll.</summary>
/// <param name="Role">The line's role, which keys its editable text, with its glyph style and the space before it.</param>
/// <param name="Top">The distance from the roll's top to the line's top, in pixels.</param>
/// <param name="Height">The line's height, in pixels.</param>
public readonly record struct CreditsRollLine(CreditsLineDefinition Role, int Top, int Height)
{
    /// <summary>The line's picture name in the credits sheet, as <see cref="EndingPictureNames.CreditsLine"/> spells it.</summary>
    public string Picture => EndingPictureNames.CreditsLine(Role.Role);
}
