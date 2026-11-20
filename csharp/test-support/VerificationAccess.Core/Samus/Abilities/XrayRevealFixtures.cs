using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.Samus.Abilities.Artwork;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Complete X-ray reveal pictures for focused fixtures: every drawable collision kind and reaction shows blocks the fixture
/// chooses, as an installation would, since Core holds no compiled copy of the reveal blocks (#1596).
/// </summary>
internal static class XrayRevealFixtures
{
    /// <summary>A reveal grid showing what <paramref name="shown"/> chooses at each place.</summary>
    internal static XrayRevealGrid Grid(Func<Position16<RoomBlocks>, RevealedBlock?> shown)
    {
        var grid = new XrayRevealGrid();
        foreach (Position16<RoomBlocks> place in XrayRevealGrid.Places)
            grid.Show(place, shown(place));
        return grid;
    }

    /// <summary>Every block <paramref name="reveal"/> shows, in <see cref="XrayRevealGrid.Places"/> order.</summary>
    internal static RevealedBlock?[] Shown(XrayRevealPlane reveal) => [.. XrayRevealGrid.Places.Select(place => reveal[place])];

    /// <summary>Every drawable collision kind and reaction, each showing the blocks <paramref name="shown"/> chooses for its command.</summary>
    internal static List<XrayRevealVisualDefinition> EveryDrawable(Func<XrayRevealShape, XrayRevealVisualBlocks> shown)
    {
        var entries = new List<XrayRevealVisualDefinition>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        foreach (Enum? reaction in BlockReactionFamilies.ReadingsOf(type))
            if (XrayReveals.Find(type, reaction) is { } command && InstalledXrayReveals.IsDrawable(command))
                entries.Add(new(type, reaction, shown(command)));
        return entries;
    }

    /// <summary>
    /// The blocks <paramref name="shape"/> copies out of a square of four: the top left alone, the top pair, the left pair or
    /// all four.
    /// </summary>
    internal static XrayRevealVisualBlocks Corners(XrayRevealShape shape, int topLeft, int topRight, int bottomLeft, int bottomRight) =>
        shape switch
        {
            XrayRevealShape.CopyWide => XrayRevealVisualBlocks.Wide(topLeft, topRight),
            XrayRevealShape.CopyTall => XrayRevealVisualBlocks.Tall(topLeft, bottomLeft),
            XrayRevealShape.CopySquare => XrayRevealVisualBlocks.Square(topLeft, topRight, bottomLeft, bottomRight),
            XrayRevealShape.CopyOne or XrayRevealShape.CopyBrinstar => XrayRevealVisualBlocks.One(topLeft),
            XrayRevealShape.HorizontalExtension or XrayRevealShape.VerticalExtension =>
                throw new ArgumentOutOfRangeException(nameof(shape), shape, "An extension shape shows no blocks."),
            _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, $"Undefined {nameof(XrayRevealShape)}."),
        };

    /// <summary>
    /// Complete reveal pictures whose blocks <paramref name="shown"/> chooses per command, with overlays that reveal no special
    /// room and show block zero for every tank.
    /// </summary>
    internal static InstalledXrayReveals Complete(Func<XrayRevealShape, XrayRevealVisualBlocks> shown) =>
        new(EveryDrawable(shown), InstalledXrayOverlays.FromOverlaysForVerification(
            InstalledXrayOverlays.TankKinds.ToDictionary(tank => tank, _ => RoomBlockAppearance.Of(0)), []));
}
