using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The base background colours one room graphics-set palette source gives each background set (#1464). The import
/// pipeline decodes the installed palette, which keeps the cartridge's colour order, into the sets.
/// </summary>
public sealed class RoomStaticPalette
{
    /// <summary>Holds the colours the import pipeline decoded for each background set.</summary>
    /// <param name="sets">The colours a room load gives each background set.</param>
    internal RoomStaticPalette(ShownBackgroundColourSets sets) => Sets = Ensure.NotNull(sets);

    /// <summary>The colours a room load gives each background set (#1464).</summary>
    public ShownBackgroundColourSets Sets { get; }
}
