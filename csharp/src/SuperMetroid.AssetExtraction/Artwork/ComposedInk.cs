using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The colour a composed sheet's pixel was painted in (#2563): the colour group it was coloured from and its colour in that
/// group, colour zero being the group's clear colour. A composed manifest lists each group's colours in turn, so the colour
/// lies at its place in that list (<see cref="PlaceIn(SpriteImageColours)"/>). The default ink, the first group's clear
/// colour, is the one a pixel nothing painted holds.
/// </summary>
internal readonly record struct ComposedInk
{
    /// <summary>Names colour <paramref name="colour"/> of colour group <paramref name="group"/>.</summary>
    /// <param name="group">The group's place among the sheet's groups, from zero.</param>
    /// <param name="colour">The colour's place in its group, from zero, the group's clear colour.</param>
    /// <exception cref="ArgumentOutOfRangeException">The group or the colour lies before the first.</exception>
    internal ComposedInk(int group, int colour)
    {
        Group = Ensure.AtLeastZero(group);
        Colour = Ensure.AtLeastZero(colour);
    }

    /// <summary>The ink of a pixel nothing painted: the first group's clear colour.</summary>
    internal static ComposedInk None => default;

    /// <summary>The colour group's place among the sheet's groups.</summary>
    internal int Group { get; }

    /// <summary>The colour's place in its group; zero is the group's clear colour.</summary>
    internal int Colour { get; }

    /// <summary>Whether the ink is its group's clear colour.</summary>
    internal bool IsClear => Colour == 0;

    /// <summary>The clear colour of this ink's group.</summary>
    internal ComposedInk GroupClear => new(Group, 0);

    /// <summary>The colours of each group a sheet composed in <paramref name="palette"/> is painted in.</summary>
    internal static int GroupColoursOf(SpriteImageColours palette) =>
        Ensure.Listed(SpriteImageColourDefinitions.Colourings, palette).SetColours;

    /// <summary>The ink at <paramref name="place"/> of the colour list of a sheet composed in <paramref name="palette"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place lies before the first colour.</exception>
    internal static ComposedInk AtPlace(int place, SpriteImageColours palette)
    {
        int groupColours = GroupColoursOf(palette);
        Ensure.AtLeastZero(place);
        return new(place / groupColours, place % groupColours);
    }

    /// <summary>The ink's place in the colour list of a sheet composed in <paramref name="palette"/>.</summary>
    /// <exception cref="InvalidDataException">The ink's colour lies past its group's colours.</exception>
    internal int PlaceIn(SpriteImageColours palette)
    {
        int groupColours = GroupColoursOf(palette);
        if (Colour >= groupColours)
            throw new InvalidDataException($"Ink colour {Colour} of group {Group} lies past a {palette} sheet's {groupColours} colours a group.");
        return Group * groupColours + Colour;
    }
}
