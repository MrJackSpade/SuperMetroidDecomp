namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where a frontend screen holds one of its colour groups today (#1464): a run of <paramref name="ColourCount"/> colours
/// from colour <paramref name="FirstColour"/> of row <paramref name="Row"/>. A run may continue into the following rows.
/// Only <see cref="ScreenColourGroups"/> reads it.
/// </summary>
/// <param name="Row">The row the group starts in.</param>
/// <param name="FirstColour">The group's first colour within that row.</param>
/// <param name="ColourCount">The number of colours in the group.</param>
internal abstract record ScreenColourGroupDefinition(int Row, int FirstColour, int ColourCount);
