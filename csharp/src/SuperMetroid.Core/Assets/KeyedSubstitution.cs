namespace SuperMetroid.Core.Assets;

/// <summary>One colour a shade shows (#1633): the pixels of <paramref name="Replaced"/> show <paramref name="Shown"/>.</summary>
/// <param name="Replaced">The key of the pixels recoloured.</param>
/// <param name="Shown">The colour they show.</param>
public readonly record struct KeyedSubstitution(ColourKey Replaced, RgbColor Shown);
