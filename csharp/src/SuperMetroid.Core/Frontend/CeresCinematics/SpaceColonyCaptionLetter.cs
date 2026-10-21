namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>A letter of the space colony caption and its column.</summary>
/// <param name="Column">The column.</param>
/// <param name="Tile">The tile.</param>
internal readonly record struct SpaceColonyCaptionLetter(int Column, int Tile);
