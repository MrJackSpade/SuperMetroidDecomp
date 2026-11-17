namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Exact discrete Phantoon figure-eight movement, not a sampled analytic curve.</summary>
public static class PhantoonPathDefinitions
{
    /// <summary>The path's 534 signed X/Y unit steps.</summary>
    public const int Length = 534;

    /// <summary>The path's last place, where a figure-eight run backwards from its start begins (#1940).</summary>
    public const int LastPlace = Length - 1;

    /// <summary>The place on the path he reappears at on the right of his room, at <see cref="RightAppearancePosition"/> (#1940).</summary>
    public const int RightAppearance = 136;

    /// <summary>The place on the path he reappears at on the left of his room, at <see cref="LeftAppearancePosition"/> (#1940).</summary>
    public const int LeftAppearance = 399;

    /// <summary>The height he reappears at, on either side of his room.</summary>
    public const int AppearanceY = 96;

    /// <summary>Where he reappears on the right of his room, at <see cref="RightAppearance"/> on the path.</summary>
    public static Position16<RoomPixels> RightAppearancePosition { get; } = new(208, AppearanceY);

    /// <summary>Where he reappears on the left of his room, at <see cref="LeftAppearance"/> on the path.</summary>
    public static Position16<RoomPixels> LeftAppearancePosition { get; } = new(48, AppearanceY);

    /// <summary>Lossless keypad notation for unit steps with screen Y increasing downward; source rows are formatting only.</summary>
    /// <remarks>8/6/2/4 mean north/east/south/west; 7/9/1/3 are diagonals. His place on the path crosses string-row boundaries without resetting.</remarks>
    internal const string Directions =
        "8686898686886868688686868686886868686868668686868668668668668666" +
        "6866666666663666626662662663636266262662626263333262626226226226" +
        "2226222222224222422124224242421241142424244242442414144244244424" +
        "4441444444444484444844844844847484847748484877848484848784848488" +
        "4848848484884848784848848484878484848487848484848474848474844844" +
        "8448444484444444444144442444244244141424424244242424241112424242" +
        "2421224222422222222622262262262262626233263626262662626626363662" +
        "6626662666636666666666866668668668668668686868696868686868868686" +
        "8686886868986868986868";

    /// <summary>The signed unit step each keypad direction stands for, with screen Y increasing downward.</summary>
    internal static readonly Dictionary<char, Offset16<RoomPixels>> Steps = new()
    {
        ['1'] = new(-1, 1),
        ['2'] = new(0, 1),
        ['3'] = new(1, 1),
        ['4'] = new(-1, 0),
        ['6'] = new(1, 0),
        ['7'] = new(-1, -1),
        ['8'] = new(0, -1),
        ['9'] = new(1, -1),
    };
}
