namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>One staff line on the credits roll (#1467, #1601): its picture and where it lies on the roll.</summary>
/// <param name="Picture">The line's picture name in the credits sheet, as <see cref="EndingPictureNames.CreditsLine"/> spells it.</param>
/// <param name="Top">The distance from the roll's top to the line's top, in pixels.</param>
/// <param name="Height">The line's height, in pixels.</param>
public readonly record struct CreditsRollLine(string Picture, int Top, int Height);
